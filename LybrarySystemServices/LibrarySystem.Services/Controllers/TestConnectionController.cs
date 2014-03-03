using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace LibrarySystem.Services.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TestConnectionController : ApiController
    {
        [HttpGet]
        [ActionName("checkConnection")]
        public HttpResponseMessage CheckConnection()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, true);
        }
    }
}
