﻿using System.Diagnostics;
using System.IO;

namespace Pinpoint.Plugin.AppSearch.Models;

public class StandardApp : IApp
{
    public string Name { get; set; }

    public string FilePath { get; set; }

    public string IconLocation { get; set; }

    public void Open()
    {
        Process.Start("explorer.exe", "\"" + FilePath + "\"");
    }

    public void OpenDirectory()
    {
        Process.Start("explorer.exe", Path.GetDirectoryName(FilePath));
    }
}