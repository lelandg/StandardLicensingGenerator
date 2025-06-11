// To use custom icons instead of system icons, add the following files to the Resources folder:
//
//     - error.png
//     - warning.png
//     - info.png
//     - question.png
//
// Recommended size: 32x32 pixels with a transparent background.
//
//     When these files are present, the CustomMessageBox will use them instead of system icons.
//     If the files are not found, it will automatically fall back to system icons.
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StandardLicensingGenerator.Views
{
    public partial class CustomMessageBox : Window
    {
        public MessageBoxResult Result { get; private set; }

        private CustomMessageBox()
        {
            InitializeComponent();
            KeyDown += (sender, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    Result = MessageBoxResult.Cancel;
                    DialogResult = false;
                    Close();
                }
            };
        }

        private void SetupButtons(MessageBoxButton buttons)
        {
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
        }

        private void SetIcon(MessageBoxImage image)
        {
            string iconUri = "";

            switch (image)
            {
                case MessageBoxImage.Error:
                    iconUri = "pack://application:,,,/Resources/error.png";
                    try {
                        // Fallback to system icon if resource not found
                        MessageIcon.Source = new BitmapImage(new Uri(iconUri));
                    }
                    catch {
                        MessageIcon.Source = System.Drawing.SystemIcons.Error.ToImageSource();
                    }
                    break;
                case MessageBoxImage.Question:
                    iconUri = "pack://application:,,,/Resources/question.png";
                    try {
                        MessageIcon.Source = new BitmapImage(new Uri(iconUri));
                    }
                    catch {
                        MessageIcon.Source = System.Drawing.SystemIcons.Question.ToImageSource();
                    }
                    break;
                case MessageBoxImage.Warning:
                    iconUri = "pack://application:,,,/Resources/warning.png";
                    try {
                        MessageIcon.Source = new BitmapImage(new Uri(iconUri));
                    }
                    catch {
                        MessageIcon.Source = System.Drawing.SystemIcons.Warning.ToImageSource();
                    }
                    break;
                case MessageBoxImage.Information:
                    iconUri = "pack://application:,,,/Resources/info.png";
                    try {
                        MessageIcon.Source = new BitmapImage(new Uri(iconUri));
                    }
                    catch {
                        MessageIcon.Source = System.Drawing.SystemIcons.Information.ToImageSource();
                    }
                    break;
                case MessageBoxImage.None:
                    MessageIcon.Visibility = Visibility.Collapsed;
                    break;
            }
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

        #region Static Methods

        public static MessageBoxResult Show(string messageText)
        {
            return Show(null, messageText, "", MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(string messageText, string caption)
        {
            return Show(null, messageText, caption, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton buttons)
        {
            return Show(null, messageText, caption, buttons, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(Window owner, string messageText)
        {
            return Show(owner, messageText, "", MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(Window owner, string messageText, string caption)
        {
            return Show(owner, messageText, caption, MessageBoxButton.OK, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(Window owner, string messageText, string caption, MessageBoxButton buttons)
        {
            return Show(owner, messageText, caption, buttons, MessageBoxImage.None);
        }

        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return Show(null, messageText, caption, buttons, icon);
        }

        public static MessageBoxResult Show(Window? owner, string messageText, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return Show(owner, messageText, caption, buttons, icon, MessageBoxResult.None);
        }

        public static MessageBoxResult Show(Window? owner, string messageText, string caption, MessageBoxButton buttons, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            var msgBox = new CustomMessageBox
            {
                Title = string.IsNullOrEmpty(caption) ? "Message" : caption,
                Owner = owner
            };

            // Set default result based on parameter
            msgBox.Result = defaultResult;

            // Set title and message text
            msgBox.MessageTitle.Text = string.IsNullOrEmpty(caption) ? "" : caption;
            msgBox.MessageTitle.Visibility = string.IsNullOrEmpty(caption) ? Visibility.Collapsed : Visibility.Visible;
            msgBox.MessageText.Text = messageText;

            // Setup buttons and icon
            msgBox.SetupButtons(buttons);
            msgBox.SetIcon(icon);

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
                Owner = owner
            };

            // Set default result
            msgBox.Result = defaultResult;

            // Set title and message text
            msgBox.MessageTitle.Text = string.IsNullOrEmpty(caption) ? "" : caption;
            msgBox.MessageTitle.Visibility = string.IsNullOrEmpty(caption) ? Visibility.Collapsed : Visibility.Visible;
            msgBox.MessageText.Text = messageText;

            // Setup buttons and custom image
            msgBox.SetupButtons(buttons);
            msgBox.SetCustomImage(customImage);

            // Show dialog
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        #endregion
    }
}
