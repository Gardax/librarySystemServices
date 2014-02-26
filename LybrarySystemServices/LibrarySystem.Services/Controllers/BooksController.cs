using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Services.Models;

namespace LibrarySystem.Services.Controllers
{
    public class BooksController : ApiController
    {
        [HttpPost]
        [ActionName("importBooks")]
        public HttpResponseMessage ImportBooks(ICollection<BookModel> books)
        {
            try
            {
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
                            throw new ArgumentException("There is a book with the same unique number already!");
                        }

                    }
                    context.SaveChanges();
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
                        throw new ArgumentException("There is no book with such unique code!");
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
                        throw new ArgumentException("There is no book with such unique code!");
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
        [ActionName("getLast20Books")]
        public HttpResponseMessage GetLast20Books()
        {
            try
            {
                var context = new LibrarySystemContext();
                using (context)
                {
                    var booksEntity = context.Books.Take(20).OrderByDescending(b => b.Id);

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
            }
            catch(Exception ex)
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest,
                                          ex.Message);
                return response;
            }
        }

        [HttpGet]
        [ActionName("getBookDetailed")]
        public HttpResponseMessage getBookDetailed(string bookKey, int userUniqueNumber)
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
                using (context)
                {
                    if(query==null || query=="")
                    {
                        throw  new ArgumentException("Invalid search!");
                    }

                    IQueryable<Book> booksEntity;
                    if (searchBy == SearchBy.Key)
                    {
                        booksEntity = context.Books.Where(b => b.Key.Contains(query));
                    }
                    else if(searchBy==SearchBy.Author)
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
                                    Year = book.Year,
                                };

                    var response = this.Request.CreateResponse(HttpStatusCode.OK, books);
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
                        throw new ArgumentException("There is no such book!");
                    }
                    if(user==null)
                    {
                        throw new ArgumentException("There is no such user!");
                    }

                    var userBook = context.UsersBooks.FirstOrDefault(ub => ub.Book == book && ub.User == user);
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
                    else
                    {
                        userBook.IsReturned = false;
                        userBook.DateToReturn = DateTime.Now.AddDays(30);
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
                using (context)
                {
                    var booksEntity = context.UsersBooks.Where(ub => ub.User.UniqueNumber == uniqueNumber);

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
                        throw new ArgumentException("There is no such book!");
                    }
                    if (user == null)
                    {
                        throw new ArgumentException("There is no such user!");
                    }

                    var userBook=context.UsersBooks.FirstOrDefault(ub => ub.Book == book && ub.User == user);
                    if (userBook != null)
                    {
                        userBook.IsReturned = true;
                        context.SaveChanges();
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
                        throw new ArgumentException("There is no such book!");
                    }
                    if (user == null)
                    {
                        throw new ArgumentException("There is no such user!");
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
