using System;

namespace Pinpoint.Plugin.Everything.API
{
    public enum RequestFlag
    {
        FileName = 0x00000001,
        Path = 0x00000002,
        FullPathAndFileName = 0x00000004,
        Extension = 0x00000008,
        Size = 0x00000010,
        DateCreated = 0x00000020,
        DateModified = 0x00000040,
        DateAccessed = 0x00000080,
        Attributes = 0x00000100,
        FileListFileName = 0x00000200,
        RunCount = 0x00000400,
        DateRun = 0x00000800,
        DateRecentlyChanged = 0x00001000,
        HighlightedFileName = 0x00002000,
        HighlightedPath = 0x00004000,
        HighlightedFullPathAndFileName = 0x00008000
    }
}
