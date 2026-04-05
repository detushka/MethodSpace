using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;
using MethodSpace.Views;

namespace MethodSpace.Pages
{
    public partial class MainPage : Page
    {
        private readonly SQL _context;
        private readonly int _userId;

        public MainPage(int userId, string userRole)
        {
            _context = new SQL();
            InitializeComponent();
            _userId = userId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var user = _context.Users.Find(_userId);
            if (user != null)
            {
                WelcomeText.Text = string.Format("Добро пожаловать, {0}!", user.FullName);
            }

            var news = _context.News
                .OrderByDescending(item => item.PublishDate)
                .Take(10)
                .ToList();

            NewsList.ItemsSource = news;
        }

        private void NewsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            News news = NewsList.SelectedItem as News;
            if (news == null)
            {
                return;
            }

            string subtitle = string.Format(
                "Дата публикации: {0:dd.MM.yyyy HH:mm}{1}",
                news.PublishDate ?? DateTime.Now,
                news.IsImportant == true ? " | Важная новость" : string.Empty);

            DetailDialogHelper.Show(
                Window.GetWindow(this),
                news.Title,
                subtitle,
                news.Content);
        }
    }
}
