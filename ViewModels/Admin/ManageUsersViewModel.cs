using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Allva.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Npgsql;

namespace Allva.Desktop.ViewModels.Admin;

/// <summary>
/// ViewModel para la gestión de usuarios en el panel de administración
/// IMPORTANTE: NO muestra usuarios administradores, solo usuarios normales y flotantes
/// CON CONEXIÓN DIRECTA A POSTGRESQL - Igual que ManageComerciosViewModel
/// </summary>
public partial class ManageUsersViewModel : ObservableObject
{
    // ============================================
    // CONFIGURACIÓN DE BASE DE DATOS
    // ============================================
    
    private const string ConnectionString = "Host=switchyard.proxy.rlwy.net;Port=55839;Database=railway;Username=postgres;Password=ysTQxChOYSWUuAPzmYQokqrjpYnKSGbk;";

    // ============================================
    // PROPIEDADES OBSERVABLES - DATOS PRINCIPALES
    // ============================================

    [ObservableProperty]
    private ObservableCollection<UserModel> _usuarios = new();

    [ObservableProperty]
    private ObservableCollection<LocalFormModel> _localesDisponibles = new();

    [ObservableProperty]
    private UserModel? _usuarioSeleccionado;

    [ObservableProperty]
    private bool _mostrarFormulario;

    [ObservableProperty]
    private bool _modoEdicion;

    [ObservableProperty]
    private string _tituloFormulario = "Crear Usuario";

    // ============================================
    // CAMPOS DEL FORMULARIO
    // ============================================

    [ObservableProperty]
    private string _formNumeroUsuario = string.Empty;

    [ObservableProperty]
    private string _formNombre = string.Empty;

    [ObservableProperty]
    private string _formApellidos = string.Empty;

    [ObservableProperty]
    private string _formCorreo = string.Empty;

    [ObservableProperty]
    private string _formTelefono = string.Empty;

    [ObservableProperty]
    private string _formPassword = string.Empty;

    [ObservableProperty]
    private string _formObservaciones = string.Empty;

    [ObservableProperty]
    private bool _formEsFlotante;

    [ObservableProperty]
    private bool _formActivo = true;

    // ============================================
    // BÚSQUEDA Y ASIGNACIÓN DE LOCAL
    // ============================================

    [ObservableProperty]
    private string _busquedaLocal = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LocalFormModel> _resultadosBusquedaLocales = new();

    [ObservableProperty]
    private LocalFormModel? _localSeleccionado;

    [ObservableProperty]
    private ObservableCollection<LocalFormModel> _localesAsignados = new();

    [ObservableProperty]
    private bool _mostrarResultadosBusqueda;

    // ============================================
    // MENSAJES Y NOTIFICACIONES
    // ============================================

    [ObservableProperty]
    private string _mensaje = string.Empty;

    [ObservableProperty]
    private bool _mostrarMensaje;

    [ObservableProperty]
    private bool _mensajeEsExito;

    // ============================================
    // ESTADÍSTICAS
    // ============================================

    [ObservableProperty]
    private int _totalUsuarios;

    [ObservableProperty]
    private int _usuariosActivos;

    [ObservableProperty]
    private int _usuariosFlotantes;

    // ============================================
    // CONSTRUCTOR
    // ============================================

    public ManageUsersViewModel()
    {
        // Cargar datos al inicializar
        _ = InicializarAsync();
    }

    // ============================================
    // MÉTODOS DE INICIALIZACIÓN
    // ============================================

    public async Task InicializarAsync()
    {
        await CargarUsuariosAsync();
        await CargarLocalesDisponiblesAsync();
        ActualizarEstadisticas();
    }

    // ============================================
    // CARGA DE DATOS DESDE POSTGRESQL
    // ============================================

