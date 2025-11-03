using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Allva.Desktop.Models;

/// <summary>
/// Modelo que representa un Local físico
/// </summary>
public partial class LocalModel : ObservableObject
{
    [ObservableProperty]
    private int _idLocal;

    [ObservableProperty]
    private int _idComercio;

    [ObservableProperty]
    private string _nombreComercio = string.Empty;

    [ObservableProperty]
    private string _codigoLocal = string.Empty;

    [ObservableProperty]
    private string _nombreLocal = string.Empty;

    [ObservableProperty]
    private string _direccion = string.Empty;

    [ObservableProperty]
    private string? _telefono;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _observaciones;

    [ObservableProperty]
    private bool _activo;

    [ObservableProperty]
    private DateTime _fechaCreacion;

    // Información adicional
    [ObservableProperty]
    private int _numeroUsuarios;

    /// <summary>
    /// Estado visual para la UI
    /// </summary>
    public string EstadoTexto => Activo ? "Activo" : "Inactivo";

    /// <summary>
    /// Color del estado para la UI
    /// </summary>
    public string EstadoColor => Activo ? "#0b5394" : "#595959";

    /// <summary>
    /// Información completa del local
    /// </summary>
    public string InformacionCompleta => $"{NombreLocal} ({CodigoLocal}) - {NombreComercio}";
}