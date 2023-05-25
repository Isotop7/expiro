using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using isgood.Models;

namespace isgood.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly isgood.Database.AppDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public Product Product { get; set; } = new();

        public CreateModel(isgood.Database.AppDbContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var emptyProduct = new Product();

            if (_context.Product == null)
            {
                return Page();
            }

            if (await TryUpdateModelAsync<Product>(
                emptyProduct,
                "product",   // Prefix for form value.
                p => p.Barcode, p => p.BestBefore))
            {
                Program.APIQueue.Enqueue(emptyProduct);
                return RedirectToPage("./Index");
            }

            return Page();
        }
    }
}