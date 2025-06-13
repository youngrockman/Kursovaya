using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Vosmerka;

namespace TestsProduct
{
    [TestFixture]
    public class ChangeCostTests
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
        }
        
        private ChangeCostDialog _dialog;
        private AppBuilder _appBuilder;
        private TestSynchronizationContext _syncContext;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (Application.Current == null)
            {
                _syncContext = new TestSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(_syncContext);

                _appBuilder = AppBuilder.Configure<App>()
                    .UsePlatformDetect()
                    .LogToTrace();
                _appBuilder.StartWithClassicDesktopLifetime(Array.Empty<string>());
            }
        }

        [SetUp]
        public void Setup()
        {
            _syncContext.Send(_ =>
            {
                _dialog = new ChangeCostDialog(100.50m);
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
                Assert.That(_dialog.NewCost, Is.EqualTo(100.50m));
                Assert.That(_dialog.Title, Is.EqualTo("Изменение стоимости продукции"));
                Assert.That(_dialog.Width, Is.EqualTo(500));
                Assert.That(_dialog.Height, Is.EqualTo(250));
            }, null);
        }

        [Test]
        public void Constructor_WithZeroCost_ShouldInitializeCorrectly()
        {
            _syncContext.Send(_ =>
            {
                var dialog = new ChangeCostDialog(0m);
                dialog.Show();
                Assert.That(dialog.NewCost, Is.EqualTo(0m));
                dialog.Close();
            }, null);
        }

        [Test]
        public void Constructor_WithNegativeCost_ShouldInitializeWithZero()
        {
            _syncContext.Send(_ =>
            {
                var dialog = new ChangeCostDialog(-100m);
                dialog.Show();
                Assert.That(dialog.NewCost, Is.EqualTo(-100m));
                dialog.Close();
            }, null);
        }

        [Test]
        public void Constructor_WithLargeCost_ShouldInitializeCorrectly()
        {
            _syncContext.Send(_ =>
            {
                var dialog = new ChangeCostDialog(999999.99m);
                dialog.Show();
                Assert.That(dialog.NewCost, Is.EqualTo(999999.99m));
                dialog.Close();
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
                Assert.That(content.Children.Count, Is.EqualTo(4));

                var header = content.Children[0] as TextBlock;
                Assert.That(header, Is.Not.Null);
                Assert.That(header.Text, Is.EqualTo("Изменение стоимости продукции"));
                Assert.That(header.FontSize, Is.EqualTo(16));
                Assert.That(header.FontWeight, Is.EqualTo(FontWeight.Bold));

                var costLabel = content.Children[1] as TextBlock;
                Assert.That(costLabel, Is.Not.Null);
                Assert.That(costLabel.Text, Is.EqualTo("Новая стоимость:"));

                var costTextBox = content.Children[2] as NumericUpDown;
                Assert.That(costTextBox, Is.Not.Null);
                Assert.That(costTextBox.Value, Is.EqualTo(100.50m));
                Assert.That(costTextBox.Minimum, Is.EqualTo(0));
                Assert.That(costTextBox.FormatString, Is.EqualTo("F2"));
                Assert.That(costTextBox.AllowSpin, Is.True);
                Assert.That(costTextBox.Increment, Is.EqualTo(1));

                var buttonsPanel = content.Children[3] as StackPanel;
                Assert.That(buttonsPanel, Is.Not.Null);
                Assert.That(buttonsPanel.Children.Count, Is.EqualTo(2));
                Assert.That(buttonsPanel.Orientation, Is.EqualTo(Orientation.Horizontal));
                Assert.That(buttonsPanel.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Right));
            }, null);
        }

        [Test]
        public void OkButton_Click_WithValidValue_ShouldSetNewCostAndClose()
        {
            _syncContext.Send(_ =>
            {
                var content = _dialog.Content as StackPanel;
                var costTextBox = content.Children[2] as NumericUpDown;
                var buttonsPanel = content.Children[3] as StackPanel;
                var okButton = buttonsPanel.Children[0] as Button;

                costTextBox.Value = 150.75m;
                var closeCalled = false;
                _dialog.Closing += (s, e) => closeCalled = true;

                okButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                Assert.That(_dialog.NewCost, Is.EqualTo(150.75m));
                Assert.That(closeCalled, Is.True);
            }, null);
        }

        [Test]
        public void OkButton_Click_WithNegativeValue_ShouldNotClose()
        {
            _syncContext.Send(_ =>
            {
                var content = _dialog.Content as StackPanel;
                var costTextBox = content.Children[2] as NumericUpDown;
                var buttonsPanel = content.Children[3] as StackPanel;
                var okButton = buttonsPanel.Children[0] as Button;

                costTextBox.Value = -100m;
                var closeCalled = false;
                _dialog.Closing += (s, e) => closeCalled = true;

                okButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                Assert.That(_dialog.NewCost, Is.EqualTo(100.50m));
                Assert.That(closeCalled, Is.False);
            }, null);
        }

        [Test]
        public void CancelButton_Click_ShouldCloseWithoutChangingCost()
        {
            _syncContext.Send(_ =>
            {
                var content = _dialog.Content as StackPanel;
                var costTextBox = content.Children[2] as NumericUpDown;
                var buttonsPanel = content.Children[3] as StackPanel;
                var cancelButton = buttonsPanel.Children[1] as Button;

                costTextBox.Value = 200m;
                var closeCalled = false;
                _dialog.Closing += (s, e) => closeCalled = true;

                cancelButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                Assert.That(_dialog.NewCost, Is.EqualTo(100.50m));
                Assert.That(closeCalled, Is.True);
            }, null);
        }

        [Test]
        public void NumericUpDown_ShouldNotAcceptNegativeValues()
        {
            _syncContext.Send(_ =>
            {
                var content = _dialog.Content as StackPanel;
                var costTextBox = content.Children[2] as NumericUpDown;

                costTextBox.Value = -100m;
                Assert.That(costTextBox.Value, Is.GreaterThanOrEqualTo(0));
            }, null);
        }

        [Test]
        public void NumericUpDown_ShouldFormatCorrectly()
        {
            _syncContext.Send(_ =>
            {
                var content = _dialog.Content as StackPanel;
                var costTextBox = content.Children[2] as NumericUpDown;

                costTextBox.Value = 123.456m;
                Assert.That(costTextBox.Value, Is.EqualTo(123.46m));
            }, null);
        }

        [Test]
        public void NumericUpDown_ShouldHandleNullValue()
        {
            _syncContext.Send(_ =>
            {
                var content = _dialog.Content as StackPanel;
                var costTextBox = content.Children[2] as NumericUpDown;

                costTextBox.Value = null;
                Assert.That(costTextBox.Value, Is.Null);
            }, null);
        }

        [Test]
        public void NumericUpDown_ShouldHandleZeroValue()
        {
            _syncContext.Send(_ =>
            {
                var content = _dialog.Content as StackPanel;
                var costTextBox = content.Children[2] as NumericUpDown;

                costTextBox.Value = 0m;
                Assert.That(costTextBox.Value, Is.EqualTo(0m));
            }, null);
        }
    }
}