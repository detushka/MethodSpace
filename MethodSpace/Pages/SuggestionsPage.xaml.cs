using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class SuggestionsPage : Page
    {
        private SQL _context;
        private int _userId;
        private string _userRole;

        public SuggestionsPage(int userId, string userRole)
        {
            InitializeComponent();
            _context = new SQL();
            _userId = userId;
            _userRole = userRole;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void SendSuggestion_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SuggestionBox.Text))
            {
                MessageBox.Show("Введите текст предложения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var survey = _context.Surveys.FirstOrDefault(s => s.SurveyType == "suggestion" && s.IsActive == true);

                if (survey == null)
                {
                    MessageBox.Show("Форма для сбора предложений временно недоступна", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var response = new SurveyRespons
                {
                    SurveyID = survey.SurveyID,
                    UserID = _userId,
                    ResponseType = "suggestion",
                    Category = (CategoryBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    SuggestionText = SuggestionBox.Text,
                    Status = "new",
                    ResponseDate = DateTime.Now
                };

                _context.SurveyResponses.Add(response);
                _context.SaveChanges();
                _context.RecordActivity("Отправлено новое предложение в разделе обратной связи");

                StatusText.Text = "Предложение отправлено успешно!";
                SuggestionBox.Text = "";

                MessageBox.Show("Ваше предложение принято. Спасибо за участие!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
