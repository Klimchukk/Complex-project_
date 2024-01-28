using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Інтерфейс IRateAndCopy визначає об'єкти з властивістю Rating та методом DeepCopy.
interface IRateAndCopy
{
    double Rating { get; }
    object DeepCopy();
}

// Клас Person представляє особу з ім'ям. Реалізує порівняння та клонування.
class Person
{
    public string Name { get; set; }

    // Перевизначення методів Equals та GetHashCode для порівняння об'єктів за іменем.
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Person otherPerson = (Person)obj;
        return Name == otherPerson.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    // Використовується для клонування об'єкта Person.
    public virtual object DeepCopy()
    {
        return new Person { Name = this.Name };
    }
}

// Клас Article представляє статтю з назвою та рейтингом. Реалізує клонування.
class Article : IRateAndCopy
{
    public string Title { get; set; }
    public double Rating { get; set; }

    // Використовується для клонування об'єкта Article.
    public object DeepCopy()
    {
        return new Article { Title = this.Title, Rating = this.Rating };
    }
}

// Клас Edition представляє видання з назвою, датою виходу та тиражем. Реалізує порівняння та клонування.
class Edition
{
    public string Title { get; set; }
    public DateTime ReleaseDate { get; set; }
    private int circulation;

    // Властивість Circulation з перевіркою на від'ємні значення.
    public int Circulation
    {
        get { return circulation; }
        set
        {
            if (value < 0)
                throw new ArgumentException("Circulation cannot be negative.");
            circulation = value;
        }
    }

    // Перевизначення методів Equals та GetHashCode для порівняння об'єктів за назвою, датою та тиражем.
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        Edition otherEdition = (Edition)obj;
        return Title == otherEdition.Title && ReleaseDate == otherEdition.ReleaseDate && Circulation == otherEdition.Circulation;
    }

    public override int GetHashCode()
    {
        return (Title + ReleaseDate.ToString() + Circulation.ToString()).GetHashCode();
    }

    // Використовується для клонування об'єкта Edition.
    public virtual object DeepCopy()
    {
        return new Edition { Title = this.Title, ReleaseDate = this.ReleaseDate, Circulation = this.Circulation };
    }
}

// Клас Magazine представляє журнал, який є виданням з рейтингом та списком статей та редакторів.
class Magazine : Edition, IRateAndCopy, IEnumerable<Article>
{
    // Перерахування Frequency визначає частоту виходу журналу.
    public enum Frequency
    {
        Weekly,
        Monthly,
        Quarterly
    }

    public Frequency MagFrequency { get; set; }
    private List<Person> editors;
    private List<Article> articles;

    // Конструктор, що ініціалізує списки редакторів та статей.
    public Magazine()
    {
        editors = new List<Person>();
        articles = new List<Article>();
    }

    // Рейтинг журналу - середній рейтинг усіх статей.
    public double Rating
    {
        get
        {
            if (articles.Count == 0)
                return 0;

            double totalRating = 0;
            foreach (var article in articles)
            {
                totalRating += article.Rating;
            }

            return totalRating / articles.Count;
        }
    }

    // Додавання статей та редакторів до списків.
    public void AddArticles(params Article[] newArticles)
    {
        articles.AddRange(newArticles);
    }

    public void AddEditors(params Person[] newEditors)
    {
        editors.AddRange(newEditors);
    }

