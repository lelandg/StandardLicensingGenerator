using System.Windows;

namespace StandardLicensingGenerator.Services;

// Abstracts every dialog a ViewModel needs so ViewModel logic can run (and be
// unit tested) without touching UI types beyond the MessageBox enums.
public interface IDialogService
{
    MessageBoxResult ShowMessage(
        string messageText,
        string caption,
        MessageBoxButton buttons,
        MessageBoxImage icon,
        MessageBoxResult defaultResult = MessageBoxResult.None);

    // Returns the selected path, or null if the user cancelled.
    string? ShowOpenFileDialog(string filter);

    string? ShowSaveFileDialog(string filter, string? defaultFileName = null);
}
