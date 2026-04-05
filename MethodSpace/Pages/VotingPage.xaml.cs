using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class VotingPage : Page
    {
        private readonly SQL _context;
        private readonly int _userId;

        public VotingPage(int userId, string userRole)
        {
            InitializeComponent();
            _context = new SQL();
            _userId = userId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadVotings();
        }

        private void LoadVotings()
        {
            VotingList.ItemsSource = _context.Surveys
                .Where(item => item.SurveyType == "vote" && item.IsActive == true)
                .Where(item => item.StartDate == null || item.StartDate <= DateTime.Now)
                .Where(item => item.EndDate == null || item.EndDate >= DateTime.Now)
                .OrderByDescending(item => item.EndDate)
                .ToList();
        }

        private void VoteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Tag == null)
            {
                return;
            }

            int surveyId = (int)button.Tag;
            Survey survey = _context.Surveys.Find(surveyId);
            if (survey == null)
            {
                return;
            }

            if (_context.SurveyResponses.Any(item => item.SurveyID == surveyId && item.UserID == _userId))
            {
                MessageBox.Show("Ваш голос по этому вопросу уже учтён.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SurveyQuestion question = _context.SurveyQuestions
                .Where(item => item.SurveyID == surveyId)
                .OrderBy(item => item.OrderIndex)
                .FirstOrDefault();

            if (question == null)
            {
                MessageBox.Show("Для этого голосования пока нет вариантов.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var options = _context.SurveyOptions
                .Where(item => item.QuestionID == question.QuestionID)
                .OrderBy(item => item.OptionID)
                .ToList();

            Window dialog = new Window
            {
                Title = survey.Title,
                Width = 560,
                Height = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = (System.Windows.Media.Brush)FindResource("AppBackgroundBrush")
            };

            StackPanel content = new StackPanel { Margin = new Thickness(18) };
            content.Children.Add(new TextBlock
            {
                Text = survey.Description,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12),
                Foreground = (System.Windows.Media.Brush)FindResource("SecondaryTextBrush")
            });
            content.Children.Add(new TextBlock
            {
                Text = question.QuestionText,
                FontWeight = FontWeights.SemiBold,
                FontSize = 15,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 14)
            });

            StackPanel optionsPanel = new StackPanel();
            foreach (SurveyOption option in options)
            {
                optionsPanel.Children.Add(new RadioButton
                {
                    Content = option.OptionText,
                    GroupName = "vote-options",
                    Tag = option.OptionID,
                    Margin = new Thickness(0, 0, 0, 10)
                });
            }

            Button submitButton = new Button
            {
                Content = "Учесть голос",
                Width = 150,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 18, 0, 0),
                Style = FindResource("SuccessButtonStyle") as Style
            };

            submitButton.Click += (buttonSender, args) =>
            {
                RadioButton selectedOption = optionsPanel.Children
                    .OfType<RadioButton>()
                    .FirstOrDefault(item => item.IsChecked == true);

                if (selectedOption == null)
                {
                    MessageBox.Show("Выберите один вариант голосования.", "Проверка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.SurveyResponses.Add(new SurveyRespons
                {
                    SurveyID = survey.SurveyID,
                    UserID = _userId,
                    ResponseType = "vote",
                    QuestionID = question.QuestionID,
                    SelectedOptionID = (int)selectedOption.Tag,
                    ResponseDate = DateTime.Now,
                    Status = "new"
                });

                _context.Notifications.Add(new Notification
                {
                    UserID = _userId,
                    Title = "Голосование завершено",
                    Message = "Ваш голос по теме «" + survey.Title + "» сохранён.",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    EventType = "vote"
                });

                _context.SaveChanges();
                _context.RecordActivity("Принято участие в голосовании «" + survey.Title + "»");
                dialog.Close();
                LoadVotings();

                MessageBox.Show("Голос учтён.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            };

            content.Children.Add(optionsPanel);
            content.Children.Add(submitButton);

            dialog.Content = content;
            dialog.ShowDialog();
        }

        private void VotingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
