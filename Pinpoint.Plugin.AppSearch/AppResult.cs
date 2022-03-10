using System.Drawing;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.AppSearch
{
    public class AppResult : AbstractQueryResult
    {
        private readonly IApp _app;
        private readonly string _query;
        private readonly AppSearchPlugin _plugin;

        public AppResult(AppSearchPlugin plugin, IApp app, string query) : base(app.Name, app.FilePath)
        {
            _plugin = plugin;
            _app = app;
            _query = query;

            Options.Add(new RunAsAdminOption(app.FilePath));
            Options.Add(new OpenLocationOption(app.FilePath));
        }

        public override Bitmap Icon => IconRepository.GetIcon(_app.IconLocation) ?? FontAwesomeBitmapRepository.Get(EFontAwesomeIcon.Solid_Rocket);
        
        public override void OnSelect()
        {
            _plugin.AppSearchFrequency.Track(_query, _app.FilePath);
            (_plugin as IPlugin).Save();
            _app.Open();
        }

        public override bool OnPrimaryOptionSelect()
        {
            _app.OpenDirectory();
            return true;
        }
    }
}