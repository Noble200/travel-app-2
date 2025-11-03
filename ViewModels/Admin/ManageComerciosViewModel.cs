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
/// ACTUALIZADO: Permisos por local + Cierre automático + Animación
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
    private bool _activo = true;

    // ============================================
    // PROPIEDADES OBSERVABLES - GESTIÓN DE LOCALES
    // ACTUALIZADO: Nombre cambiado de LocalesFormulario a Locales
    // ============================================

    [ObservableProperty]
    private ObservableCollection<LocalFormModel> _locales = new();

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
                // Cargar locales del comercio CON PERMISOS
                comercio.Locales = await CargarLocalesDelComercio(connection, comercio.IdComercio);
                
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
            await Task.Delay(3000);
            MostrarMensajeExito = false;
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

    /// <summary>
    /// ACTUALIZADO: Ahora carga los permisos de módulos individuales por local
    /// </summary>
    private async Task<List<LocalSimpleModel>> CargarLocalesDelComercio(NpgsqlConnection connection, int idComercio)
    {
        var locales = new List<LocalSimpleModel>();

        var query = @"
            SELECT 
                id_local,
                codigo_local,
                nombre_local,
                direccion,
                activo,
                modulo_divisas,
                modulo_pack_alimentos,
                modulo_billetes_avion,
                modulo_pack_viajes,
                (SELECT COUNT(*) FROM usuarios WHERE id_local = l.id_local) as num_usuarios
            FROM locales l
            WHERE id_comercio = @IdComercio
            ORDER BY nombre_local";

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
                Direccion = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Activo = reader.GetBoolean(4),
                ModuloDivisas = reader.GetBoolean(5),
                ModuloPackAlimentos = reader.GetBoolean(6),
                ModuloBilletesAvion = reader.GetBoolean(7),
                ModuloPackViajes = reader.GetBoolean(8),
                NumeroUsuarios = reader.GetInt32(9)
            });
        }

        return locales;
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

    /// <summary>
    /// ACTUALIZADO: Comando para mostrar formulario de creación
    /// </summary>
    [RelayCommand]
    private void MostrarFormularioCreacion()
    {
        LimpiarFormulario();
        ModoEdicion = false;
        MostrarFormulario = true;
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

    /// <summary>
    /// ACTUALIZADO: Ahora con animación y cierre automático
    /// </summary>
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

            // ✅ NUEVO: Mensaje de éxito con cierre automático
            MensajeExito = ModoEdicion 
                ? $"✓ Comercio '{NombreComercio}' actualizado exitosamente" 
                : $"✓ Comercio '{NombreComercio}' creado exitosamente con {Locales.Count} local(es)";
            
            MostrarMensajeExito = true;
            
            // ANIMACIÓN: Esperar 2 segundos mostrando el mensaje
            await Task.Delay(2000);
            
            // Ocultar mensaje
            MostrarMensajeExito = false;
            
            // ✅ CERRAR FORMULARIO AUTOMÁTICAMENTE
            MostrarFormulario = false;
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            MensajeExito = $"❌ Error al guardar: {ex.Message}";
            MostrarMensajeExito = true;
            await Task.Delay(4000);
            MostrarMensajeExito = false;
        }
        finally
        {
            Cargando = false;
        }
    }

    /// <summary>
    /// ACTUALIZADO: Ahora crea locales con permisos individuales
    /// </summary>
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

        // 2. ✅ NUEVO: Insertar locales con sus permisos individuales
        foreach (var local in Locales)
        {
            var queryLocal = @"
                INSERT INTO locales 
                (id_comercio, codigo_local, nombre_local, direccion, activo,
                 modulo_divisas, modulo_pack_alimentos, modulo_billetes_avion, modulo_pack_viajes)
                VALUES 
                (@idComercio, @codigo, @nombre, @direccion, @activo,
                 @moduloDivisas, @moduloAlimentos, @moduloBilletes, @moduloViajes)";

            using var cmdLocal = new NpgsqlCommand(queryLocal, connection);
            cmdLocal.Parameters.AddWithValue("@idComercio", idComercio);
            cmdLocal.Parameters.AddWithValue("@codigo", local.CodigoLocal);
            cmdLocal.Parameters.AddWithValue("@nombre", local.NombreLocal);
            cmdLocal.Parameters.AddWithValue("@direccion", string.IsNullOrEmpty(local.Direccion) ? DBNull.Value : local.Direccion);
            cmdLocal.Parameters.AddWithValue("@activo", local.Activo);
            cmdLocal.Parameters.AddWithValue("@moduloDivisas", local.ModuloDivisas);
            cmdLocal.Parameters.AddWithValue("@moduloAlimentos", local.ModuloPackAlimentos);
            cmdLocal.Parameters.AddWithValue("@moduloBilletes", local.ModuloBilletesAvion);
            cmdLocal.Parameters.AddWithValue("@moduloViajes", local.ModuloPackViajes);

            await cmdLocal.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// ACTUALIZADO: Ahora actualiza locales con permisos individuales
    /// </summary>
    private async Task ActualizarComercioEnBaseDatos(NpgsqlConnection connection)
    {
        if (ComercioSeleccionado == null) return;

        // 1. Actualizar datos del comercio
        var queryComercio = @"
            UPDATE comercios 
            SET 
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

        // 2. Eliminar locales existentes
        var queryDeleteLocales = "DELETE FROM locales WHERE id_comercio = @idComercio";
        using (var cmd = new NpgsqlCommand(queryDeleteLocales, connection))
        {
            cmd.Parameters.AddWithValue("@idComercio", ComercioSeleccionado.IdComercio);
            await cmd.ExecuteNonQueryAsync();
        }

        // 3. ✅ NUEVO: Insertar locales actualizados con sus permisos individuales
        foreach (var local in Locales)
        {
            var queryLocal = @"
                INSERT INTO locales 
                (id_comercio, codigo_local, nombre_local, direccion, activo,
                 modulo_divisas, modulo_pack_alimentos, modulo_billetes_avion, modulo_pack_viajes)
                VALUES 
                (@idComercio, @codigo, @nombre, @direccion, @activo,
                 @moduloDivisas, @moduloAlimentos, @moduloBilletes, @moduloViajes)";

            using var cmdLocal = new NpgsqlCommand(queryLocal, connection);
            cmdLocal.Parameters.AddWithValue("@idComercio", ComercioSeleccionado.IdComercio);
            cmdLocal.Parameters.AddWithValue("@codigo", local.CodigoLocal);
            cmdLocal.Parameters.AddWithValue("@nombre", local.NombreLocal);
            cmdLocal.Parameters.AddWithValue("@direccion", string.IsNullOrEmpty(local.Direccion) ? DBNull.Value : local.Direccion);
            cmdLocal.Parameters.AddWithValue("@activo", local.Activo);
            cmdLocal.Parameters.AddWithValue("@moduloDivisas", local.ModuloDivisas);
            cmdLocal.Parameters.AddWithValue("@moduloAlimentos", local.ModuloPackAlimentos);
            cmdLocal.Parameters.AddWithValue("@moduloBilletes", local.ModuloBilletesAvion);
            cmdLocal.Parameters.AddWithValue("@moduloViajes", local.ModuloPackViajes);

            await cmdLocal.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// ACTUALIZADO: Ahora carga locales con permisos individuales
    /// </summary>
    [RelayCommand]
    private void EditarComercio(ComercioModel comercio)
    {
        if (comercio == null) return;

        ComercioSeleccionado = comercio;

        // Cargar datos del comercio
        NombreComercio = comercio.NombreComercio;
        NombreSrl = string.IsNullOrEmpty(comercio.NombreSrl) ? string.Empty : comercio.NombreSrl;
        DireccionCentral = comercio.DireccionCentral;
        NumeroContacto = comercio.NumeroContacto ?? string.Empty;
        MailContacto = comercio.MailContacto;
        Pais = comercio.Pais;
        Observaciones = comercio.Observaciones ?? string.Empty;
        PorcentajeComisionDivisas = comercio.PorcentajeComisionDivisas;
        Activo = comercio.Activo;

        // ✅ NUEVO: Cargar locales con sus permisos individuales Y TODOS LOS CAMPOS
        Locales.Clear();
        foreach (var local in comercio.Locales)
        {
            Locales.Add(new LocalFormModel
            {
                IdLocal = local.IdLocal,
                CodigoLocal = local.CodigoLocal,
                NombreLocal = local.NombreLocal,
                Direccion = local.Direccion,
                LocalNumero = local.LocalNumero,
                Escalera = local.Escalera,
                Piso = local.Piso,
                Telefono = local.Telefono,
                Email = local.Email,
                NumeroUsuariosMax = local.NumeroUsuariosMax,
                Observaciones = local.Observaciones,
                Activo = local.Activo,
                ModuloDivisas = local.ModuloDivisas,
                ModuloPackAlimentos = local.ModuloPackAlimentos,
                ModuloBilletesAvion = local.ModuloBilletesAvion,
                ModuloPackViajes = local.ModuloPackViajes
            });
        }

        ModoEdicion = true;
        MostrarFormulario = true;
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

            // 2. Eliminar comercio
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
            await Task.Delay(3000);
            MostrarMensajeExito = false;
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
            await Task.Delay(3000);
            MostrarMensajeExito = false;
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
        var numeroLocal = Locales.Count + 1;
        var codigoBase = NombreComercio.Length >= 3 
            ? NombreComercio.Substring(0, 3).ToUpper() 
            : "COM";
        
        var nuevoLocal = new LocalFormModel
        {
            IdLocal = 0,
            CodigoLocal = $"{codigoBase}{numeroLocal:D3}",
            NombreLocal = $"Local {numeroLocal}",
            Direccion = string.Empty,
            LocalNumero = string.Empty,
            Escalera = null,
            Piso = null,
            Telefono = null,
            Email = null,
            NumeroUsuariosMax = 10,
            Observaciones = null,
            Activo = true,
            ModuloDivisas = false,
            ModuloPackAlimentos = false,
            ModuloBilletesAvion = false,
            ModuloPackViajes = false
        };

        Locales.Add(nuevoLocal);
    }

    [RelayCommand]
    private void EliminarLocal(LocalFormModel local)
    {
        if (local == null) return;
        
        Locales.Remove(local);
        ActualizarCodigosLocales();
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

        if (Locales.Count == 0)
        {
            MensajeExito = "Debe agregar al menos un local";
            MostrarMensajeExito = true;
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                MostrarMensajeExito = false;
            });
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
        Activo = true;
        Locales.Clear();
        
        ErrorNombreComercio = string.Empty;
        ErrorMailContacto = string.Empty;
        FormularioValido = true;
    }

    private void ActualizarCodigosLocales()
    {
        var codigoBase = NombreComercio.Length >= 3 
            ? NombreComercio.Substring(0, 3).ToUpper() 
            : "COM";
        
        for (int i = 0; i < Locales.Count; i++)
        {
            Locales[i].CodigoLocal = $"{codigoBase}{(i + 1):D3}";
        }
    }
}