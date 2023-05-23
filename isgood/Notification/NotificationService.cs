using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

using isgood.Database;
using isgood.Configuration;
using isgood.Models;

namespace isgood.Notification;

public class NotificationService
{
    private NotificationConfiguration _notificationConfiguration;
    private int _bestBeforeThreshold;

    public NotificationService(NotificationConfiguration notificationConfiguration, int bestBeforeThreshold)
    {
        _notificationConfiguration = notificationConfiguration;
        _bestBeforeThreshold = bestBeforeThreshold;
    }
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
                        Console.WriteLine($"+ NotificationService: Service is now active.");
                        if (dbContext.Product != null)
                        {
                            List<Product> products = dbContext.Product.ToList();
                            foreach(Product p in products)
                            {
                                if ((p.BestBefore - DateTime.Now) <= TimeSpan.FromDays(_bestBeforeThreshold))
                                {
                                    Console.WriteLine($"+ NotificationService: Sending notification for Product with barcode {p.Barcode} and BestBefore {p.BestBefore}");
                                    SendNotification(p);
                                }
                            }
                        }
                        else
                        {
                            throw new ArgumentNullException("Database model is empty");
                        }
                        Console.WriteLine("+ NotificationService: Service is now sleeping");
                        await Task.Delay(TimeSpan.FromHours(_notificationConfiguration.IntervalInHours), cancellationToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"+ NotificationService: Cancellation was requested");
            }
        }
    }

    private void SendNotification(Product product)
    {
        try
        {
            using (SmtpClient smtpClient = new(_notificationConfiguration.SmtpServerAddress, _notificationConfiguration.SmtpServerPort))
            {
                smtpClient.EnableSsl = _notificationConfiguration.SmtpUseSSL;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(_notificationConfiguration.SmtpUsername, _notificationConfiguration.SmtpPassword);
                
                MailMessage mailMessage = new()
                {
                    From = new(_notificationConfiguration.SmtpFromAddress ?? "isgood"),
                    Subject = $"Product {product.ProductName} is soon to be expired or already expired",
                    Body = $@"
                        Product: {product.ProductName}

                        Barcode: {product.Barcode}
                        
                        BestBefore: {product.BestBefore}
                        ",
                    IsBodyHtml = true
                };

                if (_notificationConfiguration.ToAddress != null && _notificationConfiguration.ToAddress != string.Empty)
                {
                    mailMessage.To.Add(_notificationConfiguration.ToAddress);
                }
                else
                {
                    throw new ArgumentException("ToAddress is null or empty");
                }

                smtpClient.Send(mailMessage);
            }

            Console.WriteLine("+ NotificationService: Email sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"+ NotificationService: Failed to send email: {ex.Message}");
        }    
    }
}