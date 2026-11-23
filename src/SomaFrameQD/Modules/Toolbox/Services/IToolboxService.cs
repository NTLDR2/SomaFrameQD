using System;
using System.Collections.Generic;
using SomaFrameQD.Modules.Toolbox.Models;

namespace SomaFrameQD.Modules.Toolbox.Services
{
    public interface IToolboxService
    {
        IEnumerable<ToolboxItem> GetToolboxItems(Type documentType);
    }
}