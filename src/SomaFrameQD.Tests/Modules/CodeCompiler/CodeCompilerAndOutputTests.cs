using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AvalonDock;
using AvalonDock.Serializer.Xml;
using AvalonDock.Themes;
using Caliburn.Micro;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SomaFrameQD.Framework.Commands;
using SomaFrameQD.Framework.ToolBars;
using SomaFrameQD.Modules.CodeCompiler;
using SomaFrameQD.Modules.ErrorList;
using SomaFrameQD.Modules.ErrorList.ViewModels;
using SomaFrameQD.Modules.Output.ViewModels;
using SomaFrameQD.Modules.Output.Views;
using SomaFrameQD.Modules.ToolBars;
using SomaFrameQD.Tests.TestInfrastructure;

namespace SomaFrameQD.Tests.Modules.CodeCompiler
{
    [STATestClass]
    [DoNotParallelize]
    public class CodeCompilerAndOutputTests
    {
        [STATestMethod]
        public void CodeCompiler_CoexistsWithAvalonDockVs2013ThemeAndSystemTextJson_In64BitNet48Process()
        {
            using (CreateErrorListIoCScope())
            {
                // 1. Exercise AvalonDock 5.0.1 + System.Text.Json 10.0.12 (Unsafe 6.x + System.Memory 4.x) in x64 net48.
                var dockingManager = new DockingManager();
                var vs2013Dark = new Vs2013DarkTheme().ThemeResourceDictionary;
                Assert.IsNotNull(vs2013Dark);

                var serializer = new XmlLayoutSerializer(dockingManager);
                using (var layoutStream = new MemoryStream())
                {
                    serializer.Serialize(layoutStream);
                    Assert.IsTrue(layoutStream.Length > 0);
                }

                string jsonPayload = JsonSerializer.Serialize(new { Framework = "net48", PointerBytes = IntPtr.Size });
                StringAssert.Contains(jsonPayload, "\"PointerBytes\":8");

                // 2. Exercise Roslyn 3.4.0 (Microsoft.CodeAnalysis.CSharp) in the exact same 64-bit AppDomain.
                var output = new OutputViewModel();
                var outputProbe = new CapturingOutputView();
                ((IViewAware)output).AttachView(outputProbe);

                var errorList = new ErrorListViewModel();
                var compiler = new SomaFrameQD.Modules.CodeCompiler.CodeCompiler(
                    editorProvider: null,
                    output: output,
                    errorList: errorList);

                var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System;
namespace SomaFrameQD.DynamicProbe
{
    public static class ProbeCalculator
    {
        public static int ComputePointerAwareValue(int seed)
        {
            return seed + (IntPtr.Size * 10);
        }
    }
}");

                var references = new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                };

                Assembly compiledAssembly = compiler.Compile(
                    new[] { syntaxTree },
                    references,
                    "SomaFrameQD.DynamicProbe_" + Guid.NewGuid().ToString("N"));

                Assert.IsNotNull(compiledAssembly, "Expected in-memory Roslyn compilation to succeed in [net48|x64].");
                Assert.AreEqual(0, errorList.Items.Count);
                StringAssert.Contains(outputProbe.LastText, "------ Compile finished");

                var probeType = compiledAssembly.GetType("SomaFrameQD.DynamicProbe.ProbeCalculator", throwOnError: true);
                var method = probeType.GetMethod("ComputePointerAwareValue", BindingFlags.Public | BindingFlags.Static);
                Assert.IsNotNull(method);

                int result = (int)method.Invoke(null, new object[] { 22 });
                Assert.AreEqual(102, result);
            }
        }

        [STATestMethod]
        public void CodeCompiler_HiddenAndMetadataDiagnostics_ReportCleanlyWithoutThrowing()
        {
            using (CreateErrorListIoCScope())
            {
                var output = new OutputViewModel();
                var errorList = new ErrorListViewModel();
                var compiler = new SomaFrameQD.Modules.CodeCompiler.CodeCompiler(
                    editorProvider: null,
                    output: output,
                    errorList: errorList);

                // Unnecessary 'using System.Collections;' emits hidden CS8019 alongside error CS0103 in a pathless syntax tree.
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
using System;
using System.Collections;
public class BrokenSample
{
    public int Run() => UndeclaredIdentifier;
}");

                var references = new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                };

                var compiled = compiler.Compile(
                    new[] { syntaxTree },
                    references,
                    "SomaFrameQD.BrokenProbe_" + Guid.NewGuid().ToString("N"));

                Assert.IsNull(compiled);
                Assert.IsTrue(errorList.Items.Count >= 1, "Expected compiler error to be recorded in ErrorList.");
                Assert.IsTrue(errorList.Items.Any(i => i.ItemType == ErrorListItemType.Error && i.Description.Contains("UndeclaredIdentifier")));
                Assert.IsNull(errorList.Items.First().File, "Pathless syntax tree diagnostics should set File = null.");

                // Corrupted metadata reference image emits a Location.None metadata diagnostic (CS0009).
                var corruptMetadataRef = MetadataReference.CreateFromImage(new byte[] { 0x4D, 0x5A, 0x00, 0x01 });
                var validTree = CSharpSyntaxTree.ParseText("public class EmptyClass { }");
                var failedMetadataCompile = compiler.Compile(
                    new[] { validTree },
                    new MetadataReference[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location), corruptMetadataRef },
                    "SomaFrameQD.CorruptRefProbe_" + Guid.NewGuid().ToString("N"));

