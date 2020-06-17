using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace MakeIndex.Models
{
    public class Service
    {
        [IsSearchable]
        public string Name { get; set; }
    }
}
