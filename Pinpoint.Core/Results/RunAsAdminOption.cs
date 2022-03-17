using System.ComponentModel;
using System.Diagnostics;
using FontAwesome5;

namespace Pinpoint.Core.Results
{
    public class RunAsAdminOption : AbstractFontAwesomeQueryResult
    {
        private readonly string _filePath;

        public RunAsAdminOption(string filePath) : base("Run as administrator", filePath)
        {
            _filePath = filePath;
        }

        public override EFontAwesomeIcon FontAwesomeIcon { get; } = EFontAwesomeIcon.Solid_UserLock;

        public override void OnSelect()
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = _filePath, 
                    UseShellExecute = true, 
                    Verb = "runas"
                }
            };
            try
            {
                proc.Start();
            }
            catch (Win32Exception)
            {
                // In case user decides against launching as admin
            }
        }
    }
}