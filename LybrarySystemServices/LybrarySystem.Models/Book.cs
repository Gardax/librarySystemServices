using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string Title { get; set; }

        public virtual Author Author { get; set; }

        public virtual ICollection<UserBook> GotBy { get; set; }

        public virtual ICollection<Note> Notes { get; set; } 

        public int Year { get; set; }

        public string Description { get; set; }

        public Book()
        {
            this.Notes=new HashSet<Note>();
        }

    }
}
