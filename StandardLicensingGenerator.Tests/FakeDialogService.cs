using StandardLicensingGenerator.Services;
using System.Windows;

namespace StandardLicensingGenerator.Tests;

// Records every dialog a ViewModel raises and returns programmed results, so
// tests never show real UI.
public class FakeDialogService : IDialogService
{
    public List<(string Message, string Caption)> Messages { get; } = new();
    public MessageBoxResult NextMessageResult { get; set; } = MessageBoxResult.OK;
    public string? OpenFileResult { get; set; }
    public string? SaveFileResult { get; set; }

    public MessageBoxResult ShowMessage(
        string messageText,
        string caption,
        MessageBoxButton buttons,
        MessageBoxImage icon,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        Messages.Add((messageText, caption));
        return NextMessageResult;
    }

    public string? ShowOpenFileDialog(string filter) => OpenFileResult;

    public string? ShowSaveFileDialog(string filter, string? defaultFileName = null) => SaveFileResult;
}
