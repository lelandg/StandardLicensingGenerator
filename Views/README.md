# CustomMessageBox

A modern, customizable alternative to the standard WPF MessageBox that maintains a similar API while offering improved styling and additional features.

## Features

- **Drop-in replacement** for the standard WPF MessageBox
- **Modern styling** with customizable appearance
- **Custom icon support** - use your own icons or fall back to system icons
- **Keyboard shortcuts** for quick interaction (Y/N/O/C keys and Esc)
- **Owner window association** for proper modal dialog behavior
- **Flexible API** supporting various overloads similar to standard MessageBox

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
