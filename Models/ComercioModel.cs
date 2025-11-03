using System;
using System.Collections.Generic;

namespace Allva.Desktop.Models.Admin;

/// <summary>
/// Modelo de datos para Comercios/Sucursales
/// Representa la entidad principal que agrupa locales
/// </summary>
public class ComercioModel
{
    // ============================================
    // PROPIEDADES BÁSICAS
    // ============================================

    public int IdComercio { get; set; }
    
    /// <summary>
    /// Nombre comercial del negocio
    /// </summary>
    public string NombreComercio { get; set; } = string.Empty;
    
    /// <summary>
    /// Razón social / Nombre SRL
    /// </summary>
    public string NombreSrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Dirección central del comercio/sucursal
    /// </summary>
    public string DireccionCentral { get; set; } = string.Empty;
    
    /// <summary>
    /// Número de contacto principal
    /// </summary>
    public string NumeroContacto { get; set; } = string.Empty;
    
    /// <summary>
    /// Email de contacto/contrato
    /// </summary>
    public string MailContacto { get; set; } = string.Empty;
    
    /// <summary>
    /// País donde opera el comercio
    /// </summary>
    public string Pais { get; set; } = string.Empty;
    
    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; set; }
    
    // ============================================
    // CONFIGURACIÓN DE DIVISAS
    // ============================================
    
    /// <summary>
    /// Porcentaje de comisión por intercambio de divisas
    /// Solo visible para el dueño de la aplicación
    /// </summary>
    public decimal PorcentajeComisionDivisas { get; set; } = 0;
    
    // ============================================
    // PERMISOS DE MÓDULOS
    // ============================================
    
    /// <summary>
    /// Permiso para módulo de Divisas
    /// </summary>
    public bool ModuloDivisas { get; set; } = false;
    
    /// <summary>
    /// Permiso para módulo de Pack de Alimentos
    /// </summary>
    public bool ModuloPackAlimentos { get; set; } = false;
    
    /// <summary>
    /// Permiso para módulo de Billetes de Avión
    /// </summary>
    public bool ModuloBilletesAvion { get; set; } = false;
    
    /// <summary>
    /// Permiso para módulo de Pack de Viajes
    /// </summary>
    public bool ModuloPackViajes { get; set; } = false;
    
    // ============================================
    // ESTADO Y FECHAS
    // ============================================
    
    /// <summary>
    /// Indica si el comercio está activo
    /// </summary>
    public bool Activo { get; set; } = true;
    
    /// <summary>
    /// Fecha de registro del comercio
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Fecha de última modificación
    /// </summary>
    public DateTime FechaUltimaModificacion { get; set; } = DateTime.Now;
    
    // ============================================
    // RELACIONES
    // ============================================
    
    /// <summary>
    /// Lista de locales asociados a este comercio
    /// </summary>
    public List<LocalSimpleModel> Locales { get; set; } = new List<LocalSimpleModel>();
    
    /// <summary>
    /// Cantidad total de locales
    /// </summary>
    public int CantidadLocales => Locales?.Count ?? 0;
    
    /// <summary>
    /// Cantidad total de usuarios en todos los locales
    /// </summary>
    public int TotalUsuarios { get; set; } = 0;
    
    // ============================================
    // PROPIEDADES CALCULADAS PARA UI
    // ============================================
    
    /// <summary>
    /// Texto del estado para mostrar en UI
    /// </summary>
    public string EstadoTexto => Activo ? "Activo" : "Inactivo";
    
    /// <summary>
    /// Color del estado para UI
    /// </summary>
    public string EstadoColor => Activo ? "#28a745" : "#dc3545";
    
    /// <summary>
    /// Resumen de permisos para UI
    /// </summary>
    public string PermisosResumen
    {
        get
        {
            var permisos = new List<string>();
            if (ModuloDivisas) permisos.Add("Divisas");
            if (ModuloPackAlimentos) permisos.Add("Alimentos");
            if (ModuloBilletesAvion) permisos.Add("Billetes");
            if (ModuloPackViajes) permisos.Add("Viajes");
            
            return permisos.Count > 0 
                ? string.Join(", ", permisos) 
                : "Sin módulos activos";
        }
    }
}

/// <summary>
/// Modelo simplificado de Local para mostrar en la lista de comercios
/// </summary>
public class LocalSimpleModel
{
    public int IdLocal { get; set; }
    public string CodigoLocal { get; set; } = string.Empty;
    public string NombreLocal { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public int NumeroUsuarios { get; set; } = 0;
}