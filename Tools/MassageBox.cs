using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Vosmerka
{
    public class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
            
            this.Title = "Сообщение об ошибке";
        
            this.Icon = new WindowIcon("C:\\Users\\IVAN\\RiderProjects\\Vosmerka\\Vosmerka\\bin\\Debug\\net9.0\\vosmerka.ico");
        }

        private void InitializeComponent()
        {
            //AvaloniaXamlLoader.Load(this);
        }

        
        
        
        // Вывод MessageBox
        public static async Task Show(Window parent, string message, string title)
        {
            var msgBox = new MessageBox
            {
                Title = title
            };

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

            button.Click += (s, e) => msgBox.Close();

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(button);

            msgBox.Content = stackPanel;
            msgBox.Width = 300;
            msgBox.SizeToContent = SizeToContent.Height;

            await msgBox.ShowDialog(parent);
        }
    }
}