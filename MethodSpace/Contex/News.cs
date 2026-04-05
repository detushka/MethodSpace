namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class News
    {
        public int NewsID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public int? AuthorID { get; set; }

        public DateTime? PublishDate { get; set; }

        public bool? IsImportant { get; set; }

        public virtual User User { get; set; }
    }
}
