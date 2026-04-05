namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TeacherTip
    {
        [Key]
        public int TipID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(50)]
        public string TipType { get; set; }

        [Required]
        public string Content { get; set; }

        public int? AuthorID { get; set; }

        public DateTime? PublishDate { get; set; }

        public virtual User User { get; set; }
    }
}
