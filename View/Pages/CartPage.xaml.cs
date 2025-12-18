using ProductStoreDesktop.DTOs;
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
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
            LoadCart();
        }

        private void LoadCart()
        {
            try
            {
                var cart = CartService.GetCart();

                if (cart.Count == 0)
                {
                    CartSummaryText.Text = "Ваша корзина пуста";
                    CartItemsListView.ItemsSource = null;
                    CheckoutButton.IsEnabled = false;
                    return;
                }

                List<CartItemDisplay> displayItems;
                using (var db = new ProductStoreEntities())
                {
                    var productIds = cart.Keys.ToList();
                    var products = db.Products
                                     .Where(p => productIds.Contains(p.Id))
                                     .ToList();

                    displayItems = products.Select(p => new CartItemDisplay
                    {
                        Id = p.Id,
                        Name = p.Name,
                        ImageUrl = p.ImageUrl,
                        Price = p.Price,
                        Quantity = cart[p.Id]
                    }).ToList();
                }

                // Обновляем UI
                CartItemsListView.ItemsSource = displayItems;

                int totalItems = cart.Values.Sum();
                decimal totalAmount = displayItems.Sum(i => i.TotalPrice);
                CartSummaryText.Text = $"Товаров: {cart.Count} позиций, всего {totalItems} шт. на сумму {totalAmount:C}";

                CheckoutButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}");
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int productId)
            {
                CartService.RemoveItem(productId);
                LoadCart(); // перезагружаем
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int productId)
            {
                var cart = CartService.GetCart();
                if (cart.TryGetValue(productId, out int qty) && qty > 1)
                {
                    CartService.UpdateQuantity(productId, qty - 1);
                    LoadCart();
                }
                else
                {
                    // Если 1 — удаляем
                    CartService.RemoveItem(productId);
                    LoadCart();
                }
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int productId)
            {
                var cart = CartService.GetCart();
                int newQty = cart.TryGetValue(productId, out int qty) ? qty + 1 : 1;
                CartService.UpdateQuantity(productId, newQty);
                LoadCart();
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null)
            {
                MessageBox.Show("Ошибка: главное окно недоступно.");
                return;
            }
            if (mainWindow.CurrentUser == null)
            {
                MessageBox.Show("Для оформления заказа необходимо войти в аккаунт.", "Требуется авторизация",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CartService.GetCart().Count == 0)
            {
                MessageBox.Show("Корзина пуста.");
                return;
            }

            try
            {
                // Сохраняем заказ в БД
                using (var db = new ProductStoreEntities())
                {
                    var cart = CartService.GetCart();
                    var productIds = cart.Keys.ToList();
                    var products = db.Products
                                     .Where(p => productIds.Contains(p.Id))
                                     .ToList();

                    // Создаём заказ
                    var order = new Orders
                    {
                        UserId = mainWindow.CurrentUser.Id,
                        OrderDate = DateTime.Now,
                        TotalAmount = products.Sum(p => p.Price * cart[p.Id]),
                        Status = 1 // "В обработке" — ID из OrderStatuses
                    };

                    db.Orders.Add(order);
                    db.SaveChanges(); // Получаем Id заказа

                    // Добавляем позиции заказа
                    var orderItems = new List<OrderItems>();
                    foreach (var productId in cart.Keys)
                    {
                        var product = products.First(p => p.Id == productId);
                        orderItems.Add(new OrderItems
                        {
                            OrderId = order.Id,
                            ProductId = productId,
                            Quantity = cart[productId],
                            PriceAtOrder = product.Price
                        });
                    }

                    db.OrderItems.AddRange(orderItems);
                    db.SaveChanges();

                    // Очищаем корзину
                    CartService.Clear();

                    MessageBox.Show($"Заказ №{order.Id} успешно оформлен!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadCart(); // обновляем UI
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
