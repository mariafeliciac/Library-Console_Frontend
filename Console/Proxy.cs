using Microsoft.Extensions.Configuration;
using Model;
using Model.Interfaces;
using Model.Model;
using Model.ModelDto.BookDto;
using Model.ModelDto.ReservationDto;
using Model.ModelDto.UserDto;
using System.Net.Http.Json;

namespace Console
{
    internal class Proxy : IProxy
    {
        private readonly HttpClient _httpClient;
        public Proxy(IHttpClientFactory httpClientFactory, IConfigurationRoot configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(configuration["Uri"]);
        }
        public ResultAddBook AddBook(IBook book)
        {
            HttpResponseMessage response = _httpClient.PostAsJsonAsync("api/Book", book).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Content.ReadFromJsonAsync<ResultAddBook>().Result;
            }

            return 0;
        }

        public ReservationResult BookReturn(int bookid, int userid)
        {
            ReservationResult reservation = new ReservationResult();
            string queryString = $"?bookId={bookid}&userId={userid}";

            var response = _httpClient.PutAsync($"api/Reservation{queryString}", null).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var reservationResponse = response.Content.ReadFromJsonAsync<UpdateReservationResponse>().Result;

                reservation.Book = new Book { Title = reservationResponse.Title };
                reservation.Result = reservationResponse.Result;
                return reservation;
            }
            return null;
        }

        public void DeleteBook(int bookid)
        {
            string queryString = $"?bookId={bookid}";
            var response = _httpClient.DeleteAsync($"api/Book{queryString}").Result;

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var messages = response.Content.ReadFromJsonAsync<List<string>>().Result;

                List<Exception> exceptions = new List<Exception>();

                if(messages!= null && messages.Count > 0)
                {
                    foreach (var message in messages)
                    {
                        exceptions.Add(new Exception(message));
                    }
                }
                
                throw new AggregateException(exceptions);
            }
        }

        public List<ReservationViewModel> GetReservationHistory(int? bookid, int? userid, ReservationStatus? reservationStatus)
        {
            string queryString = $"?bookId={bookid}&userId={userid}&reservationStatus={reservationStatus}";

            var response = _httpClient.GetAsync($"api/Reservation{queryString}").Result;

            List<ReservationViewModel> reservationsViewModel = new List<ReservationViewModel>();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response.Content.ReadFromJsonAsync<List<GetReservationsResponse>>().Result.ForEach(r => reservationsViewModel.Add(new ReservationViewModel()
                {
                    Reservation = new Reservation { ReservationId = r.ReservationId, StartDate = r.StartDate, EndDate = r.EndDate },
                    Book = new Book
                    {
                        Title = r.Title,
                    },
                    User = new User { Username = r.Username },
                    ReservationStatus = r.ReservationStatus
                }));

                return reservationsViewModel;
            }

            return reservationsViewModel;

        }

        public IUser GetUserByUserName(string userName)
        {
            string queryString = $"?username={userName}";

            var response = _httpClient.GetAsync($"api/User{queryString}").Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var user = response.Content.ReadFromJsonAsync<User>().Result;

                return new User
                {
                    UserId = user.UserId,
                    Username = user.Username
                };
            }
            return null;
        }

        public IUser Login(string username, string password)
        {
            IUser userLogin = new User();
            var request = new UserLoginRequest
            {
                Username = username,
                Password = password
            };
            var response = _httpClient.PostAsJsonAsync("api/User/Login", request).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var userResponse = response.Content.ReadFromJsonAsync<UserLoginResponse>().Result;
                userLogin.UserId = userResponse.UserId;
                userLogin.Username = userResponse.Username;
                userLogin.Role = userResponse.Role;

                return userLogin;
            }

            return null;
        }

        public ReservationResult ReserveBook(int bookid, int userid)
        {
            ReservationResult reservation = new ReservationResult();
            string queryString = $"?bookId={bookid}&userId={userid}";

            var response = _httpClient.PostAsync($"api/Reservation{queryString}",null).Result;

            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var reservationResponse = response.Content.ReadFromJsonAsync<CreateReservationResponse>().Result;

                reservation.User = reservationResponse.User;
                reservation.Book = reservationResponse.Book;
                reservation.Result = reservationResponse.Result;
                reservation.EndDateReservation = reservationResponse.EndDateReservation;
                return reservation;
            }
            return null;
        }

        public IEnumerable<IBook> SearchBook(IBook book)
        {
            string queryString = $"?title={book.Title}&authorname={book.AuthorName}&authorsurname={book.AuthorSurname}&publishingHouse={book.PublishingHouse}";

            var response = _httpClient.GetAsync($"api/Book{queryString}").Result;

            var listBooks = new List<Book>();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response.Content.ReadFromJsonAsync<List<BookSearchResponse>>().Result.ForEach(r => listBooks.Add(new Book
                {
                    Title = r.Title,
                    AuthorName = r.AuthorName,
                    AuthorSurname = r.AuthorSurname,
                    BookId = r.BookId,
                    PublishingHouse = r.PublishingHouse,
                    Quantity = r.Quantity
                }));
            }

            return listBooks;
        }

        public List<BookViewModel> SearchBookWithAvailabilityInfos(IBook book)
        {
            string queryString = $"?title={book.Title}&authorname={book.AuthorName}&authorsurname={book.AuthorSurname}&publishingHouse={book.PublishingHouse}";

            var response = _httpClient.GetAsync($"api/Book/BooksWithAvailability{queryString}").Result;

            var listBooks = new List<BookViewModel>();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response.Content.ReadFromJsonAsync<List<SearchBookWithAvailabilityeResponse>>().Result.ForEach(r => listBooks.Add(new BookViewModel
                {
                    Book = new Book()
                    {
                        BookId = r.BookId,
                        Title = r.Title,
                        AuthorName = r.AuthorName,
                        AuthorSurname = r.AuthorSurname,
                        PublishingHouse = r.PublishingHouse,
                        Quantity = r.Quantity
                    },
                    AvailabilityBook = r.AvailabilityBook,
                    AvailabilityDate = r.AvailabilityDate,

                }));
                return listBooks;
            }
            return listBooks;
        }

        public void UpdateBook(int bookid, IBook bookWithNewValues)
        {
            string queryString = $"?lastBookId={bookid}";

            var bookAddRequest = new BookEditRequest
            {
                Title = bookWithNewValues.Title,
                AuthorName= bookWithNewValues.AuthorName,
                AuthorSurname= bookWithNewValues.AuthorSurname,
                PublishingHouse= bookWithNewValues.PublishingHouse
            };
            var response = _httpClient.PutAsJsonAsync<BookEditRequest>($"api/Book{queryString}", bookAddRequest).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var message = response.Content.ReadAsStringAsync().Result;
                throw new Exception(message);
            }

        }
    }
}
