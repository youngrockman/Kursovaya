using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Vosmerka
{
    public class MessageBox : Window
    {
        public MessageBox(string title, string message)
        {
            InitializeComponent(title, message);
            this.Icon = LoadIcon();
        }

        private void InitializeComponent(string title, string message)
        {
            this.Title = title;

            var textBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(10),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            var button = new Button
            {
                Content = "OK",
                Width = 100,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Color.Parse("#A163F5"))
            };

            button.Click += (s, e) => this.Close();

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(button);

            this.Content = stackPanel;
            this.Width = 300;
            this.SizeToContent = SizeToContent.Height;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private WindowIcon LoadIcon()
        {
            try
            {
                var uri = new Uri("avares://Vosmerka/Assets/vosmerka.ico");
                var assetLoader = AssetLoader.Open(uri);
                return new WindowIcon(assetLoader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки иконки: {ex.Message}");
                return null; 
            }
        }

        public static async Task Show(Window parent, string message, string title)
        {
            var msgBox = new MessageBox(title, message);
            await msgBox.ShowDialog(parent);
        }
    }
}