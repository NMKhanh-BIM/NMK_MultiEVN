using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NMKApp.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter?.ToString() == "Invert";
        bool boolValue = value is bool b && b;
        if (invert) boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}

public class TaskStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int status)
        {
            return status switch
            {
                0 => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)), // New - Green
                1 => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)), // Accepted - Orange
                2 => new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0)), // Started - Purple
                3 => new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)), // Completed - Blue
                4 => new SolidColorBrush(Color.FromRgb(0x60, 0x7D, 0x8B)), // Checked - Gray
                5 => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)), // Rejected - Red
                _ => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class TaskStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int status)
        {
            return status switch
            {
                0 => "New",
                1 => "Accepted",
                2 => "Start",
                3 => "Completed",
                4 => "Checked",
                5 => "Rejected",
                6 => "Cancelled",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class LeaveStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int status)
        {
            return status switch
            {
                0 => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)), // Pending - Orange
                1 => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)), // Approved - Green
                2 => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)), // Rejected - Red
                3 => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)), // Cancelled - Gray
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class LeaveStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int status)
        {
            return status switch
            {
                0 => "Pending",
                1 => "Approved",
                2 => "Rejected",
                3 => "Cancelled",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class LeaveTypeToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int leaveType)
        {
            return leaveType switch
            {
                1 => "Annual Leave",
                2 => "Sick Leave",
                3 => "Personal Leave",
                4 => "Unpaid Leave",
                5 => "Other",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class DateFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string format = parameter?.ToString() ?? "dd/MM/yyyy HH:mm";
        if (value is DateTimeOffset dto)
            return dto.LocalDateTime.ToString(format);
        if (value is DateTime dt)
            return dt.ToString(format);
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter?.ToString() == "Invert";
        bool isNull = value == null || (value is string s && string.IsNullOrEmpty(s));
        if (invert) isNull = !isNull;
        return isNull ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class EqualityToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString() ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToBrushConverter : IValueConverter
{
    public Brush? TrueBrush { get; set; }
    public Brush? FalseBrush { get; set; }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? TrueBrush : FalseBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
