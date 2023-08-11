using System;
using System.Threading;
using System.Threading.Tasks;

using isgood.Configuration;
using isgood.Database;
using isgood.Models;

namespace isgood.OpenFoodFactsApi;

internal class ApiWorkerService
{
    public ApiWorkerService()
    { }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        OpenFoodFactsApiController openFoodFactsApiController = new(AppConfiguration.OpenFoodFactsApiUrl);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var dbContext = new AppDbContext())
                {
                    while (true)
                    {
                        if (Program.ApiQueue.TryDequeue(out Product? product))
                        {
                            if (product == null)
                            {
                                throw new FormatException("Dequeued invalid object");
                            }
                            else
                            {
                                Console.WriteLine($"+ ApiWorkerService: Dequeued element with barcode '{product.Barcode}'");
                                
                                Console.WriteLine($"+ ApiWorkerService: Getting facts for product with barcode '{product.Barcode}'");
                                product = await openFoodFactsApiController.GetProductFacts(product);

                                Console.WriteLine($"+ ApiWorkerService: Enqueuing element with barcode '{product.Barcode}' for database insertion");
                                Program.DatabaseQueue.Enqueue(new(isgood.Database.DatabaseQueueElementAction.INSERT, product));
                            }
                        }

                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"+ ApiWorkerService : Cancellation was requested");
            }
        }
    }
}