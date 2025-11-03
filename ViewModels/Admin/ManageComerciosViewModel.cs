using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Allva.Desktop.Models.Admin;
using Npgsql;

namespace Allva.Desktop.ViewModels.Admin;

/// <summary>
/// ViewModel para el módulo de Gestión de Comercios
/// CON CONEXIÓN A POSTGRESQL - Totalmente funcional
/// </summary>
public partial class ManageComerciosViewModel : ObservableObject
{
    // ============================================
    // CONFIGURACIÓN DE BASE DE DATOS
    // ============================================
    
    private const string ConnectionString = "Host=switchyard.proxy.rlwy.net;Port=55839;Database=railway;Username=postgres;Password=ysTQxChOYSWUuAPzmYQokqrjpYnKSGbk;";

    // ============================================
    // PROPIEDADES OBSERVABLES - COLECCIONES
    // ============================================

    [ObservableProperty]
    private ObservableCollection<ComercioModel> _comercios = new();

    [ObservableProperty]
    private ComercioModel? _comercioSeleccionado;

    // ============================================
    // PROPIEDADES OBSERVABLES - FILTROS Y BÚSQUEDA
    // ============================================

    [ObservableProperty]
    private string _busqueda = string.Empty;

    [ObservableProperty]
    private string? _paisFiltro;

    [ObservableProperty]
    private bool? _estadoFiltro;

    // ============================================
    // PROPIEDADES OBSERVABLES - FORMULARIO
    // ============================================

    [ObservableProperty]
    private bool _mostrarFormulario = false;

    [ObservableProperty]
    private bool _modoEdicion = false;

    [ObservableProperty]
    private string _nombreComercio = string.Empty;

    [ObservableProperty]
    private string _nombreSrl = string.Empty;

    [ObservableProperty]
    private string _direccionCentral = string.Empty;

    [ObservableProperty]
    private string _numeroContacto = string.Empty;

    [ObservableProperty]
    private string _mailContacto = string.Empty;

    [ObservableProperty]
    private string _pais = string.Empty;

    [ObservableProperty]
    private string _observaciones = string.Empty;

    [ObservableProperty]
    private decimal _porcentajeComisionDivisas = 0;

    [ObservableProperty]
    private bool _moduloDivisas = false;

    [ObservableProperty]
    private bool _moduloPackAlimentos = false;

    [ObservableProperty]
    private bool _moduloBilletesAvion = false;

    [ObservableProperty]
    private bool _moduloPackViajes = false;

    [ObservableProperty]
    private bool _activo = true;

    // ============================================
    // PROPIEDADES OBSERVABLES - GESTIÓN DE LOCALES
    // ============================================

    [ObservableProperty]
    private ObservableCollection<LocalFormModel> _localesFormulario = new();

    [ObservableProperty]
    private bool _mostrarPanelLocales = false;

    // ============================================
    // PROPIEDADES OBSERVABLES - VALIDACIÓN Y MENSAJES
    // ============================================

    [ObservableProperty]
    private bool _formularioValido = true;

    [ObservableProperty]
    private string _errorNombreComercio = string.Empty;

    [ObservableProperty]
    private string _errorMailContacto = string.Empty;

    [ObservableProperty]
    private bool _mostrarMensajeExito = false;

    [ObservableProperty]
    private string _mensajeExito = string.Empty;

    [ObservableProperty]
    private bool _cargando = false;

    // ============================================
    // PROPIEDADES CALCULADAS
    // ============================================

    public string TituloFormulario => ModoEdicion 
        ? "Editar Comercio" 
        : "Nuevo Comercio";

    public int TotalComercios => Comercios.Count;
    public int ComerciosActivos => Comercios.Count(c => c.Activo);
    public int ComerciosInactivos => Comercios.Count(c => !c.Activo);
    public int TotalLocales => Comercios.Sum(c => c.CantidadLocales);

    // ============================================
    // CONSTRUCTOR
    // ============================================

    public ManageComerciosViewModel()
    {
        // Cargar datos desde la base de datos
        _ = CargarDatosDesdeBaseDatos();
    }

    // ============================================
    // MÉTODOS DE BASE DE DATOS - CARGAR
    // ============================================

