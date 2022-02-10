using System.Drawing;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppResult : AbstractQueryResult
    {
        private readonly string _filePath;
        private readonly IApp _app;
        private readonly string _query;

        public AppResult(IApp app, string query) : base(app.Name, app.FilePath)
        {
            _app = app;
            _filePath = app.FilePath;
            _query = query;

            Options.Add(new RunAsAdminOption(app.FilePath));
            Options.Add(new OpenLocationOption(app.FilePath));
        }

        public override Bitmap Icon => IconRepository.GetIcon(_filePath) ?? FontAwesomeBitmapRepository.Get(EFontAwesomeIcon.Solid_Rocket);
        
        public override void OnSelect()
        {
            AppSearchFrequency.Track(_query, _filePath);
            _app.Open();
        }

        public override bool OnPrimaryOptionSelect()
        {
            _app.OpenDirectory();
            return true;
        }
    }
}