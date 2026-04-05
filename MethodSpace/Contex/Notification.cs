namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Notification
    {
        public int NotificationID { get; set; }

        public int? UserID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? CreatedAt { get; set; }

        [StringLength(50)]
        public string EventType { get; set; }

        public virtual User User { get; set; }
    }
}
