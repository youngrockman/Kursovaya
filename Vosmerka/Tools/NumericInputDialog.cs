using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Input;

namespace Vosmerka.Tools
{
    public class NumericInputDialog : Window
    {
        private TextBox _textBox;
        private bool _result = false;

        public NumericInputDialog(string prompt)
        {
            InitializeComponent(prompt);
            this.Title = "Выбор количества материала";
            this.Icon = LoadIcon();
        }

        public WindowIcon LoadIcon()
        {
            try
            {
                var uri = new Uri("avares://Vosmerka/Assets/vosmerka.ico");
                var assetLoader = AssetLoader.Open(uri);
                return new WindowIcon(assetLoader);
            }
            catch
            {
                return null;
            }
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
            
            _textBox = new TextBox
            {
                Watermark = "Введите количество (целое число ≥1)",
                Text = "1"
            };
            
            _textBox.KeyDown += (sender, e) =>
            {
                // Разрешаем только цифры, Backspace, Delete и стрелки
                if (!(e.Key >= Key.D0 && e.Key <= Key.D9) &&
                    !(e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) &&
                    e.Key != Key.Back &&
                    e.Key != Key.Delete &&
                    e.Key != Key.Left &&
                    e.Key != Key.Right)
                {
                    e.Handled = true;
                }
            };
            
            stackPanel.Children.Add(_textBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10
            };

            var okButton = new Button { Content = "OK", Width = 80 };
            okButton.Click += async (s, e) => 
            {
                if (await ValidateInputAsync())
                {
                    _result = true;
                    Close();
                }
            };
            
            var cancelButton = new Button { Content = "Отмена", Width = 80 };
            cancelButton.Click += (s, e) => Close();

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            Content = stackPanel;
        }

        public async Task<bool> ValidateInputAsync()
        {
            if (int.TryParse(_textBox.Text, out int value) && value >= 1)
            {
                return true;
            }
            
            await MessageBox.Show(this, "Пожалуйста, введите целое число ≥1", "Ошибка");
            _textBox.Text = "1";
            _textBox.Focus();
            _textBox.SelectAll();
            return false;
        }

        public new async Task<int?> ShowDialog(Window owner)
        {
            try
            {
                await base.ShowDialog(owner);
                if (_result && int.TryParse(_textBox.Text, out int result)) 
                {
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в диалоге ввода: {ex.Message}");
                return null;
            }
        }
    }
}