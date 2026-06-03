using SistemaGVP.Application.DTOs;
using SistemaGVP.WPF.ViewModels;
using System.Collections.ObjectModel;

namespace SistemaGVP.WPF.ViewModels.Design;

public static class DesignTimeData
{
    private static readonly ObservableCollection<ProductDto> _sampleProducts = new()
    {
        new() { Id = 1, Name = "Laptop HP ProBook", Barcode = "ABC123", Price = 4500000, Cost = 3500000, CurrentStock = 15, MinStock = 5, CategoryName = "Electronicos", Unit = "UN" },
        new() { Id = 2, Name = "Mouse Inalambrico", Barcode = "DEF456", Price = 85000, Cost = 55000, CurrentStock = 3, MinStock = 10, CategoryName = "Perifericos", Unit = "UN" },
        new() { Id = 3, Name = "Teclado Mecanico", Barcode = "GHI789", Price = 250000, Cost = 180000, CurrentStock = 8, MinStock = 5, CategoryName = "Perifericos", Unit = "UN" },
    };

    public static ProductsViewModel ProductsVM
    {
        get
        {
            var vm = new ProductsViewModel(null!, null!, null!, null!, null!, null);
            vm.GetType().GetProperty("Items")?.SetValue(vm, _sampleProducts);
            vm.GetType().GetProperty("IsEditing")?.SetValue(vm, true);
            return vm;
        }
    }
}
