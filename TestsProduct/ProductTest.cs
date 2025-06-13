using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
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
                .LogToTrace()
                .With((object)new 
                {
                    UseHeadlessDrawing = true,
                    UseCompositor = false
                })
                .SetupWithoutStarting();
        }

        [SetUp]
        public async Task Setup()
        {
            _dbContext = new User6Context();
            await ClearAndSeedDatabase();
    
            
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                _mainWindow = new MainWindow();
            });
        }

        [TearDown]
        public async Task TearDown()
        {
            await _dbContext.DisposeAsync();
        }

        private async Task ClearAndSeedDatabase()
        {
            // Очистка таблиц
            _dbContext.ProductSales.RemoveRange(_dbContext.ProductSales);
            _dbContext.ProductCostHistories.RemoveRange(_dbContext.ProductCostHistories);
            _dbContext.ProductMaterials.RemoveRange(_dbContext.ProductMaterials);
            _dbContext.Products.RemoveRange(_dbContext.Products);
            _dbContext.ProductTypes.RemoveRange(_dbContext.ProductTypes);
            await _dbContext.SaveChangesAsync();

            // Тестовые данные
            var productType = new ProductType { Title = "Тестовый тип" };
            _dbContext.ProductTypes.Add(productType);
            
            _dbContext.Products.Add(new Product
            {
                Title = "Тестовый продукт",
                ArticleNumber = "TEST001",
                ProductType = productType,
                MinCostForAgent = 100,
                ProductionWorkshopNumber = 1
            });
            
            await _dbContext.SaveChangesAsync();
        }

        [Test]
        public async Task LoadProducts_ShouldLoadProductsFromDatabase()
        {
            await _mainWindow.LoadProducts();
            var products = GetPrivateField<ObservableCollection<MainWindow.ProductPresenter>>(_mainWindow, "products");
            
            Assert.AreEqual(1, products.Count);
            Assert.AreEqual("Тестовый продукт", products[0].Title);
        }

        [Test]
        public void CalculateProductCost_ShouldReturnCorrectValue()
        {
            var product = new Product
            {
                MinCostForAgent = 100,
                ProductMaterials = new System.Collections.Generic.List<ProductMaterial>
                {
                    new ProductMaterial { Count = 2, Material = new Material { Cost = 10 } },
                    new ProductMaterial { Count = 3, Material = new Material { Cost = 20 } }
                }
            };
            
            var cost = InvokePrivateMethod<decimal>(_mainWindow, "CalculateProductCost", product);
            
            Assert.AreEqual(80, cost);
        }

        [Test]
        public async Task ApplyFilters_ShouldFilterProducts()
        {
            await _mainWindow.LoadProducts();
            SetPrivateField(_mainWindow, "_searchText", "Тестовый");
            
            InvokePrivateMethod(_mainWindow, "ApplyFilters");
            
            var filtered = GetPrivateField<List<MainWindow.ProductPresenter>>(_mainWindow, "filteredProducts");
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("Тестовый продукт", filtered[0].Title);
        }
        
        [Test]
        public async Task ApplySorting_ShouldSortByNameAscending()
        {
            await _mainWindow.LoadProducts();
            SetPrivateField(_mainWindow, "filteredProducts", new List<MainWindow.ProductPresenter>
            {
                new() { Title = "B Продукт" },
                new() { Title = "A Продукт" },
                new() { Title = "C Продукт" }
            });
    
           
            SetPrivateField(_mainWindow, "_sortField", "Наименование");
            SetPrivateField(_mainWindow, "_sortOrder", "по возрастанию");
            InvokePrivateMethod(_mainWindow, "ApplySorting");
    
            
            var filtered = GetPrivateField<List<MainWindow.ProductPresenter>>(_mainWindow, "filteredProducts");
            Assert.AreEqual("A Продукт", filtered[0].Title);
            Assert.AreEqual("B Продукт", filtered[1].Title);
        }

        [Test]
        public async Task ApplySorting_ShouldSortByCostDescending()
        {
            
            await _mainWindow.LoadProducts();
            SetPrivateField(_mainWindow, "filteredProducts", new List<MainWindow.ProductPresenter>
            {
                new() { MinCostForAgent = 100 },
                new() { MinCostForAgent = 300 },
                new() { MinCostForAgent = 200 }
            });
    
          
            SetPrivateField(_mainWindow, "_sortField", "Минимальная стоимость");
            SetPrivateField(_mainWindow, "_sortOrder", "по убыванию");
            InvokePrivateMethod(_mainWindow, "ApplySorting");
    
           
            var filtered = GetPrivateField<List<MainWindow.ProductPresenter>>(_mainWindow, "filteredProducts");
            Assert.AreEqual(300, filtered[0].MinCostForAgent);
            Assert.AreEqual(200, filtered[1].MinCostForAgent);
        }
        
        [Test]
        public async Task UpdatePaginationControls_ShouldCreateCorrectButtons()
        {
            
            await _mainWindow.LoadProducts();
            SetPrivateField(_mainWindow, "filteredProducts", 
                Enumerable.Range(1, 25)
                    .Select(i => new MainWindow.ProductPresenter())
                    .ToList());
    
            
            InvokePrivateMethod(_mainWindow, "UpdatePaginationControls");
    
            
            var paginationPanel = GetPrivateField<ItemsControl>(_mainWindow, "PaginationPanel");
            Assert.AreEqual(2, paginationPanel.Items.Count); // 25 items / 20 per page = 2 pages
        }

        [Test]
        public async Task NextPage_Click_ShouldIncrementCurrentPage()
        {
            
            await _mainWindow.LoadProducts();
            SetPrivateField(_mainWindow, "filteredProducts", 
                Enumerable.Range(1, 25)
                    .Select(i => new MainWindow.ProductPresenter())
                    .ToList());
            SetPrivateField(_mainWindow, "currentPage", 1);
    
            
            InvokePrivateMethod(_mainWindow, "NextPage_Click", null, new RoutedEventArgs());
    
            
            Assert.AreEqual(2, GetPrivateField<int>(_mainWindow, "currentPage"));
        }
        
        
        [Test]
        public async Task UpdateProductsCost_ShouldUpdateCostInDatabase()
        {
            
            var testProduct = new MainWindow.ProductPresenter { Id = 1, MinCostForAgent = 100 };
            await _mainWindow.LoadProducts();
    
           
            await InvokePrivateMethod<Task>(_mainWindow, "UpdateProductsCost", 
                new List<MainWindow.ProductPresenter> { testProduct }, 150m); //m это decimal
    
            
            using var context = new User6Context();
            var dbProduct = await context.Products.FindAsync(1);
            Assert.AreEqual(150, dbProduct.MinCostForAgent);
    
            var history = await context.ProductCostHistories
                .Where(h => h.ProductId == 1)
                .OrderByDescending(h => h.ChangeDate)
                .FirstOrDefaultAsync();
            Assert.AreEqual(150, history?.CostValue);
        }
        
        
        [Test]
        public void ProductPresenter_MaterialsList_ShouldReturnCorrectString()
        {
            
            var presenter = new MainWindow.ProductPresenter
            {
                ProductMaterials = new List<ProductMaterial>
                {
                    new() { Material = new Material { Title = "Материал 1" } },
                    new() { Material = new Material { Title = "Материал 2" } }
                }
            };
    
           
            Assert.AreEqual("Материал 1, Материал 2", presenter.MaterialsList);
        }

        [Test]
        public void ProductPresenter_ProductImage_ShouldReturnDefaultWhenImageMissing()
        {
            
            var presenter = new MainWindow.ProductPresenter { Image = "missing.jpg" };
    
            
            Assert.NotNull(presenter.ProductImage); // Должен вернуть изображение по умолчанию
        }
        
        [Test]
        public void ProductListBox_SelectionChanged_ShouldUpdateButtonVisibility()
        {
            
            var mainWindow = new MainWindow();
            var testProduct = new MainWindow.ProductPresenter();
            GetPrivateField<ObservableCollection<MainWindow.ProductPresenter>>(mainWindow, "products").Add(testProduct);
    
            
            var listBox = new ListBox();
            listBox.Items.Add(testProduct);
            listBox.SelectedItem = testProduct;
            InvokePrivateMethod(mainWindow, "ProductListBox_SelectionChanged", listBox, null);
    
            
            var button = GetPrivateField<Button>(mainWindow, "_changeCostButton");
            Assert.IsTrue(button.IsVisible);
        }
        
        
        
        
        
        
        
        
        
        
        
        

        // Вспомогательные методы для доступа к private 
        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field?.GetValue(obj);
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        private static T InvokePrivateMethod<T>(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)method?.Invoke(obj, parameters);
        }

        private static void InvokePrivateMethod(object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(obj, parameters);
        }
    }
}