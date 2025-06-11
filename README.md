# StandardLicensingGenerator

A Windows desktop tool for generating licenses compatible with the [Standard.Licensing](https://www.junian.dev/Standard.Licensing/) library. The application lets you configure all available license options, sign them with your private key and save the result for distribution.

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
- Generate RSA key pairs for license signing
- Copy license text to clipboard
- Password protection for private keys
- Support for both PEM and XML key formats
- Built-in help screen describing how to use the tool

The UI is designed with sensible defaults and labeled inputs so generating a license only requires filling in the desired fields and selecting your private key.

## Usage

### Creating a License

1. Start the application.
2. Enter your product details and customer information.
3. Select the desired license type (Standard or Trial):
   - **Standard**: Full license with custom customer information
   - **Trial**: Pre-configured with trial defaults and 30-day expiration
4. Set an expiration date (required).
5. Optionally add extra attributes in JSON format (e.g. `{ "Seats": "5" }`).
6. Browse to your private key file. PEM keys are recommended, but existing XML keys are also supported.
7. If your key is password-protected, enter the password.
8. Click **Generate License** to view the resulting license text.
9. Use **File → Save License** to store the license in a `.lic` file, or copy to clipboard using the Copy button.

### Generating Key Pairs

1. Click **Generate Key Pair** in the main window.
2. Select your desired key size (2048 or 4096 bits recommended).
3. Optionally enter a password to protect your private key.
4. Click **Generate Key Pair** button.
5. Save both private and public keys using the respective buttons.
6. Optionally, use **Insert Private Key** to automatically use the generated key in the main window.

### Keyboard Shortcuts

- **F1**: Open help window
- **Esc**: Exit application (with confirmation)

The generated license can then be validated in your application using the matching public key with the Standard.Licensing library.

## Building the Application

### Building with Visual Studio

1. Open the solution file `StandardLicensingGenerator.sln` in Visual Studio 2022 or newer.
2. Select the desired build configuration (Debug/Release).
3. Build the solution using **Build > Build Solution** or press **Ctrl+Shift+B**.
4. The compiled application will be available in the `bin/{Configuration}/net9.0-windows` directory.

### Building with JetBrains Rider

1. Open the solution file `StandardLicensingGenerator.sln` in Rider.
2. Select the desired build configuration from the dropdown in the toolbar.
3. Build the solution by clicking the build icon or pressing **Ctrl+F9**.
4. The compiled application will be available in the `bin/{Configuration}/net9.0-windows` directory.

### Building with .NET CLI

1. Open a terminal or command prompt in the project directory.
2. Run the following command to build the application:
   ```
   dotnet build -c Release
   ```
3. To run the application directly:
   ```
   dotnet run -c Release
   ```

### Creating a Standalone Executable

To create a self-contained single-file executable for distribution:

```
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The resulting executable will be available in the `bin/Release/net9.0-windows/win-x64/publish` directory.

---

# Using Licenses in Your Software

This guide describes how to incorporate license validation, import, updating, and management features using the output from StandardLicensingGenerator and the [Standard.Licensing](https://github.com/dnauck/Standard.Licensing) library.

## 1. License Validation

To ensure a user holds a valid license for your application:

- **Load** the license file (e.g. `license.lic`) at application startup or before accessing protected features.
- **Validate** the license signature using your embedded or distributed public key.
- **Check** license properties: expiration dates, allowed features, seat counts, or custom attributes.

Example code using Standard.Licensing library:

```csharp
// Include the Standard.Licensing NuGet package in your project
using Standard.Licensing;
using System;
using System.IO;

// Load the license file
string licenseText = File.ReadAllText("path/to/license.lic");
var license = License.Load(licenseText);

// Load your public key (from file or embedded resource)
string publicKey = File.ReadAllText("path/to/public_key.pem");

// Verify the license signature
if (!license.VerifySignature(publicKey))
{
    // Invalid license
    throw new UnauthorizedAccessException("License verification failed.");
}

// Check license expiration
if (license.Expiration < DateTime.Now)
{
    // License has expired
    throw new Exception("License has expired.");
}

// Check license type
if (license.Type == LicenseType.Trial)
{
    // Handle trial license differently if needed
    Console.WriteLine("Running in trial mode");
}

// Access customer information
string customerName = license.Customer.Name;
string customerEmail = license.Customer.Email;

// Access custom attributes
if (license.AdditionalAttributes.ContainsKey("Seats"))
{
    string seats = license.AdditionalAttributes["Seats"];
    Console.WriteLine($"Licensed for {seats} seats");
}
```
## 2. License Import

Allow users to import their license file into your software:

- Provide a UI for selecting or drag-and-dropping a license file.
- Store the selected license, for example in the user's application data folder.
- Trigger validation immediately after import.

Example WPF implementation:

```csharp
private void ImportLicense_Click(object sender, RoutedEventArgs e)
{
    var dialog = new OpenFileDialog
    {
        Filter = "License Files (*.lic)|*.lic|All Files (*.*)|*.*",
        Title = "Import License File"
    };

    if (dialog.ShowDialog() == true)
    {
        try
        {
            // Read the license file
            string licenseText = File.ReadAllText(dialog.FileName);
            var license = License.Load(licenseText);

            // Validate the license (using public key from resources or settings)
            string publicKey = GetPublicKeyFromResources();
            if (!license.VerifySignature(publicKey))
            {
                MessageBox.Show("The license signature is invalid.", "Invalid License", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if the license is expired
            if (license.Expiration < DateTime.Now)
            {
                MessageBox.Show("This license has expired.", "Expired License", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                // Optionally still allow using an expired license
            }

            // Save the license to application data folder
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "YourCompany", "YourApp");

            Directory.CreateDirectory(appDataPath); // Ensure directory exists
            File.WriteAllText(Path.Combine(appDataPath, "license.lic"), licenseText);

            // Show license details to user
            MessageBox.Show($"License imported successfully\n\n" +
                          $"Licensed to: {license.Customer.Name}\n" +
                          $"Type: {license.Type}\n" +
                          $"Expires: {license.Expiration:d}", 
                          "License Imported", MessageBoxButton.OK, MessageBoxImage.Information);

            // Apply the license to your application (e.g., unlock features)
            ApplyLicense(license);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to import license: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

private string GetPublicKeyFromResources()
{
    // Load the public key from embedded resources
    using var stream = GetType().Assembly.GetManifestResourceStream("YourNamespace.public_key.pem");
    if (stream == null) throw new FileNotFoundException("Public key resource not found");

    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}
```

## 3. Updating a License

To support changes such as upgrading, extending, or modifying user licenses:

- Allow users to **replace** their license by importing a new file.
- Overwrite the previously stored license.
- Offer a menu or command for "Update License."
- Display relevant license status (valid/expired/type) in your application's settings or about dialog.

Example implementation for license status display:

```csharp
private void UpdateLicenseStatus()
{
    try
    {
        // Get path to stored license file
        string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "YourCompany", "YourApp");
        string licensePath = Path.Combine(appDataPath, "license.lic");

        if (!File.Exists(licensePath))
        {
            // No license found
            LicenseStatusTextBlock.Text = "No License";
            LicenseTypeTextBlock.Text = "Free Version";
            LicenseExpirationTextBlock.Text = "N/A";
            return;
        }

        // Load and validate the license
        string licenseText = File.ReadAllText(licensePath);
        var license = License.Load(licenseText);
        string publicKey = GetPublicKeyFromResources();

        bool isValid = license.VerifySignature(publicKey);
        bool isExpired = license.Expiration < DateTime.Now;

        // Update UI elements
        LicenseStatusTextBlock.Text = isValid ? (isExpired ? "Expired" : "Valid") : "Invalid";
        LicenseTypeTextBlock.Text = license.Type.ToString();
        LicenseExpirationTextBlock.Text = license.Expiration.ToString("d");
        LicenseCustomerTextBlock.Text = license.Customer.Name;

        // Optionally show more details for custom attributes
        if (license.AdditionalAttributes.Count > 0)
        {
            LicenseAttributesPanel.Visibility = Visibility.Visible;
            LicenseAttributesTextBlock.Text = string.Join("\n", 
                license.AdditionalAttributes.Select(a => $"{a.Key}: {a.Value}"));
        }
        else
        {
            LicenseAttributesPanel.Visibility = Visibility.Collapsed;
        }
    }
    catch (Exception ex)
    {
        // Handle errors reading or parsing the license
        LicenseStatusTextBlock.Text = "Error";
        LicenseTypeTextBlock.Text = "Unknown";
        LicenseExpirationTextBlock.Text = "Unknown";

        // Log the error
        Console.WriteLine($"Error updating license status: {ex.Message}");
    }
}
```

## 4. Handling License States

- **Valid License**: Grant access to features as described by the license attributes.
- **Expired or Invalid License**: Restrict access or show an appropriate message.
- **No License**: Prompt the user to enter/import a valid license.

## 5. Licensing Security Best Practices

- Always perform license validation on application start and before providing critical functionality.
- Protect your public key: embed it in your application resources, not as plain text or in a writable location.
- Never ship your private key with your distribution.
- Consider implementing obfuscation to protect your license validation code.
- For increased security, implement a time-limited license cache to reduce the frequency of file access.
- Implement tamper detection to identify if license files have been modified.
- Consider adding a hardware identifier to tie licenses to specific machines.

Example of embedding a public key as a resource:

1. Add your public key file to your project.
2. Set its build action to "Embedded Resource".
3. Access it in code:

```csharp
private string GetEmbeddedPublicKey()
{
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = "YourNamespace.public_key.pem";

    using var stream = assembly.GetManifestResourceStream(resourceName);
    if (stream == null) throw new FileNotFoundException("Public key resource not found");

    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}
```

## 6. License Attributes and Customization

The StandardLicensingGenerator allows you to add custom attributes to your licenses through the JSON attributes field. These attributes can be used to control various aspects of your application:

- **Feature Flags**: Enable/disable specific features based on license tier
- **Usage Limits**: Set maximum allowed resources (users, projects, etc.)
- **Custom Properties**: Store any license-specific configuration

**Example JSON attributes for different scenarios:**

   * Enterprise Edition with module access
```json
{
  "Edition": "Enterprise",
  "MaxUsers": "Unlimited", 
  "Modules": ["Reporting", "Administration", "Integration"]
}
```
* Professional Edition with limits
```json
{
  "Edition": "Professional",
  "MaxUsers": "10",
  "Modules": ["Reporting", "Administration"],
  "StorageLimit": "50GB"
}
```

* Basic Edition
```json
{
  "Edition": "Basic",
  "MaxUsers": "3",
  "Modules": ["Reporting"],
  "StorageLimit": "5GB"
}
```

Accessing these attributes in code:

```csharp
// Check edition
string edition = license.AdditionalAttributes["Edition"];

// Check if a module is available
string[] modules = license.AdditionalAttributes["Modules"]
    .TrimStart('[').TrimEnd(']')
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(m => m.Trim('"', ' '))
    .ToArray();

if (modules.Contains("Reporting"))
{
    // Enable reporting functionality
}

// Check numeric limits
if (int.TryParse(license.AdditionalAttributes["MaxUsers"], out int maxUsers))
{
    // Apply user limit
}
```

## 7. Revocation and Renewal

To implement license revocation and renewal in your application:

### Renewal Process

- Let users load a replacement license file with new terms or updated validity.
- When a new license is imported, verify it's for the same customer/product.
- Overwrite the previous license file with the new one.

### Online License Validation (Optional)

For applications with internet access, you can implement server-side license validation:

```csharp
public async Task<bool> ValidateLicenseOnline(License license)
{
    try
    {
        // Create HTTP client (consider using a cached/singleton instance)
        using var client = new HttpClient();

        // Prepare request with license ID
        var request = new HttpRequestMessage(HttpMethod.Get, 
            $"https://your-license-server.com/api/validate/{license.Id}");

        // Add authentication if needed
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "your-api-key");

        // Send request
        var response = await client.SendAsync(request);

        // Check if license is valid
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LicenseValidationResult>();
            return result?.IsValid == true && !result.IsRevoked;
        }

        return false;
    }
    catch (Exception ex)
    {
        // Handle network errors - typically fail open if server is unreachable
        // Log the error
        Console.WriteLine($"Online validation error: {ex.Message}");

        // Return true to allow offline usage, or false for stricter validation
        return true; // Allow offline usage
    }
}

// Model for server response
public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Message { get; set; }
}
```

### Offline Revocation List

For applications without consistent internet access:

```csharp
public class RevocationList
{
    private HashSet<Guid> _revokedLicenseIds = new();
    private readonly string _revocationListPath;
    private DateTime _lastUpdateCheck = DateTime.MinValue;

    public RevocationList(string revocationListPath)
    {
        _revocationListPath = revocationListPath;
        LoadRevocationList();
    }

    private void LoadRevocationList()
    {
        if (File.Exists(_revocationListPath))
        {
            try
            {
                string[] lines = File.ReadAllLines(_revocationListPath);
                _revokedLicenseIds = new HashSet<Guid>(
                    lines.Select(line => Guid.Parse(line.Trim()))
                );
            }
            catch (Exception ex)
            {
                // Log error reading revocation list
                Console.WriteLine($"Error loading revocation list: {ex.Message}");
            }
        }
    }

    public bool IsRevoked(Guid licenseId)
    {
        // Check if we should try to update the list (e.g., once per day)
        if (DateTime.Now - _lastUpdateCheck > TimeSpan.FromDays(1))
        {
            _lastUpdateCheck = DateTime.Now;
            UpdateRevocationListAsync().ConfigureAwait(false);
        }

        return _revokedLicenseIds.Contains(licenseId);
    }

    private async Task UpdateRevocationListAsync()
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync("https://your-license-server.com/api/revoked-licenses");

            if (response.IsSuccessStatusCode)
            {
                var revocationData = await response.Content.ReadAsStringAsync();
                File.WriteAllText(_revocationListPath, revocationData);
                LoadRevocationList(); // Reload after update
            }
        }
        catch (Exception ex)
        {
            // Log error but continue with existing list
            Console.WriteLine($"Error updating revocation list: {ex.Message}");
        }
    }
}
```

---

## Distribution

The StandardLicensingGenerator targets `.NET 9.0` for Windows and can be distributed in several ways:

### Self-contained Executable

Create a standalone executable that doesn't require .NET installation:

```PowerShell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

### Framework-dependent Executable

For users who already have .NET installed (smaller download size):

```PowerShell
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

### ClickOnce Deployment

For automatic updates using Visual Studio:

1. Right-click the project in Solution Explorer
2. Select **Publish**
3. Choose **ClickOnce** as the publish target
4. Configure your settings and publish location

## System Requirements

- Windows 10/11 or Windows Server 2016 or later
- If using framework-dependent deployment: .NET 9.0 Runtime
- No special hardware requirements

## Help

A help window is available from the **Help** menu inside the application. It provides a short overview of the workflow and explains where to place your private/public keys.

Press **F1** at any time to access the help window.