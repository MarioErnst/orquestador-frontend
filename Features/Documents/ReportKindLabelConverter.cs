using System.Globalization;
using OrquestadorFrontend.Data.Models;

namespace OrquestadorFrontend.Features.Documents;

// Renders a ReportKind as a Spanish label for the documents list pill.
public sealed class ReportKindLabelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is ReportKind kind ? DocumentsListViewModel.KindLabel(kind) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
