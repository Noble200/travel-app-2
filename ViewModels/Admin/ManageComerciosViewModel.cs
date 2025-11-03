using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Allva.Desktop.ViewModels.Admin;

public partial class ManageComerciosViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ComercioItem> _comercios = new();

    [ObservableProperty]
    private string _filtroNombre = string.Empty;

    [ObservableProperty]
    private string _filtroPais = string.Empty;

    public ManageComerciosViewModel()
    {
        CargarComerciosEjemplo();
    }

    private void CargarComerciosEjemplo()
    {
        // Datos de ejemplo - luego se reemplazarán por datos reales de la BD
        Comercios = new ObservableCollection<ComercioItem>
        {
            new ComercioItem
            {
                Id = 1,
                NombreComercio = "Agencia Suárez",
                NombreSRL = "Agencia Suárez S.L.",
                DireccionCentral = "Calle Mayor 123, Madrid, España",
                Email = "contacto@agenciasuarez.com",
                Telefono = "+34 912 345 678",
                Pais = "España",
                NumeroLocales = 2,
                CodigosLocales = "AGS0001, AGS0002",
                Activo = true
            },
            new ComercioItem
            {
                Id = 2,
                NombreComercio = "Latin Servicios",
                NombreSRL = "Latin Servicios S.A.",
                DireccionCentral = "Av. Libertador 456, Buenos Aires, Argentina",
                Email = "info@latinservicios.com",
                Telefono = "+54 11 4567 8901",
                Pais = "Argentina",
                NumeroLocales = 1,
                CodigosLocales = "LSE0001",
                Activo = true
            }
        };
    }

    // Métodos que se implementarán con los controladores:
    // - CrearComercio()
    // - EditarComercio(int id)
    // - EliminarComercio(int id)
    // - AplicarFiltros()
    // - LimpiarFiltros()
}

/// <summary>
/// Modelo para un comercio individual
/// </summary>
public class ComercioItem
{
    public int Id { get; set; }
    public string NombreComercio { get; set; } = string.Empty;
    public string NombreSRL { get; set; } = string.Empty;
    public string DireccionCentral { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public int NumeroLocales { get; set; }
    public string CodigosLocales { get; set; } = string.Empty;
    public bool Activo { get; set; }
}
