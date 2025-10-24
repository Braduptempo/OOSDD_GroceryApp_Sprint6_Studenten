using Grocery.Core.Models;

namespace Grocery.Core.Interfaces.Services;

public interface IProductCreationService
{
    (bool IsValid, List<string> Errors) ValidateProduct(Product product);
    Product CreateProduct(Client client, Product product);
}