
namespace LibrarySystem.Models
{
    public class Note
    {
        public int Id { get; set; }

        public virtual Book Book { get; set; }

        public virtual User User { get; set; }

        public string Text { get; set; }
    }
}
