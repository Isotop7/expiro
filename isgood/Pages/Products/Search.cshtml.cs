using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using isgood.Models;

namespace isgood.Pages.Products
{
    public class SearchModel : PageModel
    {
        private readonly Database.AppDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty(SupportsGet = true)]
        public string? Query { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();

        public SearchModel(isgood.Database.AppDbContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnGet()
        {
            if (_context.Product != null && !string.IsNullOrWhiteSpace(Query))
            {
                Products = _context.Product.ToList()
                                .Where(p => p.ProductName.Contains(Query, StringComparison.OrdinalIgnoreCase))
                                .ToList() ?? new();
            }
        }
    }
}