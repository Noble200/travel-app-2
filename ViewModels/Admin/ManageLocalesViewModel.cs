using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Allva.Desktop.Models;

namespace Allva.Desktop.ViewModels.Admin;

public partial class ManageLocalesViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<LocalModel> _locales = new();

    [ObservableProperty]
    private ObservableCollection<ComercioModel> _comerciosDisponibles = new();

    [ObservableProperty]
    private LocalModel? _localSeleccionado;

    [ObservableProperty]
    private bool _mostrarFormulario;

    [ObservableProperty]
    private bool _modoEdicion;

    [ObservableProperty]
    private string _tituloFormulario = "Nuevo Local";

    [ObservableProperty]
    private int _comercioSeleccionadoId;

    [ObservableProperty]
    private string _codigoLocal = string.Empty;

    [ObservableProperty]
    private string _nombreLocal = string.Empty;

    [ObservableProperty]
    private string _direccion = string.Empty;

    [ObservableProperty]
    private string _local = string.Empty;

    [ObservableProperty]
    private string _escalera = string.Empty;

    [ObservableProperty]
    private string _piso = string.Empty;

    [ObservableProperty]
    private string _telefono = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _observaciones = string.Empty;

    [ObservableProperty]
    private bool _activo = true;

    [ObservableProperty]
    private int _numeroUsuarios;

    [ObservableProperty]
    private string _busqueda = string.Empty;

    [ObservableProperty]
    private int _comercioFiltroId;

    [ObservableProperty]
    private bool _soloActivos = true;

    [ObservableProperty]
    private bool _formularioValido;

    [ObservableProperty]
    private string _errorComercio = string.Empty;

    [ObservableProperty]
    private string _errorCodigo = string.Empty;

    [ObservableProperty]
    private string _errorNombre = string.Empty;

    [ObservableProperty]
    private string _errorDireccion = string.Empty;

    [ObservableProperty]
    private bool _mostrarMensajeExito;

    [ObservableProperty]
    private string _mensajeExito = string.Empty;

    public ManageLocalesViewModel()
    {
        CargarComercios();
        CargarLocales();
    }

    [RelayCommand]
    private void NuevoLocal()
    {
        LimpiarFormulario();
        ModoEdicion = false;
        TituloFormulario = "Nuevo Local";
        MostrarFormulario = true;
    }

    [RelayCommand]
    private void EditarLocal(LocalModel local)
    {
        if (local == null) return;

        LocalSeleccionado = local;
        ModoEdicion = true;
        TituloFormulario = $"Editar Local: {local.NombreLocal}";

        ComercioSeleccionadoId = local.IdComercio;
        CodigoLocal = local.CodigoLocal;
        NombreLocal = local.NombreLocal;
        ParsearDireccion(local.Direccion ?? string.Empty);
        Telefono = local.Telefono ?? string.Empty;
        Email = local.Email ?? string.Empty;
        Observaciones = local.Observaciones ?? string.Empty;
        Activo = local.Activo;
        NumeroUsuarios = local.NumeroUsuarios;

        MostrarFormulario = true;
    }

    [RelayCommand]
    private async Task EliminarLocal(LocalModel local)
    {
        if (local == null) return;

        if (local.NumeroUsuarios > 0)
        {
            return;
        }
        
        Locales.Remove(local);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task GuardarLocal()
    {
        if (!ValidarFormulario())
        {
            return;
        }

        var direccionCompleta = ConstruirDireccionCompleta();

        if (!ModoEdicion)
        {
            var comercio = ComerciosDisponibles.FirstOrDefault(c => c.IdComercio == ComercioSeleccionadoId);
            
            var nuevoLocal = new LocalModel
            {
                IdLocal = Locales.Count + 1,
                IdComercio = ComercioSeleccionadoId,
                NombreComercio = comercio?.NombreComercio ?? "Desconocido",
                CodigoLocal = CodigoLocal.ToUpper(),
                NombreLocal = NombreLocal,
                Direccion = direccionCompleta,
                Telefono = Telefono,
                Email = Email,
                Observaciones = Observaciones,
                Activo = Activo,
                FechaCreacion = DateTime.Now,
                NumeroUsuarios = 0
            };

            Locales.Add(nuevoLocal);
            
            MensajeExito = $"Local '{NombreLocal}' creado exitosamente y asignado a '{comercio?.NombreComercio}'";
            MostrarMensajeExito = true;
        }
        else if (LocalSeleccionado != null)
        {
            var comercio = ComerciosDisponibles.FirstOrDefault(c => c.IdComercio == ComercioSeleccionadoId);
            
            LocalSeleccionado.IdComercio = ComercioSeleccionadoId;
            LocalSeleccionado.NombreComercio = comercio?.NombreComercio ?? "Desconocido";
            LocalSeleccionado.CodigoLocal = CodigoLocal.ToUpper();
            LocalSeleccionado.NombreLocal = NombreLocal;
            LocalSeleccionado.Direccion = direccionCompleta;
            LocalSeleccionado.Telefono = Telefono;
            LocalSeleccionado.Email = Email;
            LocalSeleccionado.Observaciones = Observaciones;
            LocalSeleccionado.Activo = Activo;

            MensajeExito = $"Local '{NombreLocal}' actualizado exitosamente";
            MostrarMensajeExito = true;
        }

        await Task.Delay(3000);
        MostrarMensajeExito = false;

        MostrarFormulario = false;
        LimpiarFormulario();
    }

    [RelayCommand]
    private void CancelarEdicion()
    {
        MostrarFormulario = false;
        LimpiarFormulario();
    }

    [RelayCommand]
    private async Task ToggleActivoLocal(LocalModel local)
    {
        if (local == null) return;
        local.Activo = !local.Activo;
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void VerUsuarios(LocalModel local)
    {
        if (local == null) return;
    }

    [RelayCommand]
    private async Task FiltrarPorComercio()
    {
        if (ComercioFiltroId == 0)
        {
            CargarLocales();
        }
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task Buscar()
    {
        if (string.IsNullOrWhiteSpace(Busqueda))
        {
            CargarLocales();
            return;
        }

        var resultados = Locales.Where(l => 
            l.CodigoLocal.Contains(Busqueda, StringComparison.OrdinalIgnoreCase) ||
            l.NombreLocal.Contains(Busqueda, StringComparison.OrdinalIgnoreCase) ||
            (l.Direccion?.Contains(Busqueda, StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();

        Locales = new ObservableCollection<LocalModel>(resultados);
        await Task.CompletedTask;
    }

    private bool ValidarFormulario()
    {
        bool esValido = true;
        
        ErrorComercio = string.Empty;
        ErrorCodigo = string.Empty;
        ErrorNombre = string.Empty;
        ErrorDireccion = string.Empty;

        if (ComercioSeleccionadoId == 0)
        {
            ErrorComercio = "Debe seleccionar un comercio/sucursal";
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(CodigoLocal))
        {
            ErrorCodigo = "El código de local es obligatorio";
            esValido = false;
        }
        else if (CodigoLocal.Length < 3)
        {
            ErrorCodigo = "El código debe tener al menos 3 caracteres";
            esValido = false;
        }
        else if (!ModoEdicion && Locales.Any(l => l.CodigoLocal.Equals(CodigoLocal, StringComparison.OrdinalIgnoreCase)))
        {
            ErrorCodigo = "Ya existe un local con este código";
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(NombreLocal))
        {
            ErrorNombre = "El nombre del local es obligatorio";
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(Local) && string.IsNullOrWhiteSpace(Direccion))
        {
            ErrorDireccion = "Debe ingresar al menos el número de local o una dirección";
            esValido = false;
        }

        FormularioValido = esValido;
        return esValido;
    }

    private void LimpiarFormulario()
    {
        ComercioSeleccionadoId = 0;
        CodigoLocal = string.Empty;
        NombreLocal = string.Empty;
        Direccion = string.Empty;
        Local = string.Empty;
        Escalera = string.Empty;
        Piso = string.Empty;
        Telefono = string.Empty;
        Email = string.Empty;
        Observaciones = string.Empty;
        Activo = true;
        NumeroUsuarios = 0;
        
        ErrorComercio = string.Empty;
        ErrorCodigo = string.Empty;
        ErrorNombre = string.Empty;
        ErrorDireccion = string.Empty;
        FormularioValido = false;
        
        LocalSeleccionado = null;
    }

    private void ParsearDireccion(string direccionCompleta)
    {
        Direccion = direccionCompleta;
        var partes = direccionCompleta.Split(',');
        
        foreach (var parte in partes)
        {
            var texto = parte.Trim();
            if (texto.StartsWith("Local", StringComparison.OrdinalIgnoreCase))
            {
                Local = texto.Replace("Local", "").Trim();
            }
            else if (texto.StartsWith("Escalera", StringComparison.OrdinalIgnoreCase))
            {
                Escalera = texto.Replace("Escalera", "").Trim();
            }
            else if (texto.StartsWith("Piso", StringComparison.OrdinalIgnoreCase))
            {
                Piso = texto.Replace("Piso", "").Trim();
            }
        }
    }

    private string ConstruirDireccionCompleta()
    {
        var partes = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(Local))
            partes.Add($"Local {Local}");
            
        if (!string.IsNullOrWhiteSpace(Escalera))
            partes.Add($"Escalera {Escalera}");
            
        if (!string.IsNullOrWhiteSpace(Piso))
            partes.Add($"Piso {Piso}");
        
        if (partes.Count > 0)
            return string.Join(", ", partes);
        
        return Direccion;
    }

    private void CargarComercios()
    {
        ComerciosDisponibles = new ObservableCollection<ComercioModel>
        {
            new ComercioModel
            {
                IdComercio = 1,
                NombreComercio = "Allva Travel SRL",
                Activo = true
            }
        };
    }

    private void CargarLocales()
    {
        Locales = new ObservableCollection<LocalModel>
        {
            new LocalModel
            {
                IdLocal = 1,
                IdComercio = 1,
                NombreComercio = "Allva Travel SRL",
                CodigoLocal = "CENTRAL",
                NombreLocal = "Casa Central",
                Direccion = "Local 1, Escalera A, Piso 2",
                Telefono = "+54 11 4567-8901",
                Email = "central@allvatravel.com",
                Activo = true,
                FechaCreacion = DateTime.Now.AddMonths(-12),
                NumeroUsuarios = 3
            },
            new LocalModel
            {
                IdLocal = 2,
                IdComercio = 1,
                NombreComercio = "Allva Travel SRL",
                CodigoLocal = "BELGRANO",
                NombreLocal = "Sucursal Belgrano",
                Direccion = "Local 5, Piso 1",
                Telefono = "+54 11 4567-8902",
                Email = "belgrano@allvatravel.com",
                Activo = true,
                FechaCreacion = DateTime.Now.AddMonths(-6),
                NumeroUsuarios = 2
            }
        };
    }
}