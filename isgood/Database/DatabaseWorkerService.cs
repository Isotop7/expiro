namespace isgood.Database;

using System;
using System.Threading;
using System.Threading.Tasks;

using isgood.Models;

public class DatabaseWorkerService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var dbContext = new AppDbContext())
                {
                    while (true)
                    {
                        if (Program.DatabaseQueue.TryDequeue(out Product? product))
                        {
                            if (product == null)
                            {
                                throw new FormatException("Dequeued invalid object");
                            }
                            else
                            {
                                Console.WriteLine($"+ DatabaseWorkerService: Dequeued element with barcode '{product.Barcode}' and saving it to database");
                                dbContext.Products.Add(product);
                                await dbContext.SaveChangesAsync();
                                
                                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                            }
                        }

                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"+ DatabaseWorkerService: Cancellation was requested");
            }
        }
    }
}