namespace MethodSpace.Contex
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SurveyResponses")]
    public partial class SurveyRespons
    {
        [Key]
        public int ResponseID { get; set; }

        public int? SurveyID { get; set; }

        public int? UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string ResponseType { get; set; }

        public int? QuestionID { get; set; }

        public string AnswerText { get; set; }

        public int? SelectedOptionID { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        public string SuggestionText { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public string AdminComment { get; set; }

        public DateTime? ResponseDate { get; set; }

        public virtual SurveyOption SurveyOption { get; set; }

        public virtual SurveyQuestion SurveyQuestion { get; set; }

        public virtual Survey Survey { get; set; }

        public virtual User User { get; set; }
    }
}
