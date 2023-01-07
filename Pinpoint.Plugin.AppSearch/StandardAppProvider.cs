using System;
using System.Collections.Generic;

namespace Pinpoint.Plugin.AppSearch;

public class StandardAppProvider : IAppProvider
{
    private static readonly string[] Paths = {
        @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
        $@"C:\Users\{Environment.UserName}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs",
        $@"C:\Users\{Environment.UserName}\Desktop"
    };
    
    private readonly DirectoryAppProvider _directoryAppProvider;

    public StandardAppProvider()
    {
        _directoryAppProvider = new DirectoryAppProvider(Paths);
    }

    public IEnumerable<IApp> Provide() => _directoryAppProvider.Provide();
}