using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibrarySystem.Services.Models
{
    public class LoggedUserModel
    {
        public int UniqueNumber { get; set; }

        public string Name { get; set; }

        public string SessionKey { get; set; }
    }
}