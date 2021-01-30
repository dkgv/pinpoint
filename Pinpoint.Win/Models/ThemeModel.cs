using System.Windows.Media;

namespace Pinpoint.Win.Models
{
    internal class ThemeModel
    {
        public string Name { get; set; }

        public SolidColorBrush Background { get; set; }
        
        public SolidColorBrush TxtQueryForeground { get; set; }
        
        public SolidColorBrush TxtQueryCaretBrush { get; set; }
        
        public SolidColorBrush TxtResultSubtitleForeground { get; set; }

        public bool IsLight { get; set; }
        
        public static readonly ThemeModel DarkTheme = new ThemeModel
        {
            Name = "Dark",
            Background = ToBrush("#E5343434"),
            TxtQueryForeground = ToBrush("#FFD8D8D8"),
            TxtQueryCaretBrush = ToBrush("#FFD8D8D8"),
            TxtResultSubtitleForeground = ToBrush("#FF9E9E9E"),
            IsLight = false
        };
        
        public static readonly ThemeModel LightTheme = new ThemeModel
        {
            Name = "Light",
            Background = ToBrush("#EEEEEEF0"),
            TxtQueryForeground = ToBrush("#000000"),
            TxtQueryCaretBrush = ToBrush("#000000"),
            TxtResultSubtitleForeground = ToBrush("#000000"),
            IsLight = true
        };

        private static SolidColorBrush ToBrush(string value)
        {
            return (SolidColorBrush) new BrushConverter().ConvertFromString(value);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}