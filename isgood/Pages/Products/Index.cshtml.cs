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
    public readonly isgood.Configuration.ProductConfiguration ProductConfiguration;

    public IndexModel(ILogger<IndexModel> logger, isgood.Database.AppDbContext appDbContext, isgood.Configuration.ProductConfiguration productConfiguration)
    {
        _logger = logger;
        _appDbContext = appDbContext;
        ProductConfiguration = productConfiguration;
    }

    public async Task OnGetAsync()
    {
        if (_appDbContext.Product != null)
        {
            Products = await _appDbContext.Product.ToListAsync();
        }
    }
}
