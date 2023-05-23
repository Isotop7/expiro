using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace isgood.Models;

[Table("Products")]
public class Product
{
    [Key]
    public int Id { get; set; }

    [JsonProperty("barcode")]
    public string Barcode { get; set; }
    
    public string ProductName { get; set; }

    public string Categories { get; set; }

    public string Countries { get; set; }

    public string ImageUrl { get; set; }

    [JsonProperty("bestBefore")]
    public DateTime BestBefore { get; set; }

    public Product() {
        Barcode = string.Empty;
        ProductName = string.Empty;
        Categories = string.Empty;
        Countries = string.Empty;
        ImageUrl = string.Empty;
        BestBefore = DateTime.MaxValue;
    }
}