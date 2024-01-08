using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Windower;

public class WindowerPlugin : AbstractPlugin
{
    public override PluginManifest Manifest { get; } = new("Window Plugin")
    {
        Description = "Easily search and toggle between open app windows."
    };

    public async override IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, CancellationToken ct)
    {
        var app = string.Join(" ", query.Parts[1..]);
        var windows = GetOpenWindows();

        foreach (var result in windows
            .Where(w => w.QueryMatch(app))
            .Select(w => new WindowResult(w)))
        {
            yield return result;
        }
    }

    public override Task<bool> ShouldActivate(Query query)
    {
        return Task.FromResult(query.Prefix() == "w" && query.Parts.Length > 1);
    }

    private static List<WindowModel> GetOpenWindows()
    {
        var shell = GetShellWindow();
        var windows = new List<WindowModel>();

        EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
        {
            if (hWnd == shell || !IsWindowVisible(hWnd))
            {
                return true;
            }

            var length = GetWindowTextLength(hWnd);
            if (length == 0)
            {
                return true;
            }

            var builder = new StringBuilder(length);
            GetWindowText(hWnd, builder, length + 1);

            GetWindowThreadProcessId(hWnd, out var pid);
            var process = Process.GetProcessById(pid);

            windows.Add(new WindowModel(builder.ToString(), process.ProcessName, hWnd));

            return true;
        }, IntPtr.Zero);

        return windows;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
}
