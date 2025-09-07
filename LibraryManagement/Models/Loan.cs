namespace LibraryManagement.Models
{
    public class Loan
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }

        public Book Book { get; set; }

        public ApplicationUser User { get; set; } // حتماً navigation property داشته باش
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }


}
