using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystem.Services.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public int UniqueNumber { get; set; }

        public string Name { get; set; }

        public string SessionKey { get; set; }

        public string AuthCode { get; set; }

    }
}