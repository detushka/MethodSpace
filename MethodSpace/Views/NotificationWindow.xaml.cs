using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MethodSpace.Contex;

namespace MethodSpace.Views
{
    public partial class NotificationWindow : Window
    {
        private readonly SQL _context;
        private readonly int _userId;

        public NotificationWindow(int userId)
        {
            InitializeComponent();
            _context = new SQL();
            _userId = userId;
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            var notifications = _context.Notifications
                .Where(item => item.UserID == _userId)
                .OrderByDescending(item => item.CreatedAt)
                .ToList();

            NotificationsList.ItemsSource = notifications;
        }

        private void NotificationsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Notification notification = NotificationsList.SelectedItem as Notification;
            if (notification == null)
            {
                return;
            }

            if (notification.IsRead == false)
            {
                notification.IsRead = true;
                _context.SaveChanges();
                LoadNotifications();
            }

            string subtitle = string.Format(
                "Тип: {0} | Дата: {1:dd.MM.yyyy HH:mm}",
                GetEventTypeDisplay(notification.EventType),
                notification.CreatedAt ?? DateTime.Now);

            DetailDialogHelper.Show(
                this,
                notification.Title,
                subtitle,
                notification.Message);
        }

        private string GetEventTypeDisplay(string eventType)
        {
            switch (eventType)
            {
                case "course":
                    return "Курс";
                case "message":
                    return "Сообщение";
                case "survey":
                    return "Опрос";
                case "vote":
                    return "Голосование";
                default:
                    return string.IsNullOrWhiteSpace(eventType) ? "Уведомление" : eventType;
            }
        }
    }
}
