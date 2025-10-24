using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.App.ViewModels;

public partial class NewProductViewModel : BaseViewModel
{
    private readonly IProductCreationService _productCreationService;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private int stock;
    
    [ObservableProperty]
    private DateTime shelfLifeDateTime = DateTime.Today;
    
    public DateOnly ShelfLife
    {
        get => DateOnly.FromDateTime(ShelfLifeDateTime);
        set => ShelfLifeDateTime = value.ToDateTime(TimeOnly.MinValue);
    }

    [ObservableProperty]
    private decimal price;

    private readonly Client _client;

    public NewProductViewModel(IProductCreationService productCreationService, GlobalViewModel global)
    {
        _productCreationService = productCreationService;
        _client = global.Client;

    }

    [RelayCommand]
    public async Task CreateProduct()
    {
        var product = new Product
        {
            Name = Name?.Trim(),
            Stock = Stock,
            ShelfLife = ShelfLife,
            Price = Price
        };

        try
        {
            _productCreationService.CreateProduct(_client, product);

            await Shell.Current.DisplayAlert("Success",
                $"Product '{product.Name}' added successfully.",
                "OK");

            Name = string.Empty;
            Stock = 0;
            Price = 0;
            ShelfLife = DateOnly.FromDateTime(DateTime.Today);
        }
        catch (ArgumentException ex)
        {
            await Shell.Current.DisplayAlert("Invalid Input", ex.Message, "OK");
        }
        catch (UnauthorizedAccessException ex)
        {
            await Shell.Current.DisplayAlert("Permission Denied", ex.Message, "OK");
        }
       
    }
}