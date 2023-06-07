namespace isgood.Configuration;

public class ProductConfiguration
{
    public int? BestBeforeWarnDelta { get; set; }

    public ProductConfiguration()
    {
    }

    public bool IsValid() => true;
}