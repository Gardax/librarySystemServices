using System.Collections.Generic;

namespace LibrarySystem.Models
{
    public class User
    {
        public int Id { get; set; }

        public int UniqueNumber { get; set; }

        public string Name { get; set; }

        public string AuthCode { get; set; }

        public string SessionKey { get; set; }

        public virtual ICollection<UserBook> GotBooks { get; set; } 
    }
}
