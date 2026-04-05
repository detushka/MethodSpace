namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Event
    {
        public int EventID { get; set; }

        [Required]
        [StringLength(200)]
        public string EventName { get; set; }

        public DateTime EventDate { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        public string Description { get; set; }

        public int? OrganizerID { get; set; }

        public virtual User User { get; set; }
    }
}
