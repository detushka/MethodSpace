using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class EventsPage : Page
    {
        private SQL _context;
        private int _userId;
        private string _userRole;

        public EventsPage(int userId, string userRole)
        {
            InitializeComponent();
            _context = new SQL();
            _userId = userId;
            _userRole = userRole;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var events = _context.Events
                .Select(ev => new
                {
                    ev.EventID,
                    ev.EventName,
                    ev.EventDate,
                    ev.Location,
                    ev.Description,
                    OrganizerName = ev.User != null ? ev.User.FullName : "Неизвестно"
                })
                .OrderBy(ev => ev.EventDate)
                .ToList();

            EventsList.ItemsSource = events;
        }
    }
}