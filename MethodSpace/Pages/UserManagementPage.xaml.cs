using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class UserManagementPage : Page
    {
        private readonly SQL _context;
        private readonly int _currentUserId;

        public UserManagementPage(int userId, string userRole)
        {
            InitializeComponent();
            _context = new SQL();
            _currentUserId = userId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            UsersList.ItemsSource = _context.Users
                .Select(user => new
                {
                    user.UserID,
                    user.FullName,
                    user.Email,
                    RoleDisplay = GetRoleDisplayName(user.Role),
                    StatusDisplay = user.IsActive ? "Активен" : "Заблокирован",
                    ToggleLabel = user.IsActive ? "🔒" : "🔓"
                })
                .OrderBy(user => user.FullName)
                .ToList();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            ShowUserDialog("Добавить пользователя", null);
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Tag == null)
            {
                return;
            }

            User user = _context.Users.Find((int)button.Tag);
            if (user == null)
            {
                return;
            }

            ShowUserDialog("Редактировать пользователя", user);
        }

        private void ToggleUserStatus_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Tag == null)
            {
                return;
            }

            int userId = (int)button.Tag;
            User user = _context.Users.Find(userId);
            if (user == null)
            {
                return;
            }

            if (user.UserID == _currentUserId)
            {
                MessageBox.Show("Нельзя изменить статус текущего пользователя.", "Ограничение",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            user.IsActive = !user.IsActive;
            _context.SaveChanges();
            _context.RecordActivity("Изменён статус пользователя " + user.FullName);
            LoadUsers();

            MessageBox.Show(
                string.Format("Пользователь {0}.", user.IsActive ? "активирован" : "заблокирован"),
                "Готово",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void ShowUserDialog(string title, User existingUser)
        {
            Window dialog = new Window
            {
                Title = title,
                Width = 430,
                Height = 470,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = (System.Windows.Media.Brush)FindResource("AppBackgroundBrush")
            };

            StackPanel content = new StackPanel { Margin = new Thickness(18) };

            TextBox nameBox = new TextBox { Margin = new Thickness(0, 0, 0, 14) };
            TextBox emailBox = new TextBox { Margin = new Thickness(0, 0, 0, 14) };
            PasswordBox passwordBox = new PasswordBox { Margin = new Thickness(0, 0, 0, 14) };
            ComboBox roleBox = new ComboBox { Margin = new Thickness(0, 0, 0, 18) };

            roleBox.Items.Add(new ComboBoxItem { Content = "Преподаватель", Tag = "teacher" });
            roleBox.Items.Add(new ComboBoxItem { Content = "Методист", Tag = "methodist" });
            roleBox.Items.Add(new ComboBoxItem { Content = "Администратор", Tag = "admin" });
            roleBox.SelectedIndex = 0;

            if (existingUser != null)
            {
                nameBox.Text = existingUser.FullName;
                emailBox.Text = existingUser.Email;
                passwordBox.Password = existingUser.Password;

                foreach (ComboBoxItem item in roleBox.Items)
                {
                    if ((string)item.Tag == existingUser.Role)
                    {
                        roleBox.SelectedItem = item;
                        break;
                    }
                }
            }

            content.Children.Add(CreateLabel("ФИО"));
            content.Children.Add(nameBox);
            content.Children.Add(CreateLabel("Email"));
            content.Children.Add(emailBox);
            content.Children.Add(CreateLabel("Пароль"));
            content.Children.Add(passwordBox);
            content.Children.Add(CreateLabel("Роль"));
            content.Children.Add(roleBox);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button cancelButton = new Button
            {
                Content = "Отмена",
                Width = 110,
                Height = 38,
                Margin = new Thickness(0, 0, 10, 0),
                Style = FindResource("ActionButtonStyle") as Style
            };

            Button saveButton = new Button
            {
                Content = existingUser == null ? "Добавить" : "Сохранить",
                Width = 110,
                Height = 38,
                Style = FindResource("SuccessButtonStyle") as Style
            };

            cancelButton.Click += (sender, args) => dialog.Close();
            saveButton.Click += (sender, args) =>
            {
                string email = emailBox.Text.Trim();
                string password = passwordBox.Password.Trim();
                string fullName = nameBox.Text.Trim();
                string role = ((ComboBoxItem)roleBox.SelectedItem).Tag as string;

                if (string.IsNullOrWhiteSpace(fullName) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(role))
                {
                    MessageBox.Show("Заполните все поля.", "Проверка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool emailExists = _context.Users.Any(user =>
                    user.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                    (existingUser == null || user.UserID != existingUser.UserID));

                if (emailExists)
                {
                    MessageBox.Show("Пользователь с таким email уже существует.", "Проверка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (existingUser == null)
                {
                    _context.Users.Add(new User
                    {
                        FullName = fullName,
                        Email = email,
                        Password = password,
                        Role = role,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });

                    _context.RecordActivity("Добавлен новый пользователь " + fullName);
                }
                else
                {
                    existingUser.FullName = fullName;
                    existingUser.Email = email;
                    existingUser.Password = password;
                    existingUser.Role = role;
                    _context.RecordActivity("Обновлены данные пользователя " + fullName);
                }

                _context.SaveChanges();
                LoadUsers();
                dialog.Close();
            };

            buttons.Children.Add(cancelButton);
            buttons.Children.Add(saveButton);
            content.Children.Add(buttons);

            dialog.Content = content;
            dialog.ShowDialog();
        }

        private TextBlock CreateLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 6)
            };
        }

        private string GetRoleDisplayName(string role)
        {
            switch (role)
            {
                case "admin":
                    return "Администратор";
                case "methodist":
                    return "Методист";
                case "teacher":
                    return "Преподаватель";
                default:
                    return role;
            }
        }
    }
}
