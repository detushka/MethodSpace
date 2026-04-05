using System.Linq;
using System.Windows;
using System.Windows.Input;
using MethodSpace.Contex;

namespace MethodSpace.Views
{
    public partial class LoginWindow : Window
    {
        private readonly SQL _context;

        public LoginWindow()
        {
            _context = new SQL();
            InitializeComponent();

            ConfigureModeBanner();
        }

        private void ConfigureModeBanner()
        {
            
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите email и пароль.");
                return;
            }

            var user = _context.Users
                .FirstOrDefault(item => item.Email == email && item.Password == password && item.IsActive);

            if (user == null)
            {
                ShowError(_context.IsDatabaseAvailable
                    ? "Неверный email или пароль."
                    : "Не удалось войти. Проверьте данные или используйте быстрый вход ниже.");
                return;
            }

            var mainWindow = new MainWindow(user.UserID, user.Role, user.FullName);
            mainWindow.Show();
            Close();
        }

        private void UseAdmin_Click(object sender, RoutedEventArgs e)
        {
            ApplyDemoCredentials("admin@methodspace.local", "admin123");
        }

        private void UseMethodist_Click(object sender, RoutedEventArgs e)
        {
            ApplyDemoCredentials("methodist@methodspace.local", "method123");
        }

        private void UseTeacher_Click(object sender, RoutedEventArgs e)
        {
            ApplyDemoCredentials("teacher@methodspace.local", "teacher123");
        }

        private void ApplyDemoCredentials(string email, string password)
        {
            EmailBox.Text = email;
            PasswordBox.Password = password;
            ErrorText.Visibility = Visibility.Collapsed;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(LoginButton, new RoutedEventArgs());
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
