# StandardLicensingGenerator
# CustomMessageBox for WPF

A modern, customizable WPF message box replacement that follows the same API pattern as the standard WPF MessageBox while offering enhanced styling and features.

## Features

- **Drop-in replacement** for the standard WPF MessageBox
- **Modern UI** with customizable styling
- **Custom icons** support
- **Keyboard shortcuts** (Y/N/O/C keys and Esc)
- **Owner window** association for proper modal behavior
- **Consistent API** matching WPF's native MessageBox

## Screenshot

*[Add a screenshot of your CustomMessageBox here]*

## Usage

### Basic Usage

The CustomMessageBox can be used just like the standard MessageBox:

```csharp
// Simple message
CustomMessageBox.Show("Operation completed successfully.");

// With caption
CustomMessageBox.Show("Operation completed successfully.", "Success");

// With buttons
MessageBoxResult result = CustomMessageBox.Show(
    "Do you want to save changes?", 
    "Save Changes", 
    MessageBoxButton.YesNo);

// With buttons and icon
MessageBoxResult result = CustomMessageBox.Show(
    "Changes could not be saved.", 
    "Error", 
    MessageBoxButton.OK, 
    MessageBoxImage.Error);

// With owner window for proper modal behavior
MessageBoxResult result = CustomMessageBox.Show(
    this,  // owner window
    "Do you want to proceed?", 
    "Confirmation", 
    MessageBoxButton.YesNoCancel, 
    MessageBoxImage.Question);

// With default result
MessageBoxResult result = CustomMessageBox.Show(
    this,
    "Do you want to save changes?",
    "Save Changes", 
    MessageBoxButton.YesNoCancel, 
    MessageBoxImage.Warning,
    MessageBoxResult.Yes);  // default is 'Yes'
```

### Using Custom Icons

You can use custom icons instead of system icons by adding these image files to your Resources folder:

- `error.png`
- `warning.png`
- `info.png`
- `question.png`

Recommended size: 32x32 pixels with a transparent background.

When these files are present, the CustomMessageBox will use them instead of system icons. If not found, it will automatically fall back to system icons.

### Using a Custom Image

You can also use any custom image as an icon:

```csharp
// Create a custom image source
BitmapImage customImage = new BitmapImage(new Uri("pack://application:,,,/Resources/custom-icon.png"));

// Show message box with custom image
MessageBoxResult result = CustomMessageBox.ShowWithImage(
    this,  // owner window
    "This is a message with a custom icon.",
    "Custom Icon", 
    MessageBoxButton.OKCancel, 
    customImage);
```

## Installation

### Option 1: Add files to your project

1. Copy these files to your project:
   - `CustomMessageBox.xaml` and `CustomMessageBox.xaml.cs`
   - `SystemIconExtensions.cs`

2. Update the namespace in the files to match your project

### Option 2: Add as a reference

If using the component across multiple projects:

1. Create a separate Class Library project
2. Add the files to that project
3. Reference the library from your WPF applications

## Customization

### Styling

You can customize the appearance by modifying the XAML in `CustomMessageBox.xaml`. The message box uses a clean, modern style with a light background and blue accent buttons by default.

### Layout

The layout uses a responsive grid that automatically resizes based on content while maintaining proper spacing and alignment.

## Handling Results

Process the result the same way you would with a standard MessageBox:

```csharp
MessageBoxResult result = CustomMessageBox.Show("Save changes?", "Confirm", MessageBoxButton.YesNoCancel);

switch (result)
{
    case MessageBoxResult.Yes:
        SaveChanges();
        break;
    case MessageBoxResult.No:
        DiscardChanges();
        break;
    case MessageBoxResult.Cancel:
        // User canceled the operation
        break;
}
```

## Keyboard Shortcuts

The CustomMessageBox supports these keyboard shortcuts:

- `Y` - Clicks the Yes button (when visible)
- `N` - Clicks the No button (when visible)
- `O` - Clicks the OK button (when visible)
- `C` - Clicks the Cancel button (when visible)
- `Esc` - Clicks Cancel or No button (based on which is visible)
- `Enter` - Clicks the default button (typically Yes or OK)

