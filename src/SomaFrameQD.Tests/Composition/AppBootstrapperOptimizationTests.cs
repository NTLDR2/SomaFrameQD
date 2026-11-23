using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SomaFrameQD.Tests.Composition
{
    [STATestClass]
    [DoNotParallelize]
    public class AppBootstrapperOptimizationTests
    {
        [STATestMethod]
        public void ShouldInspectAssemblyFile_SkipsKnownVendorAndBclAssemblies_AllowsSomaFrameQDAndCustomPlugins()
        {
            var bootstrapper = new TestAppBootstrapper();

            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\Microsoft.CodeAnalysis.CSharp.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\System.Text.Json.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\netstandard.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\mscorlib.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\Caliburn.Micro.Platform.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\AvalonDock.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\AvalonDock.Themes.VS2013.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\MahApps.Metro.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\ControlzEx.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\ICSharpCode.AvalonEdit.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\DotNetProjects.Wpf.Extended.Toolkit.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\HelixToolkit.Wpf.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\SomaFrameQD.resources.dll"));
            Assert.IsFalse(bootstrapper.InspectFile(@"C:\app\readme.txt"));

            Assert.IsTrue(bootstrapper.InspectFile(@"C:\app\SomaFrameQD.dll"));
            Assert.IsTrue(bootstrapper.InspectFile(@"C:\app\SomaFrameQD.Modules.CodeEditor.dll"));
            Assert.IsTrue(bootstrapper.InspectFile(@"C:\app\SomaFrameQD.Modules.Inspector.dll"));
            Assert.IsTrue(bootstrapper.InspectFile(@"C:\app\SomaFrameQD.Demo.exe"));
            Assert.IsTrue(bootstrapper.InspectFile(@"C:\app\Contoso.CustomPlugin.dll"));
        }

        [STATestMethod]
        public void MultiCoreJit_WhenProfileInitializationFails_DegradesCleanlyAndWritesDebugLogFile()
        {
            var tempRoot = Path.Combine(Path.GetTempPath(), "SomaFrameQD.JitTests." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);

            try
            {
                // Point JitProfileDirectory at an existing regular file so Directory.CreateDirectory fails deterministically.
                var blockedProfilePath = Path.Combine(tempRoot, "blocked-profile-file");
                File.WriteAllText(blockedProfilePath, "not-a-directory");

                var debugLogPath = Path.Combine(tempRoot, "Logs", "SomaFrameQD.JitFallback.debug.log");
                var bootstrapper = new TestAppBootstrapper(
                    enableJitOnManualRun: true,
                    jitProfileDirectory: blockedProfilePath,
                    jitFallbackLogPath: debugLogPath);

                bootstrapper.RunConfigureMultiCoreJit();

                Assert.IsTrue(File.Exists(debugLogPath), "Expected fallback debug log file to be created.");
                var logContents = File.ReadAllText(debugLogPath);
                StringAssert.Contains(logContents, "Multi-Core JIT degraded to standard CLR 4.8 JIT.");
                StringAssert.Contains(logContents, "Is64BitProcess: True");
                StringAssert.Contains(logContents, blockedProfilePath);
                StringAssert.Contains(logContents, nameof(IOException));
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, recursive: true);
                }
            }
        }

        [STATestMethod]
        public void AssemblyResolveUnification_ResolvesHigherVersion_WhenLowerStrongNameRequested()
        {
            // Ensure System.Runtime.CompilerServices.Unsafe is loaded in the 64-bit net48 test AppDomain.
            GC.KeepAlive(typeof(Unsafe).Assembly);

            var resolved = TestAppBootstrapper.UnifyAssembly(
                "System.Runtime.CompilerServices.Unsafe, Version=4.0.4.1, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

            Assert.IsNotNull(resolved);
            Assert.AreEqual("System.Runtime.CompilerServices.Unsafe", resolved.GetName().Name);
            Assert.IsTrue(resolved.GetName().Version >= new Version(6, 0, 0, 0));

            // Mismatched PublicKeyToken or impossible higher version must never unify.
            var wrongToken = TestAppBootstrapper.UnifyAssembly(
                "System.Runtime.CompilerServices.Unsafe, Version=4.0.4.1, Culture=neutral, PublicKeyToken=0000000000000000");
            Assert.IsNull(wrongToken);

            var higherThanAvailable = TestAppBootstrapper.UnifyAssembly(
                "System.Runtime.CompilerServices.Unsafe, Version=99.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            Assert.IsNull(higherThanAvailable);
        }

        [STATestMethod]
        public void DemoAppManifestAndConfigs_DeclareWin7Sp1ThroughWin11SupportedOsSystemDpiAndNet48Sku()
        {
            var repoRoot = FindRepositoryRoot();
            var manifestPath = Path.Combine(repoRoot, "src", "SomaFrameQD.Demo", "app.manifest");
            var coreConfigPath = Path.Combine(repoRoot, "src", "SomaFrameQD", "app.config");
            var demoConfigPath = Path.Combine(repoRoot, "src", "SomaFrameQD.Demo", "app.config");
            var demoAppXamlPath = Path.Combine(repoRoot, "src", "SomaFrameQD.Demo", "App.xaml");

            Assert.IsTrue(File.Exists(manifestPath), "Missing SomaFrameQD.Demo/app.manifest.");

            var manifestDoc = XDocument.Load(manifestPath);
            XNamespace compatNs = "urn:schemas-microsoft-com:compatibility.v1";
            XNamespace dpi2005Ns = "http://schemas.microsoft.com/SMI/2005/WindowsSettings";
            XNamespace dpi2016Ns = "http://schemas.microsoft.com/SMI/2016/WindowsSettings";
            XNamespace asmV3Ns = "urn:schemas-microsoft-com:asm.v3";

            var supportedOsIds = manifestDoc
                .Descendants(compatNs + "supportedOS")
                .Select(e => (string)e.Attribute("Id"))
                .ToList();

            CollectionAssert.Contains(supportedOsIds, "{35138b9a-5d96-4fbd-8e2d-a2440225f93a}"); // Windows 7 SP1
            CollectionAssert.Contains(supportedOsIds, "{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}"); // Windows 8
            CollectionAssert.Contains(supportedOsIds, "{1f676c76-80e1-4239-95bb-83d0f6d0da78}"); // Windows 8.1
            CollectionAssert.Contains(supportedOsIds, "{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}"); // Windows 10 & Windows 11

            var dpiAware = manifestDoc.Descendants(dpi2005Ns + "dpiAware").Single().Value.Trim();
            var dpiAwareness = manifestDoc.Descendants(dpi2016Ns + "dpiAwareness").Single().Value.Trim();
            Assert.AreEqual("true", dpiAware);
            Assert.AreEqual("system", dpiAwareness);

            var executionLevel = manifestDoc.Descendants(asmV3Ns + "requestedExecutionLevel").Single();
            Assert.AreEqual("asInvoker", (string)executionLevel.Attribute("level"));
            Assert.AreEqual("false", (string)executionLevel.Attribute("uiAccess"));

            foreach (var configPath in new[] { coreConfigPath, demoConfigPath })
            {
                var configDoc = XDocument.Load(configPath);
                var supportedRuntime = configDoc.Descendants("supportedRuntime").Single();
                Assert.AreEqual("v4.0", (string)supportedRuntime.Attribute("version"));
                Assert.AreEqual(".NETFramework,Version=v4.8", (string)supportedRuntime.Attribute("sku"));
            }

            var demoAppXaml = File.ReadAllText(demoAppXamlPath);
            StringAssert.Contains(demoAppXaml, "<local:DemoAppBootstrapper x:Key=\"bootstrapper\" />");
            Assert.IsFalse(demoAppXaml.Contains("<!--"), "Obsolete commented bootstrapper markup should be removed.");
        }

        [STATestMethod]
        public void PopulateAssemblySourceUsingDirectoryCatalog_IgnoresNativeDllInProbingDirectory()
        {
            var tempRoot = Path.Combine(Path.GetTempPath(), "SomaFrameQD.ProbingTests." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempRoot);

            try
            {
                var fakeNativeDll = Path.Combine(tempRoot, "CustomNativeHelper.dll");
                File.WriteAllBytes(fakeNativeDll, new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 });

                var bootstrapper = new TestAppBootstrapper();
                bootstrapper.ProbeDirectory(tempRoot);
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, recursive: true);
                }
            }
        }

        [STATestMethod]
        public void NetworkDefaults_PreservesSystemDefaultOnWin10Win11_AndSupportsTls12OnWin7()
        {
            // Windows 7 SP1 / Server 2008 R2 (NT 6.1) upgrades SystemDefault (0) to Tls12.
            Assert.AreEqual(
                SecurityProtocolType.Tls12,
                AppBootstrapper.ResolveCrossOsSecurityProtocol(
                    new Version(6, 1, 7601),
                    SecurityProtocolType.SystemDefault));

            // Explicit host-configured protocols on Windows 7 are respected unchanged.
            var customWin7 = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            Assert.AreEqual(
                customWin7,
                AppBootstrapper.ResolveCrossOsSecurityProtocol(
                    new Version(6, 1, 7601),
                    customWin7));

            // Windows 8.1, Windows 10, and Windows 11 preserve SystemDefault (0) so OS-native TLS 1.2 + TLS 1.3 negotiate freely.
            Assert.AreEqual(
                SecurityProtocolType.SystemDefault,
                AppBootstrapper.ResolveCrossOsSecurityProtocol(
                    new Version(6, 3, 9600),
                    SecurityProtocolType.SystemDefault));
            Assert.AreEqual(
                SecurityProtocolType.SystemDefault,
                AppBootstrapper.ResolveCrossOsSecurityProtocol(
                    new Version(10, 0, 19045),
                    SecurityProtocolType.SystemDefault));
            Assert.AreEqual(
                SecurityProtocolType.SystemDefault,
                AppBootstrapper.ResolveCrossOsSecurityProtocol(
                    new Version(10, 0, 26100),
                    SecurityProtocolType.SystemDefault));
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "src", "SomaFrameQD.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate SomaFrameQD repository root.");
        }

        private sealed class TestAppBootstrapper : AppBootstrapper
        {
            private readonly string _jitProfileDirectory;
            private readonly string _jitFallbackLogPath;
            private bool _jitEnabled;

            public TestAppBootstrapper(
                bool enableJitOnManualRun = false,
                string jitProfileDirectory = null,
                string jitFallbackLogPath = null)
                : base(useApplication: false)
            {
                _jitEnabled = enableJitOnManualRun;
                _jitProfileDirectory = jitProfileDirectory;
                _jitFallbackLogPath = jitFallbackLogPath;
            }

            protected override bool EnableMultiCoreJit => _jitEnabled;

            protected override string JitProfileDirectory => _jitProfileDirectory;

            protected override string JitFallbackDebugLogFilePath => _jitFallbackLogPath;

            public bool InspectFile(string filePath)
                => ShouldInspectAssemblyFile(filePath);

            public void RunConfigureMultiCoreJit()
                => ConfigureMultiCoreJit();

            public void ProbeDirectory(string directoryPath)
                => PopulateAssemblySourceUsingDirectoryCatalog(directoryPath);

            public static Assembly UnifyAssembly(string requestedAssemblyName, string probingDirectory = null)
                => TryUnifyStrongNamedAssembly(requestedAssemblyName, probingDirectory);
        }
    }
}
