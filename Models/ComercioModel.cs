using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Allva.Desktop.Models;

/// <summary>
/// Modelo que representa un Comercio/Sucursal
/// </summary>
public partial class ComercioModel : ObservableObject
{
    [ObservableProperty]
    private int _idComercio;

    [ObservableProperty]
    private string _nombreComercio = string.Empty;

    [ObservableProperty]
    private string? _nombreSrl;

    [ObservableProperty]
    private string _direccionCentral = string.Empty;

    [ObservableProperty]
    private string? _numeroContacto;

    [ObservableProperty]
    private string _mailContacto = string.Empty;

    [ObservableProperty]
    private string _pais = string.Empty;

    [ObservableProperty]
    private string? _observaciones;

    [ObservableProperty]
    private decimal _porcentajeComisionDivisas;

    [ObservableProperty]
    private bool _activo;

    [ObservableProperty]
    private DateTime _fechaRegistro;

    [ObservableProperty]
    private DateTime? _fechaUltimaModificacion;

    // Permisos de módulos
    [ObservableProperty]
    private bool _moduloDivisas;

    [ObservableProperty]
    private bool _moduloPackAlimentos;

    [ObservableProperty]
    private bool _moduloBilletesAvion;

    [ObservableProperty]
    private bool _moduloPackViajes;

    // Información adicional
    [ObservableProperty]
    private int _numeroLocales;

    /// <summary>
    /// Estado visual para la UI
    /// </summary>
    public string EstadoTexto => Activo ? "Activo" : "Inactivo";

    /// <summary>
    /// Color del estado para la UI
    /// </summary>
    public string EstadoColor => Activo ? "#0b5394" : "#595959";
}