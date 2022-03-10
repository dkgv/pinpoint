using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Everything.API;

namespace Pinpoint.Plugin.Everything
{
    public class EverythingPlugin : IPlugin
    {
        private const string KeyIgnoreTempFolder = "Ignore temp folder items";
        private const string KeyIgnoreHiddenFolders = "Ignore hidden folder items";
        private const string KeyIgnoreWindows = "Ignore items in Windows folder";
        private const string Description = "Search for files on your computer via Everything by David Carpenter.";

        private static readonly Regex ImageRegex = new(@"png|jpg|gif|psd|svg|raw|jpeg|bmp|tiff");
        private static readonly Regex VideoRegex = new(@"mp4|avi|mkv|flv|webm|mov|wmv|mpg|m4v|mpeg|wmv");
        private static readonly Regex AudioRegex = new(@"mp3|flac|wma|alac");
        private static readonly Regex ZipRegex = new(@"zip|tar|rar|gx|7z|apk|dmg|tar\.(gz|lz|xz)|zz");
        private static readonly Regex WordRegex = new(@"doc|docx");
        private static readonly Regex PowerPointRegex = new(@"ppt|pptx");
        private static readonly Regex TextRegex = new(@"txt|md|rtf");
        private static readonly Regex CodeRegex = new(@"java|cs|py|cpp|cc|rs|php|js|css|html|rb|pl|h|c|m|swift|xaml");
        private static readonly Regex SpreadsheetRegex = new(@"xls|xlsm|xlsx|numbers|ots|xlr");
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        public PluginMeta Meta { get; set; } = new("Everything (File Search)", Description, PluginPriority.Lowest);

        public PluginStorage Storage { get; set; } = new();

        public bool IsLoaded { get; set; }

        private IEverythingClient _everything;

        public Task<bool> TryLoad()
        {
            _everything = new EverythingClient(new DefaultSearchConfig());

            if (Storage.UserSettings.Count != 3)
            {
                Storage.UserSettings.Put(KeyIgnoreTempFolder, true);
                Storage.UserSettings.Put(KeyIgnoreHiddenFolders, true);
                Storage.UserSettings.Put(KeyIgnoreWindows, true);
            }

            return Task.FromResult(IsLoaded = true);
        }

        public void Unload() => _everything.Dispose();

        public async Task<bool> Activate(Query query)
        {
            return query.RawQuery.Length >= 3 && query.ResultCount < 3 && !query.RawQuery.Any(ch => InvalidFileNameChars.Contains(ch));
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            await foreach (var result in _everything.SearchAsync(query.RawQuery, ct))
            {
                if (result == null)
                {
                    continue;
                }

                if (Storage.UserSettings.Bool(KeyIgnoreTempFolder) && IsInTempFolder(result.FullPath))
                {
                    continue;
                }

                if (Storage.UserSettings.Bool(KeyIgnoreWindows) && result.FullPath.StartsWith(@"C:\Windows"))
                {
                    continue;
                }

                if (Storage.UserSettings.Bool(KeyIgnoreHiddenFolders) && IsInHiddenFolder(result.FullPath))
                {
                    continue;
                }

                AssignIcon(result);

                yield return new EverythingResult(result);
            }
        }

        private void AssignIcon(QueryResultItem result)
        {
            Bitmap icon;
            if (File.Exists(result.FullPath))
            {
                icon = Icon.ExtractAssociatedIcon(result.FullPath).ToBitmap();
            }
            else
            {
                icon = FontAwesomeBitmapRepository.Get(MapResultTypeToIcon(result));
            }

            result.Icon = icon;
        }

        private bool IsInHiddenFolder(string path)
        {
            var parts = path.Split(Path.DirectorySeparatorChar);
            return parts.Any(part => part.StartsWith("."));
        }

        private bool IsInTempFolder(string path)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToLower();
            if (appData.Contains("roaming"))
            {
                appData = Path.GetFullPath(Path.Combine(appData, @"..\"));
            }

            return path.ToLower().Contains(appData);
        }

        private EFontAwesomeIcon MapResultTypeToIcon(QueryResultItem result)
        {
            return result.ResultType switch
            {
                ResultType.Directory => EFontAwesomeIcon.Regular_Folder,
                ResultType.File => MapFileTypeToIcon(result),
                ResultType.Unknown => EFontAwesomeIcon.Solid_Question,
                ResultType.Volume => EFontAwesomeIcon.Regular_Hdd,
                _ => EFontAwesomeIcon.Solid_Question
            };
        }

        private EFontAwesomeIcon MapFileTypeToIcon(QueryResultItem result)
        {
            var extension = Path.GetExtension(result.FullPath);

            if (string.IsNullOrEmpty(extension))
            {
                return EFontAwesomeIcon.Regular_File;
            }

            extension = extension[1..];

            if (ImageRegex.IsMatch(extension))
            {
                return EFontAwesomeIcon.Regular_FileImage;
            }

            if (VideoRegex.IsMatch(extension))
            {
                return EFontAwesomeIcon.Regular_FileVideo;
            }

            if (AudioRegex.IsMatch(extension))
            {
                return EFontAwesomeIcon.Regular_FileAudio;
            }

            if (extension.Equals("pdf"))
            {
                return EFontAwesomeIcon.Regular_FilePdf;
            }

            if (TextRegex.IsMatch(extension))
            {
                return EFontAwesomeIcon.Regular_FileAlt;
            }

            if (ZipRegex.IsMatch(extension))
            {
                return EFontAwesomeIcon.Regular_FileArchive;
            }

            if (WordRegex.IsMatch(extension))
            {
                return EFontAwesomeIcon.Regular_FileWord;
            }

            if (PowerPointRegex.IsMatch(extension))
            {
                return EFontAwesomeIcon.Regular_FilePowerpoint;
            }

            if (CodeRegex.IsMatch(extension))
            {
                return EFontAwesomeIcon.Regular_FileCode;
            }

            return EFontAwesomeIcon.Regular_File;
        }
    }
}
