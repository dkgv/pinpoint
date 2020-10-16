using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Pinpoint.Plugin.Everything.API
{
    public static class EverythingDll
    {
        private const string Dll = "Everything64.dll";

        [DllImport(Dll, CharSet = CharSet.Unicode)]
        public static extern uint Everything_SetSearchW(string lpSearchString);

        [DllImport(Dll)]
        public static extern void Everything_SetMatchPath(bool bEnable);

        [DllImport(Dll)]
        public static extern void Everything_SetMatchCase(bool bEnable);

        [DllImport(Dll)]
        public static extern void Everything_SetMatchWholeWord(bool bEnable);

        [DllImport(Dll)]
        public static extern void Everything_SetRegex(bool bEnable);

        [DllImport(Dll)]
        public static extern void Everything_SetMax(uint dwMax);

        [DllImport(Dll)]
        public static extern void Everything_SetOffset(uint dwOffset);

        [DllImport(Dll)]
        public static extern uint Everything_GetRequestFlags();

        [DllImport(Dll)]
        public static extern bool Everything_GetMatchPath();

        [DllImport(Dll)]
        public static extern bool Everything_GetMatchCase();

        [DllImport(Dll)]
        public static extern bool Everything_GetMatchWholeWord();

        [DllImport(Dll)]
        public static extern bool Everything_GetRegex();

        [DllImport(Dll)]
        public static extern uint Everything_GetMax();

        [DllImport(Dll)]
        public static extern uint Everything_GetOffset();

        [DllImport(Dll, CharSet = CharSet.Unicode)]
        public static extern string Everything_GetSearchW();

        [DllImport(Dll)]
        public static extern StatusCode Everything_GetLastError();

        [DllImport(Dll, CharSet = CharSet.Unicode)]
        public static extern bool Everything_QueryW(bool bWait);

        [DllImport(Dll)]
        public static extern void Everything_SetSort(uint dwSortType);

        [DllImport(Dll)]
        public static extern uint Everything_GetSort();

        [DllImport(Dll)]
        public static extern uint Everything_GetNumFileResults();

        [DllImport(Dll)]
        public static extern uint Everything_GetNumFolderResults();

        [DllImport(Dll)]
        public static extern uint Everything_GetNumResults();

        [DllImport(Dll)]
        public static extern uint Everything_GetTotFileResults();

        [DllImport(Dll)]
        public static extern uint Everything_GetTotFolderResults();

        [DllImport(Dll)]
        public static extern uint Everything_GetTotResults();

        [DllImport(Dll)]
        public static extern bool Everything_IsVolumeResult(uint nIndex);

        [DllImport(Dll)]
        public static extern bool Everything_IsFolderResult(uint nIndex);

        [DllImport(Dll)]
        public static extern bool Everything_IsFileResult(uint nIndex);

        [DllImport(Dll, CharSet = CharSet.Unicode)]
        public static extern void Everything_GetResultFullPathNameW(uint nIndex, StringBuilder lpString, uint nMaxCount);
        
        // https://www.voidtools.com/forum/viewtopic.php?t=8169
        [DllImport(Dll, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileNameW(uint nIndex);
        
        [DllImport(Dll, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedPathW(uint nIndex);
        
        [DllImport(Dll, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFileNameW(uint nIndex);
        
        [DllImport(Dll, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFullPathAndFileNameW(uint nIndex);
        
        [DllImport(Dll)]
        public static extern void Everything_SetRequestFlags(RequestFlag flag);

        [DllImport(Dll)]
        public static extern void Everything_Reset();
    }
}
