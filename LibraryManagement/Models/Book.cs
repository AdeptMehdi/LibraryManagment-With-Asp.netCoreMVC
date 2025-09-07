namespace LibraryManagement.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الزامی است")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "نویسنده الزامی است")]
        public string Author { get; set; } = "";

        public bool IsAvailable { get; set; } = true;

        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }

}
