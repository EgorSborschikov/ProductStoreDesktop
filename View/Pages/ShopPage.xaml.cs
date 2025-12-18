using ProductStoreDesktop.Models;
using ProductStoreDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProductStoreDesktop.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для ShopPage.xaml
    /// </summary>
    public partial class ShopPage : Page
    {
        public ShopPage()
        {
            InitializeComponent();
            LoadCategories();
            LoadProducts();
        }

        private void LoadCategories()
        {
            try
            {
                using (var db = new ProductStoreEntities())
                {
                    var categories = db.Categories.ToList();

                    var allItem = CategoryFilterComboBox.Items[0];

                    CategoryFilterComboBox.Items.Clear();
                    CategoryFilterComboBox.Items.Add(allItem);

                    foreach (var cat in categories)
                    {
                        CategoryFilterComboBox.Items.Add(new ComboBoxItem
                        {
                            Content = cat.Name,
                            Tag = cat.Id,
                        });
                    }

                    CategoryFilterComboBox.SelectedIndex = 0;
                }
            } 
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var db = new ProductStoreEntities())
                {
                    var products = db.Products.AsEnumerable();

                    var selectedCategory = CategoryFilterComboBox.SelectedItem as ComboBoxItem;
                    if (selectedCategory != null && selectedCategory.Tag.ToString() != "-1")
                    {
                        int catId = (int)selectedCategory.Tag;
                        products = products.Where(p => p.CategoryId == catId);
                    }

                    switch (SortComboBox.SelectedIndex)
                    {
                        case 1:
                            products = products.OrderBy(p => p.Price);
                            break;
                        case 2:
                            products = products.OrderByDescending(p => p.Price);
                            break;
                        default:
                            products = products.OrderBy(p => p.Name);
                            break;
                    }

                    var displayProducts = products.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        // 🔑 Ключевое изменение: преобразуем путь в изображение
                        ImageUrl = !string.IsNullOrEmpty(p.ImageUrl)
                            ? new BitmapImage(new Uri($"pack://application:,,,/{p.ImageUrl.Replace("\\", "/")}"))
                            : null
                    }).ToList();

                    ProductsItemsControl.ItemsSource = displayProducts.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продуктов: {ex.Message}");
            }
        }

        private void RefreshProducts_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int productId)
            {
                CartService.AddItem(productId);
                MessageBox.Show("Товар добавлен в корзину!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
        }
    }
}
