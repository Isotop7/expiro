using Newtonsoft.Json;

namespace isgood.OpenFoodFactsApi;

public class ApiProduct
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

public class OpenFoodFactsApiModel
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("product")]
    public ApiProduct ApiProduct { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("status_verbose")]
    public string StatusVerbose { get; set; }

    public OpenFoodFactsApiModel()
    {
        Code = string.Empty;
        ApiProduct = new();
        Status = int.MinValue;
        StatusVerbose = string.Empty;
    }
}