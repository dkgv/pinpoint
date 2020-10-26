using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Plugin.Everything.API;

namespace Pinpoint.Plugin.Everything
{
    public class EverythingPlugin : IPlugin
    {
        private static readonly Regex ImageRegex = new Regex(@"png|jpg|gif|psd|svg|raw|jpeg|bmp|tiff");
        private static readonly Regex VideoRegex = new Regex(@"mp4|avi|mkv|flv|webm|mov|wmv|mpg|m4v|mpeg|wmv");
        private static readonly Regex AudioRegex = new Regex(@"mp3|flac|wma|alac");
        private static readonly Regex ZipRegex = new Regex(@"zip|tar|rar|gx|7z|apk|dmg|tar\.(gz|lz|xz)|zz");
        private static readonly Regex WordRegex = new Regex(@"doc|docx");
        private static readonly Regex PowerPointRegex = new Regex(@"ppt|pptx");
        private static readonly Regex CodeRegex = new Regex(@"java|cs|py|cpp|cc|rs|php|js|css|html|rb|pl|h|c|m|swift|xaml");
        private static readonly Regex SpreadsheetRegex = new Regex(@"xls|xlsm|xlsx|numbers|ots|xlr");

        public PluginMeta Meta { get; set; } = new PluginMeta("Everything (File Search)", PluginPriority.Lowest);

        private IEverythingClient _everything;

        public void Load()
        {
            _everything = new EverythingClient(new DefaultSearchConfig());
        }

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return query.RawQuery.Length >= 3;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            await foreach (var result in _everything.SearchAsync(query.RawQuery, new CancellationToken()))
            {
                if (result == null)
                {
                    continue;
                }
                yield return new EverythingResult(result, BestFittingIcon(result));
            }
        }

        private EFontAwesomeIcon BestFittingIcon(QueryResultItem result)
        {
            return result.ResultType switch
            {
                ResultType.Directory => EFontAwesomeIcon.Regular_Folder,
                ResultType.File => BestFileIcon(result),
                ResultType.Unknown => EFontAwesomeIcon.Solid_Question,
                ResultType.Volume => EFontAwesomeIcon.Regular_Hdd,
                _ => EFontAwesomeIcon.Solid_Question
            };
        }

        private EFontAwesomeIcon BestFileIcon(QueryResultItem result)
        {
            var extension = Path.GetExtension(result.FullPath);

            if (string.IsNullOrEmpty(extension))
            {
                return EFontAwesomeIcon.Regular_File;
            }

            extension = extension.Substring(1);

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
