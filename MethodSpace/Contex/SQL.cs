using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;

namespace MethodSpace.Contex
{
    public partial class SQL : IDisposable
    {
        private readonly DatabaseSqlContext _databaseContext;
        private readonly MethodSpaceDataStore _store;
        private readonly bool _useDatabase;

        public SQL()
        {
            _store = MethodSpaceDataStore.Instance;

            try
            {
                _databaseContext = new DatabaseSqlContext();
                _databaseContext.Users.Select(user => user.UserID).FirstOrDefault();
                _useDatabase = true;
                _store.RecordActivity("Установлено подключение к базе CollegeMethodService");
            }
            catch
            {
                _useDatabase = false;
                _databaseContext?.Dispose();
            }

            CourseRegistrations = _useDatabase
                ? CreateEntitySet(_databaseContext.CourseRegistrations, item => item.RegistrationID)
                : _store.CourseRegistrations;
            Courses = _useDatabase
                ? CreateEntitySet(_databaseContext.Courses, item => item.CourseID)
                : _store.Courses;
            Documents = _useDatabase
                ? CreateEntitySet(_databaseContext.Documents, item => item.DocumentID)
                : _store.Documents;
            Events = _useDatabase
                ? CreateEntitySet(_databaseContext.Events, item => item.EventID)
                : _store.Events;
            Messages = _useDatabase
                ? CreateEntitySet(_databaseContext.Messages, item => item.MessageID)
                : _store.Messages;
            News = _useDatabase
                ? CreateEntitySet(_databaseContext.News, item => item.NewsID)
                : _store.News;
            Notifications = _useDatabase
                ? CreateEntitySet(_databaseContext.Notifications, item => item.NotificationID)
                : _store.Notifications;
            SurveyOptions = _useDatabase
                ? CreateEntitySet(_databaseContext.SurveyOptions, item => item.OptionID)
                : _store.SurveyOptions;
            SurveyQuestions = _useDatabase
                ? CreateEntitySet(_databaseContext.SurveyQuestions, item => item.QuestionID)
                : _store.SurveyQuestions;
            SurveyResponses = _useDatabase
                ? CreateEntitySet(_databaseContext.SurveyResponses, item => item.ResponseID)
                : _store.SurveyResponses;
            Surveys = _useDatabase
                ? CreateEntitySet(_databaseContext.Surveys, item => item.SurveyID)
                : _store.Surveys;
            TeacherAttestations = _useDatabase
                ? CreateEntitySet(_databaseContext.TeacherAttestations, item => item.AttestationID)
                : _store.TeacherAttestations;
            TeacherTips = _useDatabase
                ? CreateEntitySet(_databaseContext.TeacherTips, item => item.TipID)
                : _store.TeacherTips;
            Users = _useDatabase
                ? CreateEntitySet(_databaseContext.Users, item => item.UserID)
                : _store.Users;
        }

        public EntitySet<CourseRegistration> CourseRegistrations { get; private set; }

        public EntitySet<Cours> Courses { get; private set; }

        public EntitySet<Document> Documents { get; private set; }

        public EntitySet<Event> Events { get; private set; }

        public EntitySet<Message> Messages { get; private set; }

        public EntitySet<News> News { get; private set; }

        public EntitySet<Notification> Notifications { get; private set; }

        public EntitySet<SurveyOption> SurveyOptions { get; private set; }

        public EntitySet<SurveyQuestion> SurveyQuestions { get; private set; }

        public EntitySet<SurveyRespons> SurveyResponses { get; private set; }

        public EntitySet<Survey> Surveys { get; private set; }

        public EntitySet<TeacherAttestation> TeacherAttestations { get; private set; }

        public EntitySet<TeacherTip> TeacherTips { get; private set; }

        public EntitySet<User> Users { get; private set; }

        public bool IsDatabaseAvailable
        {
            get { return _useDatabase; }
        }

        public int SaveChanges()
        {
            if (_useDatabase && _databaseContext != null)
            {
                PreparePendingEntitiesForDatabase();

                try
                {
                    return _databaseContext.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    throw new InvalidOperationException(BuildValidationMessage(ex), ex);
                }
            }

            _store.RefreshRelationships();
            return 1;
        }

        public void RecordActivity(string action)
        {
            _store.RecordActivity(action);
        }

        public IReadOnlyList<ActivityLogEntry> GetRecentActivities(int takeCount = 8)
        {
            return _store.GetRecentActivities(takeCount);
        }

        public void Dispose()
        {
            _databaseContext?.Dispose();
        }

        private void PreparePendingEntitiesForDatabase()
        {
            PrepareDocuments();
            PrepareNews();
            PrepareMessages();
            PrepareNotifications();
            PrepareSurveys();
            PrepareSurveyQuestions();
            PrepareSurveyResponses();
            PrepareTeacherTips();
            PrepareCourses();
            PrepareEvents();
            PrepareUsers();
        }

