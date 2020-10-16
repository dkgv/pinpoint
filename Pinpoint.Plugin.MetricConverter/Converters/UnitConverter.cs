namespace Pinpoint.Plugin.MetricConverter.Converters
{
    public class UnitConverter
    {
        public double Amount;
        public readonly string Unit;
        
        public UnitConverter(double amount, string unit)
        {
            Amount = amount; 
            Unit = unit;
        }

        private UnitConverter ConvertToCm()
        {
            return Unit switch
            {
                "m" => new UnitConverter(Amount * 100, "cm"),
                "cm" => this,
                "km" => new UnitConverter(Amount * 100000, "cm"),
                "mm" => new UnitConverter(Amount / 10, "cm"),
                "micrometer" => new UnitConverter(Amount / 10000, "cm"),
                "nm" => new UnitConverter(Amount / 100000000, "cm"),
                "mi" => new UnitConverter(Amount * 160934.4, "cm"),
                "yd" => new UnitConverter(Amount * 91.44, "cm"),
                "ft" => new UnitConverter(Amount * 30.48, "cm"),
                "in" => new UnitConverter(Amount * 2.54, "cm"),
                _ => this
            };
        }
        
        public UnitConverter ConvertTo(string toUnit)
        {
            var amountInCm = ConvertToCm().Amount;
            return toUnit switch
            {
                "m" => new UnitConverter(amountInCm / 100, "m"),
                "cm" => ConvertToCm(),
                "km" => new UnitConverter(amountInCm / 100000, "km"),
                "mm" => new UnitConverter(amountInCm * 10, "mm"),
                "micrometer" => new UnitConverter(amountInCm * 10000, "micrometer"),
                "nm" => new UnitConverter(amountInCm * 100000000, "nm"),
                "mi" => new UnitConverter(amountInCm / 160934.4, "mi"),
                "yd" => new UnitConverter(amountInCm / 91.44, "yd"),
                "ft" => new UnitConverter(amountInCm / 30.48, "ft"),
                "in" => new UnitConverter(amountInCm / 2.54, "in"),
                _ => this
            };
        }

        public override string ToString()
        {
            return Amount + " " + Unit;
        }
    }
}