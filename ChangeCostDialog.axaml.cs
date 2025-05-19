using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using System;
using Avalonia.Media;

namespace Vosmerka;

public partial class ChangeCostDialog : Window
{
    public decimal NewCost { get; private set; }

    public ChangeCostDialog(decimal averageCost)
    {
        this.Width = 500;
        this.Height = 250;
        this.Title = "Изменение стоимости продукции";
        
        this.Icon = new WindowIcon("C:\\Users\\IVAN\\RiderProjects\\Vosmerka\\Vosmerka\\bin\\Debug\\net9.0\\vosmerka.ico");
        
        NewCost = averageCost;

        InitializeComponents(averageCost);
    }

    private void InitializeComponents(decimal averageCost)
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15
        };
        
        var header = new TextBlock
        {
            Text = "Изменение стоимости продукции",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var costLabel = new TextBlock { Text = "Новая стоимость:" };
        var costTextBox = new NumericUpDown
        {
            Value = averageCost,
            Minimum = 0,
            FormatString = "F2"
        };
        
        var buttonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };
        
        var okButton = new Button { Content = "Изменить" };
        okButton.Click += (s, e) => 
        {
            NewCost = (decimal)costTextBox.Value;
            Close(true);
        };
        
        var cancelButton = new Button { Content = "Отмена" };
        cancelButton.Click += (s, e) => Close(false);
        
        buttonsPanel.Children.Add(okButton);
        buttonsPanel.Children.Add(cancelButton);
        
        panel.Children.Add(header); 
        panel.Children.Add(costLabel);
        panel.Children.Add(costTextBox);
        panel.Children.Add(buttonsPanel);
        
        this.Content = panel;
    }
}