        private void PrepareDocuments()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<Document>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                Document document = entry.Entity;
                document.Title = NormalizeText(document.Title, 200, "Новый документ");
                document.DocumentType = NormalizeText(document.DocumentType, 50, "instruction");
                document.DisciplineName = NormalizeText(document.DisciplineName, 100, null);
                document.UploadDate = document.UploadDate ?? DateTime.Now;
                document.ValidFrom = document.ValidFrom ?? DateTime.Today;
                document.ValidTo = document.ValidTo ?? DateTime.Today.AddYears(1);
                document.DownloadsCount = document.DownloadsCount ?? 0;

                if (string.IsNullOrWhiteSpace(document.FilePath) || !File.Exists(document.FilePath))
                {
                    document.FilePath = CreateGeneratedDocumentFile(document);
                }

                document.FilePath = NormalizeText(document.FilePath, 500, CreateGeneratedDocumentFile(document));
            }
        }

        private void PrepareNews()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<News>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                entry.Entity.Title = NormalizeText(entry.Entity.Title, 200, "Новая новость");
                entry.Entity.Content = string.IsNullOrWhiteSpace(entry.Entity.Content)
                    ? "Подробное описание будет добавлено позже."
                    : entry.Entity.Content.Trim();
                entry.Entity.PublishDate = entry.Entity.PublishDate ?? DateTime.Now;
                entry.Entity.IsImportant = entry.Entity.IsImportant ?? false;
            }
        }

        private void PrepareMessages()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<Message>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                entry.Entity.MessageType = NormalizeText(entry.Entity.MessageType, 50, "message_to_admin");
                entry.Entity.Subject = NormalizeText(entry.Entity.Subject, 200, "Сообщение");
                entry.Entity.MessageText = string.IsNullOrWhiteSpace(entry.Entity.MessageText)
                    ? "Сообщение без текста."
                    : entry.Entity.MessageText.Trim();
                entry.Entity.Status = NormalizeText(entry.Entity.Status, 50, null);
                entry.Entity.SentDate = entry.Entity.SentDate ?? DateTime.Now;
                entry.Entity.IsAnswered = entry.Entity.IsAnswered ?? false;
            }
        }

        private void PrepareNotifications()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<Notification>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                entry.Entity.Title = NormalizeText(entry.Entity.Title, 200, "Уведомление");
                entry.Entity.Message = string.IsNullOrWhiteSpace(entry.Entity.Message)
                    ? "Подробности уведомления отсутствуют."
                    : entry.Entity.Message.Trim();
                entry.Entity.EventType = NormalizeText(entry.Entity.EventType, 50, "system");
                entry.Entity.CreatedAt = entry.Entity.CreatedAt ?? DateTime.Now;
                entry.Entity.IsRead = entry.Entity.IsRead ?? false;
            }
        }

        private void PrepareSurveys()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<Survey>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                entry.Entity.Title = NormalizeText(entry.Entity.Title, 200, "Новая форма");
                entry.Entity.SurveyType = NormalizeText(entry.Entity.SurveyType, 50, "questionnaire");
                entry.Entity.TargetGroup = NormalizeText(entry.Entity.TargetGroup, 50, "all");
                entry.Entity.StartDate = entry.Entity.StartDate ?? DateTime.Now;
                entry.Entity.IsActive = entry.Entity.IsActive ?? true;
            }
        }

        private void PrepareSurveyQuestions()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<SurveyQuestion>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                entry.Entity.QuestionText = string.IsNullOrWhiteSpace(entry.Entity.QuestionText)
                    ? "Текст вопроса не указан."
                    : entry.Entity.QuestionText.Trim();
                entry.Entity.QuestionType = NormalizeText(entry.Entity.QuestionType, 50, "text");
                entry.Entity.OrderIndex = entry.Entity.OrderIndex ?? 1;
            }
        }

        private void PrepareSurveyResponses()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<SurveyRespons>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                entry.Entity.ResponseType = NormalizeText(entry.Entity.ResponseType, 50, "text");
                entry.Entity.Category = NormalizeText(entry.Entity.Category, 100, null);
                entry.Entity.Status = NormalizeText(entry.Entity.Status, 50, null);
                entry.Entity.ResponseDate = entry.Entity.ResponseDate ?? DateTime.Now;
            }
        }

        private void PrepareTeacherTips()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<TeacherTip>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                entry.Entity.Title = NormalizeText(entry.Entity.Title, 200, "Новый совет");
                entry.Entity.TipType = NormalizeText(entry.Entity.TipType, 50, "lesson_tip");
                entry.Entity.Content = string.IsNullOrWhiteSpace(entry.Entity.Content)
                    ? "Подробности совета будут добавлены позже."
                    : entry.Entity.Content.Trim();
                entry.Entity.PublishDate = entry.Entity.PublishDate ?? DateTime.Now;
            }
        }

        private void PrepareCourses()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<Cours>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                entry.Entity.CourseName = NormalizeText(entry.Entity.CourseName, 200, "Новый курс");
                entry.Entity.Location = NormalizeText(entry.Entity.Location, 200, null);
                entry.Entity.MaxParticipants = entry.Entity.MaxParticipants ?? 25;
                entry.Entity.CurrentParticipants = entry.Entity.CurrentParticipants ?? 0;
            }
        }

        private void PrepareEvents()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<Event>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                entry.Entity.EventName = NormalizeText(entry.Entity.EventName, 200, "Новое мероприятие");
                entry.Entity.Location = NormalizeText(entry.Entity.Location, 200, null);
            }
        }

        private void PrepareUsers()
        {
            foreach (var entry in _databaseContext.ChangeTracker.Entries<User>()
                .Where(item => item.State == EntityState.Added || item.State == EntityState.Modified))
            {
                entry.Entity.FullName = NormalizeText(entry.Entity.FullName, 100, "Новый пользователь");
                entry.Entity.Email = NormalizeText(entry.Entity.Email, 100, "user@methodspace.local");
                entry.Entity.Password = NormalizeText(entry.Entity.Password, 100, "password123");
                entry.Entity.Role = NormalizeText(entry.Entity.Role, 50, "teacher");
            }
        }

        private static string NormalizeText(string value, int maxLength, string fallback)
        {
            string normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

            if (string.IsNullOrEmpty(normalized))
            {
                return normalized;
            }

            return normalized.Length <= maxLength
                ? normalized
                : normalized.Substring(0, maxLength);
        }

        private static string CreateGeneratedDocumentFile(Document document)
        {
            string root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MethodSpace",
                "GeneratedDocuments");

            Directory.CreateDirectory(root);

            string safeName = MakeSafeFileName(string.IsNullOrWhiteSpace(document.Title) ? "document" : document.Title);
            string fileName = string.Format("{0}_{1:yyyyMMdd_HHmmss}.txt", safeName, DateTime.Now);
            string filePath = Path.Combine(root, fileName);

            string description = string.IsNullOrWhiteSpace(document.Description)
                ? "Описание не указано."
                : document.Description.Trim();

            var builder = new StringBuilder();
            builder.AppendLine("MethodSpace");
            builder.AppendLine();
            builder.AppendLine("Название: " + (document.Title ?? "Документ"));
            builder.AppendLine("Тип: " + (document.DocumentType ?? "instruction"));

            if (!string.IsNullOrWhiteSpace(document.DisciplineName))
            {
                builder.AppendLine("Дисциплина: " + document.DisciplineName);
            }

            builder.AppendLine();
            builder.AppendLine("Описание:");
            builder.AppendLine(description);
            builder.AppendLine();
            builder.AppendLine("Файл создан автоматически для записи документа в базу данных.");

            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
            return filePath;
        }

        private static string MakeSafeFileName(string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder();

            foreach (char symbol in value)
            {
                builder.Append(invalidChars.Contains(symbol) ? '_' : symbol);
            }

            string result = builder.ToString().Trim();
            if (string.IsNullOrWhiteSpace(result))
            {
                result = "document";
            }

            return result.Length <= 60 ? result : result.Substring(0, 60);
        }

        private static string BuildValidationMessage(DbEntityValidationException exception)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Ошибка сохранения в базу данных.");
            builder.AppendLine("Проверьте обязательные поля:");

            foreach (DbEntityValidationResult entityResult in exception.EntityValidationErrors)
            {
                string entityName = entityResult.Entry.Entity != null
                    ? entityResult.Entry.Entity.GetType().Name
                    : "UnknownEntity";

                foreach (DbValidationError validationError in entityResult.ValidationErrors)
                {
                    builder.AppendLine(string.Format(
                        "- {0}.{1}: {2}",
                        entityName,
                        validationError.PropertyName,
                        validationError.ErrorMessage));
                }
            }

            return builder.ToString().Trim();
        }

        private static EntitySet<T> CreateEntitySet<T>(DbSet<T> dbSet, Func<T, int> getId) where T : class
        {
            return new EntitySet<T>(
                () => dbSet,
                id => dbSet.Find(id),
                item => dbSet.Add(item),
                item =>
                {
                    if (item == null)
                    {
                        return false;
                    }

                    return dbSet.Remove(item) != null;
                });
        }
    }
}
