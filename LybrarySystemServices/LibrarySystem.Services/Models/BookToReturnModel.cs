using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystem.Services.Models
{
    public class BookToReturnModel
    {
        public string Key { get; set; }

        public string Title { get; set; }

        public string AuthorName { get; set; }

        public int Year { get; set; }

        public string Description { get; set; }

        public DateTime DateToreturn { get; set; }
    }
}