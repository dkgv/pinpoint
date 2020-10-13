using System;
using System.Collections.Generic;

namespace Pinpoint.Plugin
{
    public interface IContextMenuPlugin : IPlugin
    {
        List<Tuple<string, string, Action>> MenuItems();
    }
}
