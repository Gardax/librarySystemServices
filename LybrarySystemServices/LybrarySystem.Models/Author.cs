using System.Collections.Generic;

namespace LibrarySystem.Models
{
    public class Author
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Book> Books { get; set; }
 
        public Author()
        {
            this.Books=new HashSet<Book>();
        }
    }
}
