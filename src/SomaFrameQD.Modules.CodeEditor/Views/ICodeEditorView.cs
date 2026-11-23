using ICSharpCode.AvalonEdit;

namespace SomaFrameQD.Modules.CodeEditor.Views
{
    public interface ICodeEditorView
    {
        TextEditor TextEditor { get; }

        void ApplySettings();
    }
}
