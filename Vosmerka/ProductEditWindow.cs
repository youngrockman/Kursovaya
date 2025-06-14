using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Data;
using Vosmerka.Models;
using Vosmerka.Tools;

namespace Vosmerka
{
    public partial class ProductEditWindow : Window
    {
        public Product _product;
        private bool _isEditMode;
        private string? _imagePath;
        public List<ProductMaterial> _productMaterials = new List<ProductMaterial>();
        public List<Material> _allMaterials = new List<Material>();
        private List<ProductType> _productTypes = new List<ProductType>();

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                DeleteButton.IsVisible = value;
            }
        }

        public ProductEditWindow()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadDataAsync();
        }

        public ProductEditWindow(Product product) : this()
        {
            _product = product;
            IsEditMode = true;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var context = new User6Context();

               
                _productTypes = await context.ProductTypes.ToListAsync();
                ProductTypeBox.ItemsSource = _productTypes;
                ProductTypeBox.DisplayMemberBinding = new Binding("Title");

              
                _allMaterials = await context.Materials
                    .Include(m => m.MaterialType)
                    .OrderBy(m => m.Title)
                    .ToListAsync();
                    
              
                MaterialsComboBox.ItemsSource = _allMaterials;
                MaterialsComboBox.DisplayMemberBinding = new Binding("Title");

                
                if (IsEditMode && _product != null)
                {
                    
                    _productMaterials = await context.ProductMaterials
                        .Include(pm => pm.Material)
                        .ThenInclude(m => m.MaterialType)
                        .Where(pm => pm.ProductId == _product.Id)
                        .ToListAsync();

                    MaterialsListBox.ItemsSource = _productMaterials;
                    
                    
                    LoadProductData();
                }
                else
                {
                    
                    MaterialsListBox.ItemsSource = _productMaterials;
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
        }

        public void MaterialSearchBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MaterialSearchBox.Text))
            {
                
                MaterialsComboBox.ItemsSource = _allMaterials;
                return;
            }

            // Фильтруем материалы по введенному тексту
            var searchText = MaterialSearchBox.Text.ToLower();
            var filteredMaterials = _allMaterials
                .Where(m => m.Title.ToLower().Contains(searchText) || 
                           m.MaterialType.Title.ToLower().Contains(searchText))
                .ToList();

            
            MaterialsComboBox.ItemsSource = filteredMaterials;
        }

        public void LoadProductData()
        {
            if (_product == null) return;

            ArticleNumberBox.Text = _product.ArticleNumber;
            TitleBox.Text = _product.Title;
            DescriptionBox.Text = _product.Description;
            PersonCountBox.Value = _product.ProductionPersonCount ?? 1;
            WorkshopNumberBox.Value = _product.ProductionWorkshopNumber ?? 0;
            MinCostBox.Value = _product.MinCostForAgent.HasValue
                ? (decimal?)_product.MinCostForAgent.Value
                : null;

            if (_product.ProductTypeId != null)
            {
                ProductTypeBox.SelectedItem = _productTypes
                    .FirstOrDefault(pt => pt.Id == _product.ProductTypeId);
            }

            if (!string.IsNullOrEmpty(_product.Image))
            {
                try
                {
                    var imagePath = Path.Combine(AppContext.BaseDirectory, _product.Image.TrimStart('\\'));
                    if (File.Exists(imagePath))
                    {
                        ProductImage.Source = new Bitmap(imagePath);
                        _imagePath = imagePath;
                    }
                }
                catch { }
            }
        }

        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter { Name = "Images", Extensions = { "png", "jpg", "jpeg" } });
            dialog.AllowMultiple = false;

            var result = await dialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                try
                {
                    _imagePath = result[0];
                    ProductImage.Source = new Bitmap(_imagePath);
                }
                catch (Exception ex)
                {
                    await MessageBox.Show(this, $"Ошибка загрузки изображения: {ex.Message}", "Ошибка");
                }
            }
        }

        public async void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsComboBox.SelectedItem is Material selectedMaterial)
            {
                if (_productMaterials.Any(pm => pm.MaterialId == selectedMaterial.Id))
                {
                    await MessageBox.Show(this, "Этот материал уже добавлен", "Ошибка");
                    return;
                }

                // Проверяем доступное количество материала
                using var context = new User6Context();
                var totalUsedCount = await context.ProductMaterials
                    .Where(pm => pm.MaterialId == selectedMaterial.Id)
                    .SumAsync(pm => pm.Count ?? 0);

                var availableCount = (selectedMaterial.CountInStock ?? 0) - totalUsedCount;

                if (availableCount <= 0)
                {
                    await MessageBox.Show(this, "Нет доступного количества материала на складе", "Ошибка");
                    return;
                }

                var dialog = new NumericInputDialog($"Введите количество (доступно: {availableCount}):");
                var quantity = await dialog.ShowDialog(this);

                if (!quantity.HasValue || quantity <= 0)
                {
                    await MessageBox.Show(this, "Введите число ≥1", "Ошибка");
                    return;
                }

                if (quantity > availableCount)
                {
                    await MessageBox.Show(this, $"Количество не может превышать доступное ({availableCount})", "Ошибка");
                    return;
                }

                _productMaterials.Add(new ProductMaterial
                {
                    Material = selectedMaterial,
                    MaterialId = selectedMaterial.Id,
                    Count = quantity.Value
                });

                MaterialsListBox.ItemsSource = null;
                MaterialsListBox.ItemsSource = _productMaterials;
            }
            else
            {
                await MessageBox.Show(this, "Выберите материал из списка", "Ошибка");
            }
        }

        public void RemoveMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ProductMaterial material)
            {
                _productMaterials.Remove(material);
                MaterialsListBox.ItemsSource = null;
                MaterialsListBox.ItemsSource = _productMaterials;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                using var context = new User6Context();

                if (!IsEditMode || (_product?.ArticleNumber != ArticleNumberBox.Text))
                {
                    bool articleExists = await context.Products
                        .AnyAsync(p => p.ArticleNumber == ArticleNumberBox.Text);

                    if (articleExists)
                    {
                        await MessageBox.Show(this, "Продукт с таким артикулом уже существует", "Ошибка");
                        return;
                    }
                }

                string? imageRelativePath = null;
                if (!string.IsNullOrEmpty(_imagePath))
                {
                    var imagesDir = Path.Combine(AppContext.BaseDirectory, "Images");
                    Directory.CreateDirectory(imagesDir);

                    var ext = Path.GetExtension(_imagePath);
                    var newFileName = $"{Guid.NewGuid()}{ext}";
                    var destPath = Path.Combine(imagesDir, newFileName);

                    File.Copy(_imagePath, destPath, true);
                    imageRelativePath = Path.Combine("Images", newFileName);
                }

                if (IsEditMode && _product != null)
                {
                    await UpdateProduct(context, imageRelativePath);
                }
                else
                {
                    await CreateProduct(context, imageRelativePath);
                }

                Close(true);
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
        }

        public bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ArticleNumberBox.Text))
            {
                MessageBox.Show(this, "Артикул не может быть пустым", "Ошибка");
                return false;
            }

            if (ArticleNumberBox.Text.Any(c => !char.IsDigit(c)))
            {
                MessageBox.Show(this, "Артикул должен содержать только цифры", "Ошибка");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show(this, "Наименование не может быть пустым", "Ошибка");
                return false;
            }

            if (ProductTypeBox.SelectedItem == null)
            {
                MessageBox.Show(this, "Выберите тип продукта", "Ошибка");
                return false;
            }

            if (!PersonCountBox.Value.HasValue || PersonCountBox.Value < 1)
            {
                MessageBox.Show(this, "Количество работников должно быть больше 0", "Ошибка");
                return false;
            }

            return true;
        }

        private async Task UpdateProduct(User6Context context, string? imagePath)
        {
            var dbProduct = await context.Products
                .Include(p => p.ProductMaterials)
                .FirstOrDefaultAsync(p => p.Id == _product.Id);

            if (dbProduct == null) return;

            dbProduct.ArticleNumber = ArticleNumberBox.Text;
            dbProduct.Title = TitleBox.Text;
            dbProduct.Description = DescriptionBox.Text;
            dbProduct.ProductionPersonCount = (int)PersonCountBox.Value;
            dbProduct.ProductionWorkshopNumber = (int)WorkshopNumberBox.Value;
            dbProduct.MinCostForAgent = (decimal?)MinCostBox.Value;
            dbProduct.ProductTypeId = ((ProductType)ProductTypeBox.SelectedItem).Id;

            if (imagePath != null)
                dbProduct.Image = imagePath;

            context.ProductMaterials.RemoveRange(dbProduct.ProductMaterials);

            foreach (var pm in _productMaterials)
            {
                context.ProductMaterials.Add(new ProductMaterial
                {
                    ProductId = dbProduct.Id,
                    MaterialId = pm.MaterialId,
                    Count = pm.Count
                });
            }

            await context.SaveChangesAsync();
        }

        private async Task CreateProduct(User6Context context, string? imagePath)
        {
            var newProduct = new Product
            {
                ArticleNumber = ArticleNumberBox.Text,
                Title = TitleBox.Text,
                Description = DescriptionBox.Text,
                ProductionPersonCount = (int)PersonCountBox.Value,
                ProductionWorkshopNumber = (int)WorkshopNumberBox.Value,
                MinCostForAgent = (decimal?)MinCostBox.Value,
                ProductTypeId = ((ProductType)ProductTypeBox.SelectedItem).Id,
                Image = imagePath
            };

            context.Products.Add(newProduct);
            await context.SaveChangesAsync();

            foreach (var pm in _productMaterials)
            {
                context.ProductMaterials.Add(new ProductMaterial
                {
                    ProductId = newProduct.Id,
                    MaterialId = pm.MaterialId,
                    Count = pm.Count
                });
            }

            await context.SaveChangesAsync();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_product == null) return;

            try
            {
                using var context = new User6Context();

                if (await context.ProductSales.AnyAsync(ps => ps.ProductId == _product.Id))
                {
                    await MessageBox.Show(this,
                        "Невозможно удалить продукт, так как есть информация о его продажах",
                        "Ошибка удаления");
                    return;
                }

                var productToDelete = await context.Products
                    .Include(p => p.ProductMaterials)
                    .FirstOrDefaultAsync(p => p.Id == _product.Id);

                if (productToDelete != null)
                {
                    context.ProductMaterials.RemoveRange(productToDelete.ProductMaterials);
                    context.Products.Remove(productToDelete);
                    await context.SaveChangesAsync();
                    Close(true);
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"Ошибка удаления: {ex.Message}", "Ошибка");
            }
        }

        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        public void ArticleNumber_TextInput(object? sender, TextInputEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text[0]);
        }
    }
}