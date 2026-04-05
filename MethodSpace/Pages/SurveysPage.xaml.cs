using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;

namespace MethodSpace.Pages
{
    public partial class SurveysPage : Page
    {
        private readonly SQL _context;
        private readonly int _userId;

        public SurveysPage(int userId, string userRole)
        {
            InitializeComponent();
            _context = new SQL();
            _userId = userId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSurveys();
        }

        private void LoadSurveys()
        {
            SurveysList.ItemsSource = _context.Surveys
                .Where(item => item.SurveyType == "questionnaire" && item.IsActive == true)
                .Where(item => item.StartDate == null || item.StartDate <= DateTime.Now)
                .Where(item => item.EndDate == null || item.EndDate >= DateTime.Now)
                .OrderByDescending(item => item.EndDate)
                .ToList();
        }

        private void TakeSurvey_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Tag == null) return;

            int surveyId = (int)button.Tag;
            Survey survey = _context.Surveys.Find(surveyId);
            if (survey == null) return;

            // ПРОВЕРКА: не проходил ли пользователь уже этот опрос
            var existingResponse = _context.SurveyResponses
                .FirstOrDefault(r => r.SurveyID == surveyId && r.UserID == _userId);

            if (existingResponse != null)
            {
                MessageBox.Show("Вы уже прошли этот опрос.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSurveys();
                return;
            }

            List<SurveyQuestion> questions = _context.SurveyQuestions
                .Where(q => q.SurveyID == surveyId)
                .OrderBy(q => q.OrderIndex)
                .ToList();

            if (questions.Count == 0)
            {
                MessageBox.Show("В этом опросе пока нет вопросов.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Создаем окно опроса
            Window dialog = new Window
            {
                Title = survey.Title,
                Width = 640,
                Height = 620,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.CanResize
            };

            ScrollViewer scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel content = new StackPanel { Margin = new Thickness(18) };
            Dictionary<int, TextBox> textAnswers = new Dictionary<int, TextBox>();
            Dictionary<int, RadioButton> selectedOptions = new Dictionary<int, RadioButton>();

            content.Children.Add(new TextBlock
            {
                Text = survey.Description,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 16),
                Foreground = System.Windows.Media.Brushes.Gray
            });

            foreach (SurveyQuestion question in questions)
            {
                Border card = new Border
                {
                    Background = System.Windows.Media.Brushes.White,
                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(16),
                    Margin = new Thickness(0, 0, 0, 14)
                };

                StackPanel cardContent = new StackPanel();
                cardContent.Children.Add(new TextBlock
                {
                    Text = question.QuestionText,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 12)
                });

                if (question.QuestionType == "text")
                {
                    TextBox answerBox = new TextBox
                    {
                        Height = 80,
                        TextWrapping = TextWrapping.Wrap,
                        AcceptsReturn = true,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    };
                    textAnswers[question.QuestionID] = answerBox;
                    cardContent.Children.Add(answerBox);
                }
                else
                {
                    List<SurveyOption> options = _context.SurveyOptions
                        .Where(o => o.QuestionID == question.QuestionID)
                        .OrderBy(o => o.OptionID)
                        .ToList();

                    string groupName = "opt_" + question.QuestionID;
                    foreach (SurveyOption option in options)
                    {
                        RadioButton optionButton = new RadioButton
                        {
                            Content = option.OptionText,
                            GroupName = groupName,
                            Margin = new Thickness(0, 0, 0, 8),
                            Tag = option.OptionID
                        };
                        cardContent.Children.Add(optionButton);

                        // Сохраняем выбранный вариант для каждого вопроса
                        optionButton.Checked += (s, args) =>
                        {
                            selectedOptions[question.QuestionID] = optionButton;
                        };
                    }
                }

                card.Child = cardContent;
                content.Children.Add(card);
            }

            Button submitButton = new Button
            {
                Content = "Отправить ответы",
                Height = 40,
                Width = 160,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0),
                Background = System.Windows.Media.Brushes.Green,
                Foreground = System.Windows.Media.Brushes.White
            };

            submitButton.Click += (buttonSender, args) =>
            {
                try
                {
                    // Еще одна проверка перед сохранением
                    var checkAgain = _context.SurveyResponses
                        .FirstOrDefault(r => r.SurveyID == surveyId && r.UserID == _userId);

                    if (checkAgain != null)
                    {
                        MessageBox.Show("Вы уже прошли этот опрос.", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        dialog.Close();
                        LoadSurveys();
                        return;
                    }

                    foreach (SurveyQuestion question in questions)
                    {
                        if (question.QuestionType == "text")
                        {
                            if (!textAnswers.ContainsKey(question.QuestionID))
                            {
                                MessageBox.Show("Ошибка: не найден ответ на вопрос.", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            TextBox answerBox = textAnswers[question.QuestionID];
                            if (string.IsNullOrWhiteSpace(answerBox.Text))
                            {
                                MessageBox.Show("Ответьте на все вопросы опроса.", "Проверка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            _context.SurveyResponses.Add(new SurveyRespons
                            {
                                SurveyID = survey.SurveyID,
                                UserID = _userId,
                                ResponseType = "survey_answer",
                                QuestionID = question.QuestionID,
                                AnswerText = answerBox.Text.Trim(),
                                ResponseDate = DateTime.Now,
                                Status = "new"
                            });
                        }
                        else
                        {
                            if (!selectedOptions.ContainsKey(question.QuestionID) || selectedOptions[question.QuestionID] == null)
                            {
                                MessageBox.Show("Выберите вариант ответа для каждого вопроса.", "Проверка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            _context.SurveyResponses.Add(new SurveyRespons
                            {
                                SurveyID = survey.SurveyID,
                                UserID = _userId,
                                ResponseType = "survey_answer",
                                QuestionID = question.QuestionID,
                                SelectedOptionID = (int)selectedOptions[question.QuestionID].Tag,
                                ResponseDate = DateTime.Now,
                                Status = "new"
                            });
                        }
                    }

                    _context.SaveChanges();
                    _context.RecordActivity("Пройден опрос «" + survey.Title + "»");
                    dialog.Close();
                    LoadSurveys();

                    MessageBox.Show("Ответы успешно отправлены.", "Готово",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            content.Children.Add(submitButton);
            scrollViewer.Content = content;
            dialog.Content = scrollViewer;
            dialog.ShowDialog();
        }

        private void SurveysList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}