using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StandardLicensingGenerator.Views;
public partial class CustomMessageBox : Window
{
    // Store the current style for this instance
    private CustomMessageBoxStyle? _instanceStyle;
    public MessageBoxResult Result { get; private set; }
    private CustomMessageBox()
    {
        InitializeComponent();
        KeyDown += CustomMessageBox_KeyDown;

        // Apply current style generator settings
        ApplyGeneratorStyles();

        // Handle window loaded to set focus on default button
        Loaded += CustomMessageBox_Loaded;
    }

    private void CustomMessageBox_Loaded(object sender, RoutedEventArgs e)
    {
        // Focus the default button when the window opens
        if (OkButton.IsDefault && OkButton.Visibility == Visibility.Visible)
            OkButton.Focus();
        else if (YesButton.IsDefault && YesButton.Visibility == Visibility.Visible)
            YesButton.Focus();
        else if (NoButton.IsDefault && NoButton.Visibility == Visibility.Visible)
            NoButton.Focus();
        else if (CancelButton.IsDefault && CancelButton.Visibility == Visibility.Visible)
            CancelButton.Focus();
    }

    private void ApplyGeneratorStyles()
    {
        var generator = MessageBoxStyleGenerator.Current;

        // Apply colors from the generator
        Background = generator.WindowBackground;
        BorderBrush.BorderBrush = generator.BorderBrush;
        MessageTitle.Background = generator.TitleBackground;
        MessageTitle.Foreground = generator.TitleForeground;

        // Update button style resources
        Resources["MessageBoxButtonBackground"] = generator.ButtonBackground;
        Resources["MessageBoxButtonHoverBackground"] = generator.ButtonHoverBackground;
        Resources["MessageBoxButtonPressedBackground"] = generator.ButtonPressedBackground;
        Resources["MessageBoxButtonDisabledBackground"] = generator.ButtonDisabledBackground;
        Resources["MessageBoxButtonForeground"] = generator.ButtonForeground;
        Resources["MessageBoxButtonDisabledForeground"] = generator.ButtonDisabledForeground;
    }

