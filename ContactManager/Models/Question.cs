using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ContactManager.Models
{
    public enum QuestionType
    {
        Text = 1,
        Checkbox = 2,
        Radio = 3
    }

    public class Question
    {
        [Key]
        public int ID { get; set; }

        [StringLength(512),Required]
        public string Text { get; set; }

        [StringLength(512)]
        public string HelpText { get; set; }

        [Required, Column(TypeName = "int")]
        public QuestionType Type { get; set; }

        public virtual ICollection<QuestionOption> Options { get; set; }

        public bool CheckAnswer()
        {
            throw new NotImplementedException();
        }
    }
}