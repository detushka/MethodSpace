using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MethodSpace.Contex;
using Microsoft.Win32;

namespace MethodSpace.Pages
{
    public partial class AdminPanelPage : Page
    {
        private readonly SQL _context;
        private readonly int _userId;

        public AdminPanelPage(int userId, string userRole)
        {
            InitializeComponent();
            _context = new SQL();
            _userId = userId;

            Loaded += (sender, args) => LoadStatistics();
        }

        private void LoadStatistics()
        {
            UsersCount.Text = _context.Users.Count().ToString();
            DocumentsCount.Text = _context.Documents.Count().ToString();
            CoursesCount.Text = _context.Courses.Count().ToString();
            NewsCount.Text = _context.News.Count().ToString();
            RecentActivities.ItemsSource = _context.GetRecentActivities();
        }

        private void AddNews_Click(object sender, RoutedEventArgs e)
        {
            Window dialog = CreateDialogWindow("Добавить новость", 520, 430);
            StackPanel content = CreateFormContainer();

            TextBox titleBox = CreateTextBox();
            TextBox bodyBox = CreateTextArea();
            CheckBox importantCheckBox = new CheckBox { Content = "Пометить как важную новость" };

            content.Children.Add(CreateLabel("Заголовок"));
            content.Children.Add(titleBox);
            content.Children.Add(CreateLabel("Содержание"));
            content.Children.Add(bodyBox);
            content.Children.Add(importantCheckBox);

            content.Children.Add(CreateActionButtons(dialog, () =>
            {
                if (string.IsNullOrWhiteSpace(titleBox.Text) || string.IsNullOrWhiteSpace(bodyBox.Text))
                {
                    MessageBox.Show(
                        "Заполните заголовок и текст новости.",
                        "Проверка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                _context.News.Add(new News
                {
                    Title = titleBox.Text.Trim(),
                    Content = bodyBox.Text.Trim(),
                    AuthorID = _userId,
                    PublishDate = DateTime.Now,
                    IsImportant = importantCheckBox.IsChecked == true
                });

                _context.SaveChanges();
                _context.RecordActivity("Добавлена новая новость в админ-панели");
                LoadStatistics();
                dialog.Close();
            }));

            dialog.Content = content;
            dialog.ShowDialog();
        }

        private void AddDocument_Click(object sender, RoutedEventArgs e)
        {
            Window dialog = CreateDialogWindow("Добавить документ", 560, 640);
            StackPanel content = CreateFormContainer();

            TextBox titleBox = CreateTextBox();
            ComboBox typeBox = new ComboBox { Margin = new Thickness(0, 0, 0, 14) };
            typeBox.Items.Add(new ComboBoxItem { Content = "Рабочая программа", Tag = "working_program" });
            typeBox.Items.Add(new ComboBoxItem { Content = "Методические рекомендации", Tag = "methodical_recommendation" });
            typeBox.Items.Add(new ComboBoxItem { Content = "Нормативный документ", Tag = "regulation" });
            typeBox.Items.Add(new ComboBoxItem { Content = "Инструкция", Tag = "instruction" });
            typeBox.SelectedIndex = 0;

            TextBox disciplineBox = CreateTextBox();
            TextBox descriptionBox = CreateTextArea();
            TextBox filePathBox = CreateTextBox();
            filePathBox.IsReadOnly = true;
            filePathBox.ToolTip = "Путь к выбранному файлу";

            string selectedFilePath = null;

            Button chooseFileButton = new Button
            {
                Content = "Выбрать файл",
                Width = 130,
                Height = 36,
                Margin = new Thickness(10, 0, 0, 14),
                Style = FindResource("ActionButtonStyle") as Style
            };

            chooseFileButton.Click += (buttonSender, buttonArgs) =>
            {
                OpenFileDialog fileDialog = new OpenFileDialog
                {
                    Title = "Выберите файл документа",
                    Filter = "Поддерживаемые файлы|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;*.txt|Все файлы|*.*",
                    CheckFileExists = true,
                    Multiselect = false
                };

                if (fileDialog.ShowDialog() == true)
                {
                    selectedFilePath = fileDialog.FileName;
                    filePathBox.Text = selectedFilePath;

                    if (string.IsNullOrWhiteSpace(titleBox.Text))
                    {
                        titleBox.Text = Path.GetFileNameWithoutExtension(selectedFilePath);
                    }
                }
            };

            Grid fileRow = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            fileRow.ColumnDefinitions.Add(new ColumnDefinition());
            fileRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(filePathBox, 0);
            Grid.SetColumn(chooseFileButton, 1);
            fileRow.Children.Add(filePathBox);
            fileRow.Children.Add(chooseFileButton);

            content.Children.Add(CreateLabel("Название"));
            content.Children.Add(titleBox);
            content.Children.Add(CreateLabel("Тип документа"));
            content.Children.Add(typeBox);
            content.Children.Add(CreateLabel("Дисциплина"));
            content.Children.Add(disciplineBox);
            content.Children.Add(CreateLabel("Файл документа"));
            content.Children.Add(fileRow);
            content.Children.Add(CreateLabel("Описание"));
            content.Children.Add(descriptionBox);

            content.Children.Add(CreateActionButtons(dialog, () =>
            {
                string documentType = GetSelectedTag(typeBox);

                if (string.IsNullOrWhiteSpace(titleBox.Text) || string.IsNullOrWhiteSpace(documentType))
                {
                    MessageBox.Show(
                        "Заполните название и выберите тип документа.",
                        "Проверка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(selectedFilePath) || !File.Exists(selectedFilePath))
                {
                    MessageBox.Show(
                        "Выберите файл, который нужно сохранить в систему.",
                        "Проверка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                string storedFilePath;
                try
                {
                    storedFilePath = CopyDocumentToStorage(selectedFilePath, titleBox.Text.Trim());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Не удалось сохранить файл: " + ex.Message,
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                _context.Documents.Add(new Document
                {
                    Title = titleBox.Text.Trim(),
                    DocumentType = documentType,
                    DisciplineName = disciplineBox.Text.Trim(),
                    Description = descriptionBox.Text.Trim(),
                    FilePath = storedFilePath,
                    UploadedBy = _userId,
                    UploadDate = DateTime.Now,
                    DownloadsCount = 0,
                    ValidFrom = DateTime.Today,
                    ValidTo = DateTime.Today.AddYears(1)
                });

                _context.SaveChanges();
                _context.RecordActivity("Добавлен новый документ через админ-панель");
                LoadStatistics();
                dialog.Close();
            }));

            dialog.Content = content;
            dialog.ShowDialog();
        }

        private void AddCourse_Click(object sender, RoutedEventArgs e)
        {
            Window dialog = CreateDialogWindow("Добавить курс", 520, 500);
            StackPanel content = CreateFormContainer();

            TextBox nameBox = CreateTextBox();
            TextBox locationBox = CreateTextBox();
            DatePicker startDatePicker = new DatePicker { SelectedDate = DateTime.Today.AddDays(7), Margin = new Thickness(0, 0, 0, 14) };
            DatePicker endDatePicker = new DatePicker { SelectedDate = DateTime.Today.AddDays(14), Margin = new Thickness(0, 0, 0, 14) };
            TextBox participantsBox = CreateTextBox();
            participantsBox.Text = "20";

            content.Children.Add(CreateLabel("Название курса"));
            content.Children.Add(nameBox);
            content.Children.Add(CreateLabel("Место проведения"));
            content.Children.Add(locationBox);
            content.Children.Add(CreateLabel("Дата начала"));
            content.Children.Add(startDatePicker);
            content.Children.Add(CreateLabel("Дата окончания"));
            content.Children.Add(endDatePicker);
            content.Children.Add(CreateLabel("Максимум участников"));
            content.Children.Add(participantsBox);

            content.Children.Add(CreateActionButtons(dialog, () =>
            {
                int maxParticipants;
                if (string.IsNullOrWhiteSpace(nameBox.Text) ||
                    !startDatePicker.SelectedDate.HasValue ||
                    !endDatePicker.SelectedDate.HasValue ||
                    !int.TryParse(participantsBox.Text, out maxParticipants))
                {
                    MessageBox.Show(
                        "Проверьте заполнение формы курса.",
                        "Проверка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (endDatePicker.SelectedDate.Value < startDatePicker.SelectedDate.Value)
                {
                    MessageBox.Show(
                        "Дата окончания не может быть раньше даты начала.",
                        "Проверка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                _context.Courses.Add(new Cours
                {
                    CourseName = nameBox.Text.Trim(),
                    Location = locationBox.Text.Trim(),
                    StartDate = startDatePicker.SelectedDate.Value,
                    EndDate = endDatePicker.SelectedDate.Value,
                    MaxParticipants = maxParticipants,
                    CurrentParticipants = 0
                });

                _context.SaveChanges();
                _context.RecordActivity("Добавлен новый курс повышения квалификации");
                LoadStatistics();
                dialog.Close();
            }));

            dialog.Content = content;
            dialog.ShowDialog();
        }

        private void CreateSurvey_Click(object sender, RoutedEventArgs e)
        {
            Window dialog = CreateDialogWindow("Создать опрос или голосование", 560, 610);
            StackPanel content = CreateFormContainer();

            TextBox titleBox = CreateTextBox();
            TextBox descriptionBox = CreateTextArea();
            ComboBox typeBox = new ComboBox { Margin = new Thickness(0, 0, 0, 14) };
            typeBox.Items.Add(new ComboBoxItem { Content = "Опрос", Tag = "questionnaire" });
            typeBox.Items.Add(new ComboBoxItem { Content = "Голосование", Tag = "vote" });
            typeBox.Items.Add(new ComboBoxItem { Content = "Форма предложений", Tag = "suggestion" });
            typeBox.SelectedIndex = 0;

            DatePicker endDatePicker = new DatePicker { SelectedDate = DateTime.Today.AddDays(14), Margin = new Thickness(0, 0, 0, 14) };
            TextBox questionBox = CreateTextArea();
            questionBox.Height = 90;
            TextBox optionsBox = CreateTextArea();
            optionsBox.Height = 120;

            content.Children.Add(CreateLabel("Название"));
            content.Children.Add(titleBox);
            content.Children.Add(CreateLabel("Описание"));
            content.Children.Add(descriptionBox);
            content.Children.Add(CreateLabel("Тип"));
            content.Children.Add(typeBox);
            content.Children.Add(CreateLabel("Дата окончания"));
            content.Children.Add(endDatePicker);
            content.Children.Add(CreateLabel("Текст вопроса"));
            content.Children.Add(questionBox);
            content.Children.Add(CreateLabel("Варианты ответа (каждый с новой строки, можно оставить пустым для текстового вопроса)"));
            content.Children.Add(optionsBox);

            content.Children.Add(CreateActionButtons(dialog, () =>
            {
                string surveyType = GetSelectedTag(typeBox);
                if (string.IsNullOrWhiteSpace(titleBox.Text) || string.IsNullOrWhiteSpace(surveyType))
                {
                    MessageBox.Show(
                        "Заполните название и тип формы.",
                        "Проверка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                string[] options = optionsBox.Text
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => item.Trim())
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .ToArray();

                if (surveyType != "suggestion" && string.IsNullOrWhiteSpace(questionBox.Text))
                {
                    MessageBox.Show(
                        "Для опроса или голосования нужно указать вопрос.",
                        "Проверка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (surveyType == "vote" && options.Length < 2)
                {
                    MessageBox.Show(
                        "Для голосования добавьте минимум два варианта ответа.",
                        "Проверка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                Survey survey = new Survey
                {
                    Title = titleBox.Text.Trim(),
                    Description = descriptionBox.Text.Trim(),
                    SurveyType = surveyType,
                    TargetGroup = "all",
                    StartDate = DateTime.Now,
                    EndDate = endDatePicker.SelectedDate ?? DateTime.Today.AddDays(14),
                    IsActive = true
                };

                _context.Surveys.Add(survey);
                _context.SaveChanges();

                if (surveyType != "suggestion")
                {
                    bool hasOptions = options.Length > 0;

                    SurveyQuestion question = new SurveyQuestion
                    {
                        SurveyID = survey.SurveyID,
                        QuestionText = questionBox.Text.Trim(),
                        QuestionType = hasOptions ? "single_choice" : "text",
                        OrderIndex = 1
                    };

                    _context.SurveyQuestions.Add(question);
                    _context.SaveChanges();

                    foreach (string optionText in options)
                    {
                        _context.SurveyOptions.Add(new SurveyOption
                        {
                            QuestionID = question.QuestionID,
                            OptionText = optionText
                        });
                    }

                    _context.SaveChanges();
                }

                _context.RecordActivity("Создан новый опрос или голосование");
                LoadStatistics();
                dialog.Close();
            }));

            dialog.Content = content;
            dialog.ShowDialog();
        }

        private Window CreateDialogWindow(string title, double width, double height)
        {
            return new Window
            {
                Title = title,
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = (System.Windows.Media.Brush)FindResource("AppBackgroundBrush")
            };
        }

        private StackPanel CreateFormContainer()
        {
            return new StackPanel
            {
                Margin = new Thickness(18)
            };
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

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Margin = new Thickness(0, 0, 0, 14)
            };
        }

        private TextBox CreateTextArea()
        {
            return new TextBox
            {
                Margin = new Thickness(0, 0, 0, 14),
                Height = 130,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        private FrameworkElement CreateActionButtons(Window dialog, Action onSave)
        {
            StackPanel panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 6, 0, 0)
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

            cancelButton.Click += (sender, args) => dialog.Close();
            saveButton.Click += (sender, args) => onSave();

            panel.Children.Add(cancelButton);
            panel.Children.Add(saveButton);

            return panel;
        }

        private string GetSelectedTag(ComboBox comboBox)
        {
            ComboBoxItem item = comboBox.SelectedItem as ComboBoxItem;
            return item != null ? item.Tag as string : null;
        }

        private string CopyDocumentToStorage(string sourceFilePath, string documentTitle)
        {
            string storageRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MethodSpace",
                "UploadedDocuments");

            Directory.CreateDirectory(storageRoot);

            string extension = Path.GetExtension(sourceFilePath);
            string safeTitle = MakeSafeFileName(documentTitle);
            string targetFileName = string.Format(
                "{0}_{1}{2}",
                safeTitle,
                DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                extension);

            string targetFilePath = Path.Combine(storageRoot, targetFileName);
            File.Copy(sourceFilePath, targetFilePath, false);
            return targetFilePath;
        }

        private string MakeSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "document";
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            char[] safeChars = value
                .Trim()
                .Select(character => invalidChars.Contains(character) ? '_' : character)
                .ToArray();

            string result = new string(safeChars).Replace(' ', '_');
            return string.IsNullOrWhiteSpace(result) ? "document" : result;
        }
    }
}
