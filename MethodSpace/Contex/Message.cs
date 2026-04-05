namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Message
    {
        public int MessageID { get; set; }

        public int? SenderID { get; set; }

        [Required]
        [StringLength(50)]
        public string MessageType { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        public string MessageText { get; set; }

        public DateTime? SentDate { get; set; }

        public bool? IsAnswered { get; set; }

        public string AnswerText { get; set; }

        public DateTime? AnswerDate { get; set; }

        public DateTime? ConsultationDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public virtual User User { get; set; }
    }
}
