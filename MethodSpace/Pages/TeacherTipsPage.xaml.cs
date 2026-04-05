using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;
using MethodSpace.Views;

namespace MethodSpace.Pages
{
    public partial class TeacherTipsPage : Page
    {
        private readonly SQL _context;

        public TeacherTipsPage(int userId, string userRole)
        {
            _context = new SQL();
            InitializeComponent();

            Loaded += (sender, args) => LoadTips();
        }

        private void LoadTips()
        {
            try
            {
                if (_context == null)
                {
                    return;
                }

                var query = _context.TeacherTips.AsQueryable();

                if (LessonTips != null && LessonTips.IsChecked == true)
                {
                    query = query.Where(item => item.TipType == "lesson_tip");
                }
                else if (PracticalRecs != null && PracticalRecs.IsChecked == true)
                {
                    query = query.Where(item => item.TipType == "practical_recommendation");
                }

                var tips = query
                    .OrderByDescending(item => item.PublishDate)
                    .ToList();

                if (TipsList != null)
                {
                    TipsList.ItemsSource = tips;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки советов: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            LoadTips();
        }

        private void TipsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TeacherTip tip = TipsList.SelectedItem as TeacherTip;
            if (tip == null)
            {
                return;
            }

            string subtitle = string.Format(
                "{0} | Опубликовано: {1:dd.MM.yyyy}",
                GetTipTypeDisplay(tip.TipType),
                tip.PublishDate ?? DateTime.Now);

            DetailDialogHelper.Show(
                Window.GetWindow(this),
                tip.Title,
                subtitle,
                tip.Content);
        }

        private string GetTipTypeDisplay(string tipType)
        {
            switch (tipType)
            {
                case "lesson_tip":
                    return "Совет для урока";
                case "practical_recommendation":
                    return "Практическая рекомендация";
                default:
                    return "Совет педагогу";
            }
        }
    }
}
