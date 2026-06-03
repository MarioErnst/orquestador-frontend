using System.Globalization;

namespace OrquestadorFrontend.Core;

// True when the bound value is not null. Used as an IsVisible source for
// containers that depend on a plain reference being populated (the home
// dashboard quick-access cards bind LatestReport and FeaturedBoard, which
// are nullable references rather than ResultState wrappers).
public sealed class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
