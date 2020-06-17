using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace MakeIndex.Models
{
    public class Equipment
    {
        [IsSearchable]
        public string Name { get; set; }
        [IsSearchable]
        public string Description { get; set; }
    }
}
