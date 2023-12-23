using Avalonia.Data.Converters;
using Avalonia.Data;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Attestator.Converters;

public class StringComparerConverter : IValueConverter
{

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string pattern = parameter.ToString();
        bool invers = parameter.ToString().StartsWith('!');

        return value.ToString().Equals(invers ? pattern.Substring(1) : pattern) ? !invers : invers;
    }

    public object ConvertBack(object? value, Type targetType,
                                object? parameter, CultureInfo culture)
    {
        return value;
    }
}