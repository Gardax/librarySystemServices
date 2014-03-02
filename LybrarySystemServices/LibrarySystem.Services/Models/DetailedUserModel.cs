using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystem.Services.Models
{
    public class DetailedUserModel
    {
        public int Id { get; set; }

        public int UniqueNumber { get; set; }

        public string Name { get; set; }

        public ICollection<BookToReturnModel> BooksToReturn { get; set; }
    }
}