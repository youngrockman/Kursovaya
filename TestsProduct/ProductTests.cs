using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using NUnit.Framework;
using Vosmerka;
using Vosmerka.Models;

namespace TestsProduct
{
    [TestFixture]
    public class MainWindowTests
    {
        private MainWindow _mainWindow;
        
        [OneTimeSetUp]
        public void InitAvalonia()
        {
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .SetupWithoutStarting();
        }

        [SetUp]
        public void Setup()
        {
            _mainWindow = new MainWindow();
        }

        [Test]
        public void CalculateProductCost_WithMaterials_ShouldReturnCorrectValue()
        {
            var product = new Product
            {
                MinCostForAgent = 100,
                ArticleNumber = "TEST001",
                ProductMaterials = new List<ProductMaterial>
                {
                    new() { Count = 2, Material = new Material { Cost = 10 } },
                    new() { Count = 3, Material = new Material { Cost = 20 } }
                }
            };
            
            var cost = _mainWindow.CalculateProductCost(product);
            
            Assert.That(cost, Is.EqualTo(80));
        }

        [Test]
        public void CalculateProductCost_WithoutMaterials_ShouldReturnMinCostForAgent()
        {
            var product = new Product
            {
                MinCostForAgent = 100,
                ArticleNumber = "TEST001",
                ProductMaterials = new List<ProductMaterial>()
            };
            
            var cost = _mainWindow.CalculateProductCost(product);
            
            Assert.That(cost, Is.EqualTo(100));
        }

        [Test]
        public void ApplyFilters_ShouldFilterBySearchText()
        {
            _mainWindow.allProducts = new List<MainWindow.ProductPresenter>
            {
                new() { Title = "Тестовый продукт 1", ArticleNumber = "TEST001" },
                new() { Title = "Другой продукт", ArticleNumber = "TEST002" }
            };
            
            
            _mainWindow.SearchBox = new TextBox { Text = "Тестовый" };

            _mainWindow.ApplyFilters();

            Assert.That(_mainWindow.filteredProducts.Count, Is.EqualTo(1));
            Assert.That(_mainWindow.filteredProducts[0].Title, Is.EqualTo("Тестовый продукт 1"));
        }

        [Test]
        public void ApplyFilters_ShouldFilterByProductType()
        {
            var productType = new ProductType { Title = "Тип 1" };
            _mainWindow.allProducts = new List<MainWindow.ProductPresenter>
            {
                new() { Title = "Продукт 1", ArticleNumber = "TEST001", ProductType = productType },
                new() { Title = "Продукт 2", ArticleNumber = "TEST002", ProductType = new ProductType { Title = "Тип 2" } }
            };
    
            
            _mainWindow.FilterBox = new ComboBox
            {
                ItemsSource = new List<string> { "Все типы", "Тип 1", "Тип 2" },
                SelectedItem = "Тип 1"
            };

            _mainWindow.ApplyFilters();

            Assert.That(_mainWindow.filteredProducts.Count, Is.EqualTo(1));
            Assert.That(_mainWindow.filteredProducts[0].Title, Is.EqualTo("Продукт 1"));
        }

        [Test]
        public void ApplySorting_ShouldSortByNameAscending()
        {
            _mainWindow.filteredProducts = new List<MainWindow.ProductPresenter>
            {
                new() { Title = "B Продукт", ArticleNumber = "TEST002" },
                new() { Title = "A Продукт", ArticleNumber = "TEST001" },
                new() { Title = "C Продукт", ArticleNumber = "TEST003" }
            };
            
            
            _mainWindow.SortBox = new ComboBox { SelectedItem = "Наименование" };
            _mainWindow.SortOrderBox = new ComboBox { SelectedItem = "по возрастанию" };

            _mainWindow.ApplySorting();

            Assert.That(_mainWindow.filteredProducts[0].Title, Is.EqualTo("A Продукт"));
            Assert.That(_mainWindow.filteredProducts[1].Title, Is.EqualTo("B Продукт"));
            Assert.That(_mainWindow.filteredProducts[2].Title, Is.EqualTo("C Продукт"));
        }

        [Test]
        public void ProductPresenter_MaterialsList_ShouldReturnCorrectString()
        {
            var presenter = new MainWindow.ProductPresenter
            {
                ArticleNumber = "TEST001",
                ProductMaterials = new List<ProductMaterial>
                {
                    new() { Material = new Material { Title = "Материал 1" } },
                    new() { Material = new Material { Title = "Материал 2" } }
                }
            };

            Assert.That(presenter.MaterialsList, Is.EqualTo("Материал 1, Материал 2"));
        }

        [Test]
        public void ProductPresenter_MaterialsList_ShouldReturnEmptyString_WhenNoMaterials()
        {
            var presenter = new MainWindow.ProductPresenter
            {
                ArticleNumber = "TEST001",
                ProductMaterials = new List<ProductMaterial>()
            };

            Assert.That(presenter.MaterialsList, Is.Empty);
        }
    }
}