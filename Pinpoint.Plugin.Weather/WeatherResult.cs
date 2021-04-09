using System.Drawing;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.Weather.Models;

namespace Pinpoint.Plugin.Weather
{
    public class WeatherResult : AbstractQueryResult
    {
        public WeatherResult(WeatherDayModel model) : base(model.DayOfWeek + ": " + model.MinTempC + "-" + model.MaxTempC + "(C) | " + model.MinTempF + "-" + model.MaxTempF + "(F)", model.Condition.Text + " | Rain " + model.ChanceOfRain + "% | Snow " + model.ChanceOfSnow + "%")
        {
            Icon = WeatherIconRepository.Get(model.Condition.IconUrl);
        }

        public override Bitmap Icon { get; }

        public override void OnSelect()
        {
        }
    }
}
