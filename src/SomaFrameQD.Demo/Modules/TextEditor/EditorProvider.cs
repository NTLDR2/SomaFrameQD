using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using SomaFrameQD.Demo.Modules.TextEditor.ViewModels;
using SomaFrameQD.Framework;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Demo.Properties;

namespace SomaFrameQD.Demo.Modules.TextEditor
{
    [Export(typeof(IEditorProvider))]
    public class EditorProvider : IEditorProvider
    {
        private static readonly List<string> _extensions = new List<string>
        {
            ".txt",
            ".cmd"
        };

        public IEnumerable<EditorFileType> FileTypes
        {
            get { yield return new EditorFileType(Resources.EditorProviderTextFile, ".txt"); }
        }

        public bool CanCreateNew => true;

        public bool Handles(string path)
        {
            var extension = Path.GetExtension(path);
            return _extensions.Contains(extension);
        }

        public IDocument Create() => new EditorViewModel();

        public async Task New(IDocument document, string name) => await ((EditorViewModel)document).New(name);

        public async Task Open(IDocument document, string path) => await ((EditorViewModel)document).Load(path);
    }
}
