using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Highlighting;

namespace SomaFrameQD.Modules.CodeEditor
{
    public interface ILanguageDefinition
    {
        string Name { get; }
        IEnumerable<string> FileExtensions { get; set; }
        IHighlightingDefinition SyntaxHighlighting { get; }
        string CustomSyntaxHighlightingFileName { get; set; }
    }
}