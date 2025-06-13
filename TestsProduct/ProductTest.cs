using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Vosmerka;
using Vosmerka.Models;

namespace TestsProduct
{
    [TestFixture]
    public class MainWindowTests
    {
        private User6Context _dbContext;
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
            _dbContext = new User6Context();
            _mainWindow = new MainWindow();
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
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
            _mainWindow.SearchBox.Text = "Тестовый";

            
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
                new() { 
                    Title = "Продукт 1", 
                    ArticleNumber = "TEST001", 
                    ProductType = productType 
                },
                new() { 
                    Title = "Продукт 2", 
                    ArticleNumber = "TEST002", 
                    ProductType = new ProductType { Title = "Тип 2" } 
                }
            };
    
            
            _mainWindow.FilterBox.ItemsSource = new List<string> { "Все типы", "Тип 1", "Тип 2" };
            _mainWindow.FilterBox.SelectedItem = "Тип 1";

            
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
            _mainWindow.SortBox.SelectedItem = "Наименование";
            _mainWindow.SortOrderBox.SelectedItem = "по возрастанию";

           
            _mainWindow.ApplySorting();

           
            Assert.That(_mainWindow.filteredProducts[0].Title, Is.EqualTo("A Продукт"));
            Assert.That(_mainWindow.filteredProducts[1].Title, Is.EqualTo("B Продукт"));
            Assert.That(_mainWindow.filteredProducts[2].Title, Is.EqualTo("C Продукт"));
        }

        [Test]
        public void ApplySorting_ShouldSortByWorkshopNumberDescending()
        {
           
            var originalProducts = new List<MainWindow.ProductPresenter>
            {
                new() { 
                    Title = "Продукт 1", 
                    ArticleNumber = "TEST001", 
                    ProductionWorkshopNumber = 1 
                },
                new() { 
                    Title = "Продукт 2", 
                    ArticleNumber = "TEST002", 
                    ProductionWorkshopNumber = 3 
                },
                new() { 
                    Title = "Продукт 3", 
                    ArticleNumber = "TEST003", 
                    ProductionWorkshopNumber = 2 
                }
            };
    
           
            _mainWindow.filteredProducts = new List<MainWindow.ProductPresenter>(originalProducts);
            _mainWindow.SortBox = new ComboBox
            {
                ItemsSource = new List<string> { "Наименование", "Номер цеха", "Минимальная стоимость" },
                SelectedItem = "Номер цеха"
            };
            _mainWindow.SortOrderBox = new ComboBox
            {
                ItemsSource = new List<string> { "по возрастанию", "по убыванию" },
                SelectedItem = "по убыванию"
            };

            
            _mainWindow.ApplySorting();

            
            Assert.That(_mainWindow.filteredProducts, Is.Not.Null);
            Assert.That(_mainWindow.filteredProducts.Count, Is.EqualTo(3), 
                "Количество продуктов после сортировки не должно измениться");
    
            Assert.That(_mainWindow.filteredProducts[0].ProductionWorkshopNumber, Is.EqualTo(3),
                "Первый продукт должен иметь наибольший номер цеха");
            Assert.That(_mainWindow.filteredProducts[1].ProductionWorkshopNumber, Is.EqualTo(2),
                "Второй продукт должен иметь средний номер цеха");
            Assert.That(_mainWindow.filteredProducts[2].ProductionWorkshopNumber, Is.EqualTo(1),
                "Третий продукт должен иметь наименьший номер цеха");
        }

        [Test]
        public void UpdateDisplay_ShouldUpdateProductsCollection()
        {
            
            _mainWindow.filteredProducts = new List<MainWindow.ProductPresenter>
            {
                new() { Title = "Продукт 1", ArticleNumber = "TEST001" },
                new() { Title = "Продукт 2", ArticleNumber = "TEST002" }
            };
            _mainWindow.currentPage = 1;

           
            _mainWindow.UpdateDisplay();

      
            Assert.That(_mainWindow.products.Count, Is.EqualTo(2));
            Assert.That(_mainWindow.products[0].Title, Is.EqualTo("Продукт 1"));
            Assert.That(_mainWindow.products[1].Title, Is.EqualTo("Продукт 2"));
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