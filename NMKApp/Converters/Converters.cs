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
                0  => new SolidColorBrush(Color.FromRgb(0x00, 0x89, 0x7B)), // Complete      – Teal
                1  => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)), // NewDontSend   – Gray
                2  => new SolidColorBrush(Color.FromRgb(0x78, 0x90, 0x9C)), // EditDontSend  – Blue-Gray
                3  => new SolidColorBrush(Color.FromRgb(0x1E, 0x88, 0xE5)), // New           – Blue
                4  => new SolidColorBrush(Color.FromRgb(0xE6, 0x4A, 0x19)), // Checked       – Deep Orange
                5  => new SolidColorBrush(Color.FromRgb(0xFB, 0x8C, 0x00)), // ReChecked     – Orange
                6  => new SolidColorBrush(Color.FromRgb(0x8E, 0x24, 0xAA)), // Start         – Purple
                7  => new SolidColorBrush(Color.FromRgb(0xF5, 0x7F, 0x17)), // Accepted      – Amber
                10 => new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)), // Interrupted   – Red
                _  => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
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
                0  => "Complete",
                1  => "New",
                2  => "Edit",
                3  => "New",
                4  => "Checked",
                5  => "ReChecked",
                6  => "Start",
                7  => "Accepted",
                10 => "Interrupted",
                _  => "Unknown"
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

public class StringNotEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter?.ToString() == "Invert";
        bool hasValue = value is string s && !string.IsNullOrEmpty(s);
        if (invert) hasValue = !hasValue;
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class StringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorStr && !string.IsNullOrEmpty(colorStr))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorStr);
                return new SolidColorBrush(color);
            }
            catch { }
        }
        return new SolidColorBrush(Color.FromRgb(0x19, 0x76, 0xD2));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BindingProxy : System.Windows.Freezable
{
    protected override System.Windows.Freezable CreateInstanceCore() => new BindingProxy();

    public static readonly System.Windows.DependencyProperty DataProperty =
        System.Windows.DependencyProperty.Register(
            "Data", typeof(object), typeof(BindingProxy), new System.Windows.UIPropertyMetadata(null));

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }
}
