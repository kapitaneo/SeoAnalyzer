using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebApplication8.Models
{
    public class ModelResults
    {
        [Display(Name = "Count links")]
        public int? Hrefcount { get; set; }

        public ICollection<KeyIndex> FindTextconsid;

        public ICollection<KeyIndex> FindKeywordsConsid;

        public ModelResults()
        {
            FindTextconsid = new List<KeyIndex>();
            FindKeywordsConsid = new List<KeyIndex>();
        }
    }

    public class KeyIndex
    {
        public int? Count { get; set; }

        public string Word { get; set; }
    }
}