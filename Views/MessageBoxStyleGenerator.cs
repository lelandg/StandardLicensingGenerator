using System.Windows;
using System.Windows.Media;

namespace StandardLicensingGenerator.Views;

/// <summary>
/// Generator for customizing message box appearance across the application
/// </summary>
public class MessageBoxStyleGenerator
{
    private static MessageBoxStyleGenerator? _instance;

    // Color properties
    public Brush TitleBackground { get; set; } = new SolidColorBrush(Color.FromRgb(0x2E, 0x75, 0xB6));
    public Brush WindowBackground { get; set; } = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
    public Brush BorderBrush { get; set; } = new SolidColorBrush(Color.FromRgb(0x2E, 0x75, 0xB6));
    public Brush ButtonBackground { get; set; } = new SolidColorBrush(Color.FromRgb(0x2E, 0x75, 0xB6));
    public Brush ButtonHoverBackground { get; set; } = new SolidColorBrush(Color.FromRgb(0x3E, 0x85, 0xC6));
    public Brush ButtonPressedBackground { get; set; } = new SolidColorBrush(Color.FromRgb(0x1E, 0x65, 0xA6));
    public Brush ButtonDisabledBackground { get; set; } = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
    public Brush TitleForeground { get; set; } = Brushes.White;
    public Brush ButtonForeground { get; set; } = Brushes.White;
    public Brush ButtonDisabledForeground { get; set; } = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));

    /// <summary>
    /// Gets the current generator instance or creates a new one with default settings
    /// </summary>
    public static MessageBoxStyleGenerator Current => _instance ??= new MessageBoxStyleGenerator();

    /// <summary>
    /// Sets a new generator instance as the current one
    /// </summary>
    /// <param name="generator">The generator to set as current</param>
    public static void SetCurrent(MessageBoxStyleGenerator generator)
    {        
        _instance = generator ?? throw new ArgumentNullException(nameof(generator));
    }

    /// <summary>
    /// Creates a new style generator that uses the system dialog colors
    /// </summary>
    /// <returns>A new style generator with system colors</returns>
    public static MessageBoxStyleGenerator CreateSystemColorsGenerator()
    {
        var generator = new MessageBoxStyleGenerator
        {
            // Use SystemColors brushes for dialog appearance
            TitleBackground = SystemColors.ActiveCaptionBrush,
            WindowBackground = SystemColors.ControlBrush,
            BorderBrush = SystemColors.ActiveBorderBrush,
            ButtonBackground = SystemColors.ControlBrush,
            ButtonHoverBackground = SystemColors.ControlLightBrush,
            ButtonPressedBackground = SystemColors.ControlDarkBrush,
            ButtonDisabledBackground = SystemColors.ControlLightLightBrush,
            TitleForeground = SystemColors.ActiveCaptionTextBrush,
            ButtonForeground = SystemColors.ControlTextBrush,
            ButtonDisabledForeground = SystemColors.GrayTextBrush
        };

        return generator;
    }

    /// <summary>
    /// Resets the current generator to null, causing a new default one to be created on next access
    /// </summary>
    public static void ResetToDefaults()
    {
        _instance = null;
    }
}
