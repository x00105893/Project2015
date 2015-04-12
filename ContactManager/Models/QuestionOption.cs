using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ContactManager.Models
{
    public class QuestionOption
    {
        [Key]
        public int ID { get; set; }

        public string Text { get; set; }

        public bool IsRight { get; set; }
    }
}