    private async Task CargarDatosDesdeBaseDatos()
    {
        Cargando = true;
        
        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var comercios = await CargarComercios(connection);
            
            Comercios.Clear();
            foreach (var comercio in comercios)
            {
                // Cargar locales del comercio
                comercio.Locales = await CargarLocalesDelComercio(connection, comercio.IdComercio);
                
                // Cargar permisos del comercio
                await CargarPermisosDelComercio(connection, comercio);
                
                // Contar usuarios
                comercio.TotalUsuarios = await ContarUsuariosDelComercio(connection, comercio.IdComercio);
                
                Comercios.Add(comercio);
            }

            // Actualizar contadores
            OnPropertyChanged(nameof(TotalComercios));
            OnPropertyChanged(nameof(ComerciosActivos));
            OnPropertyChanged(nameof(ComerciosInactivos));
            OnPropertyChanged(nameof(TotalLocales));
        }
        catch (Exception ex)
        {
            MensajeExito = $"Error al cargar datos: {ex.Message}";
            MostrarMensajeExito = true;
        }
        finally
        {
            Cargando = false;
        }
    }

    private async Task<List<ComercioModel>> CargarComercios(NpgsqlConnection connection)
    {
        var comercios = new List<ComercioModel>();

        var query = @"
            SELECT 
                id_comercio,
                nombre_comercio,
                nombre_srl,
                direccion_central,
                numero_contacto,
                mail_contacto,
                pais,
                observaciones,
                porcentaje_comision_divisas,
                activo,
                fecha_registro,
                fecha_ultima_modificacion
            FROM comercios
            ORDER BY nombre_comercio";

        using var cmd = new NpgsqlCommand(query, connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            comercios.Add(new ComercioModel
            {
                IdComercio = reader.GetInt32(0),
                NombreComercio = reader.GetString(1),
                NombreSrl = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                DireccionCentral = reader.GetString(3),
                NumeroContacto = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                MailContacto = reader.GetString(5),
                Pais = reader.GetString(6),
                Observaciones = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                PorcentajeComisionDivisas = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                Activo = reader.GetBoolean(9),
                FechaRegistro = reader.GetDateTime(10),
                FechaUltimaModificacion = reader.GetDateTime(11)
            });
        }

        return comercios;
    }

    private async Task<List<LocalSimpleModel>> CargarLocalesDelComercio(NpgsqlConnection connection, int idComercio)
    {
        var locales = new List<LocalSimpleModel>();

        var query = @"
            SELECT 
                id_local,
                codigo_local,
                nombre_local,
                direccion,
                activo
            FROM locales
            WHERE id_comercio = @IdComercio
            ORDER BY codigo_local";

        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@IdComercio", idComercio);
        
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            locales.Add(new LocalSimpleModel
            {
                IdLocal = reader.GetInt32(0),
                CodigoLocal = reader.GetString(1),
                NombreLocal = reader.GetString(2),
                Direccion = reader.GetString(3),
                Activo = reader.GetBoolean(4),
                NumeroUsuarios = 0 // Se cargará después
            });
        }

        return locales;
    }

    private async Task CargarPermisosDelComercio(NpgsqlConnection connection, ComercioModel comercio)
    {
        var query = @"
            SELECT 
                modulo_divisas,
                modulo_pack_alimentos,
                modulo_billetes_avion,
                modulo_pack_viajes
            FROM permisos_modulos
            WHERE id_comercio = @IdComercio";

        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@IdComercio", comercio.IdComercio);
        
        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            comercio.ModuloDivisas = reader.GetBoolean(0);
            comercio.ModuloPackAlimentos = reader.GetBoolean(1);
            comercio.ModuloBilletesAvion = reader.GetBoolean(2);
            comercio.ModuloPackViajes = reader.GetBoolean(3);
        }
    }

    private async Task<int> ContarUsuariosDelComercio(NpgsqlConnection connection, int idComercio)
    {
        var query = @"
            SELECT COUNT(*)
            FROM usuarios u
            INNER JOIN locales l ON u.id_local = l.id_local
            WHERE l.id_comercio = @IdComercio";

        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@IdComercio", idComercio);
        
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    // ============================================
    // COMANDOS - NAVEGACIÓN Y VISTAS
    // ============================================

    [RelayCommand]
    private void NuevoComercio()
    {
        LimpiarFormulario();
        ModoEdicion = false;
        MostrarFormulario = true;
        MostrarPanelLocales = false;
        LocalesFormulario.Clear();
    }

    [RelayCommand]
    private void CancelarFormulario()
    {
        MostrarFormulario = false;
        LimpiarFormulario();
    }

    [RelayCommand]
    private async Task Buscar()
    {
        await CargarDatosDesdeBaseDatos();
    }

    // ============================================
    // COMANDOS - GESTIÓN DE COMERCIOS
    // ============================================

    [RelayCommand]
    private async Task GuardarComercio()
    {
        if (!ValidarFormulario())
        {
            return;
        }

        Cargando = true;

        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            if (!ModoEdicion)
            {
                // CREAR NUEVO COMERCIO
                await CrearComercioEnBaseDatos(connection);
            }
            else if (ComercioSeleccionado != null)
            {
                // ACTUALIZAR COMERCIO EXISTENTE
                await ActualizarComercioEnBaseDatos(connection);
            }

            // Recargar datos
            await CargarDatosDesdeBaseDatos();

            MensajeExito = ModoEdicion 
                ? $"Comercio '{NombreComercio}' actualizado exitosamente" 
                : $"Comercio '{NombreComercio}' creado exitosamente con {LocalesFormulario.Count} local(es)";
            
            MostrarMensajeExito = true;
            
            await Task.Delay(3000);
            MostrarMensajeExito = false;

            MostrarFormulario = false;
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            MensajeExito = $"Error al guardar: {ex.Message}";
            MostrarMensajeExito = true;
        }
        finally
        {
            Cargando = false;
        }
    }

    private async Task CrearComercioEnBaseDatos(NpgsqlConnection connection)
    {
        // 1. Insertar comercio
        var queryComercio = @"
            INSERT INTO comercios 
            (nombre_comercio, nombre_srl, direccion_central, numero_contacto, 
             mail_contacto, pais, observaciones, porcentaje_comision_divisas, activo)
            VALUES 
            (@nombre, @srl, @direccion, @telefono, @email, @pais, @obs, @comision, @activo)
            RETURNING id_comercio";

        int idComercio;
        using (var cmd = new NpgsqlCommand(queryComercio, connection))
        {
            cmd.Parameters.AddWithValue("@nombre", NombreComercio);
            cmd.Parameters.AddWithValue("@srl", string.IsNullOrEmpty(NombreSrl) ? DBNull.Value : NombreSrl);
            cmd.Parameters.AddWithValue("@direccion", DireccionCentral);
            cmd.Parameters.AddWithValue("@telefono", string.IsNullOrEmpty(NumeroContacto) ? DBNull.Value : NumeroContacto);
            cmd.Parameters.AddWithValue("@email", MailContacto);
            cmd.Parameters.AddWithValue("@pais", Pais);
            cmd.Parameters.AddWithValue("@obs", string.IsNullOrEmpty(Observaciones) ? DBNull.Value : Observaciones);
            cmd.Parameters.AddWithValue("@comision", PorcentajeComisionDivisas);
            cmd.Parameters.AddWithValue("@activo", Activo);

            idComercio = (int)(await cmd.ExecuteScalarAsync() ?? 0);
        }

        // 2. Insertar permisos de módulos
        var queryPermisos = @"
            INSERT INTO permisos_modulos 
            (id_comercio, modulo_divisas, modulo_pack_alimentos, modulo_billetes_avion, modulo_pack_viajes)
            VALUES 
            (@idComercio, @divisas, @alimentos, @billetes, @viajes)";

        using (var cmd = new NpgsqlCommand(queryPermisos, connection))
        {
            cmd.Parameters.AddWithValue("@idComercio", idComercio);
            cmd.Parameters.AddWithValue("@divisas", ModuloDivisas);
            cmd.Parameters.AddWithValue("@alimentos", ModuloPackAlimentos);
            cmd.Parameters.AddWithValue("@billetes", ModuloBilletesAvion);
            cmd.Parameters.AddWithValue("@viajes", ModuloPackViajes);

            await cmd.ExecuteNonQueryAsync();
        }

        // 3. Insertar locales
        foreach (var local in LocalesFormulario)
        {
            var queryLocal = @"
                INSERT INTO locales 
                (id_comercio, codigo_local, nombre_local, direccion, activo)
                VALUES 
                (@idComercio, @codigo, @nombre, @direccion, @activo)";

            using var cmd = new NpgsqlCommand(queryLocal, connection);
            cmd.Parameters.AddWithValue("@idComercio", idComercio);
            cmd.Parameters.AddWithValue("@codigo", local.CodigoLocal);
            cmd.Parameters.AddWithValue("@nombre", local.NombreLocal);
            cmd.Parameters.AddWithValue("@direccion", local.Direccion);
            cmd.Parameters.AddWithValue("@activo", local.Activo);

            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task ActualizarComercioEnBaseDatos(NpgsqlConnection connection)
    {
        if (ComercioSeleccionado == null) return;

        // 1. Actualizar comercio
        var queryComercio = @"
            UPDATE comercios SET
                nombre_comercio = @nombre,
                nombre_srl = @srl,
                direccion_central = @direccion,
                numero_contacto = @telefono,
                mail_contacto = @email,
                pais = @pais,
                observaciones = @obs,
                porcentaje_comision_divisas = @comision,
                activo = @activo,
                fecha_ultima_modificacion = CURRENT_TIMESTAMP
            WHERE id_comercio = @idComercio";

        using (var cmd = new NpgsqlCommand(queryComercio, connection))
        {
            cmd.Parameters.AddWithValue("@nombre", NombreComercio);
            cmd.Parameters.AddWithValue("@srl", string.IsNullOrEmpty(NombreSrl) ? DBNull.Value : NombreSrl);
            cmd.Parameters.AddWithValue("@direccion", DireccionCentral);
            cmd.Parameters.AddWithValue("@telefono", string.IsNullOrEmpty(NumeroContacto) ? DBNull.Value : NumeroContacto);
            cmd.Parameters.AddWithValue("@email", MailContacto);
            cmd.Parameters.AddWithValue("@pais", Pais);
            cmd.Parameters.AddWithValue("@obs", string.IsNullOrEmpty(Observaciones) ? DBNull.Value : Observaciones);
            cmd.Parameters.AddWithValue("@comision", PorcentajeComisionDivisas);
            cmd.Parameters.AddWithValue("@activo", Activo);
            cmd.Parameters.AddWithValue("@idComercio", ComercioSeleccionado.IdComercio);

            await cmd.ExecuteNonQueryAsync();
        }

        // 2. Actualizar permisos
        var queryPermisos = @"
            UPDATE permisos_modulos SET
                modulo_divisas = @divisas,
                modulo_pack_alimentos = @alimentos,
                modulo_billetes_avion = @billetes,
                modulo_pack_viajes = @viajes,
                fecha_modificacion = CURRENT_TIMESTAMP
            WHERE id_comercio = @idComercio";

        using (var cmd = new NpgsqlCommand(queryPermisos, connection))
        {
            cmd.Parameters.AddWithValue("@divisas", ModuloDivisas);
            cmd.Parameters.AddWithValue("@alimentos", ModuloPackAlimentos);
            cmd.Parameters.AddWithValue("@billetes", ModuloBilletesAvion);
            cmd.Parameters.AddWithValue("@viajes", ModuloPackViajes);
            cmd.Parameters.AddWithValue("@idComercio", ComercioSeleccionado.IdComercio);

            await cmd.ExecuteNonQueryAsync();
        }

        // 3. Actualizar locales (eliminar existentes y crear nuevos)
        var queryEliminarLocales = "DELETE FROM locales WHERE id_comercio = @idComercio";
        using (var cmd = new NpgsqlCommand(queryEliminarLocales, connection))
        {
            cmd.Parameters.AddWithValue("@idComercio", ComercioSeleccionado.IdComercio);
            await cmd.ExecuteNonQueryAsync();
        }

        // Insertar locales actualizados
        foreach (var local in LocalesFormulario)
        {
            var queryLocal = @"
                INSERT INTO locales 
                (id_comercio, codigo_local, nombre_local, direccion, activo)
                VALUES 
                (@idComercio, @codigo, @nombre, @direccion, @activo)";

            using var cmd = new NpgsqlCommand(queryLocal, connection);
            cmd.Parameters.AddWithValue("@idComercio", ComercioSeleccionado.IdComercio);
            cmd.Parameters.AddWithValue("@codigo", local.CodigoLocal);
            cmd.Parameters.AddWithValue("@nombre", local.NombreLocal);
            cmd.Parameters.AddWithValue("@direccion", local.Direccion);
            cmd.Parameters.AddWithValue("@activo", local.Activo);

            await cmd.ExecuteNonQueryAsync();
        }
    }

    [RelayCommand]
    private void EditarComercio(ComercioModel comercio)
    {
        if (comercio == null) return;

        ComercioSeleccionado = comercio;
        
        // Cargar datos en el formulario
        NombreComercio = comercio.NombreComercio;
        NombreSrl = comercio.NombreSrl ?? string.Empty;
        DireccionCentral = comercio.DireccionCentral;
        NumeroContacto = comercio.NumeroContacto ?? string.Empty;
        MailContacto = comercio.MailContacto;
        Pais = comercio.Pais;
        Observaciones = comercio.Observaciones ?? string.Empty;
        PorcentajeComisionDivisas = comercio.PorcentajeComisionDivisas;
        ModuloDivisas = comercio.ModuloDivisas;
        ModuloPackAlimentos = comercio.ModuloPackAlimentos;
        ModuloBilletesAvion = comercio.ModuloBilletesAvion;
        ModuloPackViajes = comercio.ModuloPackViajes;
        Activo = comercio.Activo;

        // Cargar locales
        LocalesFormulario.Clear();
        foreach (var local in comercio.Locales)
        {
            LocalesFormulario.Add(new LocalFormModel
            {
                IdLocal = local.IdLocal,
                CodigoLocal = local.CodigoLocal,
                NombreLocal = local.NombreLocal,
                Direccion = local.Direccion,
                Activo = local.Activo
            });
        }

        ModoEdicion = true;
        MostrarFormulario = true;
        MostrarPanelLocales = LocalesFormulario.Count > 0;
    }

    [RelayCommand]
    private async Task EliminarComercio(ComercioModel comercio)
    {
        if (comercio == null) return;

        if (comercio.TotalUsuarios > 0)
        {
            MensajeExito = $"No se puede eliminar el comercio '{comercio.NombreComercio}' porque tiene {comercio.TotalUsuarios} usuario(s) activo(s)";
            MostrarMensajeExito = true;
            await Task.Delay(3000);
            MostrarMensajeExito = false;
            return;
        }

        Cargando = true;

        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            // 1. Eliminar locales
            var queryLocales = "DELETE FROM locales WHERE id_comercio = @idComercio";
            using (var cmd = new NpgsqlCommand(queryLocales, connection))
            {
                cmd.Parameters.AddWithValue("@idComercio", comercio.IdComercio);
                await cmd.ExecuteNonQueryAsync();
            }

            // 2. Eliminar permisos
            var queryPermisos = "DELETE FROM permisos_modulos WHERE id_comercio = @idComercio";
            using (var cmd = new NpgsqlCommand(queryPermisos, connection))
            {
                cmd.Parameters.AddWithValue("@idComercio", comercio.IdComercio);
                await cmd.ExecuteNonQueryAsync();
            }

            // 3. Eliminar comercio
            var queryComercio = "DELETE FROM comercios WHERE id_comercio = @idComercio";
            using (var cmd = new NpgsqlCommand(queryComercio, connection))
            {
                cmd.Parameters.AddWithValue("@idComercio", comercio.IdComercio);
                await cmd.ExecuteNonQueryAsync();
            }

            // Recargar datos
            await CargarDatosDesdeBaseDatos();

            MensajeExito = $"Comercio '{comercio.NombreComercio}' eliminado exitosamente";
            MostrarMensajeExito = true;
            
            await Task.Delay(3000);
            MostrarMensajeExito = false;
        }
        catch (Exception ex)
        {
            MensajeExito = $"Error al eliminar: {ex.Message}";
            MostrarMensajeExito = true;
        }
        finally
        {
            Cargando = false;
        }
    }

    [RelayCommand]
    private async Task CambiarEstadoComercio(ComercioModel comercio)
    {
        if (comercio == null) return;

        Cargando = true;

        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var nuevoEstado = !comercio.Activo;

            var query = @"
                UPDATE comercios 
                SET activo = @activo, fecha_ultima_modificacion = CURRENT_TIMESTAMP
                WHERE id_comercio = @idComercio";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@activo", nuevoEstado);
            cmd.Parameters.AddWithValue("@idComercio", comercio.IdComercio);

            await cmd.ExecuteNonQueryAsync();

            // Actualizar en la colección local
            comercio.Activo = nuevoEstado;
            OnPropertyChanged(nameof(Comercios));
            OnPropertyChanged(nameof(ComerciosActivos));
            OnPropertyChanged(nameof(ComerciosInactivos));
        }
        catch (Exception ex)
        {
            MensajeExito = $"Error al cambiar estado: {ex.Message}";
            MostrarMensajeExito = true;
        }
        finally
        {
            Cargando = false;
        }
    }

    // ============================================
    // COMANDOS - GESTIÓN DE LOCALES
    // ============================================

    [RelayCommand]
    private void AgregarLocal()
    {
        var numeroLocal = LocalesFormulario.Count + 1;
        var codigoBase = NombreComercio.Length >= 3 
            ? NombreComercio.Substring(0, 3).ToUpper() 
            : "COM";
        
        var nuevoLocal = new LocalFormModel
        {
            IdLocal = numeroLocal,
            CodigoLocal = $"{codigoBase}{(Comercios.Count + 1):D3}-{numeroLocal:D2}",
            NombreLocal = $"Local {numeroLocal}",
            Direccion = string.Empty,
            Activo = true
        };

        LocalesFormulario.Add(nuevoLocal);
        MostrarPanelLocales = true;
    }

    [RelayCommand]
    private void EliminarLocal(LocalFormModel local)
    {
        if (local == null) return;
        
        LocalesFormulario.Remove(local);
        ActualizarCodigosLocales();
        
        if (LocalesFormulario.Count == 0)
        {
            MostrarPanelLocales = false;
        }
    }

    // ============================================
    // MÉTODOS PRIVADOS - VALIDACIÓN
    // ============================================

    private bool ValidarFormulario()
    {
        bool esValido = true;
        
        ErrorNombreComercio = string.Empty;
        ErrorMailContacto = string.Empty;

        if (string.IsNullOrWhiteSpace(NombreComercio))
        {
            ErrorNombreComercio = "El nombre del comercio es obligatorio";
            esValido = false;
        }
        else if (NombreComercio.Length < 3)
        {
            ErrorNombreComercio = "El nombre debe tener al menos 3 caracteres";
            esValido = false;
        }

        if (string.IsNullOrWhiteSpace(MailContacto))
        {
            ErrorMailContacto = "El email de contacto es obligatorio";
            esValido = false;
        }
        else if (!MailContacto.Contains("@") || !MailContacto.Contains("."))
        {
            ErrorMailContacto = "Ingrese un email válido";
            esValido = false;
        }

        if (!ModuloDivisas && !ModuloPackAlimentos && !ModuloBilletesAvion && !ModuloPackViajes)
        {
            esValido = false;
        }

        FormularioValido = esValido;
        return esValido;
    }

    // ============================================
    // MÉTODOS PRIVADOS - UTILIDADES
    // ============================================

    private void LimpiarFormulario()
    {
        NombreComercio = string.Empty;
        NombreSrl = string.Empty;
        DireccionCentral = string.Empty;
        NumeroContacto = string.Empty;
        MailContacto = string.Empty;
        Pais = string.Empty;
        Observaciones = string.Empty;
        PorcentajeComisionDivisas = 0;
        ModuloDivisas = false;
        ModuloPackAlimentos = false;
        ModuloBilletesAvion = false;
        ModuloPackViajes = false;
        Activo = true;
        LocalesFormulario.Clear();
        MostrarPanelLocales = false;
        
        ErrorNombreComercio = string.Empty;
        ErrorMailContacto = string.Empty;
        FormularioValido = true;
    }

    private void ActualizarCodigosLocales()
    {
        var codigoBase = NombreComercio.Length >= 3 
            ? NombreComercio.Substring(0, 3).ToUpper() 
            : "COM";
        
        for (int i = 0; i < LocalesFormulario.Count; i++)
        {
            LocalesFormulario[i].CodigoLocal = $"{codigoBase}{(Comercios.Count + 1):D3}-{(i + 1):D2}";
            LocalesFormulario[i].IdLocal = i + 1;
        }
    }
}

/// <summary>
/// Modelo auxiliar para gestionar locales en el formulario
/// </summary>
public class LocalFormModel
{
    public int IdLocal { get; set; }
    public string CodigoLocal { get; set; } = string.Empty;
    public string NombreLocal { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}