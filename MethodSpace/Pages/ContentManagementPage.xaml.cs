using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class ContentManagementPage : Page
    {
        private readonly SQL _context;

        public ContentManagementPage(int userId, string userRole)
        {
            InitializeComponent();
            _context = new SQL();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadNews();
            LoadDocuments();
            LoadCourses();
        }

        private void LoadNews()
        {
            NewsList.ItemsSource = _context.News
                .Select(item => new
                {
                    item.NewsID,
                    item.Title,
                    item.PublishDate,
                    AuthorName = item.User != null ? item.User.FullName : "Неизвестно"
                })
                .OrderByDescending(item => item.PublishDate)
                .ToList();
        }

        private void LoadDocuments()
        {
            DocumentsList.ItemsSource = _context.Documents
                .Select(item => new
                {
                    item.DocumentID,
                    item.Title,
                    DocumentType = GetDocumentTypeDisplay(item.DocumentType),
                    item.UploadDate
                })
                .OrderByDescending(item => item.UploadDate)
                .ToList();
        }

        private void LoadCourses()
        {
            CoursesList.ItemsSource = _context.Courses
                .Select(item => new
                {
                    item.CourseID,
                    item.CourseName,
                    item.StartDate,
                    item.EndDate
                })
                .OrderBy(item => item.StartDate)
                .ToList();
        }

        private void EditNews_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Tag == null)
            {
                return;
            }

            News news = _context.News.Find((int)button.Tag);
            if (news == null)
            {
                return;
            }

            Window dialog = new Window
            {
                Title = "Редактировать новость",
                Width = 520,
                Height = 430,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = (System.Windows.Media.Brush)FindResource("AppBackgroundBrush")
            };

            StackPanel content = new StackPanel { Margin = new Thickness(18) };
            TextBox titleBox = new TextBox { Margin = new Thickness(0, 0, 0, 14), Text = news.Title };
            TextBox bodyBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 14),
                Height = 180,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Text = news.Content
            };
            CheckBox importantBox = new CheckBox
            {
                Content = "Важная новость",
                IsChecked = news.IsImportant == true
            };

            content.Children.Add(CreateLabel("Заголовок"));
            content.Children.Add(titleBox);
            content.Children.Add(CreateLabel("Текст новости"));
            content.Children.Add(bodyBox);
            content.Children.Add(importantBox);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
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
                Content = "Сохранить",
                Width = 110,
                Height = 38,
                Style = FindResource("SuccessButtonStyle") as Style
            };

            cancelButton.Click += (buttonSender, args) => dialog.Close();
            saveButton.Click += (buttonSender, args) =>
            {
                if (string.IsNullOrWhiteSpace(titleBox.Text) || string.IsNullOrWhiteSpace(bodyBox.Text))
                {
                    MessageBox.Show("Заполните заголовок и текст новости.", "Проверка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                news.Title = titleBox.Text.Trim();
                news.Content = bodyBox.Text.Trim();
                news.IsImportant = importantBox.IsChecked == true;
                news.PublishDate = DateTime.Now;

                _context.SaveChanges();
                _context.RecordActivity("Отредактирована новость «" + news.Title + "»");
                LoadNews();
                dialog.Close();
            };

            buttons.Children.Add(cancelButton);
            buttons.Children.Add(saveButton);
            content.Children.Add(buttons);

            dialog.Content = content;
            dialog.ShowDialog();
        }

        private void DeleteNews_Click(object sender, RoutedEventArgs e)
        {
            DeleteEntity(sender as Button, "Удалить новость?", () =>
            {
                News news = _context.News.Find((int)((Button)sender).Tag);
                if (news == null)
                {
                    return false;
                }

                _context.News.Remove(news);
                _context.SaveChanges();
                _context.RecordActivity("Удалена новость «" + news.Title + "»");
                LoadNews();
                return true;
            }, "Новость удалена.");
        }

        private void DeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            DeleteEntity(sender as Button, "Удалить документ?", () =>
            {
                Document document = _context.Documents.Find((int)((Button)sender).Tag);
                if (document == null)
                {
                    return false;
                }

                _context.Documents.Remove(document);
                _context.SaveChanges();
                _context.RecordActivity("Удалён документ «" + document.Title + "»");
                LoadDocuments();
                return true;
            }, "Документ удалён.");
        }

        private void DeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            DeleteEntity(sender as Button, "Удалить курс?", () =>
            {
                Cours course = _context.Courses.Find((int)((Button)sender).Tag);
                if (course == null)
                {
                    return false;
                }

                _context.Courses.Remove(course);
                _context.SaveChanges();
                _context.RecordActivity("Удалён курс «" + course.CourseName + "»");
                LoadCourses();
                return true;
            }, "Курс удалён.");
        }

        private void DeleteEntity(Button button, string question, Func<bool> deleteAction, string successMessage)
        {
            if (button == null || button.Tag == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(question, "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes && deleteAction())
            {
                MessageBox.Show(successMessage, "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

        private string GetDocumentTypeDisplay(string documentType)
        {
            switch (documentType)
            {
                case "working_program":
                    return "Рабочая программа";
                case "methodical_recommendation":
                    return "Методические рекомендации";
                case "regulation":
                    return "Нормативный документ";
                case "instruction":
                    return "Инструкция";
                default:
                    return documentType;
            }
        }
    }
}
