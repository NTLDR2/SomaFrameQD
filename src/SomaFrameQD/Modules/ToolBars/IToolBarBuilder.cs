using SomaFrameQD.Framework.ToolBars;

namespace SomaFrameQD.Modules.ToolBars
{
    public interface IToolBarBuilder
    {
        void BuildToolBars(IToolBars result);
        void BuildToolBar(ToolBarDefinition toolBarDefinition, IToolBar result);
    }
}