using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Vosmerka.Tools;

public class NumericInputDialog : Window
{
    private NumericUpDown _numericUpDown;

    public NumericInputDialog(string prompt)
    {
        InitializeComponent(prompt);
    }

    private void InitializeComponent(string prompt)
    {
        Width = 300;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var stackPanel = new StackPanel
        {
            Margin = new Thickness(10),
            Spacing = 10
        };

        stackPanel.Children.Add(new TextBlock { Text = prompt });
        
        _numericUpDown = new NumericUpDown
        {
            Minimum = (decimal)0.01,
            Maximum = 100000,
            FormatString = "F2"
        };
        stackPanel.Children.Add(_numericUpDown);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };

        var okButton = new Button { Content = "OK", Width = 80 };
        okButton.Click += (s, e) => Close(true);
        
        var cancelButton = new Button { Content = "Отмена", Width = 80 };
        cancelButton.Click += (s, e) => Close(false);

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        stackPanel.Children.Add(buttonPanel);

        Content = stackPanel;
    }

    public async Task<T?> ShowDialog<T>(Window owner)
    {
        await ShowDialog(owner);

        if (_numericUpDown.Value is decimal value)
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            object result = Convert.ChangeType(value, targetType);
            return (T?)result;
        }

        return default;
    }


}