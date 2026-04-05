using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MethodSpace.Pages;
using MethodSpace.Views;

namespace MethodSpace
{
    public partial class MainWindow : Window
    {
        private readonly int _currentUserId;
        private readonly string _userRole;

        public MainWindow(int userId, string userRole, string userFullName)
        {
            InitializeComponent();

            _currentUserId = userId;
            _userRole = userRole;

            UserNameText.Text = userFullName;
            UserRoleText.Text = GetRoleDisplayName(userRole);

            ConfigureMenuByRole();
            NavigateToPage(new MainPage(_currentUserId, _userRole), "main");
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
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

        private void ConfigureMenuByRole()
        {
            bool showAdminZone = false;

            switch (_userRole)
            {
                case "admin":
                    AdminPanelBtn.Visibility = Visibility.Visible;
                    UserManagementBtn.Visibility = Visibility.Visible;
                    ContentManagementBtn.Visibility = Visibility.Visible;
                    showAdminZone = true;
                    break;
                case "methodist":
                    ContentManagementBtn.Visibility = Visibility.Visible;
                    showAdminZone = true;
                    break;
                default:
                    AdminPanelBtn.Visibility = Visibility.Collapsed;
                    UserManagementBtn.Visibility = Visibility.Collapsed;
                    ContentManagementBtn.Visibility = Visibility.Collapsed;
                    break;
            }

            AdminSeparator.Visibility = showAdminZone ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NavigateToPage(Page page, string menuTag)
        {
            MainFrame.Navigate(page);
            HighlightMenu(menuTag);
        }

        private void HighlightMenu(string menuTag)
        {
            Brush activeBrush = FindResource("SidebarSecondaryBrush") as Brush;

            foreach (Button button in MenuPanel.Children.OfType<Button>())
            {
                string tag = button.Tag as string;
                button.Background = tag == menuTag ? activeBrush : Brushes.Transparent;
            }
        }

        private void NavigateToMain(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new MainPage(_currentUserId, _userRole), "main");
        }

        private void NavigateToDocuments(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new DocumentsPage(_currentUserId, _userRole), "documents");
        }

        private void NavigateToCourses(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new CoursesPage(_currentUserId, _userRole), "courses");
        }

        private void NavigateToAttestation(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new AttestationPage(_currentUserId, _userRole), "attestation");
        }

        private void NavigateToEvents(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new EventsPage(_currentUserId, _userRole), "events");
        }

        private void NavigateToTips(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new TeacherTipsPage(_currentUserId, _userRole), "tips");
        }

        private void NavigateToMessages(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new MessagesPage(_currentUserId, _userRole), "messages");
        }

        private void NavigateToFeedback(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new FeedbackPage(_currentUserId, _userRole), "feedback");
        }

        private void NavigateToSurveys(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new SurveysPage(_currentUserId, _userRole), "surveys");
        }

        private void NavigateToSuggestions(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new SuggestionsPage(_currentUserId, _userRole), "suggestions");
        }

        private void NavigateToVoting(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new VotingPage(_currentUserId, _userRole), "voting");
        }

        private void NavigateToSearch(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new SearchPage(_currentUserId, _userRole), "search");
        }

        private void NavigateToAdminPanel(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new AdminPanelPage(_currentUserId, _userRole), "admin");
        }

        private void NavigateToUserManagement(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new UserManagementPage(_currentUserId, _userRole), "user-management");
        }

        private void NavigateToContentManagement(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new ContentManagementPage(_currentUserId, _userRole), "content-management");
        }

        private void ShowNotifications(object sender, RoutedEventArgs e)
        {
            var notificationWindow = new NotificationWindow(_currentUserId);
            notificationWindow.Owner = this;
            notificationWindow.ShowDialog();
        }

        private void Logout(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
