# CustomMessageBox

A modern, customizable alternative to the standard WPF MessageBox that maintains a similar API while offering improved styling and additional features.

## GitHub
The latest version of this project is always here:
[CustomMessageBox on GitHub](https://github.com/lelandg/CustomMessageBox)

## Features

- **Drop-in replacement** for the standard WPF MessageBox
- **Modern styling** with customizable appearance
- **Custom icon support** - use your own icons or fall back to system icons
- **Keyboard shortcuts** for quick interaction (Y/N/O/C keys and Esc)
- **Owner window association** for proper modal dialog behavior
- **Flexible API** supporting various overloads similar to standard MessageBox
- **Customizable style** - apply specific styles to individual message boxes

## Usage

### Basic Usage

```csharp
// Simple message
CustomMessageBox.Show("This is a simple message.");

// With title
CustomMessageBox.Show("Operation completed successfully.", "Success");

// With buttons
MessageBoxResult result = CustomMessageBox.Show(
    "Do you want to save changes?", 
    "Save Changes", 
    MessageBoxButton.YesNo);

// With owner window (recommended for proper modal behavior)
MessageBoxResult result = CustomMessageBox.Show(
    this,  // owner window
    "Do you want to proceed?", 
    "Confirmation", 
    MessageBoxButton.YesNoCancel);
```

### Using Icons

```csharp
// Information icon
CustomMessageBox.Show(
    "The process has completed.", 
    "Information", 
    MessageBoxButton.OK, 
    MessageBoxImage.Information);

// Warning icon
CustomMessageBox.Show(
    "This action cannot be undone.", 
    "Warning", 
    MessageBoxButton.OKCancel, 
    MessageBoxImage.Warning);

// Error icon
CustomMessageBox.Show(
    "An error occurred while saving the file.", 
    "Error", 
    MessageBoxButton.OK, 
    MessageBoxImage.Error);

// Question icon
MessageBoxResult result = CustomMessageBox.Show(
    "Do you want to save changes before closing?", 
    "Question", 
    MessageBoxButton.YesNoCancel, 
    MessageBoxImage.Question);
```

### Custom Icons

To use custom icons instead of system icons, add the following PNG files to your Resources folder:

- `error.png`
- `warning.png`
- `info.png`
- `question.png`

Recommended size: 32x32 pixels with a transparent background.

The component will automatically detect and use these custom icons if available, falling back to system icons if not found.

### Using Custom Images

You can also use any image as an icon with the `ShowWithImage` method:

```csharp
// Create a BitmapImage from a resource
BitmapImage customImage = new BitmapImage(new Uri(
    "pack://application:,,,/YourAssemblyName;component/Resources/custom_icon.png",
    UriKind.Absolute));

// Show message box with custom image
MessageBoxResult result = CustomMessageBox.ShowWithImage(
    this,  // owner window
    "This message uses a custom icon.", 
    "Custom Icon", 
    MessageBoxButton.OKCancel, 
    customImage);
```

### Setting Default Result

You can set a default button for keyboard focus:

```csharp
MessageBoxResult result = CustomMessageBox.Show(
    this,
    "Do you want to save changes?",
    "Save Changes", 
    MessageBoxButton.YesNoCancel, 
    MessageBoxImage.Warning,
    MessageBoxResult.Yes);  // Yes will be the default focused button
```

## Processing Results

Handle the result just like with a standard MessageBox:

```csharp
MessageBoxResult result = CustomMessageBox.Show(
    "Do you want to save changes?", 
    "Confirmation", 
    MessageBoxButton.YesNoCancel, 
    MessageBoxImage.Question);

switch (result)
{
    case MessageBoxResult.Yes:
        SaveChanges();
        break;
    case MessageBoxResult.No:
        DiscardChanges();
        break;
    case MessageBoxResult.Cancel:
        // User canceled operation
        break;
}
```

## Keyboard Shortcuts

The CustomMessageBox supports these keyboard shortcuts:

- `Y` - Activates the Yes button (when visible)
- `N` - Activates the No button (when visible)
- `O` - Activates the OK button (when visible)
- `C` - Activates the Cancel button (when visible)
- `Esc` - Activates Cancel or No button (depending on visibility)
- `Enter` - Activates the default button (typically Yes or OK)

## Custom Styling

You can customize the appearance of each message box individually using the `CustomMessageBoxStyle` class and the `ShowWithStyle` methods.

### Creating a Custom Style

```csharp
// Create a style that only changes specific aspects while keeping others the same
var customStyle = new CustomMessageBoxStyle {
    // Only set the properties you want to change
    TitleBackground = new SolidColorBrush(Colors.DarkGreen),
    ButtonBackground = new SolidColorBrush(Colors.DarkGreen),
    ButtonHoverBackground = new SolidColorBrush(Colors.Green)
    // Other properties will keep their current values
};
```

### Applying a Custom Style

```csharp
// Apply the style to a single message box
CustomMessageBox.ShowWithStyle(
    "This message box has a custom style.",
    "Custom Style",
    MessageBoxButton.OKCancel,
    MessageBoxImage.Information,
    customStyle);
```

### Applying Custom Style with Custom Image

```csharp
// Create a custom image
BitmapImage customImage = new BitmapImage(new Uri(
    "pack://application:,,,/YourAssemblyName;component/Resources/custom_icon.png",
    UriKind.Absolute));

// Show message box with both custom style and image
MessageBoxResult result = CustomMessageBox.ShowWithImageAndStyle(
    this,  // owner window
    "This message uses both a custom style and icon.", 
    "Custom Style Example", 
    MessageBoxButton.OKCancel, 
    customImage,
    customStyle);
```

### Available Style Properties

The `CustomMessageBoxStyle` class lets you customize these aspects:

- `WindowBackground` - Background color of the message box
- `BorderBrush` - Border color around the message box
- `TitleBackground` - Background color of the title bar
- `TitleForeground` - Text color of the title
- `ButtonBackground` - Normal background color for buttons
- `ButtonHoverBackground` - Background color when mouse hovers over buttons
- `ButtonPressedBackground` - Background color when buttons are clicked
- `ButtonDisabledBackground` - Background color for disabled buttons
- `ButtonForeground` - Text color for buttons
- `ButtonDisabledForeground` - Text color for disabled buttons

Leave any property as `null` to use the current default style for that aspect.

### Cycling Through Color Themes

You can implement a feature to cycle through different color themes with a button click:

```csharp
// Define some color themes
private readonly Color[][] _colorThemes = new Color[][] {
    new[] { Colors.Purple, Colors.MediumPurple },
    new[] { Colors.DarkRed, Colors.Crimson },
    new[] { Colors.DarkGreen, Colors.ForestGreen },
    new[] { Colors.DarkOrange, Colors.Orange },
    new[] { Colors.Teal, Colors.CadetBlue }
};

private int _currentThemeIndex = 0;

private void OnUniqueColorsButtonClick(object sender, RoutedEventArgs e)
{
    // Cycle to the next color theme
    _currentThemeIndex = (_currentThemeIndex + 1) % _colorThemes.Length;
    var theme = _colorThemes[_currentThemeIndex];

    // Create a custom style with the theme colors
    var customStyle = new CustomMessageBoxStyle {
        TitleBackground = new SolidColorBrush(theme[0]),
        BorderBrush = new SolidColorBrush(theme[0]),
        ButtonBackground = new SolidColorBrush(theme[0]),
        ButtonHoverBackground = new SolidColorBrush(theme[1])
    };

    // Show a message box with the custom style
    CustomMessageBox.ShowWithStyle(
        $"Using color theme: {theme[0].ToString()}",
        "Unique Colors",
        MessageBoxButton.OK, 
        MessageBoxImage.Information,
        customStyle);
}
```

This allows you to quickly showcase different color themes while keeping the same interface.

## Integration in Another Application

### Option 1: Copy the Files

1. Copy these files to your project:
   - `CustomMessageBox.xaml` and `CustomMessageBox.xaml.cs`
   - `SystemIconExtensions.cs` (required for system icon conversion)

2. Update the namespace in the files to match your project structure

3. If you want to use custom icons, add the corresponding PNG files to your Resources folder

### Option 2: Reference as a Library

If you need to use the component in multiple applications:

1. Create a separate Class Library project
2. Add the CustomMessageBox files to that project
3. Reference the library from your WPF applications

## Example Usage

```csharp
// Basic examples
public static void ShowExamples(Window owner)
{
    // Simple message
    CustomMessageBox.Show(owner, "This is a simple message.", "Information");

    // With buttons and icon
    CustomMessageBox.Show(owner, 
        "Would you like to proceed?", 
        "Confirmation", 
        MessageBoxButton.YesNo, 
        MessageBoxImage.Question);

    // Error message
    CustomMessageBox.Show(owner, 
        "An error occurred while processing your request.", 
        "Error", 
        MessageBoxButton.OK, 
        MessageBoxImage.Error);

    // With custom image
    BitmapImage customImage = new BitmapImage(new Uri(
        "pack://application:,,,/YourAssemblyName;component/Resources/custom_icon.png",
        UriKind.Absolute));

    CustomMessageBox.ShowWithImage(owner, 
        "This message uses a custom icon.", 
        "Custom Icon Example", 
        MessageBoxButton.OK, 
        customImage);
}
```
