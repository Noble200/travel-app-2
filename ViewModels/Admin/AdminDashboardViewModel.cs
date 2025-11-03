using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Allva.Desktop.Services;
using Allva.Desktop.Views.Admin;

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
            "comercios" => new ManageComerciosView(), // ✅ VISTA REAL FUNCIONAL
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
            CornerRadius = new Avalonia.CornerRadius(8),
            Padding = new Avalonia.Thickness(40),
            Margin = new Avalonia.Thickness(20),
            BoxShadow = new Avalonia.Media.BoxShadows(
                new Avalonia.Media.BoxShadow
                {
                    Blur = 15,
                    Color = Avalonia.Media.Color.FromArgb(20, 0, 0, 0),
                    OffsetX = 0,
                    OffsetY = 4
                })
        };

        var contentPanel = new StackPanel
        {
            Spacing = 25,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        // Icono decorativo
        var iconBlock = new TextBlock
        {
            Text = "🚧",
            FontSize = 64,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 15)
        };

        // Título principal
        var titleBlock = new TextBlock
        {
            Text = title,
            FontSize = 26,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0b5394")),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment = Avalonia.Media.TextAlignment.Center
        };

        // Subtítulo
        var subtitleBlock = new TextBlock
        {
            Text = subtitle,
            FontSize = 16,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#666666")),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };

        // Separador
        var separator = new Border
        {
            Height = 2,
            Width = 100,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#ffd966")),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 10, 0, 20)
        };

        // Panel de características
        var featuresPanel = new StackPanel
        {
            Spacing = 12,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Avalonia.Thickness(40, 20, 40, 20)
        };

        var featuresTitle = new TextBlock
        {
            Text = "Características del módulo:",
            FontSize = 15,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0b5394")),
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };

        var featuresText = new TextBlock
        {
            Text = features,
            FontSize = 14,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#555555")),
            LineHeight = 24
        };

        featuresPanel.Children.Add(featuresTitle);
        featuresPanel.Children.Add(featuresText);

        // Banner de estado
        var statusBanner = new Border
        {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#fff3cd")),
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#ffd966")),
            BorderThickness = new Avalonia.Thickness(2),
            CornerRadius = new Avalonia.CornerRadius(8),
            Padding = new Avalonia.Thickness(20, 15),
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };

        var statusText = new TextBlock
        {
            Text = "⚠️ Este módulo está en desarrollo y pronto estará disponible",
            FontSize = 14,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#856404")),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            FontWeight = Avalonia.Media.FontWeight.Medium
        };

        statusBanner.Child = statusText;

        // Ensamblar todo
        contentPanel.Children.Add(iconBlock);
        contentPanel.Children.Add(titleBlock);
        contentPanel.Children.Add(subtitleBlock);
        contentPanel.Children.Add(separator);
        contentPanel.Children.Add(featuresPanel);
        contentPanel.Children.Add(statusBanner);

        cardBorder.Child = contentPanel;
        mainPanel.Children.Add(cardBorder);

        return new UserControl
        {
            Content = mainPanel
        };
    }
}