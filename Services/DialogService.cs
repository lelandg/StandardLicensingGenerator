using Microsoft.Win32;
using System.Windows;

namespace StandardLicensingGenerator.Services;

public class DialogService : IDialogService
{
    private readonly Window _owner;

    public DialogService(Window owner)
    {
        _owner = owner;
    }

    public MessageBoxResult ShowMessage(
        string messageText,
        string caption,
        MessageBoxButton buttons,
        MessageBoxImage icon,
        MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        return Views.CustomMessageBox.Show(_owner, messageText, caption, buttons, icon, defaultResult);
    }

    public string? ShowOpenFileDialog(string filter)
    {
        var dlg = new OpenFileDialog { Filter = filter };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? ShowSaveFileDialog(string filter, string? defaultFileName = null)
    {
        var dlg = new SaveFileDialog { Filter = filter };
        if (defaultFileName != null)
            dlg.FileName = defaultFileName;
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }
}
