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
        private Product _product;
        private bool _isEditMode;
        private string? _imagePath;
        private List<ProductMaterial> _productMaterials = new();

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
            LoadDataAsync();
        }

        public ProductEditWindow(Product product) : this()
        {
            _product = product;
            IsEditMode = true;
            Loaded += async (_, _) =>
            {
                await LoadDataAsync();
                LoadProductData(); 
            };
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var context = new User6Context();

                var productTypes = await context.ProductTypes.ToListAsync();
                ProductTypeBox.ItemsSource = productTypes;
                ProductTypeBox.DisplayMemberBinding = new Binding("Title");

                var materials = await context.Materials.Include(m => m.MaterialType).ToListAsync();
                MaterialsComboBox.ItemsSource = materials;
                MaterialsComboBox.DisplayMemberBinding = new Binding("Title");

                if (_product != null)
                {
                    _productMaterials = await context.ProductMaterials
                        .Include(pm => pm.Material)
                        .Where(pm => pm.ProductId == _product.Id)
                        .ToListAsync();

                    MaterialsGrid.ItemsSource = _productMaterials;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }
        }


        private void LoadProductData()
        {
            ArticleNumberBox.Text = _product.ArticleNumber;
            TitleBox.Text = _product.Title;
            DescriptionBox.Text = _product.Description;
            PersonCountBox.Value = _product.ProductionPersonCount ?? 0;
            WorkshopNumberBox.Value = _product.ProductionWorkshopNumber ?? 0;
            MinCostBox.Value = _product.MinCostForAgent.HasValue
                ? (decimal?)Convert.ToDouble(_product.MinCostForAgent.Value)
                : null;

            if (_product.ProductTypeId != null)
            {
                foreach (var item in ProductTypeBox.Items)
                {
                    if (item is ProductType type && type.Id == _product.ProductTypeId)
                    {
                        ProductTypeBox.SelectedItem = item;
                        break;
                    }
                }
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

        //Выбор картинки
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
                    Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
        }

        
        //Добавление материала
        private async void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsComboBox.SelectedItem is Material selectedMaterial)
            {
                if (_productMaterials.Any(pm => pm.MaterialId == selectedMaterial.Id))
                {
                    await MessageBox.Show(this, "Этот материал уже добавлен", "Ошибка");
                    return;
                }

                var dialog = new NumericInputDialog("Введите количество:");
                var quantity = await dialog.ShowDialog(this);
    
                if (!quantity.HasValue)
                {
                    return; 
                }

                if (quantity <= 0)
                {
                    await MessageBox.Show(this, "Введите число ≥1", "Ошибка");
                    return;
                }

                var productMaterial = new ProductMaterial
                {
                    Material = selectedMaterial,
                    MaterialId = selectedMaterial.Id,
                    Count = quantity.Value
                };

                _productMaterials.Add(productMaterial);
                MaterialsGrid.ItemsSource = null;
                MaterialsGrid.ItemsSource = _productMaterials;
            }
        }

        
        // Для очистки материала
        private void RemoveMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ProductMaterial material)
            {
                _productMaterials.Remove(material);
                MaterialsGrid.ItemsSource = null;
                MaterialsGrid.ItemsSource = _productMaterials;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ArticleNumberBox.Text))
            {
                await MessageBox.Show(this, "Артикул не может быть пустым", "Ошибка");
                return;
            }

            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                await MessageBox.Show(this, "Наименование не может быть пустым", "Ошибка");
                return;
            }

            if (ProductTypeBox.SelectedItem is not ProductType selectedType)
            {
                await MessageBox.Show(this, "Выберите тип продукта", "Ошибка");
                return;
            }

            try
            {
                using var context = new User6Context();

                if (!IsEditMode || (_product.ArticleNumber != ArticleNumberBox.Text))
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
                    if (!Directory.Exists(imagesDir))
                        Directory.CreateDirectory(imagesDir);

                    var ext = Path.GetExtension(_imagePath);
                    var newFileName = $"{Guid.NewGuid()}{ext}";
                    var destPath = Path.Combine(imagesDir, newFileName);
                    File.Copy(_imagePath, destPath, true);
                    imageRelativePath = Path.Combine("Images", newFileName);
                }

                if (IsEditMode)
                {
                    var dbProduct = await context.Products
                        .Include(p => p.ProductMaterials)
                        .FirstOrDefaultAsync(p => p.Id == _product.Id);

                    if (dbProduct != null)
                    {
                        dbProduct.ArticleNumber = ArticleNumberBox.Text;
                        dbProduct.Title = TitleBox.Text;
                        dbProduct.Description = DescriptionBox.Text;
                        dbProduct.ProductionPersonCount = (int)PersonCountBox.Value;
                        dbProduct.ProductionWorkshopNumber = (int)WorkshopNumberBox.Value;
                        dbProduct.MinCostForAgent = MinCostBox.Value.HasValue
                            ? Convert.ToDecimal(MinCostBox.Value.Value)
                            : (decimal?)null;
                        dbProduct.ProductTypeId = selectedType.Id;
                        if (imageRelativePath != null)
                            dbProduct.Image = imageRelativePath;

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
                }
                else
                {
                    var newProduct = new Product
                    {
                        ArticleNumber = ArticleNumberBox.Text,
                        Title = TitleBox.Text,
                        Description = DescriptionBox.Text,
                        ProductionPersonCount = (int)PersonCountBox.Value,
                        ProductionWorkshopNumber = (int)WorkshopNumberBox.Value,
                        MinCostForAgent = MinCostBox.Value.HasValue
                            ? Convert.ToDecimal(MinCostBox.Value.Value)
                            : (decimal?)null,
                        ProductTypeId = selectedType.Id,
                        Image = imageRelativePath
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

                Close(true);
            }
            catch (Exception ex)
            {
                await MessageBox.Show(this, $"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var context = new User6Context();
                bool hasSales = await context.ProductSales.AnyAsync(ps => ps.ProductId == _product.Id);
                if (hasSales)
                {
                    await MessageBox.Show(this, "Невозможно удалить продукт, так как есть информация о его продажах", "Ошибка удаления");
                    return;
                }

                var productToDelete = await context.Products
                    .Include(p => p.ProductMaterials)
                    .Include(p => p.ProductCostHistories)
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
        
        private void MaterialsGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && 
                e.Column.Header.ToString() == "Количество" &&
                e.EditingElement is TextBox textBox)
            {
                if (!double.TryParse(textBox.Text, out double value) || value <= 0)
                {
                    e.Cancel = true;
                    MessageBox.Show(this, "Введите положительное числовое значение", "Ошибка");
                }
            }
        }
        
    }
}