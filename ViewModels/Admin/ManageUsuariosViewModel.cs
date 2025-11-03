using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Allva.Desktop.ViewModels.Admin;

public partial class ManageUsuariosViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<UsuarioItem> _usuarios = new();

    [ObservableProperty]
    private string _filtroNombre = string.Empty;

    [ObservableProperty]
    private string _filtroComercio = string.Empty;

    [ObservableProperty]
    private string _filtroLocal = string.Empty;

    public ManageUsuariosViewModel()
    {
        CargarUsuariosEjemplo();
    }

    private void CargarUsuariosEjemplo()
    {
        // Datos de ejemplo - luego se reemplazarán por datos reales de la BD
        Usuarios = new ObservableCollection<UsuarioItem>
        {
            new UsuarioItem
            {
                Id = 1,
                NumeroUsuario = "ANA_GÓMEZ",
                Nombre = "Ana",
                Apellidos = "Gómez",
                Telefono = "600123456",
                Email = "ana@gmail.com",
                Comercio = "Agencia Suárez",
                Locales = "AGS0001",
                EsFlotante = false,
                Activo = true
            },
            new UsuarioItem
            {
                Id = 2,
                NumeroUsuario = "LUIS_PÉREZ",
                Nombre = "Luis",
                Apellidos = "Pérez",
                Telefono = "600654321",
                Email = "luis@gmail.com",
                Comercio = "Agencia Suárez",
                Locales = "AGS0002",
                EsFlotante = false,
                Activo = true
            },
            new UsuarioItem
            {
                Id = 3,
                NumeroUsuario = "CARLOS_RAMÍREZ",
                Nombre = "Carlos",
                Apellidos = "Ramírez",
                Telefono = "600456789",
                Email = "carlos@gmail.com",
                Comercio = "Agencia Suárez",
                Locales = "AGS0001, AGS0002",
                EsFlotante = true,
                Activo = true
            },
            new UsuarioItem
            {
                Id = 4,
                NumeroUsuario = "MARTA_LOPEZ",
                Nombre = "Marta",
                Apellidos = "Lopez",
                Telefono = "600987654",
                Email = "marta@gmail.com",
                Comercio = "Latin Servicios",
                Locales = "LSE0001",
                EsFlotante = false,
                Activo = true
            }
        };
    }

    // Métodos que se implementarán con los controladores:
    // - CrearUsuario()
    // - EditarUsuario(int id)
    // - EliminarUsuario(int id)
    // - AplicarFiltros()
    // - LimpiarFiltros()
}

/// <summary>
/// Modelo para un usuario individual
/// </summary>
public class UsuarioItem
{
    public int Id { get; set; }
    public string NumeroUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Comercio { get; set; } = string.Empty;
    public string Locales { get; set; } = string.Empty;
    public bool EsFlotante { get; set; }
    public bool Activo { get; set; }
    
    public string TipoUsuario => EsFlotante ? "Flotante" : "Regular";
}