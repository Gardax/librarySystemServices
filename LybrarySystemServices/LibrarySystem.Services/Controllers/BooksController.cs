using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Services.Models;

namespace LibrarySystem.Services.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class BooksController : ApiController
    {
        [HttpPost]
        [ActionName("importBooks")]
        public HttpResponseMessage ImportBooks(ICollection<BookModel> books)
        {
            try
            {
                int errors = 0;
                var context = new LibrarySystemContext();
                using(context)
                {
                    foreach (var book in books)
                    {
                        var author = context.Authors.FirstOrDefault(a => a.Name == book.AuthorName);
                        if(author==null)
                        {
                            author=context.Authors.Add(new Author()
                                                    {
                                                        Name = book.AuthorName
                                                    });
                            context.SaveChanges();
                        }

                        var existBook = context.Books.FirstOrDefault(b => b.Key == book.Key);
                        if(existBook==null)
                        {
                            context.Books.Add(new Book()
                                                  {
                                                      Title = book.Title,
                                                      Author = author,
                                                      Key = book.Key,
                                                      Year = book.Year,
                                                      Description = book.Description
                                                  });
                        }
                        else
                        {
                            errors++;
                        }

                    }
                    context.SaveChanges();
                    
                }
                if (errors > 0)
                {
                    throw new Exception("Има книга със същото заглавие и тя НЕ е заменена!");
                }
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                          ex.Message);
                return response;
            }
        }

        [HttpGet]
        [ActionName("deleteBook")]
        public HttpResponseMessage DeleteBook(string key)
        {
            try
            {
                var context = new LibrarySystemContext();
                using(context)
                {
                    var book = context.Books.FirstOrDefault(b => b.Key == key);
                    if(book==null)
                    {
                        throw new ArgumentException("Няма книга с такъв уникален номер!");
                    }
                    context.Books.Remove(book);
                    context.SaveChanges();
                }

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch(Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                          ex.Message);
                return response;
            }
        }

        [HttpPost]
        [ActionName("modifyBook")]
        public HttpResponseMessage ModifyBook(BookModel model, string key)
        {
            try
            {
                var context = new LibrarySystemContext();
                using (context)
                {
                    var book = context.Books.FirstOrDefault(b => b.Key == key);
                    if (book == null)
                    {
                        throw new ArgumentException("Няма книга с такъв уникален номер!");
                    }

                    var author = context.Authors.FirstOrDefault(a => a.Name == model.AuthorName);
                    if (author == null)
                    {
                        context.Authors.Add(new Author()
                        {
                            Name = model.AuthorName
                        });
                        context.SaveChanges();
                    }

                    book.Title = model.Title;
                    book.Key = model.Key;
                    book.Author = author;
                    book.Year = model.Year;
                    book.Description = model.Description;

                    context.SaveChanges();
                }

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.NotModified,
                                          ex.Message);
                return response;
            }
        }

        [HttpGet]
        [ActionName("get20BooksFrom")]
        public HttpResponseMessage GetLast20Books(int start)
        {
            try
            {
                var context = new LibrarySystemContext();

                var booksEntity = context.Books.OrderByDescending(b => b.Id).Skip(start).Take(20);

                    var books = from book in booksEntity
                                select new BookModel()
                                           {
                                               Title = book.Title,
                                               AuthorName = book.Author.Name,
                                               Description = book.Description,
                                               Key = book.Key,
                                               Year = book.Year,
                                           };

                    var response = this.Request.CreateResponse(HttpStatusCode.OK, books);
                    return response;
                
            }
            catch(Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                          ex.Message);
                return response;
            }
        }

        [HttpGet]
        [ActionName("getBooksThatMustBeReturned")]
        public HttpResponseMessage GetLBooksThatMustBeReturned()
        {
            try
            {
                var context = new LibrarySystemContext();

                var booksEntity = context.UsersBooks.Where(ub => ub.DateToReturn < DateTime.Now);

                var books = from book in booksEntity
                            select new BookToReturnModel()
                                       {
                                           Title = book.Book.Title,
                                           AuthorName = book.Book.Author.Name,
                                           Description = book.Book.Description,
                                           Key = book.Book.Key,
                                           Year = book.Book.Year,
                                           UserUniqueNumber = book.User.UniqueNumber,
                                           UserName = book.User.Name,
                                           DateToreturn = book.DateToReturn
                                       };

                var response = this.Request.CreateResponse(HttpStatusCode.OK, books);
                return response;

            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                                           ex.Message);
                return response;
            }
        }

        [HttpGet]
        [ActionName("getBookDetailed")]
        public HttpResponseMessage GetBookDetailed(string bookKey)
        {
            try
            {
                var context = new LibrarySystemContext();
                using (context)
                {
                    var book = context.Books.FirstOrDefault(b => b.Key == bookKey);

                    var theBook = new BookDetailsModel()
                                      {
                                          Title = book.Title,
                                          AuthorName = book.Author.Name,
                                          Description = book.Description,
                                          Key = book.Key,
                                          Year = book.Year,
                                      };

                    foreach (var note in book.Notes)
                    {
                        theBook.Notes.Add(new NoteModel()
                                              {
                                                  Text = note.Text
                                              });
                    }

                    var response = this.Request.CreateResponse(HttpStatusCode.OK, theBook);
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

        [HttpGet]
        [ActionName("search")]
        public HttpResponseMessage SearchForBook(string query, SearchBy searchBy=SearchBy.All)
        {
            try
            {
                var context = new LibrarySystemContext();

                if (query == null || query == "")
                {
                    throw new ArgumentException("Невалидно търсене!");
                }

                IQueryable<Book> booksEntity;
                if (searchBy == SearchBy.Key)
                {
                    booksEntity = context.Books.Where(b => b.Key.Contains(query));
                }
                else if (searchBy == SearchBy.Author)
                {
                    booksEntity = context.Books.Where(b => b.Author.Name.Contains(query));
                }
                else if (searchBy == SearchBy.Title)
                {
                    booksEntity = context.Books.Where(b => b.Title.Contains(query));
                }
                else
                {
                    booksEntity = context.Books.Where(b => b.Key.Contains(query) || b.Author.Name.Contains(query)
                                                           || b.Title.Contains(query));
                }

                var books = from book in booksEntity
                            select new BookModel()
                                       {
                                           Title = book.Title,
                                           AuthorName = book.Author.Name,
                                           Description = book.Description,
                                           Key = book.Key,
                                           Year = book.Year
                                       };

                var response = this.Request.CreateResponse(HttpStatusCode.OK, books);
                return response;

            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                           ex.Message);
                return response;
            }
            
        }

        [HttpGet]
        [ActionName("getBook")]
        public HttpResponseMessage GetBook(string bookCode, int userNumber)
        {
            try
            {
                var context = new LibrarySystemContext();
                using (context)
                {
                    var book = context.Books.FirstOrDefault(b => b.Key == bookCode);
                    var user = context.Users.FirstOrDefault(u => u.UniqueNumber == userNumber);
                    if(book==null)
                    {
                        throw new ArgumentException("Няма такава книга!");
                    }
                    if(user==null)
                    {
                        throw new ArgumentException("Няма такъв потребител!");
                    }

                    var userBook = context.UsersBooks.FirstOrDefault(ub => ub.Book.Key == book.Key
                        && ub.User.UniqueNumber == user.UniqueNumber);
                    if (userBook == null)
                    {
                        context.UsersBooks.Add(new UserBook()
                                                   {
                                                       Book = book,
                                                       User = user,
                                                       IsReturned = false,
                                                       DateToReturn = DateTime.Now.AddDays(30)
                                                   });
                    }
                    else if(userBook.IsReturned==true)
                    {
                        userBook.IsReturned = false;
                        userBook.DateToReturn = DateTime.Now.AddDays(30);
                    }
                    else
                    {
                        throw new Exception("Тази книга вече е взета!");
                    }
                    context.SaveChanges();

                    var response = this.Request.CreateResponse(HttpStatusCode.OK);
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

        [HttpGet]
        [ActionName("getGotBooksFromUser")]
        public HttpResponseMessage GetGotBooksFromUser(int uniqueNumber)
        {
            try
            {
                var context = new LibrarySystemContext();
                
                    var booksEntity = context.UsersBooks.Where(ub => ub.User.UniqueNumber == uniqueNumber)
                        .OrderByDescending(ub=>ub.DateToReturn);

                    var books = from book in booksEntity
                                select new BookToReturnModel()
                                           {
                                               Title = book.Book.Title,
                                               AuthorName = book.Book.Author.Name,
                                               Description = book.Book.Description,
                                               Key = book.Book.Key,
                                               Year = book.Book.Year,
                                               DateToreturn = book.DateToReturn
                                           };

                    var response = this.Request.CreateResponse(HttpStatusCode.OK, books);
                    return response;
                
            }
            catch (Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                          ex.Message);
                return response;
            }
        }

        [HttpGet]
        [ActionName("returnBook")]
        public HttpResponseMessage ReturnBook(string bookCode, int userNumber)
        {
            try
            {
                var context = new LibrarySystemContext();
                using (context)
                {
                    var book = context.Books.FirstOrDefault(b => b.Key == bookCode);
                    var user = context.Users.FirstOrDefault(u => u.UniqueNumber == userNumber);
                    if (book == null)
                    {
                        throw new ArgumentException("Няма такава книга!");
                    }
                    if (user == null)
                    {
                        throw new ArgumentException("Няма такъв потребител!");
                    }

                    var userBook=context.UsersBooks.FirstOrDefault(ub => ub.Book.Key == book.Key && 
                        ub.User.UniqueNumber == user.UniqueNumber);
                    if (userBook != null && userBook.IsReturned==false)
                    {
                        userBook.IsReturned = true;
                        context.SaveChanges();
                    }
                    else 
                    {
                        throw new Exception("Тази книга не е взета от този потребител!");
                    }
                   

                    var response = this.Request.CreateResponse(HttpStatusCode.OK);
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

        [HttpPost]
        [ActionName("AddNote")]
        public HttpResponseMessage AddNote(NoteModel model)
        {
            try
            {
                var context = new LibrarySystemContext();
                using (context)
                {
                    var book = context.Books.FirstOrDefault(b => b.Key == model.BookKey);
                    var user = context.Users.FirstOrDefault(u => u.UniqueNumber == model.UserUniqueNumber);
                    if (book == null)
                    {
                        throw new ArgumentException("Няма такава книга!");
                    }
                    if (user == null)
                    {
                        throw new ArgumentException("Няма такъв потребител!");
                    }

                    book.Notes.Add(new Note()
                                       {
                                           User = user,
                                           Text = model.Text
                                       });

                    context.SaveChanges();

                    var response = this.Request.CreateResponse(HttpStatusCode.OK);
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

        public enum SearchBy
        {
            Title,
            Author,
            Key,
            All
        };
    }
}
