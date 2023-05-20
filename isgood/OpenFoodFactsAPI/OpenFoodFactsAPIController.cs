namespace isgood;

using System;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

internal class OpenFoodFactsAPIController
{
    private string _baseUrl;

    public OpenFoodFactsAPIController(string baseUrl)
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
            OpenFoodFactsAPIModel? openFoodFactsAPIModel = JsonConvert.DeserializeObject<OpenFoodFactsAPIModel>(content);

            if (openFoodFactsAPIModel == null)
            {
                throw new InvalidCastException("Could not convert response to API model");
            }
            else
            {
                product.ProductName = openFoodFactsAPIModel.APIProduct.ProductName ?? string.Empty;
                product.Categories = openFoodFactsAPIModel.APIProduct.Categories ?? string.Empty;
                product.Countries = openFoodFactsAPIModel.APIProduct.Countries ?? string.Empty;
                product.ImageUrl = openFoodFactsAPIModel.APIProduct.ImageUrl ?? string.Empty;
            }
        }

        return product;
    }
}