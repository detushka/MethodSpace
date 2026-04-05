namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Document
    {
        public int DocumentID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; }

        [StringLength(100)]
        public string DisciplineName { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ValidFrom { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ValidTo { get; set; }

        public int? UploadedBy { get; set; }

        public DateTime? UploadDate { get; set; }

        public int? DownloadsCount { get; set; }

        public virtual User User { get; set; }
    }
}