    private async Task CargarUsuariosAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    u.id_usuario,
                    u.id_comercio,
                    u.id_local,
                    u.id_rol,
                    c.nombre_comercio,
                    COALESCE(l.codigo_local, '') as codigo_local,
                    COALESCE(l.nombre_local, 'Sin local') as nombre_local,
                    u.numero_usuario,
                    u.nombre,
                    u.apellidos,
                    u.correo,
                    u.telefono,
                    r.nombre_rol,
                    u.es_flotante,
                    u.activo,
                    u.idioma,
                    u.fecha_creacion,
                    u.ultimo_acceso,
                    COALESCE(u.observaciones, '') as observaciones,
                    u.primer_login,
                    COALESCE(u.intentos_fallidos, 0) as intentos_fallidos
                FROM usuarios u
                INNER JOIN comercios c ON u.id_comercio = c.id_comercio
                LEFT JOIN locales l ON u.id_local = l.id_local
                INNER JOIN roles r ON u.id_rol = r.id_rol
                WHERE u.id_rol != 1
                ORDER BY u.numero_usuario";

            using var cmd = new NpgsqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            Usuarios.Clear();
            while (await reader.ReadAsync())
            {
                Usuarios.Add(new UserModel
                {
                    IdUsuario = reader.GetInt32(0),
                    IdComercio = reader.GetInt32(1),
                    IdLocal = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    IdRol = reader.GetInt32(3),
                    NombreComercio = reader.GetString(4),
                    CodigoLocal = reader.GetString(5),
                    NombreLocal = reader.GetString(6),
                    NumeroUsuario = reader.GetString(7),
                    Nombre = reader.GetString(8),
                    Apellidos = reader.GetString(9),
                    Correo = reader.GetString(10),
                    Telefono = reader.IsDBNull(11) ? null : reader.GetString(11),
                    NombreRol = reader.GetString(12),
                    EsFlotante = reader.GetBoolean(13),
                    Activo = reader.GetBoolean(14),
                    Idioma = reader.IsDBNull(15) ? null : reader.GetString(15),
                    FechaCreacion = reader.GetDateTime(16),
                    UltimoAcceso = reader.IsDBNull(17) ? null : reader.GetDateTime(17),
                    Observaciones = reader.GetString(18),
                    PrimerLogin = reader.GetBoolean(19),
                    IntentosFallidos = reader.GetInt32(20)
                });
            }

