using System.Windows;
using System.Windows.Controls;

namespace StandardLicensingGenerator.Views;

// Keeps a PasswordBox and its visible TextBox mirror in sync, toggles which
// of the two is shown, and reports the current password to the ViewModel.
// Shared by MainWindow and KeyPairGeneratorWindow, which previously
// duplicated this logic verbatim.
public class PasswordReveal
{
    private readonly PasswordBox _passwordBox;
    private readonly TextBox _textBox;
    private bool _showPassword;

    public PasswordReveal(PasswordBox passwordBox, TextBox textBox, Button toggleButton, Action<string> passwordChanged)
    {
        _passwordBox = passwordBox;
        _textBox = textBox;

        // Ensure the password TextBox is hidden and PasswordBox is visible on window loads
        _textBox.Visibility = Visibility.Collapsed;
        _passwordBox.Visibility = Visibility.Visible;

        _passwordBox.PasswordChanged += (_, _) =>
        {
            if (_textBox.Text != _passwordBox.Password)
                _textBox.Text = _passwordBox.Password;
            passwordChanged(_passwordBox.Password);
        };
        _textBox.TextChanged += (_, _) =>
        {
            if (_passwordBox.Password != _textBox.Text)
                _passwordBox.Password = _textBox.Text;
            passwordChanged(_textBox.Text);
        };
        toggleButton.Click += (_, _) => Toggle();
    }

    private void Toggle()
    {
        _showPassword = !_showPassword;

        if (_showPassword)
        {
            if (_textBox.Text != _passwordBox.Password)
                _textBox.Text = _passwordBox.Password;

            _passwordBox.Visibility = Visibility.Collapsed;
            _textBox.Visibility = Visibility.Visible;
            _textBox.Focus();
            _textBox.SelectionStart = _textBox.Text.Length; // Position cursor at end
        }
        else
        {
            if (_passwordBox.Password != _textBox.Text)
                _passwordBox.Password = _textBox.Text;

            _textBox.Visibility = Visibility.Collapsed;
            _passwordBox.Visibility = Visibility.Visible;
            _passwordBox.Focus();
        }
    }
}
