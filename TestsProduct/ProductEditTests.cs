using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using NUnit.Framework;
using Vosmerka;
using Vosmerka.Models;

namespace TestsProduct
{
    [TestFixture]
    public class ProductEditTests
    {
        private ProductEditWindow _editWindow;
        private App _app;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (Application.Current == null)
            {
                _app = new App();
                _app.Initialize();
            }
        }

        [SetUp]
        public void Setup()
        {
            _editWindow = new ProductEditWindow();
            _editWindow.Show();
        }

        [TearDown]
        public void TearDown()
        {
            _editWindow.Close();
        }

        [Test]
        public void Constructor_WithProduct_ShouldSetEditMode()
        {
            var product = new Product
            {
                Id = 1,
                ArticleNumber = "TEST001",
                Title = "Test Product"
            };

            var editWindow = new ProductEditWindow(product);
            editWindow.Show();

            Assert.That(editWindow.IsEditMode, Is.True);
            editWindow.Close();
        }

        [Test]
        public void LoadProductData_ShouldSetCorrectValues()
        {
            var product = new Product
            {
                ArticleNumber = "TEST001",
                Title = "Test Product",
                Description = "Test Description",
                ProductionPersonCount = 5,
                ProductionWorkshopNumber = 1,
                MinCostForAgent = 100,
                ProductTypeId = 1
            };

            _editWindow._product = product;
            _editWindow.LoadProductData();

            Assert.That(_editWindow.ArticleNumberBox.Text, Is.EqualTo("TEST001"));
            Assert.That(_editWindow.TitleBox.Text, Is.EqualTo("Test Product"));
            Assert.That(_editWindow.DescriptionBox.Text, Is.EqualTo("Test Description"));
            Assert.That(_editWindow.PersonCountBox.Value, Is.EqualTo(5));
            Assert.That(_editWindow.WorkshopNumberBox.Value, Is.EqualTo(1));
            Assert.That(_editWindow.MinCostBox.Value, Is.EqualTo(100));
        }

        [Test]
        public void RemoveMaterial_Click_ShouldRemoveMaterialFromList()
        {
            var material = new ProductMaterial
            {
                MaterialId = 1,
                Material = new Material { Title = "Test Material" }
            };
            _editWindow._productMaterials.Add(material);

            var button = new Button { DataContext = material };
            _editWindow.RemoveMaterial_Click(button, null);

            Assert.That(_editWindow._productMaterials.Count, Is.EqualTo(0));
        }

        [Test]
        public void ArticleNumber_TextInput_ShouldOnlyAcceptDigits()
        {
            var args = new TextInputEventArgs { Text = "123" };
            _editWindow.ArticleNumber_TextInput(null, args);
            Assert.That(args.Handled, Is.False);

            args = new TextInputEventArgs { Text = "abc" };
            _editWindow.ArticleNumber_TextInput(null, args);
            Assert.That(args.Handled, Is.True);
        }

        [Test]
        public void ValidateInput_WithEmptyArticleNumber_ShouldReturnFalse()
        {
            _editWindow.ArticleNumberBox.Text = "";
            Assert.That(_editWindow.ValidateInput(), Is.False);
        }

        [Test]
        public void ValidateInput_WithNonNumericArticleNumber_ShouldReturnFalse()
        {
            _editWindow.ArticleNumberBox.Text = "123abc";
            Assert.That(_editWindow.ValidateInput(), Is.False);
        }

        [Test]
        public void ValidateInput_WithEmptyTitle_ShouldReturnFalse()
        {
            _editWindow.ArticleNumberBox.Text = "123";
            _editWindow.TitleBox.Text = "";
            Assert.That(_editWindow.ValidateInput(), Is.False);
        }

        [Test]
        public void ValidateInput_WithNoProductType_ShouldReturnFalse()
        {
            _editWindow.ArticleNumberBox.Text = "123";
            _editWindow.TitleBox.Text = "Test";
            _editWindow.ProductTypeBox.SelectedItem = null;
            Assert.That(_editWindow.ValidateInput(), Is.False);
        }

        [Test]
        public void ValidateInput_WithZeroPersonCount_ShouldReturnFalse()
        {
            _editWindow.ArticleNumberBox.Text = "123";
            _editWindow.TitleBox.Text = "Test";
            _editWindow.ProductTypeBox.SelectedItem = new ProductType();
            _editWindow.PersonCountBox.Value = 0;
            Assert.That(_editWindow.ValidateInput(), Is.False);
        }

        [Test]
        public void ValidateInput_WithValidData_ShouldReturnTrue()
        {
            _editWindow.ArticleNumberBox.Text = "123";
            _editWindow.TitleBox.Text = "Test";
            _editWindow.ProductTypeBox.SelectedItem = new ProductType();
            _editWindow.PersonCountBox.Value = 1;
            Assert.That(_editWindow.ValidateInput(), Is.True);
        }

        [Test]
        public void MaterialSearchBox_TextChanged_ShouldFilterMaterials()
        {
            _editWindow._allMaterials = new List<Material>
            {
                new() { Title = "Test Material 1", MaterialType = new MaterialType { Title = "Type 1" } },
                new() { Title = "Another Material", MaterialType = new MaterialType { Title = "Type 2" } }
            };

            _editWindow.MaterialSearchBox.Text = "Test";
            _editWindow.MaterialSearchBox_TextChanged(null, null);

            var filteredMaterials = _editWindow.MaterialsComboBox.ItemsSource as List<Material>;
            Assert.That(filteredMaterials.Count, Is.EqualTo(1));
            Assert.That(filteredMaterials[0].Title, Is.EqualTo("Test Material 1"));
        }

        [Test]
        public void MaterialSearchBox_TextChanged_WithEmptyText_ShouldShowAllMaterials()
        {
            _editWindow._allMaterials = new List<Material>
            {
                new() { Title = "Test Material 1" },
                new() { Title = "Test Material 2" }
            };

            _editWindow.MaterialSearchBox.Text = "";
            _editWindow.MaterialSearchBox_TextChanged(null, null);

            var filteredMaterials = _editWindow.MaterialsComboBox.ItemsSource as List<Material>;
            Assert.That(filteredMaterials.Count, Is.EqualTo(2));
        }

        [Test]
        public void Cancel_Click_ShouldCloseWindow()
        {
            var closeCalled = false;
            _editWindow.Closing += (s, e) => closeCalled = true;
            _editWindow.Cancel_Click(null, null);
            Assert.That(closeCalled, Is.True);
        }
    }
}