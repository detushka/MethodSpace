using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class MessagesPage : Page
    {
        private SQL _context;
        private int _userId;
        private string _userRole;
        private Message _selectedMessage;
        private StackPanel _detailStackPanel;

        public MessagesPage(int userId, string userRole)
        {
            InitializeComponent();
            _context = new SQL();
            _userId = userId;
            _userRole = userRole;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMessages();
        }

        private void LoadMessages()
        {
            var messages = _context.Messages
                .Where(m => (_userRole == "admin" || _userRole == "methodist") ?
                            m.MessageType == "message_to_admin" || m.MessageType == "consultation_request" :
                            m.SenderID == _userId)
                .Select(m => new
                {
                    m.MessageID,
                    SenderName = m.User != null ? m.User.FullName : "Неизвестно",
                    m.Subject,
                    m.MessageText,
                    m.SentDate,
                    m.IsAnswered,
                    m.AnswerText,
                    m.AnswerDate,
                    m.ConsultationDate,
                    m.Status,
                    m.MessageType,
                    StatusDisplay = m.IsAnswered == true ? "Отвечено" : (m.MessageType == "consultation_request" ? "Запрос на консультацию" : "Новое")
                })
                .OrderByDescending(m => m.SentDate)
                .ToList();

            MessagesList.ItemsSource = messages;
        }

        private void Message_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (MessagesList.SelectedItem == null) return;

            dynamic selected = MessagesList.SelectedItem;
            _selectedMessage = _context.Messages.Find(selected.MessageID);

            if (_selectedMessage != null)
            {
                // Показываем панель деталей
                MessageDetailPanel.Visibility = Visibility.Visible;

                // Очищаем старый контент
                MessageDetailPanel.Child = null;

                // Создаем новый StackPanel для контента
                _detailStackPanel = new StackPanel();

                // Заголовок
                var subjectText = new TextBlock
                {
                    Text = _selectedMessage.Subject,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                _detailStackPanel.Children.Add(subjectText);

                // От кого и дата
                var fromText = new TextBlock
                {
                    Text = $"От: {(_selectedMessage.User?.FullName ?? "Неизвестно")} | Дата: {_selectedMessage.SentDate:dd.MM.yyyy HH:mm}",
                    Foreground = (Brush)new BrushConverter().ConvertFrom("#7F8C8D"),
                    Margin = new Thickness(0, 0, 0, 10)
                };
                _detailStackPanel.Children.Add(fromText);

                // Текст сообщения
                var messageText = new TextBlock
                {
                    Text = _selectedMessage.MessageText,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                _detailStackPanel.Children.Add(messageText);

                // Если это запрос на консультацию, показываем дату
                if (_selectedMessage.MessageType == "consultation_request" && _selectedMessage.ConsultationDate.HasValue)
                {
                    var consultationText = new TextBlock
                    {
                        Text = $"Запрошенная дата консультации: {_selectedMessage.ConsultationDate.Value:dd.MM.yyyy}",
                        Margin = new Thickness(0, 0, 0, 15),
                        Foreground = (Brush)new BrushConverter().ConvertFrom("#E67E22")
                    };
                    _detailStackPanel.Children.Add(consultationText);
                }

                // Если есть ответ, показываем его
                if (_selectedMessage.IsAnswered == true && !string.IsNullOrEmpty(_selectedMessage.AnswerText))
                {
                    var answerText = new TextBlock
                    {
                        Text = $"Ответ:\n{_selectedMessage.AnswerText}\nДата ответа: {_selectedMessage.AnswerDate:dd.MM.yyyy HH:mm}",
                        Margin = new Thickness(0, 15, 0, 0),
                        Foreground = Brushes.Green
                    };
                    _detailStackPanel.Children.Add(answerText);
                }

                // Панель для ответа (только для админа/методиста на неотвеченные сообщения)
                if ((_userRole == "admin" || _userRole == "methodist") && _selectedMessage.IsAnswered == false)
                {
                    var answerLabel = new TextBlock { Text = "Ответ:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 15, 0, 5) };
                    _detailStackPanel.Children.Add(answerLabel);

                    var answerBox = new TextBox { Height = 100, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };
                    answerBox.Name = "AnswerBox";
                    _detailStackPanel.Children.Add(answerBox);

                    var sendButton = new Button { Content = "Отправить ответ", Width = 150, Height = 35, HorizontalAlignment = HorizontalAlignment.Right };
                    sendButton.Click += (s, args) => SendAnswer_Click(answerBox);
                    _detailStackPanel.Children.Add(sendButton);
                }

                MessageDetailPanel.Child = _detailStackPanel;
            }
        }

        private void SendAnswer_Click(TextBox answerBox)
        {
            if (string.IsNullOrWhiteSpace(answerBox.Text))
            {
                System.Windows.MessageBox.Show("Введите ответ", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _selectedMessage.IsAnswered = true;
                _selectedMessage.AnswerText = answerBox.Text;
                _selectedMessage.AnswerDate = DateTime.Now;
                _context.SaveChanges();

                // Создаем уведомление для отправителя
                var notification = new Notification
                {
                    UserID = _selectedMessage.SenderID,
                    Title = "Ответ на ваше сообщение",
                    Message = $"На ваше сообщение \"{_selectedMessage.Subject}\" получен ответ",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    EventType = "message"
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();

                System.Windows.MessageBox.Show("Ответ отправлен", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем список и скрываем детали
                LoadMessages();
                MessageDetailPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}