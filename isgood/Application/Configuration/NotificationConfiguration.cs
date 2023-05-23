using System;

namespace isgood.Configuration;

public class NotificationConfiguration
{
    public bool? Enabled { get; set; }
    public double IntervalInHours { get; set; }
    public string? SmtpServerAddress { get; set; }
    public int SmtpServerPort { get; set; }
    public bool SmtpUseSSL { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpFromAddress { get; set; }
    public string? ToAddress { get; set; }

    public NotificationConfiguration()
    {
        IntervalInHours = 8;
        SmtpUseSSL = true;
        SmtpServerPort = 587;
    }

    public bool IsValid()
    {
        if (Enabled == null)
        {
            throw new ArgumentNullException("Property 'Enabled' is missing but has to be true or false");
        }

        if (SmtpServerAddress == null || SmtpServerAddress == string.Empty)
        {
            throw new ArgumentNullException("Property 'SmtpServerAddress' is not specified or empty");
        }

        if (SmtpUsername == null || SmtpUsername == string.Empty)
        {
            throw new ArgumentNullException("Property 'SmtpUsername' is not specified or empty");
        }

        if (SmtpPassword == null || SmtpPassword == string.Empty)
        {
            throw new ArgumentNullException("Property 'SmtpPassword' is not specified or empty");
        }

        if (SmtpFromAddress == null || SmtpFromAddress == string.Empty)
        {
            throw new ArgumentNullException("Property 'SmtpFromAddress' is not specified or empty");
        }
        return true;
    }
}