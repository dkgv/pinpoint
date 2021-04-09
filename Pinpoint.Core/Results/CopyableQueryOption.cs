using System.Diagnostics;
using FontAwesome5;

namespace Pinpoint.Core.Results
{
    public class CopyabableQueryOption : AbstractFontAwesomeQueryResult
    {
        public readonly string Content;
        
        public CopyabableQueryOption(string title, string content) : base(title, "Copy content to clipboard")
        {
            Content = content;
        }

        public CopyabableQueryOption(string content) : base(content, "Copy content to clipboard")
        {
            Content = content;
        }
        
        public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Regular_Copy;

        public override void OnSelect()
        {
            var clipboardExecutable = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardInput = true, FileName = @"clip",
                }
            };
            clipboardExecutable.Start();
            clipboardExecutable.StandardInput.Write(Content);
            clipboardExecutable.StandardInput.Close();
        }
    }
}
