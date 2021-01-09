using System;
using System.Collections.Generic;

namespace Pinpoint.Core
{
    public interface IContextMenuPlugin : IPlugin
    {
        List<Tuple<string, string, Action>> MenuItems();
    }
}
