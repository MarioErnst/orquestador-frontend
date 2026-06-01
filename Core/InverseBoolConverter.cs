using System.Globalization;

namespace OrquestadorFrontend.Core;

// Inverts a bool. Used on IsVisible to hide a region when a boolean property
// is true (e.g. show the list only when IsLoading is false).
public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}
