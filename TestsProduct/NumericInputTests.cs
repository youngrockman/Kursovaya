using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Vosmerka.Tools;

namespace TestsProduct
{
    [TestFixture]
    public class NumericInputTests : TestBase
    {
        public class TestSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object state)
            {
                d(state);
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                d(state);
            }

            public Task<T> SendAsync<T>(Func<object, Task<T>> func, object state)
            {
                return func(state);
            }
        }
        
        private NumericInputDialog _dialog;
        private TestSynchronizationContext _syncContext;

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            _syncContext = new TestSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_syncContext);
        }

        [SetUp]
        public void Setup()
        {
            _syncContext.Send(_ =>
            {
                _dialog = new NumericInputDialog("Test prompt");
                _dialog.Show();
            }, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_dialog != null)
            {
                _syncContext.Send(_ =>
                {
                    _dialog.Close();
                    _dialog = null;
                }, null);
            }
        }

        [Test]
        public void Constructor_ShouldInitializeWithCorrectValues()
        {
            _syncContext.Send(_ =>
            {
                Assert.That(_dialog.Title, Is.EqualTo("Выбор количества материала"));
                Assert.That(_dialog.Width, Is.EqualTo(300));
                Assert.That(_dialog.Height, Is.EqualTo(150));
                Assert.That(_dialog.WindowStartupLocation, Is.EqualTo(WindowStartupLocation.CenterOwner));
            }, null);
        }

        [Test]
        public void LoadIcon_ShouldNotThrowException()
        {
            _syncContext.Send(_ =>
            {
                Assert.DoesNotThrow(() => _dialog.LoadIcon());
            }, null);
        }

        [Test]
        public void InitializeComponents_ShouldCreateAllRequiredElements()
        {
            _syncContext.Send(_ =>
            {
                var content = _dialog.Content as StackPanel;
                Assert.That(content, Is.Not.Null);
                Assert.That(content.Children.Count, Is.EqualTo(3));

                var promptText = content.Children[0] as TextBlock;
                Assert.That(promptText, Is.Not.Null);
                Assert.That(promptText.Text, Is.EqualTo("Test prompt"));

                var textBox = content.Children[1] as TextBox;
                Assert.That(textBox, Is.Not.Null);
                Assert.That(textBox.Text, Is.EqualTo("1"));
                Assert.That(textBox.Watermark, Is.EqualTo("Введите количество (целое число ≥1)"));

                var buttonPanel = content.Children[2] as StackPanel;
                Assert.That(buttonPanel, Is.Not.Null);
                Assert.That(buttonPanel.Children.Count, Is.EqualTo(2));
                Assert.That(buttonPanel.Orientation, Is.EqualTo(Orientation.Horizontal));
                Assert.That(buttonPanel.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Right));
            }, null);
        }
        

        [Test]
        public async Task CancelButton_Click_ShouldCloseWithoutValue()
        {
            var result = await _syncContext.SendAsync(async _ =>
            {
                var content = _dialog.Content as StackPanel;
                var textBox = content.Children[1] as TextBox;
                var buttonPanel = content.Children[2] as StackPanel;
                var cancelButton = buttonPanel.Children[1] as Button;

                textBox.Text = "5";
                var closeCalled = false;
                _dialog.Closing += (s, e) => closeCalled = true;

                cancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.That(closeCalled, Is.True);

                return await _dialog.ShowDialog(null);
            }, null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ValidateInput_WithValidValue_ShouldReturnTrue()
        {
            _syncContext.Send(async _ =>
            {
                var content = _dialog.Content as StackPanel;
                var textBox = content.Children[1] as TextBox;
                textBox.Text = "5";

                var result = await _dialog.ValidateInputAsync();
                Assert.That(result, Is.True);
            }, null);
        }

        [Test]
        public void ValidateInput_WithZero_ShouldReturnFalse()
        {
            _syncContext.Send(async _ =>
            {
                var content = _dialog.Content as StackPanel;
                var textBox = content.Children[1] as TextBox;
                textBox.Text = "0";

                var result = await _dialog.ValidateInputAsync();
                Assert.That(result, Is.False);
                Assert.That(textBox.Text, Is.EqualTo("1"));
            }, null);
        }

        [Test]
        public void ValidateInput_WithNegativeValue_ShouldReturnFalse()
        {
            _syncContext.Send(async _ =>
            {
                var content = _dialog.Content as StackPanel;
                var textBox = content.Children[1] as TextBox;
                textBox.Text = "-5";

                var result = await _dialog.ValidateInputAsync();
                Assert.That(result, Is.False);
                Assert.That(textBox.Text, Is.EqualTo("1"));
            }, null);
        }

        [Test]
        public void ValidateInput_WithNonNumericValue_ShouldReturnFalse()
        {
            _syncContext.Send(async _ =>
            {
                var content = _dialog.Content as StackPanel;
                var textBox = content.Children[1] as TextBox;
                textBox.Text = "abc";

                var result = await _dialog.ValidateInputAsync();
                Assert.That(result, Is.False);
                Assert.That(textBox.Text, Is.EqualTo("1"));
            }, null);
        }

       

        [Test]
        public async Task ShowDialog_WithCancel_ShouldReturnNull()
        {
            var result = await _syncContext.SendAsync(async _ =>
            {
                var content = _dialog.Content as StackPanel;
                var buttonPanel = content.Children[2] as StackPanel;
                var cancelButton = buttonPanel.Children[1] as Button;

                cancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                return await _dialog.ShowDialog(null);
            }, null);

            Assert.That(result, Is.Null);
        }
    }
} 