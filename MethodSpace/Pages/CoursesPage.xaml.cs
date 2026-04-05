using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class CoursesPage : Page
    {
        private readonly SQL _context;
        private readonly int _userId;
        private readonly string _userRole;

        public CoursesPage(int userId, string userRole)
        {
            _context = new SQL();
            InitializeComponent();
            _userId = userId;
            _userRole = userRole;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCourses();
        }

        private void LoadCourses()
        {
            var query = _context.Courses.ToList();

            var coursesWithInfo = query.Select(c => new
            {
                c.CourseID,
                c.CourseName,
                c.StartDate,
                c.EndDate,
                c.Location,
                c.MaxParticipants,
                c.CurrentParticipants,
                FreePlaces = (c.MaxParticipants - c.CurrentParticipants) ?? 0
            }).ToList();

            if (ShowOnlyActive != null && ShowOnlyActive.IsChecked == true)
            {
                coursesWithInfo = coursesWithInfo.Where(c => c.StartDate > DateTime.Now.Date).ToList();
            }

            if (CoursesList != null)
            {
                CoursesList.ItemsSource = coursesWithInfo;
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            LoadCourses();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int courseId = (int)button.Tag;

            var course = _context.Courses.Find(courseId);
            if (course != null)
            {
                var existingRegistration = _context.CourseRegistrations
                    .FirstOrDefault(r => r.CourseID == courseId && r.UserID == _userId);

                if (existingRegistration != null)
                {
                    MessageBox.Show("Вы уже записаны на этот курс", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (course.CurrentParticipants >= course.MaxParticipants)
                {
                    MessageBox.Show("Нет свободных мест на этот курс", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var registration = new CourseRegistration
                {
                    CourseID = courseId,
                    UserID = _userId,
                    RegistrationDate = DateTime.Now,
                    IsConfirmed = true
                };

                course.CurrentParticipants = (course.CurrentParticipants ?? 0) + 1;

                _context.CourseRegistrations.Add(registration);
                _context.SaveChanges();

                var notification = new Notification
                {
                    UserID = _userId,
                    Title = "Подтверждение записи на курс",
                    Message = $"Вы успешно записаны на курс \"{course.CourseName}\"",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    EventType = "course"
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();

                MessageBox.Show("Вы успешно записаны на курс!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadCourses();
            }
        }

        private void CoursesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
