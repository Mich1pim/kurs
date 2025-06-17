using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using ClosedXML.Excel;
using demoModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using ClosedXML.Excel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;

namespace demo_14_4
{
    public partial class MainWindow : Window
    {
        ObservableCollection<ProductPresenter> products = new ObservableCollection<ProductPresenter>();
        ObservableCollection<string> productTypes;
        List<ProductPresenter> dataSours;

        private int currentPage = 1;
        private int itemsPerPage = 20;
        private int totalPages = 1;
        private List<ProductPresenter> filteredProducts = new List<ProductPresenter>();
        private List<ProductPresenter> selectedProducts = new List<ProductPresenter>();
        private bool isAdmin = false;
        private Button? _changeCostButton;

        public MainWindow()
        : this(false) 
        {
        }
        public MainWindow(bool isAdmin)
        {
            this.isAdmin = isAdmin;
            InitializeComponent();
            ProductBox.Tag = new Func<ProductPresenter, bool>(p => p.Images.Count() == 1);
            LoadData();
            InitializeUI();
        }

        public void LoadData()
        {
            using var context = new MydatabaseContext();

            dataSours = context.Products
                .Include(x => x.ProductMaterials)
                .ThenInclude(material => material.Material)
                .Where(it => it.ProductMaterials.Count > 0)
                .Include(type => type.ProductType)
                .Include(sale => sale.ProductSales)
                .Include(image => image.ProductImages)
                .Select(product => new ProductPresenter
                {
                    Id = product.Id,
                    Title = product.Title,
                    ProductImages = product.ProductImages,
                    ProductionWorkshopNumber = product.ProductionWorkshopNumber,
                    ArticleNumber = product.ArticleNumber,
                    MinCostForAgent = product.MinCostForAgent,
                    ProductMaterials = product.ProductMaterials,
                    ProductType = product.ProductType,
                    ProductSales = product.ProductSales
                })
                .ToList();
            
            

            var dataSourseType = context.ProductTypes.Select(it => it.Title).ToList();
            productTypes = new ObservableCollection<string>(dataSourseType);
            productTypes.Insert(0, "Все типы");
        }

        private void LogoutButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();

            loginWindow.LoginSucceeded += (_, isAdmin) =>
            {
                var newMainWindow = new MainWindow(isAdmin);

                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.MainWindow = newMainWindow;
                    newMainWindow.Show();
                }

                this.Close();
                loginWindow.Close();
            };

