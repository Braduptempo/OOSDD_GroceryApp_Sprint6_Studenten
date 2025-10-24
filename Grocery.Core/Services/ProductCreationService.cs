using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services;

public class ProductCreationService : IProductCreationService
{
    private readonly IProductService _productService;

    public ProductCreationService(IProductService productService)
    {
        _productService = productService;
    }

    public (bool IsValid, List<string> Errors) ValidateProduct(Product product)
    {
        var errors = new List<string>();
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (string.IsNullOrWhiteSpace(product.Name))
            errors.Add("Voer een product naam in.");

        if (product.Stock < 0)
            errors.Add("Product antal kan niet minder dan 0 zijn.");

        if (product.Price <= 0)
            errors.Add("Prijs moet hoger dan 0 zijn.");

        if (product.ShelfLife <= today)
            errors.Add("Houdbaarheidsdatum moet een toekomstige datum bevatten.");

        return (!errors.Any(), errors);
    }

    public Product CreateProduct(Client client, Product product)
    {
        if (client.Role != Role.Admin)
            throw new UnauthorizedAccessException("Only admins can add products.");

        var (isValid, errors) = ValidateProduct(product);
        if (!isValid)
            throw new ArgumentException(string.Join("; ", errors));

        product.Price = decimal.Round(product.Price, 2);

        return _productService.Add(product);
    }
}