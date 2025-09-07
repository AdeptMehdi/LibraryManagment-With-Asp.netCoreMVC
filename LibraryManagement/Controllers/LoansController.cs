using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagement.Controllers
{
    public class LoansController : Controller
    {
        private readonly LibraryContext _context;

        public LoansController(LibraryContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private readonly UserManager<ApplicationUser> _userManager;


        // GET: Loans
        public async Task<IActionResult> Index(string statusFilter = "All")
        {
            var loansQuery = _context.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .AsQueryable();

            // فیلتر وضعیت
            switch (statusFilter)
            {
                case "Returned":
                    loansQuery = loansQuery.Where(l => l.ReturnDate.HasValue);
                    break;
                case "Pending":
                    loansQuery = loansQuery.Where(l => !l.ReturnDate.HasValue);
                    break;
                case "All":
                default:
                    break;
            }

            ViewData["StatusFilter"] = statusFilter;
            var loans = await loansQuery.OrderByDescending(l => l.LoanDate).ToListAsync();
            return View(loans);
        }

        [Authorize(Roles = "Admin")]
        // GET: Loans/Create
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.IsAvailable), "Id", "Title");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }
        [Authorize(Roles = "Admin")]
        // POST: Loans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,UserId,LoanDate,ReturnDate")] Loan loan)
        {
            var book = await _context.Books.FindAsync(loan.BookId);
            if (book == null || !book.IsAvailable)
            {
                ModelState.AddModelError("", "این کتاب در حال حاضر در دسترس نیست.");
                ViewData["BookId"] =
                    new SelectList(_context.Books.Where(b => b.IsAvailable), "Id", "Title", loan.BookId);
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", loan.UserId);
                return View(loan);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                book.IsAvailable = false;
                _context.Update(book);

                _context.Add(loan);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "خطا در ثبت امانت کتاب.");
                return View(loan);
            }
        }
        [Authorize(Roles = "Admin")]
        // GET: Loans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.IsAvailable || b.Id == loan.BookId), "Id", "Title", loan.BookId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", loan.UserId);

            return View(loan);
        }

        // POST: Loans/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BookId,UserId,LoanDate,ReturnDate")] Loan loan)
        {
            if (id != loan.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.IsAvailable || b.Id == loan.BookId), "Id", "Title", loan.BookId);
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", loan.UserId);
                return View(loan);
            }

            var oldLoan = await _context.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == id);
            if (oldLoan == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // اگر کتاب تغییر کرد، وضعیت کتاب‌ها رو درست کنیم
                if (oldLoan.BookId != loan.BookId)
                {
                    var oldBook = await _context.Books.FindAsync(oldLoan.BookId);
                    if (oldBook != null) oldBook.IsAvailable = true;

                    var newBook = await _context.Books.FindAsync(loan.BookId);
                    if (newBook != null) newBook.IsAvailable = false;
                }

                oldLoan.BookId = loan.BookId;
                oldLoan.UserId = loan.UserId;
                oldLoan.LoanDate = loan.LoanDate;
                oldLoan.ReturnDate = loan.ReturnDate;

                _context.Update(oldLoan);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "خطا در ویرایش امانت.");
                ViewData["BookId"] = new SelectList(_context.Books.Where(b => b.IsAvailable || b.Id == loan.BookId), "Id", "Title", loan.BookId);
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", loan.UserId);
                return View(loan);
            }
        }
        
        // GET: Loans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);


            if (loan == null) return NotFound();

            return View(loan);
        }
        [Authorize(Roles = "Admin")]
        // GET: Loans/Return/5
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            return View(loan);
        }
        [Authorize(Roles = "Admin")]
        // POST: Loans/Return/5
        [HttpPost, ActionName("Return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnConfirmed(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // به‌روزرسانی وضعیت کتاب
                loan.Book.IsAvailable = true;
                _context.Update(loan.Book);

                // ثبت تاریخ بازگشت
                loan.ReturnDate = DateTime.Now;
                _context.Update(loan);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "خطا در بازگرداندن کتاب.");
                return View(loan);
            }
        }

        // GET: Loans/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            return View(loan);
        }

        // POST: Loans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan != null)
            {
                // اگر میخوای همزمان کتاب هم آزاد بشه:
                loan.Book.IsAvailable = true;
                _context.Loans.Remove(loan);
                _context.Update(loan.Book);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "User,Admin")]
        // GET: Loans/MyLoans
        public async Task<IActionResult> MyLoans()
        {
            // گرفتن کاربر فعلی
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // اگه کاربر لاگین نکرده باشه
            }

            // گرفتن فقط قرض‌هایی که کاربر گرفته
            var loans = await _context.Loans
                .Include(l => l.Book)
                .Where(l => l.UserId == user.Id)
                .ToListAsync();

            return View(loans);
        }



    }
}





