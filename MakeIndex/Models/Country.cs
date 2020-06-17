using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace MakeIndex.Models
{
    public class Country
    {
        [IsFilterable, IsFacetable]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
