using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
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
        
        [SetUp]
        public void Setup()
        {
            _editWindow = new ProductEditWindow();
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

            
            Assert.That(editWindow.IsEditMode, Is.True);
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
        public void MaterialsGrid_CellEditEnding_ShouldValidateNumericInput()
        {
            
            var textBox = new TextBox { Text = "invalid" };
            var column = new DataGridTextColumn { Header = "Количество" };
            var row = new DataGridRow();
            
            
            var args = new DataGridCellEditEndingEventArgs(
                column, 
                row, 
                textBox, 
                DataGridEditAction.Commit
            );

          
            _editWindow.MaterialsGrid_CellEditEnding(null, args);

        
            Assert.That(args.Cancel, Is.True);
        }

        [Test]
        public void MaterialsGrid_CellEditEnding_ShouldAcceptValidNumericInput()
        {
           
            var textBox = new TextBox { Text = "10" };
            var column = new DataGridTextColumn { Header = "Количество" };
            var row = new DataGridRow();
            
            
            var args = new DataGridCellEditEndingEventArgs(
                column, 
                row, 
                textBox, 
                DataGridEditAction.Commit
            );

           
            _editWindow.MaterialsGrid_CellEditEnding(null, args);

          
            Assert.That(args.Cancel, Is.False);
        }

        [Test]
        public void ProductPresenter_MaterialsList_ShouldReturnCorrectString()
        {
            
            var product = new Product
            {
                ArticleNumber = "TEST001",
                ProductMaterials = new List<ProductMaterial>
                {
                    new() { Material = new Material { Title = "Material 1" } },
                    new() { Material = new Material { Title = "Material 2" } }
                }
            };

         
            var materialsList = string.Join(", ", 
                product.ProductMaterials
                    .Where(pm => pm?.Material != null)
                    .Select(pm => pm.Material.Title));

           
            Assert.That(materialsList, Is.EqualTo("Material 1, Material 2"));
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