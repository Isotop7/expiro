using System;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using isgood.Models;

namespace isgood.OpenFoodFactsApi;

internal class OpenFoodFactsApiController
{
    private readonly string _baseUrl;

    public OpenFoodFactsApiController(string baseUrl)
    {
        _baseUrl = baseUrl;
    }
    
    public async Task<Product> GetProductFacts(Product product)
    {
        if (product.Barcode == null)
        {
            return product;
        }

        string queryUrl = $"{_baseUrl}/{product.Barcode}.json";

        HttpClient httpClient = new();
        HttpResponseMessage responseMessage = await httpClient.GetAsync(queryUrl);

        if (responseMessage.IsSuccessStatusCode)
        {
            string content = await responseMessage.Content.ReadAsStringAsync();
            OpenFoodFactsApiModel? openFoodFactsApiModel = JsonConvert.DeserializeObject<OpenFoodFactsApiModel>(content);

            if (openFoodFactsApiModel == null)
            {
                throw new InvalidCastException("Could not convert response to API model");
            }
            else
            {
                product.ProductName = openFoodFactsApiModel.ApiProduct.ProductName ?? string.Empty;
                product.Categories = openFoodFactsApiModel.ApiProduct.Categories ?? string.Empty;
                product.Countries = openFoodFactsApiModel.ApiProduct.Countries ?? string.Empty;
                product.ImageUrl = openFoodFactsApiModel.ApiProduct.ImageUrl ?? string.Empty;
            }
        }

        return product;
    }
}