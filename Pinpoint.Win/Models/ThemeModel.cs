using System.Windows.Media;

namespace Pinpoint.Win.Models
{
    public class ThemeModel
    {
        public SolidColorBrush BorderBackground { get; set; }
        
        public SolidColorBrush TxtQueryForeground { get; set; }
        
        public SolidColorBrush TxtQueryCaretBrush { get; set; }
        
        public bool IsLight { get; set; }
        

        public static readonly ThemeModel DarkTheme = new ThemeModel
        {
            BorderBackground = (SolidColorBrush)new BrushConverter().ConvertFromString("#F2343434"),
            TxtQueryForeground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFD8D8D8"),
            TxtQueryCaretBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFD8D8D8"),
            IsLight = false
        };
        
        public static readonly ThemeModel LightTheme = new ThemeModel
        {
            BorderBackground = (SolidColorBrush)new BrushConverter().ConvertFromString("#EEEEEEF0"),
            TxtQueryForeground = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000"),
            TxtQueryCaretBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000"),
            IsLight = true
        };

        public override string ToString()
        {
            return this == LightTheme ? "light" : "dark";
        }
    }
}