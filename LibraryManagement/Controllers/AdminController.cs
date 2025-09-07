using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;


namespace LibraryManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly LibraryContext _context;

        public AdminController(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel
            {
                TotalBooks = await _context.Books.CountAsync(),
                AvailableBooks = await _context.Books.CountAsync(b => b.IsAvailable),
                LoanedBooks = await _context.Books.CountAsync(b => !b.IsAvailable),
                TotalUsers = await _context.Users.CountAsync(),
                ActiveLoans = await _context.Loans.CountAsync(l => l.ReturnDate == null),
                RecentLoans = await _context.Loans
                    .Include(l => l.Book)
                    .Include(l => l.User)
                    .OrderByDescending(l => l.LoanDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}