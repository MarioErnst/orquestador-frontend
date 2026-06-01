using System.Globalization;
using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Features.Whistleblower;

// Maps WhistleblowerStatus to a Spanish label for the case lists.
public sealed class WhistleblowerStatusLabelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is WhistleblowerStatus s ? Label(s) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public static string Label(WhistleblowerStatus s) => s switch
    {
        WhistleblowerStatus.Received => "Recibido",
        WhistleblowerStatus.InInvestigation => "En investigación",
        WhistleblowerStatus.Closed => "Cerrado",
        _ => string.Empty
    };
}

// Maps WhistleblowerStatus to a sober palette colour key resolved against
// Application Resources at runtime.
public sealed class WhistleblowerStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not WhistleblowerStatus s || Application.Current is null) return null;
        var key = s switch
        {
            WhistleblowerStatus.Received => "StatusWarn",
            WhistleblowerStatus.InInvestigation => "StatusAlert",
            WhistleblowerStatus.Closed => "StatusOk",
            _ => "OnSurfaceVariant"
        };
        return Application.Current.Resources[key];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
