using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace isgood.Models;

[Table("Products")]
public class Product
{
    [NotMapped]
    private DateTime _bestBefore;
    [NotMapped]
    private DateTime _scannedAt;

    [Key]
    public int Id { get; set; }

    [JsonProperty("barcode")]
    public string Barcode { get; set; }

    public string ProductName { get; set; }

    public string Categories { get; set; }

    public string Countries { get; set; }

    public string ImageUrl { get; set; }

    [JsonProperty("bestBefore")]
    public DateTime BestBefore
    {
        get
        {
            return _bestBefore.Date;
        }
        set
        {
            _bestBefore = value.Date;
        }
    }

    [JsonProperty("scannedAt")]
    public DateTime ScannedAt
    {
        get
        {
            return _scannedAt.Date;
        }
        set
        {
            _scannedAt = value.Date;
        }
    }

    [NotMapped]
    public string BestBeforeDateOnly
    {
        get
        {
            return _bestBefore.Date.ToString("dd.MM.yyyy");
        }
    }

    [NotMapped]
    public string ScannedAtDateOnly
    {
        get
        {
            return _scannedAt.Date.ToString("dd.MM.yyyy");
        }
    }

    public Product()
    {
        Barcode = string.Empty;
        ProductName = string.Empty;
        Categories = string.Empty;
        Countries = string.Empty;
        ImageUrl = string.Empty;
        BestBefore = DateTime.MaxValue;
        ScannedAt = DateTime.Now;
    }
}