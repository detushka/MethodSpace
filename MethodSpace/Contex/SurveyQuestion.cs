namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SurveyQuestion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SurveyQuestion()
        {
            SurveyOptions = new HashSet<SurveyOption>();
            SurveyResponses = new HashSet<SurveyRespons>();
        }

        [Key]
        public int QuestionID { get; set; }

        public int? SurveyID { get; set; }

        [Required]
        public string QuestionText { get; set; }

        [StringLength(50)]
        public string QuestionType { get; set; }

        public int? OrderIndex { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SurveyOption> SurveyOptions { get; set; }

        public virtual Survey Survey { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SurveyRespons> SurveyResponses { get; set; }
    }
}
