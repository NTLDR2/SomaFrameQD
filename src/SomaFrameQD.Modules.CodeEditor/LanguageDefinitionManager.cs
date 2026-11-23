using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace SomaFrameQD.Modules.CodeEditor
{
    [Export(typeof(LanguageDefinitionManager))]
    public class LanguageDefinitionManager
    {
        private List<ILanguageDefinition> _languageDefinitions;

        public IEnumerable<ILanguageDefinition> LanguageDefinitions
        {
            get
            {
                if (_languageDefinitions == null)
                {
                    _languageDefinitions = Initialize();
                }

                return _languageDefinitions;
            }
        }

        public ILanguageDefinition GetDefinitionByExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return null;

            return LanguageDefinitions.FirstOrDefault(
                l => l.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
        }

        private List<ILanguageDefinition> Initialize()
        {
            // Create built in language definitions
            var languageDefinitions = new List<ILanguageDefinition>
            {
                new DefaultLanguageDefinition("C#", new[] { ".cs" }),
                new DefaultLanguageDefinition("JavaScript", new[] { ".js" }),
                new DefaultLanguageDefinition("HTML", new[] { ".htm", ".html" }),
                new DefaultLanguageDefinition(
                    "ASP/XHTML",
                    new[] { ".asp", ".aspx", ".asax", ".asmx", ".ascx", ".master" }),
                new DefaultLanguageDefinition("Boo", new[] { ".boo" }),
                new DefaultLanguageDefinition("Coco", new[] { ".atg" }),
                new DefaultLanguageDefinition("CSS", new[] { ".css" }),
                new DefaultLanguageDefinition("C++", new[] { ".c", ".h", ".cc", ".cpp", ".hpp" }),
                new DefaultLanguageDefinition("Java", new[] { ".java" }),
                new DefaultLanguageDefinition("Patch", new[] { ".patch", ".diff" }),
                new DefaultLanguageDefinition("PowerShell", new[] { ".ps1", ".psm1", ".psd1" }),
                new DefaultLanguageDefinition("PHP", new[] { ".php" }),
                new DefaultLanguageDefinition("TeX", new[] { ".tex" }),
                new DefaultLanguageDefinition("VBNET", new[] { ".vb" }),
                new DefaultLanguageDefinition(
                    "XML",
                    (".xml;.xsl;.xslt;.xsd;.manifest;.config;.addin;" +
                     ".xshd;.wxs;.wxi;.wxl;.proj;.csproj;.vbproj;.ilproj;" +
                     ".booproj;.build;.xfrm;.targets;.xaml;.xpt;" +
                     ".xft;.map;.wsdl;.disco;.ps1xml;.nuspec").Split(';')),
                new DefaultLanguageDefinition("MarkDown", new[] { ".md" }),
            };

            // Add imported definitions
            foreach (ILanguageDefinition importedLanguage in IoC.GetAll<ILanguageDefinition>())
            {
                ILanguageDefinition defaultLanguage =
                    languageDefinitions.FirstOrDefault(
                        l => string.Equals(l.Name, importedLanguage.Name, StringComparison.InvariantCultureIgnoreCase));

                if (defaultLanguage != null)
                {
                    languageDefinitions.Remove(defaultLanguage);
                }

                languageDefinitions.Add(importedLanguage);
            }

            // Probe BaseDirectory\SyntaxHighlighting first so Start Menu shortcuts, taskbar pins, and Explorer
            // file associations find bundled .xshd grammars reliably across Windows 7-11, with fallback to
            // CurrentDirectory\SyntaxHighlighting when distinct.
            var probingDirectories = GetSyntaxHighlightingDirectories();
            if (probingDirectories.Count == 0)
                return languageDefinitions;

            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            Dictionary<string, ILanguageDefinition> definitions = languageDefinitions.ToDictionary(
                l =>
                {
                    char[] nameChars = l.Name.ToCharArray();
                    nameChars = nameChars.Except(invalidFileNameChars).ToArray();
                    return new string(nameChars);
                },
                l => l,
                StringComparer.OrdinalIgnoreCase);

            var xmlSettings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit
            };

            foreach (string directory in probingDirectories)
            {
                foreach (string highlightingFile in Directory.EnumerateFiles(directory, "*.xshd"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(highlightingFile);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        continue;
                    }

                    if (definitions.TryGetValue(fileName, out ILanguageDefinition definition))
                    {
                        definition.CustomSyntaxHighlightingFileName = highlightingFile;
                    }
                    else
                    {
                        try
                        {
                            XshdSyntaxDefinition syntaxDefinition;
                            using (var reader = XmlReader.Create(highlightingFile, xmlSettings))
                            {
                                syntaxDefinition = HighlightingLoader.LoadXshd(reader);
                            }

                            var customDefinition = new DefaultLanguageDefinition(syntaxDefinition);
                            languageDefinitions.Add(customDefinition);
                            definitions[fileName] = customDefinition;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }

            return languageDefinitions;
        }

        private static List<string> GetSyntaxHighlightingDirectories()
        {
            var result = new List<string>(2);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrWhiteSpace(baseDir))
            {
                var baseSyntaxDir = Path.GetFullPath(Path.Combine(baseDir, "SyntaxHighlighting"));
                if (Directory.Exists(baseSyntaxDir) && seen.Add(baseSyntaxDir))
                {
                    result.Add(baseSyntaxDir);
                }
            }

            try
            {
                var currentSyntaxDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "SyntaxHighlighting"));
                if (Directory.Exists(currentSyntaxDir) && seen.Add(currentSyntaxDir))
                {
                    result.Add(currentSyntaxDir);
                }
            }
            catch
            {
            }

            return result;
        }
    }
}
