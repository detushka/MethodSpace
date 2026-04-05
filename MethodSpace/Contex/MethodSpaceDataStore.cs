using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MethodSpace.Contex
{
    internal sealed class MethodSpaceDataStore
    {
        private static readonly Lazy<MethodSpaceDataStore> LazyInstance =
            new Lazy<MethodSpaceDataStore>(() => new MethodSpaceDataStore());

        private readonly string _documentsRoot;

        private int _nextUserId = 1;
        private int _nextDocumentId = 1;
        private int _nextCourseId = 1;
        private int _nextEventId = 1;
        private int _nextMessageId = 1;
        private int _nextNewsId = 1;
        private int _nextNotificationId = 1;
        private int _nextSurveyId = 1;
        private int _nextQuestionId = 1;
        private int _nextOptionId = 1;
        private int _nextResponseId = 1;
        private int _nextAttestationId = 1;
        private int _nextTipId = 1;
        private int _nextRegistrationId = 1;

        public static MethodSpaceDataStore Instance
        {
            get { return LazyInstance.Value; }
        }

        public EntitySet<User> Users { get; private set; }

        public EntitySet<Document> Documents { get; private set; }

        public EntitySet<Cours> Courses { get; private set; }

        public EntitySet<Event> Events { get; private set; }

        public EntitySet<Message> Messages { get; private set; }

        public EntitySet<News> News { get; private set; }

        public EntitySet<Notification> Notifications { get; private set; }

        public EntitySet<Survey> Surveys { get; private set; }

        public EntitySet<SurveyQuestion> SurveyQuestions { get; private set; }

        public EntitySet<SurveyOption> SurveyOptions { get; private set; }

        public EntitySet<SurveyRespons> SurveyResponses { get; private set; }

        public EntitySet<TeacherAttestation> TeacherAttestations { get; private set; }

        public EntitySet<TeacherTip> TeacherTips { get; private set; }

        public EntitySet<CourseRegistration> CourseRegistrations { get; private set; }

        public List<ActivityLogEntry> ActivityLog { get; private set; }

        private MethodSpaceDataStore()
        {
            _documentsRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MethodSpace",
                "OfflineStorage",
                "Documents");

            Directory.CreateDirectory(_documentsRoot);

            ActivityLog = new List<ActivityLogEntry>();

            Users = new EntitySet<User>(new List<User>(), user => user.UserID, PrepareUser);
            Documents = new EntitySet<Document>(new List<Document>(), document => document.DocumentID, PrepareDocument);
            Courses = new EntitySet<Cours>(
                new List<Cours>(),
                course => course.CourseID,
                PrepareCourse,
                course => RemoveRegistrationsForCourse(course.CourseID));
            Events = new EntitySet<Event>(new List<Event>(), item => item.EventID, PrepareEvent);
            Messages = new EntitySet<Message>(new List<Message>(), message => message.MessageID, PrepareMessage);
            News = new EntitySet<News>(new List<News>(), item => item.NewsID, PrepareNews);
            Notifications = new EntitySet<Notification>(new List<Notification>(), item => item.NotificationID, PrepareNotification);
            Surveys = new EntitySet<Survey>(
                new List<Survey>(),
                survey => survey.SurveyID,
                PrepareSurvey,
                survey => RemoveSurveyDetails(survey.SurveyID));
            SurveyQuestions = new EntitySet<SurveyQuestion>(
                new List<SurveyQuestion>(),
                question => question.QuestionID,
                PrepareSurveyQuestion,
                question => RemoveQuestionDetails(question.QuestionID));
            SurveyOptions = new EntitySet<SurveyOption>(new List<SurveyOption>(), option => option.OptionID, PrepareSurveyOption);
            SurveyResponses = new EntitySet<SurveyRespons>(new List<SurveyRespons>(), response => response.ResponseID, PrepareSurveyResponse);
            TeacherAttestations = new EntitySet<TeacherAttestation>(
                new List<TeacherAttestation>(),
                attestation => attestation.AttestationID,
                PrepareAttestation);
            TeacherTips = new EntitySet<TeacherTip>(new List<TeacherTip>(), tip => tip.TipID, PrepareTeacherTip);
            CourseRegistrations = new EntitySet<CourseRegistration>(new List<CourseRegistration>(), registration => registration.RegistrationID, PrepareCourseRegistration);

            Seed();
            RefreshRelationships();
        }

        public void RefreshRelationships()
        {
            foreach (User user in Users)
            {
                user.CourseRegistrations = new HashSet<CourseRegistration>();
                user.Documents = new HashSet<Document>();
                user.Events = new HashSet<Event>();
                user.Messages = new HashSet<Message>();
                user.News = new HashSet<News>();
                user.Notifications = new HashSet<Notification>();
                user.SurveyResponses = new HashSet<SurveyRespons>();
                user.TeacherAttestations = new HashSet<TeacherAttestation>();
                user.TeacherTips = new HashSet<TeacherTip>();
            }

            foreach (Cours course in Courses)
            {
                course.CourseRegistrations = new HashSet<CourseRegistration>();
                if (!course.MaxParticipants.HasValue || course.MaxParticipants.Value < 1)
                {
                    course.MaxParticipants = 25;
                }
            }

            foreach (Survey survey in Surveys)
            {
                survey.SurveyQuestions = new HashSet<SurveyQuestion>();
                survey.SurveyResponses = new HashSet<SurveyRespons>();
            }

            foreach (SurveyQuestion question in SurveyQuestions)
            {
                question.SurveyOptions = new HashSet<SurveyOption>();
                question.SurveyResponses = new HashSet<SurveyRespons>();
            }

            foreach (SurveyOption option in SurveyOptions)
            {
                option.SurveyResponses = new HashSet<SurveyRespons>();
            }

            foreach (News item in News)
            {
                item.User = ResolveUser(item.AuthorID);
                if (item.User != null)
                {
                    item.User.News.Add(item);
                }
            }

            foreach (Document document in Documents)
            {
                document.User = ResolveUser(document.UploadedBy);
                if (document.User != null)
                {
                    document.User.Documents.Add(document);
                }
            }

            foreach (Event item in Events)
            {
                item.User = ResolveUser(item.OrganizerID);
                if (item.User != null)
                {
                    item.User.Events.Add(item);
                }
            }

            foreach (TeacherTip tip in TeacherTips)
            {
                tip.User = ResolveUser(tip.AuthorID);
                if (tip.User != null)
                {
                    tip.User.TeacherTips.Add(tip);
                }
            }

            foreach (TeacherAttestation attestation in TeacherAttestations)
            {
                attestation.User = ResolveUser(attestation.TeacherID);
                if (attestation.User != null)
                {
                    attestation.User.TeacherAttestations.Add(attestation);
                }
            }

            foreach (Message message in Messages)
            {
                message.User = ResolveUser(message.SenderID);
                if (message.User != null)
                {
                    message.User.Messages.Add(message);
                }
            }

            foreach (Notification notification in Notifications)
            {
                notification.User = ResolveUser(notification.UserID);
                if (notification.User != null)
                {
                    notification.User.Notifications.Add(notification);
                }
            }

            foreach (CourseRegistration registration in CourseRegistrations)
            {
                registration.User = ResolveUser(registration.UserID);
                registration.Cours = ResolveCourse(registration.CourseID);

                if (registration.User != null)
                {
                    registration.User.CourseRegistrations.Add(registration);
                }

                if (registration.Cours != null)
                {
                    registration.Cours.CourseRegistrations.Add(registration);
                }
            }

            foreach (Cours course in Courses)
            {
                course.CurrentParticipants = course.CourseRegistrations.Count;
            }

            foreach (SurveyQuestion question in SurveyQuestions)
            {
                question.Survey = ResolveSurvey(question.SurveyID);
                if (question.Survey != null)
                {
                    question.Survey.SurveyQuestions.Add(question);
                }
            }

            foreach (SurveyOption option in SurveyOptions)
            {
                option.SurveyQuestion = ResolveQuestion(option.QuestionID);
                if (option.SurveyQuestion != null)
                {
                    option.SurveyQuestion.SurveyOptions.Add(option);
                }
            }

            foreach (SurveyRespons response in SurveyResponses)
            {
                response.User = ResolveUser(response.UserID);
                response.Survey = ResolveSurvey(response.SurveyID);
                response.SurveyQuestion = ResolveQuestion(response.QuestionID);
                response.SurveyOption = ResolveOption(response.SelectedOptionID);

                if (response.User != null)
                {
                    response.User.SurveyResponses.Add(response);
                }

                if (response.Survey != null)
                {
                    response.Survey.SurveyResponses.Add(response);
                }

                if (response.SurveyQuestion != null)
                {
                    response.SurveyQuestion.SurveyResponses.Add(response);
                }

                if (response.SurveyOption != null)
                {
                    response.SurveyOption.SurveyResponses.Add(response);
                }
            }
        }

        public string CreateDocumentFile(string title, string description, string documentType, string disciplineName, int documentId)
        {
            string safeTitle = SanitizeFileName(title);
            if (string.IsNullOrWhiteSpace(safeTitle))
            {
                safeTitle = "document";
            }

            string fileName = string.Format("{0:D3}_{1}.txt", documentId, safeTitle);
            string filePath = Path.Combine(_documentsRoot, fileName);

            var builder = new StringBuilder();
            builder.AppendLine("MethodSpace");
            builder.AppendLine();
            builder.AppendLine("Название: " + title);
            builder.AppendLine("Тип: " + documentType);

            if (!string.IsNullOrWhiteSpace(disciplineName))
            {
                builder.AppendLine("Дисциплина: " + disciplineName);
            }

            builder.AppendLine();
            builder.AppendLine("Описание:");
            builder.AppendLine(string.IsNullOrWhiteSpace(description) ? "Без описания" : description);
            builder.AppendLine();
            builder.AppendLine("Файл создан в офлайн-режиме " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));

            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
            return filePath;
        }

        private void Seed()
        {
            DateTime now = DateTime.Now;

            Users.Add(new User
            {
                UserID = 1,
                FullName = "Анна Сергеевна Лебедева",
                Email = "admin@methodspace.local",
                Password = "admin123",
                Role = "admin",
                IsActive = true,
                CreatedAt = now.AddMonths(-14)
            });

            Users.Add(new User
            {
                UserID = 2,
                FullName = "Ирина Павловна Кузнецова",
                Email = "methodist@methodspace.local",
                Password = "method123",
                Role = "methodist",
                IsActive = true,
                CreatedAt = now.AddMonths(-11)
            });

            Users.Add(new User
            {
                UserID = 3,
                FullName = "Никита Олегович Смирнов",
                Email = "teacher@methodspace.local",
                Password = "teacher123",
                Role = "teacher",
                IsActive = true,
                CreatedAt = now.AddMonths(-8)
            });

            Users.Add(new User
            {
                UserID = 4,
                FullName = "Марина Викторовна Орлова",
                Email = "teacher2@methodspace.local",
                Password = "teacher123",
                Role = "teacher",
                IsActive = true,
                CreatedAt = now.AddMonths(-5)
            });

            Documents.Add(new Document
            {
                DocumentID = 1,
                Title = "Рабочая программа по информатике",
                DocumentType = "working_program",
                DisciplineName = "Информатика",
                Description = "Актуальная рабочая программа для 1 курса с акцентом на проектную деятельность.",
                UploadedBy = 2,
                UploadDate = now.AddDays(-12),
                ValidFrom = now.Date.AddMonths(-1),
                ValidTo = now.Date.AddMonths(10),
                DownloadsCount = 14
            });

            Documents.Add(new Document
            {
                DocumentID = 2,
                Title = "Методические рекомендации по смешанному обучению",
                DocumentType = "methodical_recommendation",
                DisciplineName = "Педагогика",
                Description = "Подборка приёмов и шаблонов для очно-дистанционного формата.",
                UploadedBy = 2,
                UploadDate = now.AddDays(-8),
                ValidFrom = now.Date.AddMonths(-2),
                ValidTo = now.Date.AddMonths(8),
                DownloadsCount = 23
            });

            Documents.Add(new Document
            {
                DocumentID = 3,
                Title = "Положение о внутреннем мониторинге качества",
                DocumentType = "regulation",
                DisciplineName = "Документация",
                Description = "Регламент мониторинга учебных результатов и методической активности.",
                UploadedBy = 1,
                UploadDate = now.AddDays(-20),
                ValidFrom = now.Date.AddMonths(-6),
                ValidTo = now.Date.AddYears(1),
                DownloadsCount = 9
            });

            Documents.Add(new Document
            {
                DocumentID = 4,
                Title = "Инструкция по оформлению электронного УМК",
                DocumentType = "instruction",
                DisciplineName = "Методическая работа",
                Description = "Краткая инструкция по структуре папок, именованию файлов и версии материалов.",
                UploadedBy = 2,
                UploadDate = now.AddDays(-4),
                ValidFrom = now.Date,
                ValidTo = now.Date.AddMonths(12),
                DownloadsCount = 5
            });

            News.Add(new News
            {
                NewsID = 1,
                Title = "Открыт новый цикл методических консультаций",
                Content = "В течение апреля методическая служба проводит серию коротких консультаций по обновлению рабочих программ и цифровых курсов.",
                AuthorID = 2,
                PublishDate = now.AddDays(-1),
                IsImportant = true
            });

            News.Add(new News
            {
                NewsID = 2,
                Title = "Обновлены шаблоны учебно-методических комплексов",
                Content = "В разделе документов опубликованы обновлённые шаблоны УМК, чек-листы качества и пример структуры папок по дисциплине.",
                AuthorID = 2,
                PublishDate = now.AddDays(-3),
                IsImportant = false
            });

            News.Add(new News
            {
                NewsID = 3,
                Title = "Подготовка к аттестации педагогов",
                Content = "Для преподавателей доступен новый пакет памяток по подготовке к аттестации, включая сроки, критерии и образцы материалов.",
                AuthorID = 1,
                PublishDate = now.AddDays(-6),
                IsImportant = true
            });

            Courses.Add(new Cours
            {
                CourseID = 1,
                CourseName = "Цифровые инструменты преподавателя",
                StartDate = now.Date.AddDays(5),
                EndDate = now.Date.AddDays(12),
                Location = "Кабинет 214 / Teams",
                MaxParticipants = 20,
                CurrentParticipants = 1
            });

            Courses.Add(new Cours
            {
                CourseID = 2,
                CourseName = "Конструирование практико-ориентированных заданий",
                StartDate = now.Date.AddDays(14),
                EndDate = now.Date.AddDays(20),
                Location = "Методический кабинет",
                MaxParticipants = 15,
                CurrentParticipants = 0
            });

            Courses.Add(new Cours
            {
                CourseID = 3,
                CourseName = "Актуальные требования к электронному курсу",
                StartDate = now.Date.AddDays(-30),
                EndDate = now.Date.AddDays(-23),
                Location = "Онлайн",
                MaxParticipants = 25,
                CurrentParticipants = 0
            });

            CourseRegistrations.Add(new CourseRegistration
            {
                RegistrationID = 1,
                CourseID = 1,
                UserID = 3,
                RegistrationDate = now.AddDays(-1),
                IsConfirmed = true
            });

            Events.Add(new Event
            {
                EventID = 1,
                EventName = "Методический совет по итогам четверти",
                EventDate = now.Date.AddDays(2).AddHours(15),
                Location = "Актовый зал",
                Description = "Обсуждение результатов мониторинга, корректировка дорожной карты и обмен лучшими практиками.",
                OrganizerID = 2
            });

            Events.Add(new Event
            {
                EventID = 2,
                EventName = "Практикум по оформлению УМК",
                EventDate = now.Date.AddDays(9).AddHours(11),
                Location = "Компьютерный класс 302",
                Description = "Работа с шаблонами, типовые ошибки и разбор реальных комплектов материалов.",
                OrganizerID = 1
            });

            TeacherTips.Add(new TeacherTip
            {
                TipID = 1,
                Title = "Три способа оживить начало занятия",
                TipType = "lesson_tip",
                Content = "Используйте мини-кейсы, быстрый опрос по карточкам и визуальный провокационный вопрос, чтобы вовлечь группу в первые три минуты.",
                AuthorID = 2,
                PublishDate = now.AddDays(-2)
            });

            TeacherTips.Add(new TeacherTip
            {
                TipID = 2,
                Title = "Как собрать доказательства практических компетенций",
                TipType = "practical_recommendation",
                Content = "Сохраняйте промежуточные артефакты проектов, фото этапов и короткие самоотчёты студентов: это ускоряет оценивание и делает его прозрачным.",
                AuthorID = 1,
                PublishDate = now.AddDays(-5)
            });

            TeacherAttestations.Add(new TeacherAttestation
            {
                AttestationID = 1,
                TeacherID = 3,
                AttestationDate = now.Date.AddMonths(-2),
                Result = "Подтверждена",
                CertificateNumber = "AT-2026-041",
                Comments = "Рекомендовано расширить пакет цифровых практик."
            });

            TeacherAttestations.Add(new TeacherAttestation
            {
                AttestationID = 2,
                TeacherID = 4,
                AttestationDate = now.Date.AddMonths(-1),
                Result = "Высшая категория",
                CertificateNumber = "AT-2026-057",
                Comments = "Сильная методическая база и активное участие в внутренних семинарах."
            });

            Messages.Add(new Message
            {
                MessageID = 1,
                SenderID = 3,
                MessageType = "message_to_admin",
                Subject = "Нужна консультация по новому шаблону УМК",
                MessageText = "Прошу подсказать, как корректно оформить раздел с критериями оценивания в обновлённом шаблоне.",
                SentDate = now.AddDays(-1).AddHours(-2),
                IsAnswered = true,
                AnswerText = "Добавили образец в документы. Если хотите, можно выбрать время для разбора на консультации.",
                AnswerDate = now.AddDays(-1),
                Status = "answered"
            });

            Messages.Add(new Message
            {
                MessageID = 2,
                SenderID = 4,
                MessageType = "consultation_request",
                Subject = "Запрос на индивидуальную консультацию",
                MessageText = "Нужна короткая консультация по планированию открытого занятия и подбору критериев самоанализа.",
                SentDate = now.AddHours(-6),
                IsAnswered = false,
                ConsultationDate = now.Date.AddDays(4),
                Status = "requested"
            });

            Notifications.Add(new Notification
            {
                NotificationID = 1,
                UserID = 3,
                Title = "Напоминание о курсе",
                Message = "Через 5 дней стартует курс «Цифровые инструменты преподавателя».",
                IsRead = false,
                CreatedAt = now.AddHours(-10),
                EventType = "course"
            });

            Notifications.Add(new Notification
            {
                NotificationID = 2,
                UserID = 3,
                Title = "Ответ на ваше сообщение",
                Message = "Методист ответил на ваш запрос по шаблону УМК.",
                IsRead = true,
                CreatedAt = now.AddDays(-1),
                EventType = "message"
            });

            Surveys.Add(new Survey
            {
                SurveyID = 1,
                Title = "Сбор идей по развитию методической службы",
                Description = "Принимаем предложения по улучшению сервисов, документов и сопровождения преподавателей.",
                SurveyType = "suggestion",
                TargetGroup = "all",
                StartDate = now.AddMonths(-1),
                EndDate = now.AddMonths(2),
                IsActive = true
            });

            Surveys.Add(new Survey
            {
                SurveyID = 2,
                Title = "Оценка обновлённого пакета методических материалов",
                Description = "Короткий опрос по качеству новых шаблонов и рекомендаций.",
                SurveyType = "questionnaire",
                TargetGroup = "all",
                StartDate = now.AddDays(-2),
                EndDate = now.AddDays(15),
                IsActive = true
            });

            SurveyQuestions.Add(new SurveyQuestion
            {
                QuestionID = 1,
                SurveyID = 2,
                QuestionText = "Насколько полезны новые шаблоны УМК в текущей работе?",
                QuestionType = "single_choice",
                OrderIndex = 1
            });

            SurveyOptions.Add(new SurveyOption
            {
                OptionID = 1,
                QuestionID = 1,
                OptionText = "Очень полезны"
            });

            SurveyOptions.Add(new SurveyOption
            {
                OptionID = 2,
                QuestionID = 1,
                OptionText = "Полезны, но нужны доработки"
            });

            SurveyOptions.Add(new SurveyOption
            {
                OptionID = 3,
                QuestionID = 1,
                OptionText = "Пока не использую"
            });

            SurveyQuestions.Add(new SurveyQuestion
            {
                QuestionID = 2,
                SurveyID = 2,
                QuestionText = "Что ещё стоит улучшить в методических материалах?",
                QuestionType = "text",
                OrderIndex = 2
            });

            Surveys.Add(new Survey
            {
                SurveyID = 3,
                Title = "Тема августовского педсовета",
                Description = "Выберите тему, которую стоит вынести в основной блок обсуждения.",
                SurveyType = "vote",
                TargetGroup = "all",
                StartDate = now.AddDays(-1),
                EndDate = now.AddDays(10),
                IsActive = true
            });

            SurveyQuestions.Add(new SurveyQuestion
            {
                QuestionID = 3,
                SurveyID = 3,
                QuestionText = "Какой трек сделать основным на педсовете?",
                QuestionType = "single_choice",
                OrderIndex = 1
            });

            SurveyOptions.Add(new SurveyOption
            {
                OptionID = 4,
                QuestionID = 3,
                OptionText = "Индивидуальные образовательные маршруты"
            });

            SurveyOptions.Add(new SurveyOption
            {
                OptionID = 5,
                QuestionID = 3,
                OptionText = "Цифровая дидактика и ИИ-инструменты"
            });

            SurveyOptions.Add(new SurveyOption
            {
                OptionID = 6,
                QuestionID = 3,
                OptionText = "Практико-ориентированное оценивание"
            });

            RecordActivity("Офлайн-режим MethodSpace подготовлен к работе", now.AddMinutes(-40));
            RecordActivity("Обновлены шаблоны УМК и инструкция по электронному курсу", now.AddMinutes(-30));
            RecordActivity("Открыт набор на курс «Цифровые инструменты преподавателя»", now.AddMinutes(-20));
            RecordActivity("Добавлена новость о консультациях методической службы", now.AddMinutes(-10));
        }

        private void PrepareUser(User user)
        {
            if (user.UserID <= 0)
            {
                user.UserID = _nextUserId++;
            }
            else if (user.UserID >= _nextUserId)
            {
                _nextUserId = user.UserID + 1;
            }

            if (!user.CreatedAt.HasValue)
            {
                user.CreatedAt = DateTime.Now;
            }

            if (user.CourseRegistrations == null)
            {
                user.CourseRegistrations = new HashSet<CourseRegistration>();
            }

            if (user.Documents == null)
            {
                user.Documents = new HashSet<Document>();
            }

            if (user.Events == null)
            {
                user.Events = new HashSet<Event>();
            }

            if (user.Messages == null)
            {
                user.Messages = new HashSet<Message>();
            }

            if (user.News == null)
            {
                user.News = new HashSet<News>();
            }

            if (user.Notifications == null)
            {
                user.Notifications = new HashSet<Notification>();
            }

            if (user.SurveyResponses == null)
            {
                user.SurveyResponses = new HashSet<SurveyRespons>();
            }

            if (user.TeacherAttestations == null)
            {
                user.TeacherAttestations = new HashSet<TeacherAttestation>();
            }

            if (user.TeacherTips == null)
            {
                user.TeacherTips = new HashSet<TeacherTip>();
            }
        }

        private void PrepareDocument(Document document)
        {
            if (document.DocumentID <= 0)
            {
                document.DocumentID = _nextDocumentId++;
            }
            else if (document.DocumentID >= _nextDocumentId)
            {
                _nextDocumentId = document.DocumentID + 1;
            }

            if (!document.UploadDate.HasValue)
            {
                document.UploadDate = DateTime.Now;
            }

            document.DownloadsCount = document.DownloadsCount ?? 0;

            if (string.IsNullOrWhiteSpace(document.FilePath) || !File.Exists(document.FilePath))
            {
                document.FilePath = CreateDocumentFile(
                    document.Title,
                    document.Description,
                    document.DocumentType,
                    document.DisciplineName,
                    document.DocumentID);
            }
        }

        private void PrepareCourse(Cours course)
        {
            if (course.CourseID <= 0)
            {
                course.CourseID = _nextCourseId++;
            }
            else if (course.CourseID >= _nextCourseId)
            {
                _nextCourseId = course.CourseID + 1;
            }

            if (course.MaxParticipants == null || course.MaxParticipants < 1)
            {
                course.MaxParticipants = 25;
            }

            if (course.CurrentParticipants == null || course.CurrentParticipants < 0)
            {
                course.CurrentParticipants = 0;
            }

            if (course.CourseRegistrations == null)
            {
                course.CourseRegistrations = new HashSet<CourseRegistration>();
            }
        }

        private void PrepareEvent(Event item)
        {
            if (item.EventID <= 0)
            {
                item.EventID = _nextEventId++;
            }
            else if (item.EventID >= _nextEventId)
            {
                _nextEventId = item.EventID + 1;
            }
        }

        private void PrepareMessage(Message message)
        {
            if (message.MessageID <= 0)
            {
                message.MessageID = _nextMessageId++;
            }
            else if (message.MessageID >= _nextMessageId)
            {
                _nextMessageId = message.MessageID + 1;
            }

            if (!message.SentDate.HasValue)
            {
                message.SentDate = DateTime.Now;
            }

            if (message.IsAnswered == null)
            {
                message.IsAnswered = false;
            }
        }

        private void PrepareNews(News item)
        {
            if (item.NewsID <= 0)
            {
                item.NewsID = _nextNewsId++;
            }
            else if (item.NewsID >= _nextNewsId)
            {
                _nextNewsId = item.NewsID + 1;
            }

            if (!item.PublishDate.HasValue)
            {
                item.PublishDate = DateTime.Now;
            }
        }

        private void PrepareNotification(Notification item)
        {
            if (item.NotificationID <= 0)
            {
                item.NotificationID = _nextNotificationId++;
            }
            else if (item.NotificationID >= _nextNotificationId)
            {
                _nextNotificationId = item.NotificationID + 1;
            }

            if (!item.CreatedAt.HasValue)
            {
                item.CreatedAt = DateTime.Now;
            }

            if (item.IsRead == null)
            {
                item.IsRead = false;
            }
        }

        private void PrepareSurvey(Survey survey)
        {
            if (survey.SurveyID <= 0)
            {
                survey.SurveyID = _nextSurveyId++;
            }
            else if (survey.SurveyID >= _nextSurveyId)
            {
                _nextSurveyId = survey.SurveyID + 1;
            }

            if (!survey.StartDate.HasValue)
            {
                survey.StartDate = DateTime.Now;
            }

            if (!survey.EndDate.HasValue)
            {
                survey.EndDate = DateTime.Now.AddDays(14);
            }

            if (survey.IsActive == null)
            {
                survey.IsActive = true;
            }

            if (survey.SurveyQuestions == null)
            {
                survey.SurveyQuestions = new HashSet<SurveyQuestion>();
            }

            if (survey.SurveyResponses == null)
            {
                survey.SurveyResponses = new HashSet<SurveyRespons>();
            }
        }

        private void PrepareSurveyQuestion(SurveyQuestion question)
        {
            if (question.QuestionID <= 0)
            {
                question.QuestionID = _nextQuestionId++;
            }
            else if (question.QuestionID >= _nextQuestionId)
            {
                _nextQuestionId = question.QuestionID + 1;
            }

            if (question.OrderIndex == null || question.OrderIndex < 1)
            {
                question.OrderIndex = question.QuestionID;
            }

            if (question.SurveyOptions == null)
            {
                question.SurveyOptions = new HashSet<SurveyOption>();
            }

            if (question.SurveyResponses == null)
            {
                question.SurveyResponses = new HashSet<SurveyRespons>();
            }
        }

        private void PrepareSurveyOption(SurveyOption option)
        {
            if (option.OptionID <= 0)
            {
                option.OptionID = _nextOptionId++;
            }
            else if (option.OptionID >= _nextOptionId)
            {
                _nextOptionId = option.OptionID + 1;
            }

            if (option.SurveyResponses == null)
            {
                option.SurveyResponses = new HashSet<SurveyRespons>();
            }
        }

        private void PrepareSurveyResponse(SurveyRespons response)
        {
            if (response.ResponseID <= 0)
            {
                response.ResponseID = _nextResponseId++;
            }
            else if (response.ResponseID >= _nextResponseId)
            {
                _nextResponseId = response.ResponseID + 1;
            }

            if (!response.ResponseDate.HasValue)
            {
                response.ResponseDate = DateTime.Now;
            }

            if (string.IsNullOrWhiteSpace(response.Status))
            {
                response.Status = "completed";
            }
        }

        private void PrepareAttestation(TeacherAttestation attestation)
        {
            if (attestation.AttestationID <= 0)
            {
                attestation.AttestationID = _nextAttestationId++;
            }
            else if (attestation.AttestationID >= _nextAttestationId)
            {
                _nextAttestationId = attestation.AttestationID + 1;
            }
        }

        private void PrepareTeacherTip(TeacherTip tip)
        {
            if (tip.TipID <= 0)
            {
                tip.TipID = _nextTipId++;
            }
            else if (tip.TipID >= _nextTipId)
            {
                _nextTipId = tip.TipID + 1;
            }

            if (!tip.PublishDate.HasValue)
            {
                tip.PublishDate = DateTime.Now;
            }
        }

        private void PrepareCourseRegistration(CourseRegistration registration)
        {
            if (registration.RegistrationID <= 0)
            {
                registration.RegistrationID = _nextRegistrationId++;
            }
            else if (registration.RegistrationID >= _nextRegistrationId)
            {
                _nextRegistrationId = registration.RegistrationID + 1;
            }

            if (!registration.RegistrationDate.HasValue)
            {
                registration.RegistrationDate = DateTime.Now;
            }

            if (registration.IsConfirmed == null)
            {
                registration.IsConfirmed = true;
            }
        }

        private void RemoveRegistrationsForCourse(int courseId)
        {
            List<CourseRegistration> registrations = CourseRegistrations
                .Where(item => item.CourseID == courseId)
                .ToList();

            foreach (CourseRegistration registration in registrations)
            {
                CourseRegistrations.Remove(registration);
            }
        }

        private void RemoveSurveyDetails(int surveyId)
        {
            List<SurveyQuestion> questions = SurveyQuestions
                .Where(item => item.SurveyID == surveyId)
                .ToList();

            foreach (SurveyQuestion question in questions)
            {
                SurveyQuestions.Remove(question);
            }

            List<SurveyRespons> responses = SurveyResponses
                .Where(item => item.SurveyID == surveyId)
                .ToList();

            foreach (SurveyRespons response in responses)
            {
                SurveyResponses.Remove(response);
            }
        }

        private void RemoveQuestionDetails(int questionId)
        {
            List<SurveyOption> options = SurveyOptions
                .Where(item => item.QuestionID == questionId)
                .ToList();

            foreach (SurveyOption option in options)
            {
                SurveyOptions.Remove(option);
            }

            List<SurveyRespons> responses = SurveyResponses
                .Where(item => item.QuestionID == questionId)
                .ToList();

            foreach (SurveyRespons response in responses)
            {
                SurveyResponses.Remove(response);
            }
        }

        private User ResolveUser(int? userId)
        {
            return userId.HasValue ? Users.Find(userId.Value) : null;
        }

        private Cours ResolveCourse(int? courseId)
        {
            return courseId.HasValue ? Courses.Find(courseId.Value) : null;
        }

        private Survey ResolveSurvey(int? surveyId)
        {
            return surveyId.HasValue ? Surveys.Find(surveyId.Value) : null;
        }

        private SurveyQuestion ResolveQuestion(int? questionId)
        {
            return questionId.HasValue ? SurveyQuestions.Find(questionId.Value) : null;
        }

        private SurveyOption ResolveOption(int? optionId)
        {
            return optionId.HasValue ? SurveyOptions.Find(optionId.Value) : null;
        }

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return string.Empty;
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sanitizedChars = fileName
                .Where(ch => !invalidChars.Contains(ch))
                .ToArray();

            return new string(sanitizedChars).Replace(" ", "_");
        }

        public IReadOnlyList<ActivityLogEntry> GetRecentActivities(int takeCount)
        {
            return ActivityLog
                .OrderByDescending(item => item.Date)
                .Take(takeCount)
                .ToList();
        }

        public void RecordActivity(string action)
        {
            RecordActivity(action, DateTime.Now);
        }

        public void RecordActivity(string action, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return;
            }

            ActivityLog.Add(new ActivityLogEntry
            {
                Date = date,
                Action = action
            });

            if (ActivityLog.Count > 60)
            {
                ActivityLog = ActivityLog
                    .OrderByDescending(item => item.Date)
                    .Take(60)
                    .ToList();
            }
        }
    }
}
