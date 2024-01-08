using System;
using System.Drawing;

namespace Pinpoint.Plugin.Windower;

public record WindowModel(string Title, string ProcessName, IntPtr Handle)
{
    private Bitmap _icon;

    public Bitmap Icon
    {
        get
        {
            _icon ??= WindowManager.GetIcon(Handle);
            return _icon;
        }
    }

    public bool QueryMatch(string query)
    {
        return Title.Contains(query, StringComparison.OrdinalIgnoreCase) || ProcessName.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}