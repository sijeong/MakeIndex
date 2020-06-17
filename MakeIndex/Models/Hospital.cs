using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace MakeIndex.Models
{
    public class Hospital
    {
        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterable]
        public string Id { get; set; }
       
        public Country Country { get; set; }
        public Doctor[] Doctors { get; set; }
        public Article[] Articles { get; set; }

        public Equipment[] Equipments { get; set; }
        public Service[] Services { get; set; }
        public Specialty[] Specialties { get; set; }
    }
}
