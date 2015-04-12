﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ContactManager.Models
{
    public class Career
    {
        [Key]
        public int ID { get; set; }

        [Required, StringLength(512)]
        public string Name { get; set; }

        [Required, StringLength(2048), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(4096)]
        public string WhatDo { get; set; }

        [StringLength(4096)]
        public string WhoHire { get; set; }

        [StringLength(4096)]
        public string KeySkills { get; set; }

        [StringLength(4096)]
        public string CareerPath { get; set; }

        public virtual ICollection<Resource> Documents { get; set; }

        public virtual ICollection<Test> Tests { get; set; }

        public bool UseLinkForVideo { get; set; }

        [StringLength(512)]
        public string VideoLink { get; set; }

        public virtual Resource Video { get; set; }

        public DateTime ModifiedDate { get; set; }

        public virtual ApplicationUser ModifiedBy { get; set; }

        public Career() { }

        public Career(Career career)
        {
            CareerPath = career.CareerPath;
            Description = career.Description;
            KeySkills = career.KeySkills;
            Name = career.Name;
            UseLinkForVideo = career.UseLinkForVideo;
            VideoLink = career.VideoLink;
            WhatDo = career.WhatDo;
            WhoHire = career.WhoHire;
            ModifiedDate = DateTime.UtcNow;
        }

        public void Update(Career career)
        {
            CareerPath = career.CareerPath;
            Description = career.Description;
            KeySkills = career.KeySkills;
            Name = career.Name;
            UseLinkForVideo = career.UseLinkForVideo;
            VideoLink = career.VideoLink;
            WhatDo = career.WhatDo;
            WhoHire = career.WhoHire;
            ModifiedDate = DateTime.UtcNow;
        }
    }
}