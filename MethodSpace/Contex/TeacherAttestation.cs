namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TeacherAttestation")]
    public partial class TeacherAttestation
    {
        [Key]
        public int AttestationID { get; set; }

        public int? TeacherID { get; set; }

        [Column(TypeName = "date")]
        public DateTime AttestationDate { get; set; }

        [StringLength(50)]
        public string Result { get; set; }

        [StringLength(100)]
        public string CertificateNumber { get; set; }

        public string Comments { get; set; }

        public virtual User User { get; set; }
    }
}
