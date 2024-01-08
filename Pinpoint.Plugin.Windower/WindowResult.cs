using System.Drawing;

namespace Pinpoint.Plugin.Windower;

public class WindowResult : AbstractQueryResult
{
    private readonly WindowModel _window;

    public WindowResult(WindowModel window)
    {
        Title = window.Title;
        Subtitle = window.ProcessName;
        _window = window;
    }

    public override Bitmap Icon => _window.Icon;

    public override void OnSelect()
    {
        WindowManager.Focus(_window.Handle);
    }
}