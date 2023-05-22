using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using isgood.Models;

namespace isgood.Pages.Products;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly isgood.Database.AppDbContext _appDbContext;
    public List<Product> Products { get; set; } = new();

    public IndexModel(ILogger<IndexModel> logger, isgood.Database.AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }

    public async Task OnGetAsync()
    {
        if (_appDbContext.Products != null)
        {
            Products = await _appDbContext.Products.ToListAsync();
        }
    }
}
