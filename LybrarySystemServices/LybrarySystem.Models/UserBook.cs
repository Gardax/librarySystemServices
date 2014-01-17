using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Models
{
    public class UserBook
    {
        [Key, Column(Order = 0)]
        public int BookId { get; set; }

        [Key, Column(Order = 1)]
        public int UserId { get; set; }

        public virtual Book Book { get; set; }

        public virtual User User { get; set; }

        public bool IsReturned { get; set; }

        public DateTime DateToReturn { get; set; }
    }
}
