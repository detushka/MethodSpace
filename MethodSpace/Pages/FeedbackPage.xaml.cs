using System;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class FeedbackPage : Page
    {
        private readonly SQL _context;
        private readonly int _userId;
        private readonly string _userRole;
        private DateTime? _selectedDate;

        public FeedbackPage(int userId, string userRole)
        {
            _context = new SQL();
            InitializeComponent();
            _userId = userId;
            _userRole = userRole;
        }

        private void MessageType_Changed(object sender, RoutedEventArgs e)
        {
            if (ConsultationRequest != null && ConsultationDatePanel != null)
            {
                ConsultationDatePanel.Visibility = ConsultationRequest.IsChecked == true
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void SelectDate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = "Выберите дату консультации",
                Width = 360,
                Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = Application.Current.TryFindResource("AppBackgroundBrush") as System.Windows.Media.Brush
            };

            var outerBorder = new Border
            {
                Margin = new Thickness(16),
                Padding = new Thickness(18),
                CornerRadius = new CornerRadius(18),
                Background = Application.Current.TryFindResource("CardBrush") as System.Windows.Media.Brush,
                BorderBrush = Application.Current.TryFindResource("CardBorderBrush") as System.Windows.Media.Brush,
                BorderThickness = new Thickness(1)
            };

            var stackPanel = new StackPanel();

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Выберите дату:",
                Margin = new Thickness(0, 0, 0, 10),
                FontWeight = FontWeights.SemiBold,
                FontSize = 14
            });

            var picker = new DatePicker { Margin = new Thickness(0, 0, 0, 20) };
            stackPanel.Children.Add(picker);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var okButton = new Button
            {
                Content = "Подтвердить",
                Width = 110,
                Margin = new Thickness(0, 0, 10, 0),
                Style = Application.Current.TryFindResource("SuccessButtonStyle") as Style
            };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 90,
                Style = Application.Current.TryFindResource("ActionButtonStyle") as Style
            };

            okButton.Click += (s, args) =>
            {
                if (picker.SelectedDate.HasValue)
                {
                    _selectedDate = picker.SelectedDate;
                    if (ConsultationDatePanel != null)
                    {
                        ConsultationDatePanel.Content = $"Дата: {_selectedDate.Value:dd.MM.yyyy}";
                    }
                }

                dialog.Close();
            };

            cancelButton.Click += (s, args) => dialog.Close();

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            outerBorder.Child = stackPanel;
            dialog.Content = outerBorder;
            dialog.ShowDialog();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectBox == null || MessageBox == null)
            {
                System.Windows.MessageBox.Show("Ошибка инициализации формы", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(SubjectBox.Text))
            {
                System.Windows.MessageBox.Show("Введите тему сообщения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(MessageBox.Text))
            {
                System.Windows.MessageBox.Show("Введите текст сообщения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ConsultationRequest != null && ConsultationRequest.IsChecked == true && !_selectedDate.HasValue)
            {
                System.Windows.MessageBox.Show("Для записи на консультацию выберите дату.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var message = new Message
                {
                    SenderID = _userId,
                    MessageType = (ConsultationRequest != null && ConsultationRequest.IsChecked == true) ? "consultation_request" : "message_to_admin",
                    Subject = SubjectBox.Text.Trim(),
                    MessageText = MessageBox.Text.Trim(),
                    SentDate = DateTime.Now,
                    IsAnswered = false,
                    Status = (ConsultationRequest != null && ConsultationRequest.IsChecked == true) ? "requested" : null
                };

                if ((ConsultationRequest != null && ConsultationRequest.IsChecked == true) && _selectedDate.HasValue)
                {
                    message.ConsultationDate = _selectedDate.Value;
                }

                _context.Messages.Add(message);
                _context.SaveChanges();

                SubjectBox.Text = string.Empty;
                MessageBox.Text = string.Empty;
                _selectedDate = null;

                if (ConsultationDatePanel != null)
                {
                    ConsultationDatePanel.Content = "Выбрать дату консультации";
                }

                _context.RecordActivity(
                    (ConsultationRequest != null && ConsultationRequest.IsChecked == true)
                        ? "Отправлен новый запрос на консультацию"
                        : "Отправлено новое сообщение администрации");

                System.Windows.MessageBox.Show("Ваше сообщение отправлено", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка при отправке: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
