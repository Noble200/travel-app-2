using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Allva.Desktop.Services;

namespace Allva.Desktop.ViewModels.Admin;

/// <summary>
/// ViewModel para el Panel de Administración
/// Exclusivo para administradores del sistema
/// MÓDULOS: Gestión de Comercios y Gestión de Usuarios
/// </summary>
public partial class AdminDashboardViewModel : ObservableObject
{
    // ============================================
    // PROPIEDADES OBSERVABLES
    // ============================================

    [ObservableProperty]
    private UserControl? _currentView;

    [ObservableProperty]
    private string _adminName = "Administrador";

    [ObservableProperty]
    private string _selectedModule = "comercios";

    /// <summary>
    /// Título del módulo seleccionado en mayúsculas para mostrar en UI
    /// </summary>
    public string SelectedModuleTitle => SelectedModule switch
    {
        "comercios" => "GESTIÓN DE COMERCIOS",
        "usuarios" => "GESTIÓN DE USUARIOS",
        _ => "PANEL DE ADMINISTRACIÓN"
    };

    // ============================================
    // CONSTRUCTOR
    // ============================================

    public AdminDashboardViewModel()
    {
        // Cargar vista inicial: Gestión de Comercios
        NavigateToModule("comercios");
    }

    /// <summary>
    /// Constructor con nombre del administrador
    /// </summary>
    public AdminDashboardViewModel(string adminName)
    {
        AdminName = adminName;
        NavigateToModule("comercios");
    }

    // ============================================
    // COMANDOS
    // ============================================

    /// <summary>
    /// Navega a un módulo específico
    /// </summary>
    [RelayCommand]
    private void NavigateToModule(string moduleName)
    {
        SelectedModule = moduleName?.ToLower() ?? "comercios";
        
        // Notificar que cambió el título también
        OnPropertyChanged(nameof(SelectedModuleTitle));

        CurrentView = SelectedModule switch
        {
            "comercios" => CreatePlaceholderView(
                "🏢 GESTIÓN DE COMERCIOS", 
                "Módulo para administrar comercios/sucursales",
                "• Crear nuevos comercios\n• Editar información de comercios\n• Asignar locales a comercios\n• Ver estadísticas por comercio"
            ),
            "usuarios" => CreatePlaceholderView(
                "👥 GESTIÓN DE USUARIOS", 
                "Módulo para administrar usuarios del sistema",
                "• Crear nuevos usuarios\n• Editar permisos de usuarios\n• Asignar usuarios a locales\n• Ver actividad de usuarios"
            ),
            _ => CreatePlaceholderView(
                "❓ MÓDULO NO DISPONIBLE", 
                "Este módulo no existe",
                "Selecciona un módulo válido del menú lateral"
            )
        };
    }

    /// <summary>
    /// Cierra sesión y vuelve al login
    /// </summary>
    [RelayCommand]
    private void Logout()
    {
        var navigationService = new NavigationService();
        navigationService.NavigateToLogin();
    }

    // ============================================
    // MÉTODOS PRIVADOS
    // ============================================

    /// <summary>
    /// Crea una vista placeholder temporal para los módulos
    /// Diseño visual Allva con colores corporativos
    /// </summary>
    private UserControl CreatePlaceholderView(string title, string subtitle, string features)
    {
        // Panel principal (sin Padding, solo Margin)
        var mainPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(0),
            Spacing = 0
        };

        // Card blanco contenedor
        var cardBorder = new Border
        {
            Background = Avalonia.Media.Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(12),
            Padding = new Avalonia.Thickness(0),
            BoxShadow = new Avalonia.Media.BoxShadows(
                new Avalonia.Media.BoxShadow
                {
                    Blur = 20,
                    Color = Avalonia.Media.Color.FromArgb(40, 0, 0, 0),
                    OffsetY = 2
                }
            )
        };

        var contentStack = new StackPanel 
        { 
            Spacing = 0 
        };

        // Header amarillo
        var headerBorder = new Border
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FFD966")),
            Padding = new Avalonia.Thickness(30, 20, 30, 20),
            CornerRadius = new Avalonia.CornerRadius(12, 12, 0, 0)
        };

        var headerText = new TextBlock
        {
            Text = title,
            FontSize = 28,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0b5394")),
            TextAlignment = Avalonia.Media.TextAlignment.Left
        };

        headerBorder.Child = headerText;
        contentStack.Children.Add(headerBorder);

        // Border contenedor para el contenido (aquí va el Padding)
        var contentBorder = new Border
        {
            Padding = new Avalonia.Thickness(40, 40, 40, 40)
        };

        // Contenido interno (sin Padding, ya está en el Border)
        var contentPanel = new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        // Subtítulo
        var subtitleBlock = new TextBlock
        {
            Text = subtitle,
            FontSize = 18,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#595959")),
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };

        // Separador
        var separator = new Border
        {
            Height = 2,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FFD966")),
            Margin = new Avalonia.Thickness(0, 10, 0, 20),
            Width = 200,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        // Features
        var featuresBlock = new TextBlock
        {
            Text = features,
            FontSize = 15,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#595959")),
            TextAlignment = Avalonia.Media.TextAlignment.Left,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            LineHeight = 24,
            Margin = new Avalonia.Thickness(0, 0, 0, 30)
        };

        // Badge "En desarrollo"
        var badge = new Border
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FFD966")),
            CornerRadius = new Avalonia.CornerRadius(20),
            Padding = new Avalonia.Thickness(20, 10, 20, 10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        var badgeText = new TextBlock
        {
            Text = "🚧 EN DESARROLLO",
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0b5394"))
        };

        badge.Child = badgeText;

        // Mensaje informativo
        var infoBlock = new TextBlock
        {
            Text = "Los controladores y funcionalidades se implementarán próximamente.\nLa interfaz visual ya está lista.",
            FontSize = 13,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#999999")),
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 20, 0, 0),
            LineHeight = 20
        };

        // Agregar elementos al panel de contenido
        contentPanel.Children.Add(subtitleBlock);
        contentPanel.Children.Add(separator);
        contentPanel.Children.Add(featuresBlock);
        contentPanel.Children.Add(badge);
        contentPanel.Children.Add(infoBlock);

        // El StackPanel va dentro del Border con padding
        contentBorder.Child = contentPanel;
        contentStack.Children.Add(contentBorder);
        
        cardBorder.Child = contentStack;
        mainPanel.Children.Add(cardBorder);

        var userControl = new UserControl
        {
            Content = mainPanel
        };

        return userControl;
    }
}