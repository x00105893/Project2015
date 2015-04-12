using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ContactManager.Models
{
    public class Test
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        public virtual Career Career { get; set; }

        public string CheckTest()
        {
            throw new NotImplementedException();
        }
    }
}