            loginWindow.LoginCancelled += (_, _) =>
            {
                loginWindow.Close();
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown();
                }
            };

            loginWindow.Show();

            
            this.Close();
        }

        private void InitializeUI()
        {
            ProductBox.ItemsSource = products;
            FilterBox.ItemsSource = productTypes;
            SortBox.SelectedIndex = 0;
            FilterBox.SelectedIndex = 0;
            ProductBox.SelectionChanged += ProductBox_SelectionChanged;
            var menuItem = this.FindControl<MenuItem>("DeleteMenuItem");
            DisplayProducts();



            if (!isAdmin && menuItem != null)
            {
                menuItem.IsVisible = false;
            }
            if (isAdmin)
            {
                var addButton = new Button
                {
                    Content = "Добавить продукт",
                    Margin = new Avalonia.Thickness(5)
                };
                addButton.Click += AddButton_Click;

                
                _changeCostButton = new Button
                {
                    Name = "ChangeCostButton",
                    Content = "Изменить стоимость на...",
                    Margin = new Avalonia.Thickness(5),
                    IsEnabled = false
                };
                _changeCostButton.Click += ChangeCostButton_Click;

                var toolPanel = this.FindControl<StackPanel>("ToolPanel");
                toolPanel.Children.Insert(1, addButton);
                toolPanel.Children.Insert(2, _changeCostButton);
            }

            ProductBox.SelectionChanged += (sender, e) => {
                selectedProducts = ProductBox.SelectedItems.Cast<ProductPresenter>().ToList();
                var selectedCountText = this.FindControl<TextBlock>("SelectedCountText");
                selectedCountText.Text = $"Выбрано: {selectedProducts.Count}";

                if (isAdmin)
                {
                    var changeCostButton = this.FindControl<Button>("ChangeCostButton");
                    if (changeCostButton != null)
                    {
                        changeCostButton.IsEnabled = selectedProducts.Count > 0;
                    }
                }
            };

            if (isAdmin)
            {
                ProductBox.DoubleTapped += ProductBox_DoubleTapped;
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductEditWindow.IsAnyWindowOpen) return;

            var window = new ProductEditWindow(isAdmin: true);
            var result = await window.ShowDialog<bool>(this);

            if (result)
            {
                LoadData();
                DisplayProducts();
            }
        }

        private async void ProductBox_DoubleTapped(object sender, RoutedEventArgs e)
        {
            if (ProductEditWindow.IsAnyWindowOpen || ProductBox.SelectedItem is not ProductPresenter product) return;

            var window = new ProductEditWindow(product, isAdmin: true);
            var result = await window.ShowDialog<bool>(this);

            if (result)
            {
                LoadData();
                DisplayProducts();
            }
        }

        public void ProductBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            selectedProducts = listBox.SelectedItems.Cast<ProductPresenter>().ToList();

            var selectedCountText = this.FindControl<TextBlock>("SelectedCountText");
            selectedCountText.Text = $"Выбрано: {selectedProducts.Count}";

            if (_changeCostButton != null)
            {
                _changeCostButton.IsEnabled = selectedProducts.Count > 0;
            }
        }

        public async void ChangeCostButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProducts.Count == 0) return;

            double averageCost = selectedProducts.Average(p => (double)p.MinCostForAgent);

            var dialog = new ChangeCostDialog(averageCost);
            var result = await dialog.ShowDialog<bool>(this);

            if (result)
            {
                double newCost = dialog.NewCost;

                using var context = new MydatabaseContext();
                foreach (var product in selectedProducts)
                {
                    var dbProduct = context.Products.FirstOrDefault(p => p.Id == product.Id);
                    if (dbProduct != null)
                    {
                        dbProduct.MinCostForAgent = (decimal)newCost;
                        product.MinCostForAgent = (decimal)newCost;
                    }
                }

                context.SaveChanges();

                DisplayProducts();
            }
        }

        public void ProductItemClick(object sender, RoutedEventArgs e)
        {
            if (!isAdmin) return;

            var product = ProductBox.SelectedItem as ProductPresenter;
            if (product == null) return;

            using var context = new MydatabaseContext();
            var productToDelete = context.Products
                .Include(p => p.ProductMaterials)
                .FirstOrDefault(p => p.Id == product.Id);

            if (productToDelete == null) return;

            context.ProductMaterials.RemoveRange(productToDelete.ProductMaterials);
            context.Products.Remove(productToDelete);

            if (context.SaveChanges() > 0)
            {
                products.Remove(product);
                dataSours.RemoveAll(p => p.Id == product.Id);
                DisplayProducts();
            }
        }


        private void DisplayProducts()
        {
            filteredProducts = dataSours.ToList();

            if (FilterBox.SelectedIndex > 0)
            {
                filteredProducts = filteredProducts.Where(it => it.TypeName.Contains(FilterBox.SelectedItem.ToString())).ToList();
            }

            if (!string.IsNullOrEmpty(SearchBox.Text))
            {
                var searchWord = SearchBox.Text.ToLower();
                filteredProducts = filteredProducts.Where(it => IsContains(it.Title, it.TypeName, searchWord)).ToList();
            }

            switch (SortBox.SelectedIndex)
            {
                case 1: filteredProducts = filteredProducts.OrderBy(p => p.Title).ToList(); break;
                case 2: filteredProducts = filteredProducts.OrderByDescending(p => p.Title).ToList(); break;
                case 3: filteredProducts = filteredProducts.OrderByDescending(p => p.ProductionWorkshopNumber).ToList(); break;
                case 4: filteredProducts = filteredProducts.OrderBy(p => p.ProductionWorkshopNumber).ToList(); break;
                case 5: filteredProducts = filteredProducts.OrderByDescending(p => p.MinCostForAgent).ToList(); break;
                case 6: filteredProducts = filteredProducts.OrderBy(p => p.MinCostForAgent).ToList(); break;
                default: break;
            }

            totalPages = (int)Math.Ceiling((double)filteredProducts.Count / itemsPerPage);
            if (currentPage > totalPages && totalPages > 0)
            {
                currentPage = totalPages;
            }
            else if (totalPages == 0)
            {
                currentPage = 1;
            }

            UpdatePaginationButtons();
            UpdateDisplayedProducts();
        }

        private void UpdateDisplayedProducts()
        {
            products.Clear();

            if (!filteredProducts.Any())
                return;

            var productsToDisplay = filteredProducts
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            foreach (var item in productsToDisplay)
                products.Add(item);

            SaveAllProductsToExcel(filteredProducts);
        }


        private void SaveAllProductsToExcel(List<ProductPresenter> allProducts)
        {
            try
            {
                var folderPath = Path.Combine(AppContext.BaseDirectory, "Product");
                Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, "AllProducts.xlsx");
                Debug.WriteLine("Путь к файлу: " + filePath);

                XLWorkbook workbook;
                IXLWorksheet worksheet;

                if (File.Exists(filePath))
                {
                    workbook = new XLWorkbook(filePath);
                    worksheet = workbook.Worksheet("Products");
                    worksheet.Clear();
                }
                else
                {
                    workbook = new XLWorkbook();
                    worksheet = workbook.Worksheets.Add("Products");
                }

                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Название";
                worksheet.Cell(1, 3).Value = "Тип";
                worksheet.Cell(1, 4).Value = "Артикул";
                worksheet.Cell(1, 5).Value = "Цех №";
                worksheet.Cell(1, 6).Value = "Мин. стоимость для агента";

                for (int i = 0; i < allProducts.Count; i++)
                {
                    var p = allProducts[i];
                    worksheet.Cell(i + 2, 1).Value = p.Id;
                    worksheet.Cell(i + 2, 2).Value = p.Title;
                    worksheet.Cell(i + 2, 3).Value = p.TypeName;
                    worksheet.Cell(i + 2, 4).Value = p.ArticleNumber;
                    worksheet.Cell(i + 2, 5).Value = p.ProductionWorkshopNumber;
                    worksheet.Cell(i + 2, 6).Value = p.MinCostForAgent;
                }

                workbook.SaveAs(filePath);
                Debug.WriteLine("\u2705 Excel файл успешно обновлён или создан.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\u274C Ошибка при сохранении Excel: " + ex.Message);
            }
        }


        private void UpdatePaginationButtons()
        {
            PaginationPanel.Children.Clear();

            for (int i = 1; i <= totalPages; i++)
            {
                var button = new Button
                {
                    Content = i.ToString(),
                    Margin = new Avalonia.Thickness(5),
                    Width = 30,
                    Height = 30,
                    IsEnabled = i != currentPage
                };

                int page = i;
                button.Click += (s, e) =>
                {
                    currentPage = page;
                    UpdateDisplayedProducts();
                    UpdatePaginationButtons();
                };

                PaginationPanel.Children.Add(button);
            }
        }

        public bool IsContains(string title, string typeName, string searchWord)
        {
            string massage = (title + typeName).ToLower();
            searchWord = searchWord.ToLower();
            return massage.Contains(searchWord);
        }

        public void SearchBoxChanging(object sender, TextChangingEventArgs eventArgs)
        {
            currentPage = 1;
            DisplayProducts();
        }

        private void SortBox_SelectionChanged(object? sender, SelectionChangedEventArgs eventArgs)
        {
            currentPage = 1;
            DisplayProducts();
        }

        private void FilterBox_SelectionChanged(object? sender, SelectionChangedEventArgs eventArgs)
        {
            currentPage = 1;
            DisplayProducts();
        }
    }

    public class ProductPresenter() : Product
    {
        public string TypeName { get => ProductType.Title; }
        public IEnumerable<Bitmap> Images => GetImages();

        private IEnumerable<Bitmap> GetImages()
        {
            var images = new List<Bitmap>();

            if (ProductImages != null && ProductImages.Any())
            {
                foreach (var image in ProductImages)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(image.Imagepath) && File.Exists(image.Imagepath))
                        {
                            images.Add(new Bitmap(image.Imagepath));
                        }
                    }
                    catch { }
                }
            }

            if (!images.Any())
            {
                var defaultImage = LoadDefaultImage();
                if (defaultImage != null)
                {
                    images.Add(defaultImage);
                }
            }

            return images;
        }

        private static Bitmap? LoadDefaultImage()
        {
            var defaultPath = Path.Combine("Images", "picture.png");
            return File.Exists(defaultPath)
                ? new Bitmap(defaultPath)
                : null;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       

        public string BackgroundColor
        {
            get
            {
                if (!ProductSales.Any(s => s.SaleDate != null))
                {
                    return "Transparent";
                }

                var lastSaleDate = ProductSales
                    .Where(s => s.SaleDate != null)
                    .Max(s => s.SaleDate);

                if (lastSaleDate < DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)))
                {
                    return "#d12e2e";
                }

                return "Transparent";
            }
        }

        
    }
}