using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StandardLicensingGenerator.Views
{
    /// <summary>
    /// Examples of how to use the CustomMessageBox
    /// </summary>
    public static class CustomMessageBoxExample
    {
        // Basic usage examples
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

            // With custom image example
            // Create a BitmapImage from a URI
            BitmapImage customImage = new BitmapImage(new System.Uri(
                "pack://application:,,,/StandardLicensingGenerator;component/Resources/custom_icon.png",
                System.UriKind.Absolute));

            // Show message box with custom image
            CustomMessageBox.ShowWithImage(owner, 
                "This message uses a custom icon.", 
                "Custom Icon Example", 
                MessageBoxButton.OK, 
                customImage);
        }
    }
}
