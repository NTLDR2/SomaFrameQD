using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using SomaFrameQD.Framework.Services;

namespace SomaFrameQD
{
    public class AppBootstrapper : BootstrapperBase
    {
        private static int _assemblyResolveRegistered;

        [ThreadStatic]
        private static HashSet<string> _resolvingAssemblyPaths;

        private static readonly string[] SkippedAssemblyFilePrefixes =
        {
            "System.",
            "Microsoft.",
            "netstandard.",
            "mscorlib.",
            "Caliburn.Micro",
            "AvalonDock",
            "MahApps.Metro",
            "ControlzEx",
            "ICSharpCode.AvalonEdit",
            "DotNetProjects.Wpf.Extended.Toolkit",
            "HelixToolkit"
        };

        private List<Assembly> _priorityAssemblies;

        protected CompositionContainer Container { get; set; }

        internal IList<Assembly> PriorityAssemblies
            => _priorityAssemblies;

        /// <summary>
        /// Gets a value indicating whether CLR 4.8 Multi-Core Background JIT (<see cref="ProfileOptimization"/>)
        /// is enabled during startup.
        /// </summary>
        protected virtual bool EnableMultiCoreJit => true;

        /// <summary>
        /// Gets the profile file name recorded by <see cref="ProfileOptimization.StartProfile(string)"/>.
        /// </summary>
        protected virtual string JitProfileFileName => "Startup.net48.jitprofile";

        /// <summary>
        /// Gets the directory where CLR 4.8 Multi-Core JIT startup profiles are stored.
        /// Defaults to <c>%LocalAppData%\&lt;EntryAssemblyName&gt;\JitProfiles</c> when an entry assembly is present.
        /// </summary>
        protected virtual string JitProfileDirectory
        {
            get
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly == null)
                {
                    return null;
                }

                var entryAssemblyName = GetEntryAssemblyFolderName(entryAssembly);
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (string.IsNullOrWhiteSpace(localAppData))
                {
                    return null;
                }

