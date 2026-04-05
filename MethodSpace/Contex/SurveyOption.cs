namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SurveyOption
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SurveyOption()
        {
            SurveyResponses = new HashSet<SurveyRespons>();
        }

        [Key]
        public int OptionID { get; set; }

        public int? QuestionID { get; set; }

        [Required]
        [StringLength(200)]
        public string OptionText { get; set; }

        public virtual SurveyQuestion SurveyQuestion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SurveyRespons> SurveyResponses { get; set; }
    }
}
