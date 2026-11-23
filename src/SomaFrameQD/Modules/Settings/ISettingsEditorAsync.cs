using System.Threading.Tasks;

namespace SomaFrameQD.Modules.Settings
{
    public interface ISettingsEditorAsync
    {
        string SettingsPageName { get; }
        string SettingsPagePath { get; }

        Task ApplyChangesAsync();
    }
}
