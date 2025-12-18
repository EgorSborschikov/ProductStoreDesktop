using ProductStoreDesktop.Models;
using ProductStoreDesktop.View.Pages;
using ProductStoreDesktop.View.Windows;
using System.Linq;
using System.Windows;

namespace ProductStoreDesktop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Models.Users CurrentUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadSavedSession();
            NavigateToShop_Click(null, null);
            UpdateAuthUI();
        }

        private void NavigateToShop_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ShopPage());
        }

        private void NavigateToCart_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CartPage());
        }

        private void NavigateToOrders_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null)
                MainFrame.Navigate(new OrdersPage());
            else
                MessageBox.Show("Пожалуйста, войдите в аккаунт для просмотра заказов.");
        }

        private void OpenLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                // Успешный вход — CurrentUser уже установлен в LoginWindow
                LoadSavedSession(); // или просто обнови UI
                UpdateAuthUI();
            }
        }

        private void OpenRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog(); // регистрация не требует немедленного входа
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser = null;
            SaveSession(null);
            UpdateAuthUI();
            NavigateToShop_Click(null, null); // возвращаем в магазин
        }

        private void UpdateAuthUI()
        {
            if (CurrentUser != null)
            {
                AuthPanel.Visibility = Visibility.Collapsed;
                ProfilePanel.Visibility = Visibility.Visible;
                UserNameText.Text = CurrentUser.Username;
                OrdersButton.IsEnabled = true;
            }
            else
            {
                AuthPanel.Visibility = Visibility.Visible;
                ProfilePanel.Visibility = Visibility.Collapsed;
                OrdersButton.IsEnabled = false;
            }
        }

        private void LoadSavedSession()
        {
            string savedUserId = Properties.Settings.Default.SavedUserId;
            if (!string.IsNullOrEmpty(savedUserId))
            {
                int userId;
                if (int.TryParse(savedUserId, out userId))
                {
                    // Получаем пользователя из БД
                    using (var db = new ProductStoreEntities()) // твой ADO.NET контекст (через LINQ to SQL или Entity Framework)
                    {
                        var user = db.Users.FirstOrDefault(u => u.Id == userId);
                        if (user != null)
                            CurrentUser = user;
                    }
                }
            }
        }

        public static void SaveSession(Users user)
        {
            if (user != null)
                Properties.Settings.Default.SavedUserId = user.Id.ToString();
            else
                Properties.Settings.Default.SavedUserId = string.Empty;

            Properties.Settings.Default.Save();
        }
    }
}