    /// <summary>
    /// Applies a custom style to this message box instance
    /// </summary>
    /// <param name="style">The style to apply</param>
    public void ApplyStyle(CustomMessageBoxStyle style)
    {
        if (style == null) return;

        _instanceStyle = style;

        // Apply each style property if it's not null
        if (style.WindowBackground != null) Background = style.WindowBackground;
        if (style.BorderBrush != null) BorderBrush.BorderBrush = style.BorderBrush;
        if (style.TitleBackground != null) MessageTitle.Background = style.TitleBackground;
        if (style.TitleForeground != null) MessageTitle.Foreground = style.TitleForeground;

        // Update button style resources only for non-null properties
        if (style.ButtonBackground != null) Resources["MessageBoxButtonBackground"] = style.ButtonBackground;
        if (style.ButtonHoverBackground != null) Resources["MessageBoxButtonHoverBackground"] = style.ButtonHoverBackground;
        if (style.ButtonPressedBackground != null) Resources["MessageBoxButtonPressedBackground"] = style.ButtonPressedBackground;
        if (style.ButtonDisabledBackground != null) Resources["MessageBoxButtonDisabledBackground"] = style.ButtonDisabledBackground;
        if (style.ButtonForeground != null) Resources["MessageBoxButtonForeground"] = style.ButtonForeground;
        if (style.ButtonDisabledForeground != null) Resources["MessageBoxButtonDisabledForeground"] = style.ButtonDisabledForeground;
    }
    private void CustomMessageBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        // Skip if Alt is pressed - this is handled by WPF's built-in access key system
        if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) == System.Windows.Input.ModifierKeys.Alt)
        {
            return;
        }

        // Handle common keyboard shortcuts for dialog buttons
        switch (e.Key)
        {
            case System.Windows.Input.Key.Escape:
                if (CancelButton.Visibility == Visibility.Visible)
                {
                    Result = MessageBoxResult.Cancel;
                    DialogResult = false;
                    Close();
                }
                else if (NoButton.Visibility == Visibility.Visible)
                {
                    Result = MessageBoxResult.No;
                    DialogResult = false;
                    Close();
                }
                break;
            case System.Windows.Input.Key.Y:
                if (YesButton.Visibility == Visibility.Visible)
                {
                    Result = MessageBoxResult.Yes;
                    DialogResult = true;
                    Close();
                }
                break;
            case System.Windows.Input.Key.N:
                if (NoButton.Visibility == Visibility.Visible)
                {
                    Result = MessageBoxResult.No;
                    DialogResult = false;
                    Close();
                }
                break;
            case System.Windows.Input.Key.O:
                if (OkButton.Visibility == Visibility.Visible)
                {
                    Result = MessageBoxResult.OK;
                    DialogResult = true;
                    Close();
                }
                break;
            case System.Windows.Input.Key.C:
                if (CancelButton.Visibility == Visibility.Visible)
                {
                    Result = MessageBoxResult.Cancel;
                    DialogResult = false;
                    Close();
                }
                break;
        }
    }
    private void SetupButtons(MessageBoxButton buttons, MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        // Reset all buttons to not be default first
        OkButton.IsDefault = false;
        YesButton.IsDefault = false;
        NoButton.IsDefault = false;
        CancelButton.IsDefault = false;

        // First set button visibility based on button type
        switch (buttons)
        {
            case MessageBoxButton.OK:
                OkButton.Visibility = Visibility.Visible;
                break;
            case MessageBoxButton.OKCancel:
                OkButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;
                break;
            case MessageBoxButton.YesNo:
                YesButton.Visibility = Visibility.Visible;
                NoButton.Visibility = Visibility.Visible;
                break;
            case MessageBoxButton.YesNoCancel:
                YesButton.Visibility = Visibility.Visible;
                NoButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Visible;
                break;
        }

        // Then set default button based on defaultResult parameter or use default behavior
        if (defaultResult != MessageBoxResult.None)
        {
            // Set default button based on specified result
            switch (defaultResult)
            {
                case MessageBoxResult.OK:
                    if (OkButton.Visibility == Visibility.Visible)
                        OkButton.IsDefault = true;
                    break;
                case MessageBoxResult.Yes:
                    if (YesButton.Visibility == Visibility.Visible)
                        YesButton.IsDefault = true;
                    break;
                case MessageBoxResult.No:
                    if (NoButton.Visibility == Visibility.Visible)
                        NoButton.IsDefault = true;
                    break;
                case MessageBoxResult.Cancel:
                    if (CancelButton.Visibility == Visibility.Visible)
                        CancelButton.IsDefault = true;
                    break;
            }
        }
        else
        {
            // Use default behavior if no specific default was requested
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    OkButton.IsDefault = true;
                    break;
                case MessageBoxButton.OKCancel:
                    OkButton.IsDefault = true;
                    break;
                case MessageBoxButton.YesNo:
                    YesButton.IsDefault = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    YesButton.IsDefault = true;
                    break;
            }
        }
    }

    private void SetIcon(MessageBoxImage image)
    {
        string iconUri = "";

        switch (image)
        {
            case MessageBoxImage.Error:
                iconUri = "pack://application:,,,/CustomMessageBox;component/Resources/error.png";
                try {
                    // Try to load custom resource first
                    MessageIcon.Source = new BitmapImage(new Uri(iconUri));
                }
                catch {
                    // Fallback: create a simple error icon using WPF geometry
                    MessageIcon.Source = CreateFallbackIcon("❌", Brushes.Red);
                }
                break;
            case MessageBoxImage.Question:
                iconUri = "pack://application:,,,/CustomMessageBox;component/Resources/Question.png";
                try {
                    MessageIcon.Source = new BitmapImage(new Uri(iconUri));
                }
                catch {
                    MessageIcon.Source = CreateFallbackIcon("❓", Brushes.Blue);
                }
                break;
            case MessageBoxImage.Warning:
                iconUri = "pack://application:,,,/CustomMessageBox;component/Resources/warning.png";
                try {
                    MessageIcon.Source = new BitmapImage(new Uri(iconUri));
                }
                catch {
                    MessageIcon.Source = CreateFallbackIcon("⚠", Brushes.Orange);
                }
                break;
            case MessageBoxImage.Information:
                iconUri = "pack://application:,,,/CustomMessageBox;component/Resources/info.png";
                try {
                    MessageIcon.Source = new BitmapImage(new Uri(iconUri));
                }
                catch {
                    MessageIcon.Source = CreateFallbackIcon("ℹ", Brushes.Blue);
                }
                break;
            case MessageBoxImage.None:
                MessageIcon.Visibility = Visibility.Collapsed;
                break;
        }
    }

    private ImageSource CreateFallbackIcon(string text, Brush color)
    {
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI Symbol"),
                24,
                color,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            context.DrawText(formattedText, new Point(0, 0));
        }

        var bitmap = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        return bitmap;
    }
    
    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Yes;
        DialogResult = true;
        Close();
    }
    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.No;
        DialogResult = false;
        Close();
    }
    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.OK;
        DialogResult = true;
        Close();
    }
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Cancel;
        DialogResult = false;
        Close();
    }
    /// <summary>
    /// Set a custom image instead of the standard system icon
    /// </summary>
    /// <param name="imageSource">The image source to use</param>
    public void SetCustomImage(ImageSource imageSource)
    {
        MessageIcon.Source = imageSource;
        MessageIcon.Visibility = Visibility.Visible;
    }
    public static MessageBoxResult Show(string messageText)
    {
        return Show(null, messageText, "", MessageBoxButton.OK, MessageBoxImage.None);
    }

    /// <summary>
    /// Display a message with custom style
    /// </summary>
    public static MessageBoxResult ShowWithStyle(string messageText, CustomMessageBoxStyle style)
    {
        return ShowWithStyle(null, messageText, "", MessageBoxButton.OK, MessageBoxImage.None, style);
    }
    public static MessageBoxResult Show(string messageText, string caption)
    {
        return Show(null, messageText, caption, MessageBoxButton.OK, MessageBoxImage.None);
    }

    /// <summary>
    /// Display a message with caption and custom style
    /// </summary>
    public static MessageBoxResult ShowWithStyle(string messageText, string caption, CustomMessageBoxStyle style)
    {
        return ShowWithStyle(null, messageText, caption, MessageBoxButton.OK, MessageBoxImage.None, style);
    }
    public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton buttons)
    {
        return Show(null, messageText, caption, buttons, MessageBoxImage.None);
    }

    /// <summary>
    /// Display a message with caption, buttons and custom style
    /// </summary>
    public static MessageBoxResult ShowWithStyle(string messageText, string caption, MessageBoxButton buttons, CustomMessageBoxStyle style)
    {
        return ShowWithStyle(null, messageText, caption, buttons, MessageBoxImage.None, style);
    }
    public static MessageBoxResult Show(Window owner, string messageText)
    {
        return Show(owner, messageText, "", MessageBoxButton.OK, MessageBoxImage.None);
    }

    /// <summary>
    /// Display a message with owner window and custom style
    /// </summary>
    public static MessageBoxResult ShowWithStyle(Window owner, string messageText, CustomMessageBoxStyle style)
    {
        return ShowWithStyle(owner, messageText, "", MessageBoxButton.OK, MessageBoxImage.None, style);
    }
    public static MessageBoxResult Show(Window owner, string messageText, string caption)
    {
        return Show(owner, messageText, caption, MessageBoxButton.OK, MessageBoxImage.None);
    }

    /// <summary>
    /// Display a message with owner window, caption and custom style
    /// </summary>
    public static MessageBoxResult ShowWithStyle(Window owner, string messageText, string caption, CustomMessageBoxStyle style)
    {
        return ShowWithStyle(owner, messageText, caption, MessageBoxButton.OK, MessageBoxImage.None, style);
    }
    public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton buttons)
    {
        return Show(owner, messageText, caption, buttons, MessageBoxImage.None);
    }

    /// <summary>
    /// Display a message with owner window, caption, buttons and custom style
    /// </summary>
    public static MessageBoxResult ShowWithStyle(Window owner, string messageText, string caption, MessageBoxButton buttons, CustomMessageBoxStyle style)
    {
        return ShowWithStyle(owner, messageText, caption, buttons, MessageBoxImage.None, style);
    }
    public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton buttons, MessageBoxImage icon)
    {
        return Show(null, messageText, caption, buttons, icon);
    }

    /// <summary>
    /// Display a message with caption, buttons, icon and custom style
    /// </summary>
    public static MessageBoxResult ShowWithStyle(string messageText, string caption, MessageBoxButton buttons, MessageBoxImage icon, CustomMessageBoxStyle style)
    {
        return ShowWithStyle(null, messageText, caption, buttons, icon, style);
    }
    public static MessageBoxResult Show(Window? owner, string messageText, string caption, MessageBoxButton buttons, MessageBoxImage icon)
    {
        return Show(owner, messageText, caption, buttons, icon, MessageBoxResult.None);
    }

    /// <summary>
    /// Display a message with owner window, caption, buttons, icon and custom style
    /// </summary>
    public static MessageBoxResult ShowWithStyle(Window? owner, string messageText, string caption, MessageBoxButton buttons, MessageBoxImage icon, CustomMessageBoxStyle style)
    {
        return ShowWithStyle(owner, messageText, caption, buttons, icon, MessageBoxResult.None, style);
    }
    public static MessageBoxResult Show(Window? owner, string messageText, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defaultResult)
    {
        var msgBox = new CustomMessageBox
        {
            Title = string.IsNullOrEmpty(caption) ? "Message" : caption,
            Owner = owner,
            // Set default result based on parameter
            Result = defaultResult,
            MessageTitle =
            {
                // Set title and message text
                Text = string.IsNullOrEmpty(caption) ? "" : caption,
                Visibility = string.IsNullOrEmpty(caption) ? Visibility.Collapsed : Visibility.Visible
            },
            MessageText =
            {
                Text = messageText
            }
        };

        // Setup buttons and icon, passing the defaultResult
        msgBox.SetupButtons(buttons, defaultResult);
        msgBox.SetIcon(icon);

        // Show dialog
        msgBox.ShowDialog();
        return msgBox.Result;
    }

    /// <summary>
    /// Display a message with owner window, caption, buttons, icon, default result and custom style
    /// </summary>
    public static MessageBoxResult ShowWithStyle(Window? owner, string messageText, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defaultResult, CustomMessageBoxStyle style)
    {
        var msgBox = new CustomMessageBox
        {
            Title = string.IsNullOrEmpty(caption) ? "Message" : caption,
            Owner = owner,
            // Set default result based on parameter
            Result = defaultResult,
            MessageTitle =
            {
                // Set title and message text
                Text = string.IsNullOrEmpty(caption) ? "" : caption,
                Visibility = string.IsNullOrEmpty(caption) ? Visibility.Collapsed : Visibility.Visible
            },
            MessageText =
            {
                Text = messageText
            }
        };

        // Setup buttons and icon, passing the defaultResult
        msgBox.SetupButtons(buttons, defaultResult);
        msgBox.SetIcon(icon);

        // Apply custom style
        msgBox.ApplyStyle(style);

        // Show dialog
        msgBox.ShowDialog();
        return msgBox.Result;
    }
    /// <summary>
    /// Display a message box with a custom image
    /// </summary>
    public static MessageBoxResult ShowWithImage(Window? owner, string messageText, string caption, 
        MessageBoxButton buttons, ImageSource customImage, MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        var msgBox = new CustomMessageBox
        {
            Title = string.IsNullOrEmpty(caption) ? "Message" : caption,
            Owner = owner,
            // Set default result
            Result = defaultResult,
            MessageTitle =
            {
                // Set title and message text
                Text = string.IsNullOrEmpty(caption) ? "" : caption,
                Visibility = string.IsNullOrEmpty(caption) ? Visibility.Collapsed : Visibility.Visible
            },
            MessageText =
            {
                Text = messageText
            }
        };

        // Setup buttons and custom image, passing the defaultResult
        msgBox.SetupButtons(buttons, defaultResult);
        msgBox.SetCustomImage(customImage);

        // Show dialog
        msgBox.ShowDialog();
        return msgBox.Result;
    }

    /// <summary>
    /// Display a message box with a custom image and style
    /// </summary>
    public static MessageBoxResult ShowWithImageAndStyle(Window? owner, string messageText, string caption, 
        MessageBoxButton buttons, ImageSource customImage, CustomMessageBoxStyle style, MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        var msgBox = new CustomMessageBox
        {
            Title = string.IsNullOrEmpty(caption) ? "Message" : caption,
            Owner = owner,
            // Set default result
            Result = defaultResult,
            MessageTitle =
            {
                // Set title and message text
                Text = string.IsNullOrEmpty(caption) ? "" : caption,
                Visibility = string.IsNullOrEmpty(caption) ? Visibility.Collapsed : Visibility.Visible
            },
            MessageText =
            {
                Text = messageText
            }
        };

        // Setup buttons and custom image, passing the defaultResult
        msgBox.SetupButtons(buttons, defaultResult);
        msgBox.SetCustomImage(customImage);

        // Apply custom style
        msgBox.ApplyStyle(style);

        // Show dialog
        msgBox.ShowDialog();
        return msgBox.Result;
    }

}