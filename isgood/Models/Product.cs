namespace isgood;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

[Table("Products")]
public class Product
{
    [Key]
    public int Id { get; set; }

    [JsonProperty("barcode")]
    public string Barcode { get; set; }
    
    public string Name { get; set; }

    [JsonProperty("bestBefore")]
    public DateTime BestBefore { get; set; }

    [NotMapped]
    public bool Ready;

    public Product() {
        Barcode = string.Empty;
        Name = string.Empty;
        BestBefore = DateTime.MaxValue;
        Ready = false;
    }
}