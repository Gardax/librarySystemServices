using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystem.Services.Models
{
    public class NoteModel
    {
        public string Text { get; set; }

        public int UserUniqueNumber { get; set; }

        public string BookKey { get; set; }
    }
}