using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using demoModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace demo_14_4
{
    public partial class ProductEditWindow : Window, INotifyPropertyChanged
    {
        private readonly bool _isAdmin;
        private readonly MydatabaseContext _context = new MydatabaseContext();
        private readonly Product? _originalProduct;
        private bool _isWindowActive = true;
        public static bool IsAnyWindowOpen { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<ProductMaterialPresenter> _materials = new();
        public ObservableCollection<ProductMaterialPresenter> Materials
        {
            get => _materials;
            set
            {
                _materials = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _productTypes = new();
        public ObservableCollection<string> ProductTypes
        {
            get => _productTypes;
            set
            {
                _productTypes = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Material> _availableMaterials = new();
        public ObservableCollection<Material> AvailableMaterials
        {
            get => _availableMaterials;
            set
            {
                _availableMaterials = value;
                OnPropertyChanged();
            }
        }

        private string _articleNumber = string.Empty;
        public string ArticleNumber
        {
            get => _articleNumber;
            set
            {
                _articleNumber = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private string _productTitle = string.Empty;
        public string ProductTitle
        {
            get => _productTitle;
            set
            {
                _productTitle = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private string _selectedProductType = string.Empty;
        public string SelectedProductType
        {
            get => _selectedProductType;
            set
            {
                _selectedProductType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private int _productionPersonCount;
        public int ProductionPersonCount
        {
            get => _productionPersonCount;
            set
            {
                _productionPersonCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private int _productionWorkshopNumber;
        public int ProductionWorkshopNumber
        {
            get => _productionWorkshopNumber;
            set
            {
                _productionWorkshopNumber = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private decimal _minCostForAgent;
        public decimal MinCostForAgent
        {
            get => _minCostForAgent;
            set
            {
                _minCostForAgent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Bitmap> _productImages = new();
        public ObservableCollection<Bitmap> ProductImages
        {
            get => _productImages;
            set
            {
                _productImages = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _imagePaths = new();
        public ObservableCollection<string> ImagePaths
        {
            get => _imagePaths;
            set
            {
                _imagePaths = value;
                OnPropertyChanged();
            }
        }

        public bool CanSave
        {
            get
            {
                if (!_isAdmin) return false; // Только админ может сохранять
                if (string.IsNullOrWhiteSpace(ArticleNumber)) return false;
                if (string.IsNullOrWhiteSpace(ProductTitle)) return false;
                if (string.IsNullOrWhiteSpace(SelectedProductType)) return false;
                if (ProductionPersonCount <= 0) return false;
                if (ProductionWorkshopNumber <= 0) return false;
                if (MinCostForAgent < 0) return false;

                if (_originalProduct == null)
                {
                    return !_context.Products.Any(p => p.ArticleNumber == ArticleNumber);
                }
                else
                {
                    return _originalProduct.ArticleNumber.Equals(ArticleNumber) ||
                           !_context.Products.Any(p => p.ArticleNumber == ArticleNumber &&
                                                     p.Id != _originalProduct.Id);
                }
            }
        }

        public ProductEditWindow()
        : this(false)  // или true, если нужен админ по умолчанию
        {
        }

        public ProductEditWindow(bool isAdmin)
        {
            _isAdmin = isAdmin;
            InitializeComponent();
            DataContext = this;
            LoadData();
            SetupUIForAdmin();
        }

        public ProductEditWindow(Product product, bool isAdmin) : this(isAdmin)
        {
            _originalProduct = product;
            Title = "Редактирование продукции";
            LoadProductData(product);
        }

        private void SetupUIForAdmin()
        {
            // Отключаем элементы управления для не-администраторов
            var saveButton = this.FindControl<Button>("SaveButton");
            var cancelButton = this.FindControl<Button>("CancelButton");
            var addImageButton = this.FindControl<Button>("AddImageButton");
            var addMaterialButton = this.FindControl<Button>("AddMaterialButton");

            if (!_isAdmin)
            {
                if (saveButton != null) saveButton.IsVisible = false;
                if (cancelButton != null) cancelButton.Content = "Закрыть";
                if (addImageButton != null) addImageButton.IsEnabled = false;
                if (addMaterialButton != null) addMaterialButton.IsEnabled = false;

                // Делаем поля только для чтения
                SetControlsReadOnly(true);
            }
        }

        private void SetControlsReadOnly(bool isReadOnly)
        {
            var articleNumberBox = this.FindControl<TextBox>("ArticleNumberBox");
            var titleBox = this.FindControl<TextBox>("TitleBox");
            var productTypeCombo = this.FindControl<ComboBox>("ProductTypeCombo");
            var personCountBox = this.FindControl<NumericUpDown>("PersonCountBox");
            var workshopNumberBox = this.FindControl<NumericUpDown>("WorkshopNumberBox");
            var costBox = this.FindControl<NumericUpDown>("CostBox");
            var descriptionBox = this.FindControl<TextBox>("DescriptionBox");

            if (articleNumberBox != null) articleNumberBox.IsReadOnly = isReadOnly;
            if (titleBox != null) titleBox.IsReadOnly = isReadOnly;
            if (productTypeCombo != null) productTypeCombo.IsEnabled = !isReadOnly;
            if (personCountBox != null) personCountBox.IsEnabled = !isReadOnly;
            if (workshopNumberBox != null) workshopNumberBox.IsEnabled = !isReadOnly;
            if (costBox != null) costBox.IsEnabled = !isReadOnly;
            if (descriptionBox != null) descriptionBox.IsReadOnly = isReadOnly;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void LoadData()
        {
            ProductTypes = new ObservableCollection<string>(_context.ProductTypes.Select(pt => pt.Title).ToList());
            AvailableMaterials = new ObservableCollection<Material>(_context.Materials.ToList());
        }

        private void LoadProductData(Product product)
        {
            ArticleNumber = product.ArticleNumber;
            ProductTitle = product.Title;
            SelectedProductType = product.ProductType?.Title ?? string.Empty;
            ProductionPersonCount = product.ProductionPersonCount ?? 1;
            ProductionWorkshopNumber = product.ProductionWorkshopNumber ?? 0;
            MinCostForAgent = product.MinCostForAgent;
            Description = product.Description ?? string.Empty;

            ProductImages.Clear();
            ImagePaths.Clear();

            if (product.ProductImages != null && product.ProductImages.Any())
            {
                foreach (var image in product.ProductImages)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(image.Imagepath))
                        {
                            var bitmap = new Bitmap(image.Imagepath);
                            ProductImages.Add(bitmap);
                            ImagePaths.Add(image.Imagepath);
                        }
                    }
                    catch { /* игнорируем невалидные изображения */ }
                }
            }

            Materials = new ObservableCollection<ProductMaterialPresenter>(
                product.ProductMaterials.Select(pm => new ProductMaterialPresenter
                {
                    Material = pm.Material,
                    Count = (pm.Count ?? 0)
                }).ToList());
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            IsAnyWindowOpen = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            IsAnyWindowOpen = false;
            _context.Dispose();
        }

        private async void SelectImages_Click(object sender, RoutedEventArgs e)
        {
            if (!_isWindowActive || !_isAdmin) return;

            try
            {
                _isWindowActive = false;

                var topLevel = TopLevel.GetTopLevel(this);
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
                {
                    Title = "Выберите изображения",
                    AllowMultiple = true,
                    FileTypeFilter = new[] {
                        new FilePickerFileType("Изображения") {
                            Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" }
                        }
                    }
                });

                if (files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.TryGetLocalPath() is { } filePath)
                        {
                            try
                            {
                                var bitmap = new Bitmap(filePath);
                                ProductImages.Add(bitmap);
                                ImagePaths.Add(filePath);
                            }
                            catch { /* игнорируем невалидные изображения */ }
                        }
                    }
                }
            }
            finally
            {
                _isWindowActive = true;
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin) return;

            if (sender is Button button && button.DataContext is Bitmap image)
            {
                var index = ProductImages.IndexOf(image);
                if (index >= 0)
                {
                    ProductImages.RemoveAt(index);
                    ImagePaths.RemoveAt(index);
                }
            }
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin) return;

            var materialComboBox = this.FindControl<ComboBox>("MaterialComboBox");
            var countTextBox = this.FindControl<TextBox>("MaterialCountTextBox");

            if (materialComboBox.SelectedItem is Material selectedMaterial &&
                int.TryParse(countTextBox.Text, out var count) && count > 0)
            {
                if (Materials.Any(m => m.Material.Id == selectedMaterial.Id))
                {
                    var existing = Materials.First(m => m.Material.Id == selectedMaterial.Id);
                    existing.Count += count;
                }
                else
                {
                    Materials.Add(new ProductMaterialPresenter
                    {
                        Material = selectedMaterial,
                        Count = count
                    });
                }

                countTextBox.Text = "1";
            }
        }

        private void RemoveMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin) return;

            if (sender is Button button && button.DataContext is ProductMaterialPresenter material)
            {
                Materials.Remove(material);
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave) return;

            try
            {
                Product product;

                if (_originalProduct == null)
                {
                    product = new Product
                    {
                        ArticleNumber = ArticleNumber,
                        Title = ProductTitle,
                        ProductType = _context.ProductTypes.FirstOrDefault(pt => pt.Title == SelectedProductType),
                        ProductionPersonCount = ProductionPersonCount,
                        ProductionWorkshopNumber = ProductionWorkshopNumber,
                        MinCostForAgent = MinCostForAgent,
                        Description = Description,
                        ProductImages = new List<ProductImage>()
                    };
                    _context.Products.Add(product);
                }
                else
                {
                    product = _context.Products
                        .Include(p => p.ProductMaterials)
                        .Include(p => p.ProductImages)
                        .FirstOrDefault(p => p.Id == _originalProduct.Id);

                    if (product == null) throw new Exception("Продукт не найден в базе данных");

                    product.ArticleNumber = ArticleNumber;
                    product.Title = ProductTitle;
                    product.ProductType = _context.ProductTypes.FirstOrDefault(pt => pt.Title == SelectedProductType);
                    product.ProductionPersonCount = ProductionPersonCount;
                    product.ProductionWorkshopNumber = ProductionWorkshopNumber;
                    product.MinCostForAgent = MinCostForAgent;
                    product.Description = Description;
                }

                await _context.SaveChangesAsync();

                if (_originalProduct != null)
                {
                    var materialsToRemove = product.ProductMaterials
                        .Where(pm => !Materials.Any(m => m.Material.Id == pm.MaterialId))
                        .ToList();

                    foreach (var material in materialsToRemove)
                    {
                        _context.ProductMaterials.Remove(material);
                    }
                }

                foreach (var material in Materials)
                {
                    var existingMaterial = product.ProductMaterials?
                        .FirstOrDefault(pm => pm.MaterialId == material.Material.Id);

                    if (existingMaterial != null)
                    {
                        existingMaterial.Count = material.Count;
                    }
                    else
                    {
                        _context.ProductMaterials.Add(new ProductMaterial
                        {
                            ProductId = product.Id,
                            MaterialId = material.Material.Id,
                            Count = material.Count
                        });
                    }
                }

                if (product.Id > 0)
                {
                    var imagesToRemove = product.ProductImages
                        .Where(pi => !ImagePaths.Contains(pi.Imagepath))
                        .ToList();

                    foreach (var image in imagesToRemove)
                    {
                        _context.ProductImages.Remove(image);
                    }

                    foreach (var path in ImagePaths)
                    {
                        if (!product.ProductImages.Any(pi => pi.Imagepath == path))
                        {
                            product.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                Imagepath = path
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                Close(true);
            }
            catch (Exception ex)
            {
                var dialog = new MessageBox("Ошибка при сохранении", ex.Message);
                await dialog.ShowDialog(this);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProductMaterialPresenter : INotifyPropertyChanged
    {
        private Material _material = null!;
        public Material Material
        {
            get => _material;
            set
            {
                _material = value;
                OnPropertyChanged();
            }
        }

        private double _count;
        public double Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}