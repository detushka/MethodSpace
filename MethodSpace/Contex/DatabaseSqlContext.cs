using System.Data.Entity;

namespace MethodSpace.Contex
{
    internal class DatabaseSqlContext : DbContext
    {
        public DatabaseSqlContext()
            : base("name=SQL")
        {
        }

        public virtual DbSet<CourseRegistration> CourseRegistrations { get; set; }

        public virtual DbSet<Cours> Courses { get; set; }

        public virtual DbSet<Document> Documents { get; set; }

        public virtual DbSet<Event> Events { get; set; }

        public virtual DbSet<Message> Messages { get; set; }

        public virtual DbSet<News> News { get; set; }

        public virtual DbSet<Notification> Notifications { get; set; }

        public virtual DbSet<SurveyOption> SurveyOptions { get; set; }

        public virtual DbSet<SurveyQuestion> SurveyQuestions { get; set; }

        public virtual DbSet<SurveyRespons> SurveyResponses { get; set; }

        public virtual DbSet<Survey> Surveys { get; set; }

        public virtual DbSet<TeacherAttestation> TeacherAttestations { get; set; }

        public virtual DbSet<TeacherTip> TeacherTips { get; set; }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SurveyOption>()
                .HasMany(item => item.SurveyResponses)
                .WithOptional(item => item.SurveyOption)
                .HasForeignKey(item => item.SelectedOptionID);

            modelBuilder.Entity<SurveyQuestion>()
                .HasMany(item => item.SurveyOptions)
                .WithOptional(item => item.SurveyQuestion)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Survey>()
                .HasMany(item => item.SurveyQuestions)
                .WithOptional(item => item.Survey)
                .WillCascadeOnDelete();

            modelBuilder.Entity<User>()
                .HasMany(item => item.Documents)
                .WithOptional(item => item.User)
                .HasForeignKey(item => item.UploadedBy);

            modelBuilder.Entity<User>()
                .HasMany(item => item.Events)
                .WithOptional(item => item.User)
                .HasForeignKey(item => item.OrganizerID);

            modelBuilder.Entity<User>()
                .HasMany(item => item.Messages)
                .WithOptional(item => item.User)
                .HasForeignKey(item => item.SenderID);

            modelBuilder.Entity<User>()
                .HasMany(item => item.News)
                .WithOptional(item => item.User)
                .HasForeignKey(item => item.AuthorID);

            modelBuilder.Entity<User>()
                .HasMany(item => item.TeacherAttestations)
                .WithOptional(item => item.User)
                .HasForeignKey(item => item.TeacherID);

            modelBuilder.Entity<User>()
                .HasMany(item => item.TeacherTips)
                .WithOptional(item => item.User)
                .HasForeignKey(item => item.AuthorID);
        }
    }
}
