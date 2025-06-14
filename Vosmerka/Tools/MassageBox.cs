using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Threading.Tasks;

namespace Vosmerka
{
    public class MessageBox : Window
    {
        public enum MessageBoxButtons
        {
            OK,
            YesNo
        }

        public enum MessageBoxResult
        {
            OK,
            Yes,
            No
        }

        public MessageBoxResult Result { get; private set; }

        public MessageBox(string title, string message, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            InitializeComponent(title, message, buttons);
            this.Icon = LoadIcon();
        }

        private void InitializeComponent(string title, string message, MessageBoxButtons buttons)
        {
            this.Title = title;

            var textBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(10),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 10
            };

            if (buttons == MessageBoxButtons.OK)
            {
                var okButton = new Button
                {
                    Content = "OK",
                    Width = 100,
                    Margin = new Thickness(10),
                    Background = new SolidColorBrush(Color.Parse("#A163F5"))
                };
                okButton.Click += (s, e) => 
                {
                    Result = MessageBoxResult.OK;
                    this.Close();
                };
                buttonPanel.Children.Add(okButton);
            }
            else if (buttons == MessageBoxButtons.YesNo)
            {
                var yesButton = new Button
                {
                    Content = "Да",
                    Width = 100,
                    Margin = new Thickness(10),
                    Background = new SolidColorBrush(Color.Parse("#A163F5"))
                };
                yesButton.Click += (s, e) => 
                {
                    Result = MessageBoxResult.Yes;
                    this.Close();
                };

                var noButton = new Button
                {
                    Content = "Нет",
                    Width = 100,
                    Margin = new Thickness(10),
                    Background = new SolidColorBrush(Color.Parse("#A163F5"))
                };
                noButton.Click += (s, e) => 
                {
                    Result = MessageBoxResult.No;
                    this.Close();
                };

                buttonPanel.Children.Add(yesButton);
                buttonPanel.Children.Add(noButton);
            }

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(buttonPanel);

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

        public static async Task<MessageBoxResult> ShowWithResult(Window parent, string message, string title, MessageBoxButtons buttons)
        {
            var msgBox = new MessageBox(title, message, buttons);
            await msgBox.ShowDialog(parent);
            return msgBox.Result;
        }
    }
}