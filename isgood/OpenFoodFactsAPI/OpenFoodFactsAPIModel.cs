namespace isgood;

using Newtonsoft.Json;

public class APIProduct
{
    [JsonProperty("product_name")]
    public string? ProductName;

    [JsonProperty("categories")]
    public string? Categories;

    [JsonProperty("countries")]
    public string? Countries;

    [JsonProperty("generic_name")]
    public string? GenericName;

    [JsonProperty("image_url")]
    public string? ImageUrl;
}

public class OpenFoodFactsAPIModel
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("product")]
    public APIProduct APIProduct { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("status_verbose")]
    public string StatusVerbose { get; set; }

    public OpenFoodFactsAPIModel()
    {
        Code = string.Empty;
        APIProduct = new();
        Status = int.MinValue;
        StatusVerbose = string.Empty;
    }
}