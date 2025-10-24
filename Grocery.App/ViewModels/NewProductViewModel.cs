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
    private DateTime shelfLifeDateTime = DateTime.Today;
    
    public DateOnly ShelfLife
    {
        get => DateOnly.FromDateTime(ShelfLifeDateTime);
        set => ShelfLifeDateTime = value.ToDateTime(TimeOnly.MinValue);
    }

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
        var validationErrors = new List<string>();
        var today = DateOnly.FromDateTime(DateTime.Today);
        
        System.Diagnostics.Debug.WriteLine($"ShelfLife (DateOnly): {ShelfLife}");
        System.Diagnostics.Debug.WriteLine($"ShelfLifeDateTime (DateTime): {ShelfLifeDateTime}");

        if (string.IsNullOrWhiteSpace(Name))
            validationErrors.Add("Voer een product naam in");

        if (Stock < 0)
            validationErrors.Add("Aantal kan niet minder dan 0 zijn");

        if (Price <= 0)
            validationErrors.Add("Prijs moet hoger dan 0 zijn");

        if (ShelfLife <= today)
            validationErrors.Add("Houdbaarheidsdatum moet een toekomstige datum bevatten");
        
        if (validationErrors.Count != 0)
        {
            await Shell.Current.DisplayAlert("Ongeldige invoer",
                string.Join("\n", validationErrors),
                "OK");
            return;
        }
        
        var product = new Product
        {
            Name = Name.Trim(),
            Stock = Stock,
            ShelfLife = ShelfLife,
            Price = Decimal.Round(Price, 2)
        };
        
        if (_client.Role != Role.Admin)
        {
            await Shell.Current.DisplayAlert("Permission Denied", 
                "Alleen admins kunnen producten toevoegen.", 
                "OK");
            return;
        }
        
        _productService.Add(product);

        await Shell.Current.DisplayAlert("Success", 
            $"Product {product.Name} Is toegevoegd aan producten.", 
            "OK");
        Name = string.Empty;
        Stock = 0;
        Price = 0;
        ShelfLife = DateOnly.FromDateTime(DateTime.Today);
    }
}