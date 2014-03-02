using System.Collections.ObjectModel;
using System.Text;
using LibrarySystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LibrarySystem.Models;
using LibrarySystem.Services.Models;

namespace LibrarySystem.Services.Controllers
{
    public class UsersController : ApiController
    {
        public const int MinNameLength = 4;

        private const string SessionKeyChars =
            "qwertyuioplkjhgfdsazxcvbnmQWERTYUIOPLKJHGFDSAZXCVBNM";

        private static readonly Random rand = new Random();

        private const int SessionKeyLength = 50;

        private const int Sha1Length = 40;

        [HttpPost]
        [ActionName("createUser")] //api/users/createUser
        public HttpResponseMessage PostCreateUser(UserModel model)
        {
            try
            {
                var dbContext = new LibrarySystemContext();
                using (dbContext)
                {
                    ValidateName(model.Name);
                    var user = new User()
                                   {
                                       Name = model.Name,
                                       AuthCode = model.AuthCode
                                   };

                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();

                    user.SessionKey = this.GenerateSessionKey(user.Id);
                    user.UniqueNumber = user.Id + 1000;
                    dbContext.SaveChanges();

                    var loggedModel = new LoggedUserModel()
                    {
                        UniqueNumber = user.UniqueNumber,
                        SessionKey = user.SessionKey
                    };

                    var response = this.Request.CreateResponse(HttpStatusCode.Created,
                                              loggedModel);
                    return response;
                }

            }
            catch(Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                              ex.Message);
                return response;
            }
        }

        [HttpPost]
        [ActionName("login")]  //api/users/login
        public HttpResponseMessage PostLoginUser(UserModel model)
        {
            try
            {
                ValidateAuthCode(model.AuthCode);

                var context = new LibrarySystemContext();
                using (context)
                {
                    var user = context.Users.FirstOrDefault(u => u.UniqueNumber == model.UniqueNumber
                        && u.AuthCode == model.AuthCode);

                    if (user == null)
                    {
                        throw new InvalidOperationException("Invalid unique number or password");
                    }
                    if (user.SessionKey == null)
                    {
                        user.SessionKey = this.GenerateSessionKey(user.Id);
                        context.SaveChanges();
                    }

                    var loggedModel = new LoggedUserModel()
                    {
                        UniqueNumber = user.UniqueNumber,
                        SessionKey = user.SessionKey,
                        Name = user.Name
                    };

                    var response = this.Request.CreateResponse(HttpStatusCode.Created,
                                        loggedModel);
                    return response;
                }
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                         ex.Message);
                return response;
            }
        }

        [HttpPut]
        [ActionName("logout")]  //api/users/logout/{sessionKey}
        public HttpResponseMessage PutLogoutUser(string sessionKey)
        {
            try
            {
                var context = new LibrarySystemContext();
                using (context)
                {
                    var user = context.Users.FirstOrDefault(u => u.SessionKey == sessionKey);
                    if (user == null)
                    {
                        throw new ArgumentException("Invalid user authentication.");
                    }

                    user.SessionKey = null;
                    context.SaveChanges();

                    var response = this.Request.CreateResponse(HttpStatusCode.OK);
                    return response;
                }
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                return response;
            }
        }

        [HttpGet]
        [ActionName("getUserByUniqueNumber")]
        public HttpResponseMessage GetUserByUniqueNumber(int uniqueNumber)
        {
            try
            {
                var context = new LibrarySystemContext();
                using (context)
                {
                    var user = context.Users.FirstOrDefault(u => u.UniqueNumber == uniqueNumber);
                    if(user==null)
                    {
                        throw new Exception("There is no such user!");
                    }

                    var booksToReturn =
                        context.UsersBooks.Where(ub => ub.User.UniqueNumber == uniqueNumber && ub.IsReturned == false);

                    var userInfo = new DetailedUserModel()
                                       {
                                           Id = user.Id,
                                           UniqueNumber = user.UniqueNumber,
                                           Name = user.Name,
                                           
                                       };
                    userInfo.BooksToReturn=new Collection<BookToReturnModel>();
                    foreach (var book in booksToReturn)
                    {
                        userInfo.BooksToReturn.Add(new BookToReturnModel()
                                                       {
                                                           Key = book.Book.Key,
                                                           Title = book.Book.Title,
                                                           AuthorName = book.Book.Author.Name,
                                                           DateToreturn = book.DateToReturn,
                                                           Year = book.Book.Year,
                                                           Description = book.Book.Description
                                                       });
                    }

                    var response = this.Request.CreateResponse(HttpStatusCode.OK, userInfo);
                    return response;
                }
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                return response;
            }
        }

        private void ValidateName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("Name cannot be null");
            }
            if (name.Length < MinNameLength)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("Name must be at least {0} characters long",
                    MinNameLength));
            }
        }

        private void ValidateAuthCode(string authCode)
        {
            if (authCode == null || authCode.Length != Sha1Length)
            {
                throw new ArgumentOutOfRangeException("Password should be encrypted");
            }
        }


        private string GenerateSessionKey(int userId)
        {
            StringBuilder skeyBuilder = new StringBuilder(SessionKeyLength);
            skeyBuilder.Append(userId);
            while (skeyBuilder.Length < SessionKeyLength)
            {
                var index = rand.Next(SessionKeyChars.Length);
                skeyBuilder.Append(SessionKeyChars[index]);
            }
            return skeyBuilder.ToString();
        }
    }
}
