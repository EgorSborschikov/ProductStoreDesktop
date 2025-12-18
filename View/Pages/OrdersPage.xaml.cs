using ProductStoreDesktop.Models;
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
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow?.CurrentUser == null)
                {
                    MessageBox.Show("Для просмотра заказов необходимо войти в аккаунт.");
                    return;
                }

                using (var db = new ProductStoreEntities()) 
                {
                    var orders = db.Orders
                                   .Where(o => o.UserId == mainWindow.CurrentUser.Id)
                                   .Select(o => new
                                   {
                                       o.Id,
                                       o.OrderDate,
                                       o.TotalAmount,
                                       StatusName = db.OrderStatuses
                                                      .Where(s => s.Id == o.Status)
                                                      .Select(s => s.StatusName)
                                                      .FirstOrDefault()
                                   })
                                   .OrderByDescending(o => o.OrderDate)
                                   .ToList();

                    OrdersListView.ItemsSource = orders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private void ViewOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is int orderId)
            {
                ShowOrderDetails(orderId);
            }
        }

        private void ShowOrderDetails(int orderId)
        {
            try
            {
                using (var db = new ProductStoreEntities()) 
                {
                    var orderItems = db.OrderItems
                                       .Where(oi => oi.OrderId == orderId)
                                       .Select(oi => new
                                       {
                                           ProductName = db.Products
                                                           .Where(p => p.Id == oi.ProductId)
                                                           .Select(p => p.Name)
                                                           .FirstOrDefault(),
                                           oi.Quantity,
                                           oi.PriceAtOrder,
                                           Total = oi.Quantity * oi.PriceAtOrder
                                       })
                                       .ToList();

                    if (!orderItems.Any())
                    {
                        MessageBox.Show("Нет данных по заказу.");
                        return;
                    }

                    // Собираем текст деталей
                    var details = $"Детали заказа №{orderId}:\n\n";
                    foreach (var item in orderItems)
                    {
                        details += $"• {item.ProductName} — {item.Quantity} шт. × {item.PriceAtOrder:C} = {item.Total:C}\n";
                    }

                    MessageBox.Show(details, "Детали заказа", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей: {ex.Message}");
            }
        }
    }
}
