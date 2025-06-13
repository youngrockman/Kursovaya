using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using Vosmerka.Models;
using Avalonia.Data.Converters;
using Avalonia.Input;

namespace Vosmerka
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ProductPresenter> products = new();
        private List<ProductPresenter> allProducts = new();
        private List<ProductPresenter> filteredProducts = new();
        private const int pageSize = 20;
        private int currentPage = 1;
        private int pageCount = 0;
        
        private Button _changeCostButton;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadProducts();
            InitializeFilters();
            InitializeSorting();
            UpdateDisplay();
            InitializeChangeCostButton();
            
            ProductListBox.SelectionChanged += ProductListBox_SelectionChanged;
        }
        
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new ProductEditWindow();
            var result = await editWindow.ShowDialog<bool>(this);
    
            if (result)
            {
                await LoadProducts();
                UpdateDisplay();
            }
        }
        
        private async void ProductListBox_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (ProductListBox.SelectedItem is ProductPresenter selectedProduct)
            {
                using var context = new User6Context();
                var product = await context.Products
                    .Include(p => p.ProductMaterials)
                    .FirstOrDefaultAsync(p => p.Id == selectedProduct.Id);
                
                if (product != null)
                {
                    var editWindow = new ProductEditWindow(product);
                    var result = await editWindow.ShowDialog<bool>(this);
                    
                    if (result)
                    {
                        await LoadProducts();
                        UpdateDisplay();
                    }
                }
            }
        }
        
        private void InitializeChangeCostButton()
        {
            _changeCostButton = new Button
            {
                Content = "Изменить стоимость на...",
                Margin = new Thickness(0, 0, 10, 0),
                IsVisible = false,
                Background = new SolidColorBrush(Color.Parse("#A163F5"))
            };
            _changeCostButton.Click += ChangeCostButton_Click;
            
            var topPanel = this.FindControl<StackPanel>("TopPanel");
            topPanel.Children.Add(_changeCostButton);
        }

        private void ProductListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обновить состояние IsSelected для всех продуктов
            foreach (var product in products)
            {
                product.IsSelected = ProductListBox.SelectedItems.Contains(product);
            }
    
            // Показать/скрыть кнопку изменения стоимости
            _changeCostButton.IsVisible = ProductListBox.SelectedItems?.Count > 0;
        }

        private async void ChangeCostButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProducts = products.Where(p => p.IsSelected).ToList();
            if (!selectedProducts.Any()) return;

            decimal averageCost = (decimal)selectedProducts.Average(p => p.MinCostForAgent);
            var dialog = new ChangeCostDialog(averageCost);
    
            try
            {
                var result = await dialog.ShowDialog<bool>(this);
                if (result)
                {
                    decimal newCost = dialog.NewCost;
                    await UpdateProductsCost(selectedProducts, newCost);
            
                    
                    ProductListBox.ItemsSource = null;
                    ProductListBox.ItemsSource = products;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }

        private async Task UpdateProductsCost(List<ProductPresenter> productsToUpdate, decimal newCost)
        {
            try
            {
                using var context = new User6Context();
                var productIds = productsToUpdate.Select(p => p.Id).ToList();
        
                
                var dbProducts = await context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var product in dbProducts)
                {
                    product.MinCostForAgent = newCost;
                    context.ProductCostHistories.Add(new ProductCostHistory
                    {
                        ProductId = product.Id,
                        ChangeDate = DateTime.Now,
                        CostValue = newCost
                    });
                }

                await context.SaveChangesAsync();
                
                foreach (var presenter in productsToUpdate)
                {
                    var product = dbProducts.FirstOrDefault(p => p.Id == presenter.Id);
                    if (product != null)
                    {
                        presenter.MinCostForAgent = product.MinCostForAgent;
                        presenter.CalculatedCost = CalculateProductCost(product);
                    }
                }
                
                UpdateDisplay();
                ProductListBox.ItemsSource = null;
                ProductListBox.ItemsSource = products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product costs: {ex.Message}");
            }
        }

        public class ProductPresenter : Product
        {
            public bool IsSelected { get; set; }
            public decimal CalculatedCost { get; set; }
            
            //Создаем список материалов
            public string MaterialsList => string.Join(", ", 
                ProductMaterials?
                    .Where(pm => pm?.Material != null)
                    .Select(pm => pm.Material.Title) 
                ?? Enumerable.Empty<string>());
            
            //Отрисовываем картинку с помощью Bitmap
            public Bitmap ProductImage 
            {
                get
                {
                    try
                    {
                        var imagePath = string.IsNullOrEmpty(Image) 
                            ? "picture.png" 
                            : Image.TrimStart('\\');
                        
                        var absolutePath = Path.Combine(AppContext.BaseDirectory, imagePath);
                        return File.Exists(absolutePath) 
                            ? new Bitmap(absolutePath) 
                            : new Bitmap(Path.Combine(AppContext.BaseDirectory, "picture.png"));
                    }
                    catch
                    {
                        return new Bitmap(Path.Combine(AppContext.BaseDirectory, "picture.png"));
                    }
                }
            }
        }

        //Подгруджаем продукты и соответствующие поля
        private async Task LoadProducts()
        {
            try
            {
                using var context = new User6Context();
                var productsFromDb = await context.Products
                    .Include(p => p.ProductType)
                    .Include(p => p.ProductMaterials)
                    .ThenInclude(pm => pm.Material)
                    .Include(p => p.ProductSales)
                    .ToListAsync();

                allProducts = productsFromDb
                    .Select(p => new ProductPresenter
                    {
                        Id = p.Id,
                        Title = p.Title ?? "Без названия",
                        ArticleNumber = p.ArticleNumber ?? "N/A",
                        Description = p.Description,
                        Image = p.Image,
                        ProductionPersonCount = p.ProductionPersonCount,
                        ProductionWorkshopNumber = p.ProductionWorkshopNumber,
                        MinCostForAgent = p.MinCostForAgent,
                        ProductType = p.ProductType,
                        ProductMaterials = p.ProductMaterials,
                        CalculatedCost = CalculateProductCost(p)
                    })
                    .ToList();

                filteredProducts = allProducts.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки продуктов: {ex.Message}");
            }
            finally
            {
                UpdateDisplay();
            }
        }

        private decimal CalculateProductCost(Product product)
        {
            decimal materialsCost = product.ProductMaterials?
                .Sum(pm => (pm.Material?.Cost ?? 0) * (decimal)(pm.Count ?? 0)) ?? 0;

            // Если стоимость материалов равна 0 или продуктов нет, используем MinCostForAgent
            if (materialsCost == 0 || product.ProductMaterials == null || !product.ProductMaterials.Any())
            {
                return product.MinCostForAgent ?? 0;
            }

            return materialsCost;
        }

        private void InitializeFilters()
        {
            try
            {
                using var context = new User6Context();
                var productTypes = context.ProductTypes
                    .Select(pt => pt.Title)
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                productTypes.Insert(0, "Все типы");
                FilterBox.ItemsSource = productTypes;
                FilterBox.SelectedItem = "Все типы";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации фильтров: {ex.Message}");
            }
        }

        private void InitializeSorting()
        {
            SortBox.ItemsSource = new List<string> { 
                "Наименование", 
                "Номер цеха", 
                "Минимальная стоимость" 
            };
            SortBox.SelectedIndex = 0;
            
            SortOrderBox.ItemsSource = new List<string> { 
                "по возрастанию", 
                "по убыванию" 
            };
            SortOrderBox.SelectedIndex = 0;
        }

        private void ApplyFilters()
        {
            try
            {
                var searchText = SearchBox.Text?.ToLower() ?? "";
                var selectedType = FilterBox.SelectedItem?.ToString() ?? "Все типы";

                filteredProducts = allProducts
                    .Where(p => (p.Title?.ToLower().Contains(searchText) ?? false) || 
                                (p.Description?.ToLower().Contains(searchText) ?? false))
                    .Where(p => selectedType == "Все типы" || 
                                (p.ProductType?.Title == selectedType))
                    .ToList();

                ApplySorting();
                currentPage = 1;
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private void ApplySorting()
        {
            var sortField = SortBox.SelectedItem?.ToString();
            var sortOrder = SortOrderBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(sortField)) return;

            filteredProducts = sortField switch
            {
                "Наименование" => sortOrder == "по возрастанию" 
                    ? filteredProducts.OrderBy(p => p.Title).ToList() 
                    : filteredProducts.OrderByDescending(p => p.Title).ToList(),
                "Номер цеха" => sortOrder == "по возрастанию" 
                    ? filteredProducts.OrderBy(p => p.ProductionWorkshopNumber ?? 0).ToList() 
                    : filteredProducts.OrderByDescending(p => p.ProductionWorkshopNumber ?? 0).ToList(),
                "Минимальная стоимость" => sortOrder == "по возрастанию" 
                    ? filteredProducts.OrderBy(p => p.MinCostForAgent).ToList() 
                    : filteredProducts.OrderByDescending(p => p.MinCostForAgent).ToList(),
                _ => filteredProducts
            };
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            products.Clear();
            var pageProducts = filteredProducts
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            foreach (var product in pageProducts)
            {
                products.Add(product);
            }
            
            ProductListBox.ItemsSource = products;
            UpdatePaginationControls();
        }

        //Пагинация
        private void UpdatePaginationControls()
        {
            pageCount = (int)Math.Ceiling(filteredProducts.Count / (double)pageSize);
            PaginationPanel.Items.Clear();

            for (int i = 1; i <= pageCount; i++)
            {
                var button = new Button { 
                    Content = i.ToString(),
                    Margin = new Thickness(2),
                    Background = i == currentPage ? Brushes.LightGray : Brushes.Transparent
                };
                button.Click += (s, e) => 
                {
                    currentPage = int.Parse((string)button.Content);
                    UpdateDisplay();
                };
                PaginationPanel.Items.Add(button);
            }
        }
        
        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdateDisplay();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < pageCount)
            {
                currentPage++;
                UpdateDisplay();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void SortOrderBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
    }
}