using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Threading.Tasks;

namespace Vosmerka
{
    public partial class ChangeCostDialog : Window
    {
        public decimal NewCost { get; private set; }
        private NumericUpDown _costTextBox;

        public ChangeCostDialog(decimal averageCost)
        {
            try
            {
                this.Width = 500;
                this.Height = 250;
                this.Title = "Изменение стоимости продукции";
                
                this.Icon = LoadIcon();
                
                NewCost = averageCost;
                InitializeComponents(averageCost);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Ошибка инициализации: {ex.Message}", "Ошибка").Wait();
            }
        }

        public WindowIcon LoadIcon()
        {
            try
            {
                var uri = new Uri("avares://Vosmerka/Assets/vosmerka.ico");
                var assetLoader = AssetLoader.Open(uri);
                return new WindowIcon(assetLoader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Ошибка загрузки иконки: {ex.Message}", "Ошибка").Wait();
                return null; 
            }
        }

        private void InitializeComponents(decimal averageCost)
        {
            try
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
                
                _costTextBox = new NumericUpDown
                {
                    Value = averageCost,
                    Minimum = 0,
                    FormatString = "F2",
                    AllowSpin = true,
                    Increment = 1,
                    NumberFormat = new System.Globalization.NumberFormatInfo
                    {
                        NumberDecimalDigits = 2
                    }
                };
                
                _costTextBox.PropertyChanged += (s, e) =>
                {
                    if (e.Property == NumericUpDown.ValueProperty && _costTextBox.Value.HasValue)
                    {
                        var newValue = Math.Round(_costTextBox.Value.Value, 2);
                        _costTextBox.Value = newValue;
                    }
                };

                var buttonsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Spacing = 10
                };
                
                var okButton = new Button 
                { 
                    Content = "Изменить",
                    Background = new SolidColorBrush(Color.Parse("#A163F5")),
                    MinWidth = 100
                };
                
                okButton.Click += async (s, e) => 
                {
                    try
                    {
                        if (!_costTextBox.Value.HasValue || _costTextBox.Value.Value < 0)
                        {
                            await MessageBox.Show(this, "Стоимость не может быть отрицательной", "Ошибка");
                            _costTextBox.Value = NewCost; 
                            return;
                        }
                        NewCost = _costTextBox.Value.Value;
                        Close(true);
                    }
                    catch (Exception ex)
                    {
                        await MessageBox.Show(this, $"Ошибка сохранения: {ex.Message}", "Ошибка");
                    }
                };
                
                var cancelButton = new Button 
                { 
                    Content = "Отмена",
                    Background = new SolidColorBrush(Color.Parse("#A163F5")),
                    MinWidth = 100
                };
                
                cancelButton.Click += (s, e) => Close(false);
                
                buttonsPanel.Children.Add(okButton);
                buttonsPanel.Children.Add(cancelButton);
                
                panel.Children.Add(header); 
                panel.Children.Add(costLabel);
                panel.Children.Add(_costTextBox);
                panel.Children.Add(buttonsPanel);
                
                this.Content = panel;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Ошибка инициализации компонентов: {ex.Message}", "Ошибка").Wait();
            }
        }
        
        public async Task<bool> ShowDialog(Window owner)
        {
            try
            {
                return await base.ShowDialog<bool>(owner);
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"Ошибка отображения диалога: {ex.Message}", "Ошибка");
                return false;
            }
        }
    }
}