    // Реалізація інтерфейсу IEnumerable<Article> для можливості ітерації по статях.
    public IEnumerator<Article> GetEnumerator()
    {
        return articles.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // Клонування об'єкта Magazine.
    public override object DeepCopy()
    {
        Magazine copyMagazine = new Magazine
        {
            MagFrequency = this.MagFrequency,
            editors = new List<Person>(this.editors.Select(e => (Person)e.DeepCopy())),
            articles = new List<Article>(this.articles.Select(a => (Article)a.DeepCopy()))
        };
        copyMagazine.Title = this.Title;
        copyMagazine.ReleaseDate = this.ReleaseDate;
        copyMagazine.Circulation = this.Circulation;

        return copyMagazine;
    }

    // Перевизначення методу ToString для зручного виведення інформації про журнал.
    public override string ToString()
    {
        return $"Magazine: {Title}, Frequency: {MagFrequency}, Release Date: {ReleaseDate}, Circulation: {Circulation}";
    }

    //Вивід короткої інформації про журнал.
    public string ToShortString()
    {
        return $"Magazine: {Title}, Frequency: {MagFrequency}, Average Rating: {Rating}";
    }

    // Властивість Quality визначає якість журналу, рахуючи середній рейтинг на тираж.
    public double Quality
    {
        get { return Rating / Circulation; }
    }

    // Методи для отримання підмножини статей за певними критеріями.
    public IEnumerable<Article> GetArticlesWithRatingGreaterThan(double rating)
    {
        return articles.Where(article => article.Rating > rating);
    }

    public IEnumerable<Article> GetArticlesWithTitleContaining(string keyword)
    {
        return articles.Where(article => article.Title.Contains(keyword));
    }

    // Методи для отримання підмножини статей та редакторів за певними критеріями.
    public IEnumerable<Article> GetArticlesByEditors()
    {
        return articles.Where(article => editors.Any(editor => article.Title.Contains(editor.Name)));
    }

    public IEnumerable<Person> GetEditorsWithArticles()
    {
        return editors.Where(editor => articles.Any(article => article.Title.Contains(editor.Name)));
    }

    public IEnumerable<Person> GetEditorsWithoutArticles()
    {
        return editors.Where(editor => !articles.Any(article => article.Title.Contains(editor.Name)));
    }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // 1. Створення двох об'єктів типу Edition
            Edition edition1 = new Edition { Title = "Edition 1", ReleaseDate = DateTime.Now, Circulation = 1000 };
            Edition edition2 = new Edition { Title = "Edition 1", ReleaseDate = DateTime.Now, Circulation = 1000 };

            // Перевірка, що посилання на об'єкти не рівні, а об'єкти рівні
            Console.WriteLine($"edition1 ReferenceEquals edition2: {ReferenceEquals(edition1, edition2)}");
            Console.WriteLine($"edition1 equals edition2: {edition1.Equals(edition2)}");
            Console.WriteLine($"HashCode edition1: {edition1.GetHashCode()}");
            Console.WriteLine($"HashCode edition2: {edition2.GetHashCode()}");

            // 2. Блок try/catch для перехоплення некоректного значення тиражу видання
            try
            {
                edition1.Circulation = -100; // Призначення некоректного значення
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            // 3. Створення об'єкту типу Magazine, додавання елементів та вивід даних
            Magazine magazine = new Magazine
            {
                Title = "Magazine 1",
                ReleaseDate = DateTime.Now,
                Circulation = 5000,
                MagFrequency = Magazine.Frequency.Monthly
            };

            Person editor1 = new Person { Name = "Editor 1" };
            Person editor2 = new Person { Name = "Editor 2" };

            Article article1 = new Article { Title = "Article 1", Rating = 4.5 };
            Article article2 = new Article { Title = "Article 2", Rating = 3.8 };

            magazine.AddEditors(editor1, editor2);
            magazine.AddArticles(article1, article2);

            Console.WriteLine("\nMagazine Data:");
            Console.WriteLine(magazine.ToString());

            // 4. Виведення значення якості типу Edition для об'єкта типу Magazine
            Console.WriteLine($"\nQuality of Magazine (Edition): {magazine.Quality}");

            // 5. DeepCopy та зміна даних вихідного об'єкта
            Magazine copyMagazine = (Magazine)magazine.DeepCopy();
            magazine.Title = "Modified Magazine";
            magazine.ReleaseDate = DateTime.Now.AddMonths(1);
            magazine.Circulation = 7000;

            Console.WriteLine("\nOriginal Magazine Data:");
            Console.WriteLine(magazine.ToString());

            Console.WriteLine("\nDeep Copy of Magazine Data:");
            Console.WriteLine(copyMagazine.ToString());

            // 6. Виведення списку статей з рейтингом більше певного значення
            Console.WriteLine("\nArticles with Rating > 4.0:");
            foreach (var article in magazine.GetArticlesWithRatingGreaterThan(4.0))
            {
                Console.WriteLine(article.ToString());
            }

            // 7. Виведення списку статей, у назві яких є заданий рядок
            Console.WriteLine("\nArticles with Title containing 'Article':");
            foreach (var article in magazine.GetArticlesWithTitleContaining("Article"))
            {
                Console.WriteLine(article.ToString());
            }

            // Додаткове завдання:

            // 8. Виведення списку статей, автори яких є редакторами журналу
            Console.WriteLine("\nArticles by Editors:");
            foreach (var article in magazine.GetArticlesByEditors())
            {
                Console.WriteLine(article.ToString());
            }

            // 9. Виведення списку редакторів журналу, які мають статі
            Console.WriteLine("\nEditors with Articles:");
            foreach (var editor in magazine.GetEditorsWithArticles())
            {
                Console.WriteLine(editor.Name);
            }

            // 10. Виведення списку редакторів, які не мають статей у журналі
            Console.WriteLine("\nEditors without Articles:");
            foreach (var editor in magazine.GetEditorsWithoutArticles())
            {
                Console.WriteLine(editor.Name);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        Console.ReadLine();

    }
}