            ActualizarEstadisticas();
        }
        catch (Exception ex)
        {
            MostrarMensajeError($"Error al cargar usuarios: {ex.Message}");
        }
    }

    private async Task CargarLocalesDisponiblesAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    id_local,
                    id_comercio,
                    codigo_local,
                    nombre_local,
                    COALESCE(direccion, '') as direccion,
                    COALESCE(local_numero, '') as local_numero,
                    escalera,
                    piso,
                    telefono,
                    email,
                    numero_usuarios_max,
                    observaciones,
                    activo,
                    modulo_divisas,
                    modulo_pack_alimentos,
                    modulo_billetes_avion,
                    modulo_pack_viajes
                FROM locales
                WHERE activo = true
                ORDER BY nombre_local";

            using var cmd = new NpgsqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            LocalesDisponibles.Clear();
            while (await reader.ReadAsync())
            {
                LocalesDisponibles.Add(new LocalFormModel
                {
                    IdLocal = reader.GetInt32(0),
                    IdComercio = reader.GetInt32(1),
                    CodigoLocal = reader.GetString(2),
                    NombreLocal = reader.GetString(3),
                    Direccion = reader.GetString(4),
                    LocalNumero = reader.GetString(5),
                    Escalera = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Piso = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Telefono = reader.IsDBNull(8) ? null : reader.GetString(8),
                    Email = reader.IsDBNull(9) ? null : reader.GetString(9),
                    NumeroUsuariosMax = reader.GetInt32(10),
                    Observaciones = reader.IsDBNull(11) ? null : reader.GetString(11),
                    Activo = reader.GetBoolean(12),
                    ModuloDivisas = reader.GetBoolean(13),
                    ModuloPackAlimentos = reader.GetBoolean(14),
                    ModuloBilletesAvion = reader.GetBoolean(15),
                    ModuloPackViajes = reader.GetBoolean(16)
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar locales: {ex.Message}");
        }
    }

    // ============================================
    // COMANDOS - NAVEGACIÓN Y UI
    // ============================================

    [RelayCommand]
    private void MostrarFormularioCrear()
    {
        LimpiarFormulario();
        ModoEdicion = false;
        TituloFormulario = "Crear Nuevo Usuario";
        MostrarFormulario = true;
    }

    [RelayCommand]
    private void CerrarFormulario()
    {
        MostrarFormulario = false;
        LimpiarFormulario();
    }

    [RelayCommand]
    private void EditarUsuario(UserModel usuario)
    {
        if (usuario == null) return;

        UsuarioSeleccionado = usuario;
        ModoEdicion = true;
        TituloFormulario = "Editar Usuario";

        // Cargar datos en el formulario
        FormNumeroUsuario = usuario.NumeroUsuario;
        FormNombre = usuario.Nombre;
        FormApellidos = usuario.Apellidos;
        FormCorreo = usuario.Correo;
        FormTelefono = usuario.Telefono ?? string.Empty;
        FormObservaciones = usuario.Observaciones;
        FormEsFlotante = usuario.EsFlotante;
        FormActivo = usuario.Activo;

        // Cargar locales asignados
        LocalesAsignados.Clear();
        if (usuario.IdLocal.HasValue)
        {
            var localAsignado = LocalesDisponibles.FirstOrDefault(l => l.IdLocal == usuario.IdLocal.Value);
            if (localAsignado != null)
            {
                LocalesAsignados.Add(localAsignado);
            }
        }

        MostrarFormulario = true;
    }

    // ============================================
    // COMANDOS - CRUD CON POSTGRESQL
    // ============================================

    [RelayCommand]
    private async Task GuardarUsuarioAsync()
    {
        if (!ValidarFormulario())
        {
            return;
        }

        try
        {
            if (ModoEdicion)
            {
                await ActualizarUsuarioAsync();
            }
            else
            {
                await CrearUsuarioAsync();
            }
        }
        catch (Exception ex)
        {
            MostrarMensajeError($"Error al guardar usuario: {ex.Message}");
        }
    }

    private async Task CrearUsuarioAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var query = @"
                INSERT INTO usuarios 
                (id_comercio, id_local, id_rol, numero_usuario, nombre, apellidos, 
                 correo, telefono, password_hash, observaciones, es_flotante, activo, primer_login)
                VALUES 
                (@idComercio, @idLocal, @idRol, @numeroUsuario, @nombre, @apellidos,
                 @correo, @telefono, @password, @observaciones, @esFlotante, @activo, @primerLogin)";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@idComercio", LocalesAsignados.FirstOrDefault()?.IdComercio ?? 1);
            cmd.Parameters.AddWithValue("@idLocal", LocalesAsignados.FirstOrDefault()?.IdLocal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@idRol", 2); // Rol empleado
            cmd.Parameters.AddWithValue("@numeroUsuario", FormNumeroUsuario);
            cmd.Parameters.AddWithValue("@nombre", FormNombre);
            cmd.Parameters.AddWithValue("@apellidos", FormApellidos);
            cmd.Parameters.AddWithValue("@correo", FormCorreo);
            cmd.Parameters.AddWithValue("@telefono", string.IsNullOrWhiteSpace(FormTelefono) ? DBNull.Value : FormTelefono);
            cmd.Parameters.AddWithValue("@password", HashPassword(FormPassword)); // Hash SHA256
            cmd.Parameters.AddWithValue("@observaciones", FormObservaciones);
            cmd.Parameters.AddWithValue("@esFlotante", FormEsFlotante);
            cmd.Parameters.AddWithValue("@activo", FormActivo);
            cmd.Parameters.AddWithValue("@primerLogin", true);

            await cmd.ExecuteNonQueryAsync();

            MostrarMensajeExito($"Usuario '{FormNombre} {FormApellidos}' creado exitosamente");
            await CargarUsuariosAsync();
            CerrarFormulario();
        }
        catch (Exception ex)
        {
            MostrarMensajeError($"Error al crear usuario: {ex.Message}");
        }
    }

    private async Task ActualizarUsuarioAsync()
    {
        if (UsuarioSeleccionado == null) return;

        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE usuarios 
                SET numero_usuario = @numeroUsuario,
                    nombre = @nombre,
                    apellidos = @apellidos,
                    correo = @correo,
                    telefono = @telefono,
                    observaciones = @observaciones,
                    es_flotante = @esFlotante,
                    activo = @activo,
                    id_local = @idLocal,
                    id_comercio = @idComercio,
                    fecha_ultima_modificacion = CURRENT_TIMESTAMP
                WHERE id_usuario = @idUsuario";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@idUsuario", UsuarioSeleccionado.IdUsuario);
            cmd.Parameters.AddWithValue("@numeroUsuario", FormNumeroUsuario);
            cmd.Parameters.AddWithValue("@nombre", FormNombre);
            cmd.Parameters.AddWithValue("@apellidos", FormApellidos);
            cmd.Parameters.AddWithValue("@correo", FormCorreo);
            cmd.Parameters.AddWithValue("@telefono", string.IsNullOrWhiteSpace(FormTelefono) ? DBNull.Value : FormTelefono);
            cmd.Parameters.AddWithValue("@observaciones", FormObservaciones);
            cmd.Parameters.AddWithValue("@esFlotante", FormEsFlotante);
            cmd.Parameters.AddWithValue("@activo", FormActivo);
            cmd.Parameters.AddWithValue("@idLocal", LocalesAsignados.FirstOrDefault()?.IdLocal ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@idComercio", LocalesAsignados.FirstOrDefault()?.IdComercio ?? UsuarioSeleccionado.IdComercio);

            await cmd.ExecuteNonQueryAsync();

            MostrarMensajeExito($"Usuario '{FormNombre} {FormApellidos}' actualizado exitosamente");
            await CargarUsuariosAsync();
            CerrarFormulario();
        }
        catch (Exception ex)
        {
            MostrarMensajeError($"Error al actualizar usuario: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task EliminarUsuarioAsync(UserModel usuario)
    {
        if (usuario == null) return;

        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM usuarios WHERE id_usuario = @idUsuario";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@idUsuario", usuario.IdUsuario);

            await cmd.ExecuteNonQueryAsync();

            MostrarMensajeExito($"Usuario '{usuario.NombreCompleto}' eliminado exitosamente");
            await CargarUsuariosAsync();
        }
        catch (Exception ex)
        {
            MostrarMensajeError($"Error al eliminar usuario: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CambiarEstadoUsuarioAsync(UserModel usuario)
    {
        if (usuario == null) return;

        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var nuevoEstado = !usuario.Activo;

            var query = "UPDATE usuarios SET activo = @activo WHERE id_usuario = @idUsuario";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@activo", nuevoEstado);
            cmd.Parameters.AddWithValue("@idUsuario", usuario.IdUsuario);

            await cmd.ExecuteNonQueryAsync();

            usuario.Activo = nuevoEstado;
            var accion = nuevoEstado ? "activado" : "desactivado";
            MostrarMensajeExito($"Usuario '{usuario.NombreCompleto}' {accion} exitosamente");
            ActualizarEstadisticas();
        }
        catch (Exception ex)
        {
            MostrarMensajeError($"Error al cambiar estado: {ex.Message}");
        }
    }

    // ============================================
    // BÚSQUEDA Y ASIGNACIÓN DE LOCALES
    // ============================================

    [RelayCommand]
    private void BuscarLocal()
    {
        if (string.IsNullOrWhiteSpace(BusquedaLocal))
        {
            ResultadosBusquedaLocales.Clear();
            MostrarResultadosBusqueda = false;
            return;
        }

        var termino = BusquedaLocal.ToLower();

        var resultados = LocalesDisponibles
            .Where(l => 
                l.CodigoLocal.ToLower().Contains(termino) ||
                l.NombreLocal.ToLower().Contains(termino) ||
                (!string.IsNullOrWhiteSpace(l.Direccion) && l.Direccion.ToLower().Contains(termino)))
            .Take(10)
            .ToList();

        ResultadosBusquedaLocales.Clear();
        foreach (var local in resultados)
        {
            ResultadosBusquedaLocales.Add(local);
        }

        MostrarResultadosBusqueda = resultados.Any();
    }

    [RelayCommand]
    private void SeleccionarLocal(LocalFormModel local)
    {
        if (local == null) return;

        // Si es flotante, puede tener múltiples locales
        if (FormEsFlotante)
        {
            if (!LocalesAsignados.Any(l => l.IdLocal == local.IdLocal))
            {
                LocalesAsignados.Add(local);
            }
        }
        else
        {
            // Si no es flotante, solo puede tener un local
            LocalesAsignados.Clear();
            LocalesAsignados.Add(local);
        }

        BusquedaLocal = string.Empty;
        ResultadosBusquedaLocales.Clear();
        MostrarResultadosBusqueda = false;
    }

    [RelayCommand]
    private void QuitarLocalAsignado(LocalFormModel local)
    {
        if (local != null)
        {
            LocalesAsignados.Remove(local);
        }
    }

    // Comando que se ejecuta cuando cambia el checkbox de "Es Flotante"
    partial void OnFormEsFlotanteChanged(bool value)
    {
        if (!value && LocalesAsignados.Count > 1)
        {
            // Si deja de ser flotante y tiene más de un local, mantener solo el primero
            var primerLocal = LocalesAsignados.First();
            LocalesAsignados.Clear();
            LocalesAsignados.Add(primerLocal);
        }
    }

    // ============================================
    // VALIDACIONES
    // ============================================

    private bool ValidarFormulario()
    {
        if (string.IsNullOrWhiteSpace(FormNumeroUsuario))
        {
            MostrarMensajeError("El número de usuario es obligatorio");
            return false;
        }

        if (string.IsNullOrWhiteSpace(FormNombre))
        {
            MostrarMensajeError("El nombre es obligatorio");
            return false;
        }

        if (string.IsNullOrWhiteSpace(FormApellidos))
        {
            MostrarMensajeError("Los apellidos son obligatorios");
            return false;
        }

        if (string.IsNullOrWhiteSpace(FormCorreo))
        {
            MostrarMensajeError("El correo electrónico es obligatorio");
            return false;
        }

        // Validación básica de formato de correo
        if (!FormCorreo.Contains("@") || !FormCorreo.Contains("."))
        {
            MostrarMensajeError("El formato del correo electrónico no es válido");
            return false;
        }

        if (!ModoEdicion && string.IsNullOrWhiteSpace(FormPassword))
        {
            MostrarMensajeError("La contraseña es obligatoria para nuevos usuarios");
            return false;
        }

        // Validación de longitud mínima de contraseña
        if (!ModoEdicion && !string.IsNullOrWhiteSpace(FormPassword) && FormPassword.Length < 6)
        {
            MostrarMensajeError("La contraseña debe tener al menos 6 caracteres");
            return false;
        }

        if (!LocalesAsignados.Any())
        {
            MostrarMensajeError("Debe asignar al menos un local al usuario");
            return false;
        }

        if (!FormEsFlotante && LocalesAsignados.Count > 1)
        {
            MostrarMensajeError("Un usuario no flotante solo puede tener un local asignado");
            return false;
        }

        return true;
    }

    // ============================================
    // MÉTODOS AUXILIARES
    // ============================================

    private void LimpiarFormulario()
    {
        FormNumeroUsuario = string.Empty;
        FormNombre = string.Empty;
        FormApellidos = string.Empty;
        FormCorreo = string.Empty;
        FormTelefono = string.Empty;
        FormPassword = string.Empty;
        FormObservaciones = string.Empty;
        FormEsFlotante = false;
        FormActivo = true;
        LocalesAsignados.Clear();
        BusquedaLocal = string.Empty;
        ResultadosBusquedaLocales.Clear();
        MostrarResultadosBusqueda = false;
        UsuarioSeleccionado = null;
    }

    private void ActualizarEstadisticas()
    {
        TotalUsuarios = Usuarios.Count;
        UsuariosActivos = Usuarios.Count(u => u.Activo);
        UsuariosFlotantes = Usuarios.Count(u => u.EsFlotante);
    }

    private void MostrarMensajeExito(string mensaje)
    {
        Mensaje = mensaje;
        MensajeEsExito = true;
        MostrarMensaje = true;

        // Ocultar mensaje después de 3 segundos
        Task.Delay(3000).ContinueWith(_ =>
        {
            MostrarMensaje = false;
        });
    }

    private void MostrarMensajeError(string mensaje)
    {
        Mensaje = mensaje;
        MensajeEsExito = false;
        MostrarMensaje = true;

        // Ocultar mensaje después de 4 segundos
        Task.Delay(4000).ContinueWith(_ =>
        {
            MostrarMensaje = false;
        });
    }

    // ============================================
    // MÉTODO DE HASH DE CONTRASEÑA
    // ============================================

    /// <summary>
    /// Hashea la contraseña usando SHA256
    /// NOTA: Para producción, considera usar BCrypt.Net-Next
    /// </summary>
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}