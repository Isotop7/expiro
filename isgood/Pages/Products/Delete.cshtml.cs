using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using isgood.Models;

namespace isgood.Pages.Products
{
    public class DeleteModel : PageModel
    {
        private readonly isgood.Database.AppDbContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(isgood.Database.AppDbContext context, ILogger<DeleteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Product Product { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (_context == null || _context.Product == null)
            {
                return NotFound();
            }

            Product? product = await _context.Product.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            
            if (product == null)
            {
                return NotFound();
            }
            else
            {
                Product = product;
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ErrorMessage = $"Delete {Product.Id} failed. Try again";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            try
            {
                _context.Product.Remove(product);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, ErrorMessage);

                return RedirectToAction("./Delete",
                                     new { id, saveChangesError = true });
            }
        }
    }
}