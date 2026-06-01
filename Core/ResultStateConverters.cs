using System.Globalization;

namespace OrquestadorFrontend.Core;

// Bind State of type ResultState<T> directly to IsVisible on the matching
// shared control. The converters check the open-generic record type so the
// same converter works for any payload T.

public sealed class IsLoadingConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => IsOfGenericKind(value, typeof(Loading<>));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    internal static bool IsOfGenericKind(object? value, Type openGeneric)
        => value is not null
           && value.GetType().IsGenericType
           && value.GetType().GetGenericTypeDefinition() == openGeneric;
}

public sealed class IsDataConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => IsLoadingConverter.IsOfGenericKind(value, typeof(Data<>));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class IsEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => IsLoadingConverter.IsOfGenericKind(value, typeof(Empty<>));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class IsFailureConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => IsLoadingConverter.IsOfGenericKind(value, typeof(Failure<>));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Extracts the inner value from Data<T> so an ItemsSource can bind directly
// to State without an intermediate property on the ViewModel.
public sealed class DataValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!IsLoadingConverter.IsOfGenericKind(value, typeof(Data<>)))
            return null;

        return value!.GetType().GetProperty("Value")?.GetValue(value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// Extracts the human message from Failure<T> for the shared ErrorState control.
public sealed class FailureMessageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!IsLoadingConverter.IsOfGenericKind(value, typeof(Failure<>)))
            return null;

        return value!.GetType().GetProperty("Message")?.GetValue(value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
