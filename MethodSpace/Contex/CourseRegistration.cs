namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CourseRegistration
    {
        [Key]
        public int RegistrationID { get; set; }

        public int? CourseID { get; set; }

        public int? UserID { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public bool? IsConfirmed { get; set; }

        public virtual Cours Cours { get; set; }

        public virtual User User { get; set; }
    }
}
