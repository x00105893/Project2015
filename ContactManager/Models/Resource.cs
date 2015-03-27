using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ContactManager.Models
{
    public enum ResourceType
    { 
        None = 0,
        Video = 1,
        Image = 2,
        Document = 3
    }

    public class Resource
    {
        [Key]
        public int ID { get; set; }

        [Required, StringLength(256)]
        public string Title { get; set; }

        [Required, StringLength(256)]
        public string Name { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        [Required, Column(TypeName="int")]
        public ResourceType Type { get; set; }

        [StringLength(512)]
        public string RootPath { get; set; }

        public DateTime UploadedDate { get; set; }

        public virtual ApplicationUser UploadedBy { get; set; }
    }
}