## License

[Specify your license information here]
A Windows desktop tool for generating licenses compatible with the [Standard.Licensing](https://github.com/dnauck/Standard.Licensing) library. The application lets you configure all available license options, sign them with your private key and save the result for distribution.

## Features

- Create **Standard**, **Trial** license types
- Set product name, version, and expiration date
- Store customer information (name and email)
- Add any additional attributes using JSON
- Sign licenses using your own RSA private key
- Save generated licenses to a file
- Built-in help screen describing how to use the tool

The UI is designed with sensible defaults and labeled inputs so generating a license only requires filling in the desired fields and selecting your private key.

## Usage

1. Start the application.
2. Enter your product details and customer information.
3. Select the desired license type and expiration date.
4. Optionally add extra attributes in JSON format (e.g. `{ "Seats": "5" }`).
5. Browse to your private key file. PEM keys are recommended, but existing XML keys are also supported.
   Keys must be unencrypted; passphrase-protected keys are not currently supported.
6. Click **Generate License** to view the resulting license text.
7. Use **File → Save License** to store the license in a `.lic` file.

The generated license can then be validated in your application using the matching public key with the Standard.Licensing library.

---

# Using Licenses in Your Software

This guide describes how to incorporate license validation, import, updating, and management features using the output from StandardLicensingGenerator and the [Standard.Licensing](https://github.com/dnauck/Standard.Licensing) library.

## 1. License Validation

To ensure a user holds a valid license for your application:

- **Load** the license file (e.g. `license.lic`) at application startup or before accessing protected features.
- **Validate** the license signature using your embedded or distributed public key.
- **Check** license properties: expiration dates, allowed features, seat counts, or custom attributes.

Example (pseudocode):
```csharp 
// C# var licenseText = File.ReadAllText("path/to/license.lic"); var license = License.Load(licenseText);
var publicKey = "..."; // Your PEM or XML public key string or file content
if (!license.VerifySignature(publicKey)) { // Invalid license throw new UnauthorizedAccessException("License verification failed."); }
if (license.Type == LicenseType.Trial && license.Expiration < DateTime.Now) { // License has expired throw new LicenseExpiredException(); }
// Access license attributes var customerName = license.Customer.Name; var seats = license.AdditionalAttributes.Get("Seats");
```
## 2. License Import

Allow users to import their license file into your software:

- Provide a UI for selecting or drag-and-dropping a license file.
- Store the selected license, for example in the user's application data folder.
- Trigger validation immediately after import.

Recommended steps:

- Validate the imported license signature.
- Optionally, inform the user about license details (customer, type, expiration).

## 3. Updating a License

To support changes such as upgrading, extending, or modifying user licenses:

- Allow users to **replace** their license by importing a new file.
- Overwrite the previously stored license.
- Offer a menu or command for "Update License."
- Display relevant license status (valid/expired/type) in your application's settings or about dialog.

## 4. Handling License States

- **Valid License**: Grant access to features as described by the license attributes.
- **Expired or Invalid License**: Restrict access or show an appropriate message.
- **No License**: Prompt the user to enter/import a valid license.

## 5. Licensing Security Best Practices

- Always perform license validation on application start and before providing critical functionality.
- Protect your public key: embed it in your application resources, not as plain text or in a writable location.
- Never ship your private key with your distribution.

## 6. License Attributes and Customization

- Use custom attributes to define seats, editions, enabled modules, etc.
- Document any custom license properties your software consumes.

## 7. Revocation and Renewal (Optional)

If your workflow supports revocation or renewal:

- Let users load a replacement license file with new terms or updated validity.
- For revocation, consider maintaining a list of revoked license IDs within your app or via online lookup (optional, requires networking).

---

## Distribution

The project targets `net9.0-windows` and can be published as a single-file executable for easy distribution (e.g. via web download). Run:

```PowerShell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```
to produce a distributable package.

## Help

A help window is available from the **Help** menu inside the application. It provides a short overview of the workflow and explains where to place your private/public keys.