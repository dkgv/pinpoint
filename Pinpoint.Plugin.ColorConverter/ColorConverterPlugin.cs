using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.ColorConverter
{
    public class ColorConverterPlugin : IPlugin
    {
        private const string RgbPattern =
            @"^rgb\(([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5]),( ?)([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5]),( ?)([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])\)$";
        private const string HexPattern = @"^(#)([0-9A-Fa-f]{8}|[0-9A-Fa-f]{6})$";
        private static readonly Regex Pattern = new Regex($@"({HexPattern})|({RgbPattern})");

        public PluginMeta Meta { get; set; } = new PluginMeta("Color Converter", PluginPriority.Highest);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public void Unload()
        {
        }

        public async Task<bool> Activate(Query query)
        {
            return Pattern.IsMatch(query.RawQuery);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var content = query.RawQuery;
            if (query.RawQuery.Contains("rgb"))
            {
                content = ConvertHexColor(query.RawQuery);
            }
            
            yield return new ColorConversionResult(ConvertHexColor(query.RawQuery), content);
        }

        private static string ConvertHexColor(string color)
        {
            if (color.Contains("#"))
            {
                // Color is hex
                var redLevel = Convert.ToInt32(color.Substring(1, 2), 16);
                var greenLevel = Convert.ToInt32(color.Substring(3, 2), 16);
                var blueLevel = Convert.ToInt32(color.Substring(5, 2), 16);
                return $"rgb({redLevel}, {greenLevel}, {blueLevel})";
            }
            
            // Color is RGB
            var modifiedColor = color.Replace("rgb(", "").Replace(")", "");
            var colorLevels = modifiedColor.Split(",");
            var red = int.Parse(colorLevels[0].Trim()).ToString("X2");
            var green = int.Parse(colorLevels[1].Trim()).ToString("X2");
            var blue = int.Parse(colorLevels[2].Trim()).ToString("X2");
            return $"#{red}{green}{blue}";
        }
    }
}