using FontAwesome5;

namespace Pinpoint.Plugin.MetricConverter
{
    public class ConversionResult : IQueryResult
    {
        public ConversionResult(string result)
        {
            Title = "= " + result;
        }

        public string Title { get; }
        
        public string Subtitle { get; }
        
        public object Instance { get; }

        public EFontAwesomeIcon Icon => EFontAwesomeIcon.Solid_Ruler;

        public void OnSelect()
        {
        }
    }
}