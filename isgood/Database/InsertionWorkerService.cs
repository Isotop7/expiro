namespace isgood;

using System;
using System.Threading.Tasks;

public class InsertionWorkerService
{
    public async Task Start()
    {
        await Task.Run(async () =>
        {
            using (var dbContext = new AppDbContext())
            {
                while (true)
                {
                    if (Program.ElementQueue.TryDequeue(out Product product))
                    {
                        Console.WriteLine($"+ InsertionWorkerService: Dequeued element with barcode '{product.Barcode}' and saving it to database");
                        dbContext.Products.Add(product);
                        await dbContext.SaveChangesAsync();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        });
    }

}