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
        private static readonly Dictionary<string, Bitmap> IconCache = new Dictionary<string, Bitmap>();

        public AppResult(string filePath) : base(Path.GetFileName(filePath).Split(".")[0], filePath)
        {
            _filePath = filePath;

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
            AppSearchFrequency.Track(AppSearchPlugin.LastQuery, _filePath);
            Process.Start("explorer.exe", "\"" + _filePath + "\"");
        }
    }
}