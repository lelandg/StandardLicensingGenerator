using System.Windows.Media;

namespace StandardLicensingGenerator.Views;

/// <summary>
/// Represents a set of style properties for the CustomMessageBox
/// </summary>
public class CustomMessageBoxStyle
{
    /// <summary>
    /// Background brush for the message box title bar
    /// </summary>
    public Brush? TitleBackground { get; set; }

    /// <summary>
    /// Background brush for the main message box window
    /// </summary>
    public Brush? WindowBackground { get; set; }

    /// <summary>
    /// Brush for the message box border
    /// </summary>
    public Brush? BorderBrush { get; set; }

    /// <summary>
    /// Background brush for buttons in normal state
    /// </summary>
    public Brush? ButtonBackground { get; set; }

    /// <summary>
    /// Background brush for buttons when mouse hovers over them
    /// </summary>
    public Brush? ButtonHoverBackground { get; set; }

    /// <summary>
    /// Background brush for buttons when pressed
    /// </summary>
    public Brush? ButtonPressedBackground { get; set; }

    /// <summary>
    /// Background brush for disabled buttons
    /// </summary>
    public Brush? ButtonDisabledBackground { get; set; }

    /// <summary>
    /// Foreground brush for the message box title text
    /// </summary>
    public Brush? TitleForeground { get; set; }

    /// <summary>
    /// Foreground brush for button text
    /// </summary>
    public Brush? ButtonForeground { get; set; }

    /// <summary>
    /// Foreground brush for disabled button text
    /// </summary>
    public Brush? ButtonDisabledForeground { get; set; }

    /// <summary>
    /// Creates a new CustomMessageBoxStyle with all properties set to null (using current styles)
    /// </summary>
    public CustomMessageBoxStyle() { }

    /// <summary>
    /// Creates a new CustomMessageBoxStyle based on an existing style generator
    /// </summary>
    /// <param name="generator">The style generator to copy styles from</param>
    public CustomMessageBoxStyle(MessageBoxStyleGenerator generator)
    {
        if (generator == null) throw new ArgumentNullException(nameof(generator));

        TitleBackground = generator.TitleBackground;
        WindowBackground = generator.WindowBackground;
        BorderBrush = generator.BorderBrush;
        ButtonBackground = generator.ButtonBackground;
        ButtonHoverBackground = generator.ButtonHoverBackground;
        ButtonPressedBackground = generator.ButtonPressedBackground;
        ButtonDisabledBackground = generator.ButtonDisabledBackground;
        TitleForeground = generator.TitleForeground;
        ButtonForeground = generator.ButtonForeground;
        ButtonDisabledForeground = generator.ButtonDisabledForeground;
    }

    /// <summary>
    /// Applies this style to a message box style generator, only updating properties that are not null
    /// </summary>
    /// <param name="generator">The generator to update with this style's properties</param>
    public void ApplyTo(MessageBoxStyleGenerator generator)
    {
        if (generator == null) throw new ArgumentNullException(nameof(generator));

        if (TitleBackground != null) generator.TitleBackground = TitleBackground;
        if (WindowBackground != null) generator.WindowBackground = WindowBackground;
        if (BorderBrush != null) generator.BorderBrush = BorderBrush;
        if (ButtonBackground != null) generator.ButtonBackground = ButtonBackground;
        if (ButtonHoverBackground != null) generator.ButtonHoverBackground = ButtonHoverBackground;
        if (ButtonPressedBackground != null) generator.ButtonPressedBackground = ButtonPressedBackground;
        if (ButtonDisabledBackground != null) generator.ButtonDisabledBackground = ButtonDisabledBackground;
        if (TitleForeground != null) generator.TitleForeground = TitleForeground;
        if (ButtonForeground != null) generator.ButtonForeground = ButtonForeground;
        if (ButtonDisabledForeground != null) generator.ButtonDisabledForeground = ButtonDisabledForeground;
    }
}
