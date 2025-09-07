using System.Collections.Generic;
using LibraryManagement.Models;

namespace LibraryManagement.Models
{
    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int AvailableBooks { get; set; }
        public int LoanedBooks { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveLoans { get; set; }
        public List<Loan> RecentLoans { get; set; }
    }
}
