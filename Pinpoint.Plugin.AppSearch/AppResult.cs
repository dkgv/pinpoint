using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppResult : AbstractQueryResult
    {
        private readonly string _filePath;
        private static readonly Dictionary<string, Bitmap> IconCache = new();
        private readonly string _query;

        public AppResult(string filePath, string query) : base(Path.GetFileName(filePath).Split(".")[0], filePath)
        {
            _filePath = filePath;
            _query = query;

            Options.Add(new RunAsAdminOption(filePath));
            Options.Add(new OpenLocationOption(filePath));
        }

        public override Bitmap Icon
        {
            get
            {
                if (!IconCache.ContainsKey(_filePath))
                {
                    IconCache[_filePath] = System.Drawing.Icon.ExtractAssociatedIcon(_filePath).ToBitmap();
                }

                return IconCache[_filePath];
            }
        }

        public override void OnSelect()
        {
            AppSearchFrequency.Track(_query, _filePath);
            Process.Start("explorer.exe", "\"" + _filePath + "\"");
        }

        public override bool OnPrimaryOptionSelect()
        {
            Process.Start("explorer.exe", Path.GetDirectoryName(_filePath));
            return true;
        }
    }
}