                return Path.Combine(localAppData, entryAssemblyName, "JitProfiles");
            }
        }

        /// <summary>
        /// Gets the diagnostic log file path written if Multi-Core JIT initialization degrades to standard CLR JIT.
        /// </summary>
        protected virtual string JitFallbackDebugLogFilePath
        {
            get
            {
                var entryAssemblyName = GetEntryAssemblyFolderName(Assembly.GetEntryAssembly());
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrWhiteSpace(localAppData))
                {
                    return Path.Combine(localAppData, entryAssemblyName, "Logs", "SomaFrameQD.JitFallback.debug.log");
                }

                return Path.Combine(Path.GetTempPath(), entryAssemblyName + ".JitFallback.debug.log");
            }
        }

        public AppBootstrapper()
            : this(true)
        {
        }

        protected AppBootstrapper(bool useApplication)
            : base(useApplication)
        {
            PreInitialize();
            if (useApplication)
            {
                Initialize();
            }
        }

        protected virtual void PreInitialize()
        {
            ConfigureCrossOsNetworkDefaults();
            RegisterAssemblyResolveHandler();
            ConfigureMultiCoreJit();

            var code = Properties.Settings.Default.LanguageCode;

            if (!string.IsNullOrWhiteSpace(code))
            {
                var culture = CultureInfo.GetCultureInfo(code);
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        /// <summary>
        /// Ensures modern TLS support on Windows 7 SP1 (<c>NT 6.1</c>) when <see cref="ServicePointManager.SecurityProtocol"/>
        /// is at <see cref="SecurityProtocolType.SystemDefault"/>, while leaving <see cref="SecurityProtocolType.SystemDefault"/>
        /// untouched on Windows 8.1, Windows 10, and Windows 11 so OS-native TLS 1.2 and TLS 1.3 negotiation remains active.
        /// </summary>
        protected virtual void ConfigureCrossOsNetworkDefaults()
        {
            try
            {
                var current = ServicePointManager.SecurityProtocol;
                var resolved = ResolveCrossOsSecurityProtocol(Environment.OSVersion.Version, current);
                if (resolved != current)
                {
                    ServicePointManager.SecurityProtocol = resolved;
                }
            }
            catch
            {
                // Never fault startup if system security policy restricts ServicePointManager changes.
            }
        }

        internal static SecurityProtocolType ResolveCrossOsSecurityProtocol(
            Version osVersion,
            SecurityProtocolType currentProtocol)
        {
            if (osVersion != null &&
                osVersion.Major == 6 &&
                osVersion.Minor == 1 &&
                currentProtocol == SecurityProtocolType.SystemDefault)
            {
                return SecurityProtocolType.Tls12;
            }

            return currentProtocol;
        }

        /// <summary>
        /// Configures CLR 4.8 Multi-Core Background JIT (<see cref="ProfileOptimization"/>).
        /// If the profile directory cannot be created or accessed, degrades silently to normal CLR JIT
        /// and appends diagnostic details to <see cref="JitFallbackDebugLogFilePath"/>.
        /// </summary>
        protected virtual void ConfigureMultiCoreJit()
        {
            if (!EnableMultiCoreJit)
            {
                return;
            }

            string profileDirectory = null;
            try
            {
                profileDirectory = JitProfileDirectory;
                if (string.IsNullOrWhiteSpace(profileDirectory))
                {
                    return;
                }

                Directory.CreateDirectory(profileDirectory);
                ProfileOptimization.SetProfileRoot(profileDirectory);
                ProfileOptimization.StartProfile(JitProfileFileName);
            }
            catch (Exception exception)
            {
                WriteMultiCoreJitFallbackDebugLog(profileDirectory, exception);
            }
        }

        /// <summary>
        /// Writes a diagnostic entry when Multi-Core JIT falls back to standard CLR JIT.
        /// Falls back to <see cref="Path.GetTempPath"/> if the primary log location is inaccessible,
        /// and never throws during application startup.
        /// </summary>
        protected virtual void WriteMultiCoreJitFallbackDebugLog(string attemptedProfileDirectory, Exception exception)
        {
            var entryAssemblyName = GetEntryAssemblyFolderName(Assembly.GetEntryAssembly());
            var logEntry = string.Format(
                CultureInfo.InvariantCulture,
                "[{0}] Multi-Core JIT degraded to standard CLR 4.8 JIT.{1}" +
                "OSVersion: {2}{1}" +
                "Is64BitProcess: {3}{1}" +
                "AttemptedProfileDirectory: {4}{1}" +
                "Exception: {5}{1}{1}",
                DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture),
                Environment.NewLine,
                Environment.OSVersion,
                Environment.Is64BitProcess,
                attemptedProfileDirectory ?? "<null>",
                exception);

            Debug.WriteLine(logEntry);

            try
            {
                var primaryLogPath = JitFallbackDebugLogFilePath;
                if (!string.IsNullOrWhiteSpace(primaryLogPath))
                {
                    var logDirectory = Path.GetDirectoryName(primaryLogPath);
                    if (!string.IsNullOrWhiteSpace(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }

                    File.AppendAllText(primaryLogPath, logEntry, Encoding.UTF8);
                    return;
                }
            }
            catch
            {
                // Fall back to %TEMP% below if %LocalAppData% or custom log directory is inaccessible.
            }

            try
            {
                var fallbackLogPath = Path.Combine(Path.GetTempPath(), entryAssemblyName + ".JitFallback.debug.log");
                File.AppendAllText(fallbackLogPath, logEntry, Encoding.UTF8);
            }
            catch
            {
                // Diagnostic logging must never fault application startup.
            }
        }

        /// <summary>
        /// By default, we are configured to use MEF with two-tier priority export resolution.
        /// </summary>
        protected override void Configure()
        {
            PopulateAssemblySource();

            // Prioritise the executable assembly. This allows the client project to override exports, including IShell.
            // The client project can override SelectAssemblies to choose which assemblies are prioritised.
            _priorityAssemblies = SelectAssemblies()
                .Where(x => x != null)
                .Distinct()
                .ToList();
            var priorityCatalog = new AggregateCatalog(_priorityAssemblies.Select(x => new AssemblyCatalog(x)));
            var priorityProvider = new CatalogExportProvider(priorityCatalog);

            // Now get all other assemblies (excluding the priority assemblies).
            var mainCatalog = new AggregateCatalog(
                AssemblySource.Instance
                    .Where(assembly => assembly != null && !_priorityAssemblies.Contains(assembly))
                    .Distinct()
                    .Select(x => new AssemblyCatalog(x)));
            var mainProvider = new CatalogExportProvider(mainCatalog);

            Container = new CompositionContainer(priorityProvider, mainProvider);
            priorityProvider.SourceProvider = Container;
            mainProvider.SourceProvider = Container;

            var batch = new CompositionBatch();

            BindServices(batch);
            batch.AddExportedValue(mainCatalog);

            Container.Compose(batch);
        }

        protected virtual void PopulateAssemblySource()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrWhiteSpace(baseDirectory) && Directory.Exists(baseDirectory))
            {
                PopulateAssemblySourceUsingDirectoryCatalog(baseDirectory);
            }
        }

        /// <summary>
        /// Determines whether a file in the probing directory should be inspected for MEF exports.
        /// Skips known non-MEF BCL and third-party vendor assemblies while allowing all <c>SomaFrameQD*</c>
        /// and custom host/plugin assemblies.
        /// </summary>
        protected virtual bool ShouldInspectAssemblyFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            var fileName = Path.GetFileName(filePath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
                !fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (fileName.EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            for (int i = 0; i < SkippedAssemblyFilePrefixes.Length; i++)
            {
                if (fileName.StartsWith(SkippedAssemblyFilePrefixes[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        protected void PopulateAssemblySourceUsingDirectoryCatalog(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return;
            }

            var existingAssemblies = new HashSet<Assembly>(AssemblySource.Instance.Where(a => a != null));
            var loadedByPath = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

            foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (loaded == null || loaded.IsDynamic)
                {
                    continue;
                }

                try
                {
                    if (!string.IsNullOrWhiteSpace(loaded.Location))
                    {
                        loadedByPath[Path.GetFullPath(loaded.Location)] = loaded;
                    }
                }
                catch (NotSupportedException)
                {
                }
            }

            foreach (var filePath in Directory.EnumerateFiles(path))
            {
                if (!ShouldInspectAssemblyFile(filePath))
                {
                    continue;
                }

                var fullPath = Path.GetFullPath(filePath);
                if (!loadedByPath.TryGetValue(fullPath, out var candidateAssembly))
                {
                    AssemblyName assemblyName;
                    try
                    {
                        assemblyName = AssemblyName.GetAssemblyName(fullPath);
                    }
                    catch (BadImageFormatException)
                    {
                        // Ignore native/unmanaged binaries located in the probing directory.
                        continue;
                    }

                    try
                    {
                        candidateAssembly = Assembly.Load(assemblyName);
                    }
                    catch (FileNotFoundException)
                    {
                        candidateAssembly = Assembly.LoadFrom(fullPath);
                    }

                    loadedByPath[fullPath] = candidateAssembly;
                }

                if (existingAssemblies.Contains(candidateAssembly))
                {
                    continue;
                }

                using (var assemblyCatalog = new AssemblyCatalog(candidateAssembly))
                {
                    if (assemblyCatalog.Parts.Any())
                    {
                        existingAssemblies.Add(candidateAssembly);
                        AssemblySource.Instance.Add(candidateAssembly);
                    }
                }
            }
        }

        protected virtual void BindServices(CompositionBatch batch)
        {
            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(Container);
            batch.AddExportedValue<AppBootstrapper>(this);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = Container.GetExports<object>(contract);

            if (exports.Any())
                return exports.First().Value;

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
            => Container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));

        protected override void BuildUp(object instance)
            => Container.SatisfyImportsOnce(instance);

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                base.OnStartup(sender, e);
                await DisplayRootViewForAsync<IMainWindow>();

                var mainWindow = (IMainWindow)GetInstance(typeof(IMainWindow), null);
                await mainWindow.ShellActivationTask;
                await mainWindow.Shell.InitializationTask;
            }
            catch (Exception exception)
            {
                HandleFatalStartupException(exception);
            }
        }

        /// <summary>
        /// Handles failures from root display, shell activation, or shell initialization.
        /// The default implementation reports the failure and terminates the application.
        /// </summary>
        protected virtual void HandleFatalStartupException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            try
            {
                MessageBox.Show(exception.ToString(), "SomaFrameQDWpf startup failure");
            }
            finally
            {
                Application.Current?.Shutdown(-1);
            }
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            return entryAssembly != null
                ? new[] { entryAssembly }
                : Enumerable.Empty<Assembly>();
        }

        /// <summary>
        /// Unifies version-skewed strong-named assembly requests (matching <see cref="AssemblyName.Name"/>
        /// and <c>PublicKeyToken</c> with an available version greater than or equal to the requested version)
        /// for dynamically discovered MEF plugins when host <c>.exe.config</c> redirects are absent or incomplete.
        /// </summary>
        protected static Assembly TryUnifyStrongNamedAssembly(string requestedAssemblyName, string probingDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(requestedAssemblyName))
            {
                return null;
            }

            AssemblyName requestedName;
            try
            {
                requestedName = new AssemblyName(requestedAssemblyName);
            }
            catch
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(requestedName.Name) ||
                requestedName.Version == null ||
                requestedName.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var requestedToken = requestedName.GetPublicKeyToken();
            if (requestedToken == null || requestedToken.Length == 0)
            {
                return null;
            }

            Assembly bestLoadedMatch = null;
            Version bestLoadedVersion = null;
            foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (loaded == null || loaded.IsDynamic)
                {
                    continue;
                }

                var candidateName = loaded.GetName();
                if (!string.Equals(candidateName.Name, requestedName.Name, StringComparison.OrdinalIgnoreCase) ||
                    candidateName.Version == null ||
                    candidateName.Version < requestedName.Version ||
                    !PublicKeyTokensEqual(candidateName.GetPublicKeyToken(), requestedToken))
                {
                    continue;
                }

                if (bestLoadedVersion == null || candidateName.Version > bestLoadedVersion)
                {
                    bestLoadedMatch = loaded;
                    bestLoadedVersion = candidateName.Version;
                }
            }

            if (bestLoadedMatch != null)
            {
                return bestLoadedMatch;
            }

            var baseDirectory = !string.IsNullOrWhiteSpace(probingDirectory)
                ? probingDirectory
                : AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrWhiteSpace(baseDirectory) || !Directory.Exists(baseDirectory))
            {
                return null;
            }

            var candidateFilePath = Path.GetFullPath(Path.Combine(baseDirectory, requestedName.Name + ".dll"));
            if (!File.Exists(candidateFilePath))
            {
                return null;
            }

            var resolvingSet = _resolvingAssemblyPaths ?? (_resolvingAssemblyPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            if (!resolvingSet.Add(candidateFilePath))
            {
                return null;
            }

            try
            {
                var diskAssemblyName = AssemblyName.GetAssemblyName(candidateFilePath);
                if (!string.Equals(diskAssemblyName.Name, requestedName.Name, StringComparison.OrdinalIgnoreCase) ||
                    diskAssemblyName.Version == null ||
                    diskAssemblyName.Version < requestedName.Version ||
                    !PublicKeyTokensEqual(diskAssemblyName.GetPublicKeyToken(), requestedToken))
                {
                    return null;
                }

                return Assembly.LoadFrom(candidateFilePath);
            }
            catch
            {
                return null;
            }
            finally
            {
                resolvingSet.Remove(candidateFilePath);
            }
        }

        private static void RegisterAssemblyResolveHandler()
        {
            if (Interlocked.CompareExchange(ref _assemblyResolveRegistered, 1, 0) == 0)
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            }
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
            => TryUnifyStrongNamedAssembly(args?.Name);

        private static bool PublicKeyTokensEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetEntryAssemblyFolderName(Assembly entryAssembly)
        {
            var name = entryAssembly?.GetName().Name;
            return string.IsNullOrWhiteSpace(name) ? "SomaFrameQD" : name;
        }
    }
}
