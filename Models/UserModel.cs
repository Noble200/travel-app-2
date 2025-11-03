using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Allva.Desktop.Models;

/// <summary>
/// Modelo que representa un Usuario del sistema
/// </summary>
public partial class UserModel : ObservableObject
{
    [ObservableProperty]
    private int _idUsuario;

    [ObservableProperty]
    private int _idComercio;

    [ObservableProperty]
    private int? _idLocal;

    [ObservableProperty]
    private int _idRol;

    [ObservableProperty]
    private string _nombreComercio = string.Empty;

    [ObservableProperty]
    private string _codigoLocal = string.Empty;

    [ObservableProperty]
    private string _nombreLocal = string.Empty;

    [ObservableProperty]
    private string _numeroUsuario = string.Empty;

    [ObservableProperty]
    private string _nombre = string.Empty;

    [ObservableProperty]
    private string _apellidos = string.Empty;

    [ObservableProperty]
    private string _correo = string.Empty;

    [ObservableProperty]
    private string? _telefono;

    [ObservableProperty]
    private string _nombreRol = string.Empty;

    [ObservableProperty]
    private bool _esFlotante;

    [ObservableProperty]
    private bool _activo;

    [ObservableProperty]
    private string? _idioma;

    [ObservableProperty]
    private DateTime _fechaCreacion;

    [ObservableProperty]
    private DateTime? _ultimoAcceso;

    // ✅ NUEVAS PROPIEDADES AGREGADAS (las que faltaban):
    
    /// <summary>
    /// Observaciones o notas sobre el usuario
    /// </summary>
    [ObservableProperty]
    private string _observaciones = string.Empty;

    /// <summary>
    /// Indica si es el primer login del usuario
    /// </summary>
    [ObservableProperty]
    private bool _primerLogin = true;

    /// <summary>
    /// Número de intentos fallidos de login
    /// </summary>
    [ObservableProperty]
    private int _intentosFallidos;

    // ============================================
    // PROPIEDADES CALCULADAS
    // ============================================

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto => $"{Nombre} {Apellidos}";

    /// <summary>
    /// Estado visual para la UI
    /// </summary>
    public string EstadoTexto => Activo ? "Activo" : "Inactivo";

    /// <summary>
    /// Color del estado para la UI
    /// </summary>
    public string EstadoColor => Activo ? "#0b5394" : "#595959";

    /// <summary>
    /// Tipo de usuario (Flotante o Fijo)
    /// </summary>
    public string TipoUsuario => EsFlotante ? "Flotante" : "Fijo";

    /// <summary>
    /// Información de último acceso formateada
    /// </summary>
    public string UltimoAccesoTexto => UltimoAcceso.HasValue 
        ? UltimoAcceso.Value.ToString("dd/MM/yyyy HH:mm") 
        : "Nunca";

    /// <summary>
    /// Color del rol para badges
    /// </summary>
    public string ColorRol => NombreRol switch
    {
        "Administrador" => "#0b5394",
        "Gerente" => "#ffd966",
        "Empleado" => "#595959",
        _ => "#595959"
    };
}