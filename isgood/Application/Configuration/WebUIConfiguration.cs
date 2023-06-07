using System;

namespace isgood.Configuration;

public class WebUIConfiguration
{
    public bool? Enabled { get; set; }

    public bool IsValid()
    {
        if (Enabled == null)
        {
            throw new ArgumentException("Property 'Enabled' is missing but has to be true or false");
        }
        return true;
    }
}