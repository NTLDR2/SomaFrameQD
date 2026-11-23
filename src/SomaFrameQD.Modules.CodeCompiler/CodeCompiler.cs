using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Caliburn.Micro;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using SomaFrameQD.Framework.Results;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Modules.ErrorList;
using SomaFrameQD.Modules.Output;

namespace SomaFrameQD.Modules.CodeCompiler
{
    /// <summary>
    /// Default MEF implementation of <see cref="ICodeCompiler"/> backed by Roslyn (<c>Microsoft.CodeAnalysis.CSharp</c> 3.4.0)
    /// for 64-bit .NET Framework 4.8 (<c>net48</c>) hosts across Windows 7 SP1 through Windows 11.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>How Roslyn PE Emission &amp; Assembly Loading Work on .NET Framework 4.8:</b>
    /// <see cref="Compile"/> builds a <see cref="CSharpCompilation"/> targeting <see cref="OutputKind.DynamicallyLinkedLibrary"/>,
    /// emits the PE image into a pre-sized in-memory <see cref="MemoryStream"/>, and loads it via <see cref="Assembly.Load(byte[])"/>
    /// into <see cref="AppDomain.CurrentDomain"/>. This preserves zero-marshaling, direct object-reference access between
    /// compiled scripts and live WPF/3D viewports.
    /// </para>
    /// <para>
    /// Because the .NET Framework 4.8 CLR retains dynamic assemblies loaded into the default <see cref="AppDomain"/> for
    /// the lifetime of the process, <see cref="GC.Collect()"/> cannot reclaim previously compiled assemblies and is
    /// intentionally omitted here so compilation never triggers a synchronous Generation-2 Stop-The-World pause on the WPF UI thread.
    /// Interactive callers (e.g., <c>HelixViewModel</c>) should debounce keystrokes and cache <see cref="MetadataReference"/> objects.
    /// </para>
    /// </remarks>
    [Export(typeof(ICodeCompiler))]
    public class CodeCompiler : ICodeCompiler
    {
        private const int InitialPeStreamCapacity = 16384;

        private readonly IEditorProvider _editorProvider;
        private readonly IOutput _output;
        private readonly IErrorList _errorList;

        [ImportingConstructor]
        public CodeCompiler(IEditorProvider editorProvider, IOutput output, IErrorList errorList)
        {
            _editorProvider = editorProvider;
            _output = output;
            _errorList = errorList;
        }

        /// <inheritdoc />
        public Assembly Compile(
            IEnumerable<SyntaxTree> syntaxTrees,
            IEnumerable<MetadataReference> references,
            string outputName)
        {
            _output.AppendLine("------ Compile started");

            var compilation = CSharpCompilation.Create(outputName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTrees);

            using (var ms = new MemoryStream(InitialPeStreamCapacity))
            {
                var result = compilation.Emit(ms);
                _errorList.Items.Clear();

                if (!result.Success)
                {
                    ProcessResult(result);
                    _output.AppendLine("------ Compile failed");
                    return null;
                }

                _output.AppendLine("------ Compile finished");
                return Assembly.Load(ms.ToArray());
            }
        }

        private void ProcessResult(EmitResult result)
        {
            foreach (var diagnostic in result.Diagnostics)
            {
                // Skip hidden analyzer/IDE diagnostics (such as CS8019 unnecessary using directives)
                // so they never clutter the Error List or fault severity mapping.
                if (diagnostic.Severity == DiagnosticSeverity.Hidden)
                {
                    continue;
                }

                var itemType = GetItemType(diagnostic.Severity);
                var description = diagnostic.GetMessage();

                if (diagnostic.Location != null && diagnostic.Location.IsInSource)
                {
                    var lineSpan = diagnostic.Location.GetLineSpan();
                    var filePath = string.IsNullOrWhiteSpace(lineSpan.Path) ? null : lineSpan.Path;
                    System.Action openAction = null;

                    if (filePath != null)
                    {
                        openAction = () =>
                        {
                            var openDocumentResult = new OpenDocumentResult(filePath);
                            IoC.BuildUp(openDocumentResult);
                            openDocumentResult.Execute(null);
                        };
                    }

                    _errorList.AddItem(
                        itemType,
                        description,
                        filePath,
                        lineSpan.StartLinePosition.Line,
                        lineSpan.StartLinePosition.Character,
                        openAction);
                }
                else
                {
                    // Assembly-level and metadata-reference diagnostics (e.g., CS0012 / CS1701) have Location.None;
                    // report them cleanly in ErrorList instead of throwing NotSupportedException.
                    _errorList.AddItem(itemType, description);
                }
            }
        }

        private static ErrorListItemType GetItemType(DiagnosticSeverity severity)
        {
            switch (severity)
            {
                case DiagnosticSeverity.Info:
                    return ErrorListItemType.Message;
                case DiagnosticSeverity.Warning:
                    return ErrorListItemType.Warning;
                case DiagnosticSeverity.Error:
                    return ErrorListItemType.Error;
                default:
                    return ErrorListItemType.Message;
            }
        }
    }
}
