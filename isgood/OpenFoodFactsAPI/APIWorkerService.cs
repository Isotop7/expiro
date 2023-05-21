namespace isgood;

using System;
using System.Threading;
using System.Threading.Tasks;

internal class APIWorkerService
{
    private AppConfiguration _appConfiguration;
    public APIWorkerService(AppConfiguration appConfiguration)
    {
        _appConfiguration = appConfiguration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        OpenFoodFactsAPIController openFoodFactsAPIController = new(AppConfiguration.OpenFoodFactsApiUrl);

        while (!cancellationToken.IsCancellationRequested)
        {
            using (var dbContext = new AppDbContext())
            {
                while (true)
                {
                    if (Program.APIQueue.TryDequeue(out Product? product))
                    {
                        if (product == null)
                        {
                            throw new FormatException("Dequeued invalid object");
                        }
                        else
                        {
                            Console.WriteLine($"+ APIWorkerService: Dequeued element with barcode '{product.Barcode}'");
                            
                            Console.WriteLine($"+ APIWorkerService: Getting facts for product with barcode '{product.Barcode}'");
                            product = await openFoodFactsAPIController.GetProductFacts(product);

                            Console.WriteLine($"+ APIWorkerService: Enqueuing element with barcode '{product.Barcode}' for database insertion");
                            Program.DatabaseQueue.Enqueue(product);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
            }
        }
    }
}