using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.App.ViewModels;

public partial class NewProductViewModel : BaseViewModel
{
    private readonly IProductService _productService;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private int stock;

    [ObservableProperty]
    private DateOnly shelfLife;

    [ObservableProperty]
    private decimal price;


    private readonly Client _client;

    public NewProductViewModel(IProductService productService, GlobalViewModel global)
    {
        _productService = productService;
        _client = global.Client;

    }

    [RelayCommand]
    public async Task CreateProduct()
    {
        var product = new Product
        {
            Name = _name,
            Stock = stock,
            ShelfLife = shelfLife,
            Price = Decimal.Round(price, 2)
        };
        
        if (_client.Role == Role.Admin)
            _productService.Add(product);
    }
}