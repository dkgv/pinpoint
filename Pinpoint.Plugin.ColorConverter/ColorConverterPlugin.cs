using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.ColorConverter
{
    public class ColorConverterPlugin : AbstractPlugin
    {
        private const string RgbPattern =
            @"^rgb\(([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5]),( ?)([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5]),( ?)([01]?[0-9]?[0-9]|2[0-4][0-9]|25[0-5])\)$";
        private const string HexPattern = @"^(#)([0-9A-Fa-f]{8}|[0-9A-Fa-f]{6})$";
        private static readonly Regex Pattern = new($@"({HexPattern})|({RgbPattern})");

        public override PluginManifest Manifest { get; } = new("Color Converter", PluginPriority.High)
        {
            Description = "Convert colors from RGB <-> Hex.\n\nExamples: \"#FF5555\", \"rgb(255,100,50)\""
        };

        public override async Task<bool> ShouldActivate(Query query)
        {
            return Pattern.IsMatch(query.Raw);
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var content = query.Raw;
            if (query.Raw.Contains("rgb"))
            {
                content = ConvertHexColor(query.Raw);
            }

            yield return new ColorConversionResult(ConvertHexColor(query.Raw), content);
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