                Assert.IsNull(failedMetadataCompile);
                Assert.IsTrue(errorList.Items.Any(i => i.ItemType == ErrorListItemType.Error), "Expected non-source metadata diagnostic in ErrorList.");
            }
        }

        [STATestMethod]
        public void OutputViewModel_ConcurrentMultiThreadAppends_PreserveCompleteTextWithoutCorruption()
        {
            var output = new OutputViewModel();
            var realView = new OutputView();
            ((IViewAware)output).AttachView(realView);

            const int workerCount = 8;
            const int linesPerWorker = 250;

            var tasks = Enumerable.Range(0, workerCount)
                .Select(workerId => Task.Run(() =>
                {
                    for (int lineIndex = 0; lineIndex < linesPerWorker; lineIndex++)
                    {
                        output.Writer.WriteLine($"W{workerId:D2}-L{lineIndex:D3}");
                    }
                }))
                .ToArray();

            Task.WaitAll(tasks);
            PumpDispatcherBackground();

            var textBox = (TextBox)realView.FindName("outputText");
            Assert.IsNotNull(textBox);

            var renderedText = textBox.Text;
            for (int workerId = 0; workerId < workerCount; workerId++)
            {
                StringAssert.Contains(renderedText, $"W{workerId:D2}-L000");
                StringAssert.Contains(renderedText, $"W{workerId:D2}-L{linesPerWorker - 1:D3}");
            }

            int nonEmptyLineCount = renderedText
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Length;
            Assert.AreEqual(workerCount * linesPerWorker, nonEmptyLineCount);
        }

        [STATestMethod]
        public void ErrorListViewModel_FilteredItems_ShortCircuitsWhenUnfilteredAndFiltersCorrectlyWhenToggled()
        {
            using (CreateErrorListIoCScope())
            {
                var errorList = new ErrorListViewModel();
                errorList.AddItem(ErrorListItemType.Error, "E1");
                errorList.AddItem(ErrorListItemType.Warning, "W1");
                errorList.AddItem(ErrorListItemType.Message, "M1");

                // Default state (all three true) returns the underlying collection directly with zero LINQ allocations.
                Assert.AreSame(errorList.Items, errorList.FilteredItems);

                errorList.ShowWarnings = false;
                CollectionAssert.AreEqual(
                    new[] { "E1", "M1" },
                    errorList.FilteredItems.Select(x => x.Description).ToArray());

                errorList.ShowErrors = false;
                errorList.ShowMessages = false;
                Assert.AreEqual(0, errorList.FilteredItems.Count());
            }
        }

        private static void PumpDispatcherBackground()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new DispatcherOperationCallback(_ =>
                {
                    frame.Continue = false;
                    return null;
                }),
                null);
            Dispatcher.PushFrame(frame);
        }

        private static IoCOverrideScope CreateErrorListIoCScope()
        {
            var commandService = new StubCommandService();
            var gestureService = new StubCommandKeyGestureService();
            var toolBarBuilder = new StubToolBarBuilder();

            return new IoCOverrideScope(
                getInstance: (serviceType, key) =>
                {
                    if (serviceType == typeof(ICommandService))
                        return commandService;
                    if (serviceType == typeof(ICommandKeyGestureService))
                        return gestureService;
                    if (serviceType == typeof(IToolBarBuilder))
                        return toolBarBuilder;
                    throw new InvalidOperationException($"Unexpected IoC service request: {serviceType}");
                },
                getAllInstances: _ => Enumerable.Empty<object>(),
                buildUp: _ => { });
        }

        private sealed class CapturingOutputView : IOutputView
        {
            public string LastText { get; private set; } = string.Empty;

            public void Clear() => LastText = string.Empty;

            public void ScrollToEnd()
            {
            }

            public void AppendText(string text) => LastText += text;

            public void SetText(string text) => LastText = text;
        }

        private sealed class StubCommandService : ICommandService
        {
            private readonly Dictionary<Type, CommandDefinitionBase> _definitions = new Dictionary<Type, CommandDefinitionBase>();
            private readonly Dictionary<CommandDefinitionBase, Command> _commands = new Dictionary<CommandDefinitionBase, Command>();

            public CommandDefinitionBase GetCommandDefinition(Type commandDefinitionType)
            {
                if (!_definitions.TryGetValue(commandDefinitionType, out var definition))
                {
                    definition = (CommandDefinitionBase)Activator.CreateInstance(commandDefinitionType);
                    _definitions[commandDefinitionType] = definition;
                }

                return definition;
            }

            public Command GetCommand(CommandDefinitionBase commandDefinition)
            {
                if (!_commands.TryGetValue(commandDefinition, out var command))
                {
                    command = new Command(commandDefinition);
                    _commands[commandDefinition] = command;
                }

                return command;
            }

            public TargetableCommand GetTargetableCommand(Command command)
                => new TargetableCommand(command);
        }

        private sealed class StubCommandKeyGestureService : ICommandKeyGestureService
        {
            public void BindKeyGestures(UIElement uiElement)
            {
            }

            public KeyGesture GetPrimaryKeyGesture(CommandDefinitionBase commandDefinition)
                => null;
        }

        private sealed class StubToolBarBuilder : IToolBarBuilder
        {
            public void BuildToolBars(IToolBars result)
            {
            }

            public void BuildToolBar(ToolBarDefinition toolBarDefinition, IToolBar result)
            {
            }
        }
    }
}
