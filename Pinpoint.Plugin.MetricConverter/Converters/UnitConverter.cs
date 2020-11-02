namespace Pinpoint.Plugin.MetricConverter.Converters
{
    public static class UnitConverter
    {
        private static double ConvertToCm(string fromUnit, double amount)
        {
            return fromUnit switch
            {
                "m" => amount * 100,
                "cm" => amount,
                "km" => amount * 100000,
                "mm" => amount / 10,
                "micrometer" => amount / 10000,
                "nm" => amount / 100000000,
                "mi" => amount * 160934.4,
                "yd" => amount * 91.44,
                "ft" => amount * 30.48,
                "in" => amount * 2.54,
                _ => amount
            };
        }
        
        public static double ConvertTo(string fromUnit, string toUnit, double amount)
        {
            var inCm = ConvertToCm(fromUnit, amount);

            return toUnit switch
            {
                "m" => inCm / 100,
                "cm" => inCm,
                "km" => inCm / 100000,
                "mm" => inCm * 10,
                "micrometer" => inCm * 10000,
                "nm" => inCm * 100000000,
                "mi" => inCm / 160934.4,
                "yd" => inCm / 91.44,
                "ft" => inCm / 30.48,
                "in" => inCm / 2.54,
                _ => inCm
            };
        }
    }
}