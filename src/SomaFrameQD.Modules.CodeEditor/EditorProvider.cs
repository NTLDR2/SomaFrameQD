using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using SomaFrameQD.Framework;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Modules.CodeEditor.ViewModels;
using SomaFrameQD.Modules.CodeEditor.Properties;

namespace SomaFrameQD.Modules.CodeEditor
{
    [Export(typeof(IEditorProvider))]
    public class EditorProvider : IEditorProvider
    {
        private readonly LanguageDefinitionManager _languageDefinitionManager;

        [ImportingConstructor]
        public EditorProvider(LanguageDefinitionManager languageDefinitionManager)
        {
            _languageDefinitionManager = languageDefinitionManager;
        }

        public IEnumerable<EditorFileType> FileTypes => _languageDefinitionManager.LanguageDefinitions
            .Select(languageDefinition => new EditorFileType
            {
                Name = languageDefinition.Name + Resources.EditorProviderFileSuffix,
                FileExtension = languageDefinition.FileExtensions.First()
            });

        public bool CanCreateNew => true;

        public bool Handles(string path)
        {
            var extension = Path.GetExtension(path);
            return extension != null && _languageDefinitionManager.GetDefinitionByExtension(extension) != null;
        }

        public IDocument Create() => IoC.Get<CodeEditorViewModel>();

        public async Task New(IDocument document, string name) => await ((CodeEditorViewModel)document).New(name);

        public async Task Open(IDocument document, string path) => await ((CodeEditorViewModel)document).Load(path);
    }
}
