using isgood.Models;

namespace isgood.Database;

public enum DatabaseQueueElementAction
{
    INSERT,
    DELETE
}

public class DatabaseQueueElement
{
    public DatabaseQueueElementAction DatabaseQueueElementAction { get; set; }
    public Product Product { get; set; }

    public DatabaseQueueElement(DatabaseQueueElementAction databaseQueueElementAction, Product product)
    {
        DatabaseQueueElementAction = databaseQueueElementAction;
        Product = product;
    }
}   