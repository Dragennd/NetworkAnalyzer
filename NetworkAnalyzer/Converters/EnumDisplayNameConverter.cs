using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Avalonia.Data.Converters;

namespace NetworkAnalyzer.Converters;

internal class EnumDisplayNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue)
            return value;

        var member = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();

        return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? enumValue.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}