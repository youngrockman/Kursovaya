using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Vosmerka.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public IBrush? TrueValue { get; set; }
        public IBrush? FalseValue { get; set; }

        
        // Флаги для преключения значений с true на false
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? TrueValue : FalseValue;
            }
            return FalseValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}