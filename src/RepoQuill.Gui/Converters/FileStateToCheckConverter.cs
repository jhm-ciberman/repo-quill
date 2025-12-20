using System;
using System.Globalization;
using Avalonia.Data.Converters;
using RepoQuill.Gui.Models;

namespace RepoQuill.Gui.Converters;

/// <summary>
/// Converts FileNodeState to nullable bool for CheckBox.IsChecked binding.
/// </summary>
public class FileStateToCheckConverter : IValueConverter
{
    public static FileStateToCheckConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FileNodeState state)
        {
            return null;
        }

        return state switch
        {
            FileNodeState.Checked => true,
            FileNodeState.Unchecked => false,
            FileNodeState.TreeOnly => false,
            FileNodeState.Indeterminate => null,
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // This is handled by the ViewModel's ToggleCheck method
        throw new NotSupportedException("Use ToggleCheck command instead of two-way binding");
    }
}
