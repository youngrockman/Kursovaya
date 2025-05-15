// MainWindow.axaml.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using Vosmerka.Models;

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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadProducts();
            InitializeFilters();
            InitializeSorting();
            UpdateDisplay();
        }

        public class ProductPresenter : Product
        {
            public decimal CalculatedCost { get; set; }
            
            public string MaterialsList => string.Join(", ", 
                ProductMaterials?
                    .Where(pm => pm?.Material != null)
                    .Select(pm => pm.Material.Title) 
                ?? Enumerable.Empty<string>());
            
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

        private void LoadProducts()
        {
            try
            {
                using var context = new User6Context();
                allProducts = context.Products
                    .Include(p => p.ProductType)
                    .Include(p => p.ProductMaterials)
                        .ThenInclude(pm => pm.Material)
                    .Include(p => p.ProductSales)
                    .AsEnumerable()
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
        }

        private decimal CalculateProductCost(Product product)
        {
            return product.ProductMaterials?
                .Sum(pm => (pm.Material?.Cost ?? 0) * (decimal)(pm.Count ?? 0)) ?? 0;
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
                    .Where(p => (p.Title?.ToLower().Contains(searchText) ?? false || 
                               (p.Description?.ToLower().Contains(searchText) ?? false)))
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