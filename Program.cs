using System;
using System.Collections.Generic;
using System.Linq;

namespace LibrarySystem
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }

        public Book(int id, string title, string author, int year)
        {
            Id = id;
            Title = title;
            Author = author;
            Year = year;
        }

        public override string ToString()
        {
            return $"[ID: {Id}] \"{Title}\" - {Author} ({Year})";
        }

        public override bool Equals(object obj)
        {
            if (obj is Book book)
                return Title == book.Title && Author == book.Author;
            return false;
        }

        public override int GetHashCode()
        {
            return (Title + Author).GetHashCode();
        }
    }

    public class User
    {
        public string Name { get; set; }
        public List<Book> BorrowedBooks { get; set; }

        public User(string name)
        {
            Name = name;
            BorrowedBooks = new List<Book>();
        }

        public void BorrowBook(Book book)
        {
            BorrowedBooks.Add(book);
            Console.WriteLine($"\n {Name} взяв книгу: {book.Title}");
        }

        public void ReturnBook(Book book)
        {
            BorrowedBooks.Remove(book);
            Console.WriteLine($"\n {Name} повернув книгу: {book.Title}");
        }

        public void ShowBorrowedBooks()
        {
            if (BorrowedBooks.Count == 0)
            {
                Console.WriteLine("\nУ вас немає взятих книг.");
            }
            else
            {
                Console.WriteLine("\n=== Ваші книги ===");
                foreach (var book in BorrowedBooks)
                {
                    Console.WriteLine(book);
                }
            }
        }
    }

    public class Library
    {
        private List<Book> books;
        private int nextId;

        public Library()
        {
            books = new List<Book>();
            nextId = 1;
        }

        public void AddBook(string title, string author, int year)
        {
            books.Add(new Book(nextId++, title, author, year));
        }

        // Показати всі унікальні книги з кількістю
        public void ShowAllBooks()
        {
            if (books.Count == 0)
            {
                Console.WriteLine("\nБібліотека порожня.");
                return;
            }

            Console.WriteLine("\n");
            Console.WriteLine("КАТАЛОГ КНИГ БІБЛІОТЕКИ");

            var groupedBooks = books.GroupBy(b => new { b.Title, b.Author })
                                   .Select(g => new
                                   {
                                       Book = g.First(),
                                       Count = g.Count()
                                   });

            foreach (var item in groupedBooks)
            {
                Console.WriteLine($"\n {item.Book.Title}");
                Console.WriteLine($"   Автор: {item.Book.Author}");
                Console.WriteLine($"   Рік: {item.Book.Year}");
                Console.WriteLine($"   Доступно примірників: {item.Count}");
            }
        }

        public bool TakeBook(User user, string title)
        {
            var book = books.FirstOrDefault(b => b.Title.ToLower() == title.ToLower());
            
            if (book != null)
            {
                books.Remove(book);
                user.BorrowBook(book);
                return true;
            }
            
            Console.WriteLine("\nError: Така книга відсутня або всі примірники взяті.");
            return false;
        }

        public bool ReturnBook(User user, string title)
        {
            var book = user.BorrowedBooks.FirstOrDefault(b => b.Title.ToLower() == title.ToLower());
            
            if (book != null)
            {
                books.Add(book);
                user.ReturnBook(book);
                return true;
            }
            
            Console.WriteLine("\nError: У вас немає такої книги.");
            return false;
        }

        // Пошук 
        public void SearchBook(string searchTerm)
        {
            var results = books.Where(b =>
                b.Title.ToLower().Contains(searchTerm.ToLower()) ||
                b.Author.ToLower().Contains(searchTerm.ToLower()))
                .GroupBy(b => new { b.Title, b.Author })
                .Select(g => new { Book = g.First(), Count = g.Count() });

            if (results.Any())
            {
                Console.WriteLine("\n Результати пошуку ");
                foreach (var item in results)
                {
                    Console.WriteLine($"{item.Book}; Доступно: {item.Count}");
                }
            }
            else
            {
                Console.WriteLine("\nError: Нічого не знайдено.");
            }
        }
    }

    // меню
    class Program
    {
        static void Main(string[] args)
        {
            // Для роботи з кириллицей 
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.InputEncoding = System.Text.Encoding.Unicode;
            
            Library library = new Library();
            User currentUser = null;

            // Додати тестові книги
            InitializeLibrary(library);

            bool exit = false;

            while (!exit)
            {
                if (currentUser == null)
                {
                    currentUser = LoginMenu();
                }
                else
                {
                    ShowMainMenu();
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            library.ShowAllBooks();
                            break;
                        
                        case "2":
                            Console.Write("\nВведіть назву книги для пошуку: ");
                            string searchTerm = Console.ReadLine();
                            library.SearchBook(searchTerm);
                            break;
                        
                        case "3":
                            Console.Write("\nВведіть назву книги, яку хочете взяти: ");
                            string bookToTake = Console.ReadLine();
                            library.TakeBook(currentUser, bookToTake);
                            break;
                        
                        case "4":
                            currentUser.ShowBorrowedBooks();
                            break;
                        
                        case "5":
                            Console.Write("\nВведіть назву книги, яку хочете повернути: ");
                            string bookToReturn = Console.ReadLine();
                            library.ReturnBook(currentUser, bookToReturn);
                            break;
                        
                        case "6":
                            Console.WriteLine($"\ngoodbye, {currentUser.Name}!");
                            currentUser = null;
                            break;
                        
                        case "0":
                            exit = true;
                            Console.WriteLine("\ngoodbye!");
                            break;
                        
                        default:
                            Console.WriteLine("\nError: Невірний вибір. Спробуйте ще раз.");
                            break;
                    }

                    if (!exit && currentUser != null)
                    {
                        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
                        Console.ReadKey();
                    }
                }
            }
        }

        static User LoginMenu()
        {
            Console.Clear();
            Console.WriteLine("МЕНЮ БІБЛІОТЕКИ");
            Console.Write("\nВведіть ваше ім'я: ");
            string name = Console.ReadLine();
            
            Console.WriteLine($"\nВітаємо, {name}!");
            Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
            Console.ReadKey();
            
            return new User(name);
        }

        static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("                  ГОЛОВНЕ МЕНЮ");
            Console.WriteLine("  1.  Переглянути всі книги");
            Console.WriteLine("  2.  Пошук книги");
            Console.WriteLine("  3.  Взяти книгу");
            Console.WriteLine("  4.  Мої книги  ");
            Console.WriteLine("  5.  Повернути книг");
            Console.WriteLine("  6.  Вийти з акаунту");
            Console.WriteLine("  0.  Вихід з програми ");
            Console.Write("\n");
            Console.Write("\nВи вибрали: ");
        }

        static void InitializeLibrary(Library library)
        {
            library.AddBook("Кобзар", "Тарас Шевченко", 1840);
            library.AddBook("Кобзар", "Тарас Шевченко", 1840);
            library.AddBook("Тіні забутих предків", "Михайло Коцюбинський", 1911);
            library.AddBook("Захар Беркут", "Іван Франко", 1883);
            library.AddBook("Захар Беркут", "Іван Франко", 1883);
            library.AddBook("Zahar Berkut", "Іван Франко", 1883);
            library.AddBook("Лісова пісня", "Леся Українка", 1911);
            library.AddBook("Intermezzo", "Михайло Коцюбинський", 1908);
            library.AddBook("Тигролови", "Іван Багряний", 1944);
            library.AddBook("Тигролови", "Іван Багряний", 1944);
        }
    }
}