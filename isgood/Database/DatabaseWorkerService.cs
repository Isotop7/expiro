using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using isgood.Models;

namespace isgood.Database;

public class DatabaseWorkerService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                if (Program.DatabaseQueue.TryDequeue(out DatabaseQueueElement? databaseQueueElement))
                {
                    if (databaseQueueElement == null)
                    {
                        throw new FormatException("Dequeued invalid object");
                    }
                    
                    Product product = databaseQueueElement.Product;
                    switch (databaseQueueElement.DatabaseQueueElementAction)
                    {
                        case DatabaseQueueElementAction.INSERT:
                            {
                                Console.WriteLine($"+ DatabaseWorkerService: Dequeued element with barcode '{product.Barcode}' and saving it to database");
                                await InsertProduct(product, cancellationToken);
                                break;
                            }
                        case DatabaseQueueElementAction.DELETE:
                            {
                                Console.WriteLine($"+ DatabaseWorkerService: Dequeued element with barcode '{product.Barcode}' and removing it from database");
                                await RemoveProduct(product, cancellationToken);
                                break;
                            }
                    }
                }
                
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"+ DatabaseWorkerService: Cancellation was requested");
        }
    }

    private static async Task InsertProduct(Product product, CancellationToken cancellationToken)
    {
        using (var dbContext = new AppDbContext())
        {
            dbContext.Product.Add(product);
            await dbContext.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"+ DatabaseWorkerService: Saved element with barcode '{product.Barcode}' to database");
        }
    }

    private static async Task RemoveProduct(Product product, CancellationToken cancellationToken)
    {
        using (var dbContext = new AppDbContext())
        {
            Product matchingProduct = dbContext.Product.Where(p => p.Barcode == product.Barcode).First();
            dbContext.Product.Remove(matchingProduct);
            await dbContext.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"+ DatabaseWorkerService: Removed element with barcode '{product.Barcode}' from database");
        }
    }
}