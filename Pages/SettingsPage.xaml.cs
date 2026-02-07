using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BrowserSelector.Services;
using Microsoft.Win32;

namespace BrowserSelector.Pages
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        public void LoadData()
        {
            UpdateRegistrationStatus();
            UpdateSystemInfo();
        }

        private void UpdateRegistrationStatus()
        {
            try
            {
                bool isRegistered = IsApplicationRegistered();
                bool isSystemDefault = RegistryHelper.IsSystemDefaultBrowser();

                // Show/Hide buttons based on registration status
                RegisterButton.Visibility = isRegistered ? Visibility.Collapsed : Visibility.Visible;
                UnregisterButton.Visibility = isRegistered ? Visibility.Visible : Visibility.Collapsed;
                SetDefaultButton.Visibility = (isRegistered && !isSystemDefault) ? Visibility.Visible : Visibility.Collapsed;

                if (!isRegistered)
                {
                    // State 1: Not Registered (Red)
                    RegistrationStatusText.Text = "Not Registered";
                    RegistrationStatusText.Foreground = new SolidColorBrush(Color.FromRgb(196, 43, 28));
                    RegistrationDetailText.Text = "Click the Register button below to set up LinkRouter.";
                    StatusIconBorder.Background = new SolidColorBrush(Color.FromRgb(253, 231, 233));
                    StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(196, 43, 28));
                    StatusIcon.Text = "\uE711"; // Close icon
                }
                else if (!isSystemDefault)
                {
                    // State 2: Registered but Not Default (Orange)
                    RegistrationStatusText.Text = "Registered - Not Default";
                    RegistrationStatusText.Foreground = new SolidColorBrush(Color.FromRgb(202, 133, 0));
                    RegistrationDetailText.Text = "LinkRouter is registered but not set as the system default browser. Click 'Set as Default' to open Windows Settings.";
                    StatusIconBorder.Background = new SolidColorBrush(Color.FromRgb(255, 244, 206));
                    StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(202, 133, 0));
                    StatusIcon.Text = "\uE7BA"; // Warning icon
                }
                else
                {
                    // State 3: Active as Default (Green)
                    RegistrationStatusText.Text = "Active (Default Browser)";
                    RegistrationStatusText.Foreground = new SolidColorBrush(Color.FromRgb(16, 124, 16));
                    RegistrationDetailText.Text = "LinkRouter is the system default browser. All HTTP/HTTPS links will be routed through LinkRouter.";
                    StatusIconBorder.Background = new SolidColorBrush(Color.FromRgb(223, 246, 221));
                    StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(16, 124, 16));
                    StatusIcon.Text = "\uE73E"; // Check icon
                }

            }
            catch (Exception ex)
            {
                Logger.Log($"SettingsPage UpdateRegistrationStatus ERROR: {ex.Message}");
            }
        }

        private bool IsApplicationRegistered()
        {
            try
            {
                // Check both old name and new name for compatibility
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Clients\StartMenuInternet\BrowserSelector"))
                {
                    if (key != null)
                    {
                        using (RegistryKey? commandKey = key.OpenSubKey(@"shell\open\command"))
                        {
                            if (commandKey != null)
                            {
                                string? registeredPath = commandKey.GetValue("")?.ToString();
                                if (!string.IsNullOrEmpty(registeredPath) &&
                                    (registeredPath.Contains("BrowserSelector.exe") || registeredPath.Contains("LinkRouter.exe")))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Clients\StartMenuInternet\LinkRouter"))
                {
                    if (key != null)
                    {
                        using (RegistryKey? commandKey = key.OpenSubKey(@"shell\open\command"))
                        {
                            if (commandKey != null)
                            {
                                string? registeredPath = commandKey.GetValue("")?.ToString();
                                return !string.IsNullOrEmpty(registeredPath) && registeredPath.Contains("LinkRouter.exe");
                            }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateSystemInfo()
        {
            try
            {
                // Get .NET version
                FrameworkText.Text = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

                // Get Windows version
                var osVersion = Environment.OSVersion;
                if (osVersion.Version.Build >= 22000)
                {
                    PlatformText.Text = "Windows 11";
                }
                else if (osVersion.Version.Major >= 10)
                {
                    PlatformText.Text = "Windows 10";
                }
                else
                {
                    PlatformText.Text = $"Windows {osVersion.Version.Major}.{osVersion.Version.Minor}";
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"SettingsPage UpdateSystemInfo ERROR: {ex.Message}");
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryHelper.RegisterAsDefaultBrowser();

                MessageBox.Show(
                    "LinkRouter has been registered successfully!\n\n" +
                    "Features:\n" +
                    "- Added to Windows startup\n" +
                    "- Ready to intercept links\n\n" +
                    "To set it as default:\n" +
                    "1. Go to Windows Settings\n" +
                    "2. Apps > Default apps\n" +
                    "3. Search for 'LinkRouter'\n" +
                    "4. Set it as default for HTTP and HTTPS",
                    "Registration Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                UpdateRegistrationStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during registration: {ex.Message}",
                    "Registration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UnregisterButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to unregister LinkRouter?\n\n" +
                "This will:\n" +
                "- Remove it from Windows startup\n" +
                "- Remove it as a browser option\n" +
                "- Keep your saved rules\n\n" +
                "You can re-register anytime.",
                "Confirm Unregister",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    RegistryHelper.UnregisterAsDefaultBrowser();

                    MessageBox.Show(
                        "LinkRouter has been unregistered and removed from startup.\n\n" +
                        "Your URL rules have been preserved.",
                        "Unregistration Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    UpdateRegistrationStatus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during unregistration: {ex.Message}",
                        "Unregistration Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void OpenWindowsSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:defaultapps",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Logger.Log($"OpenWindowsSettings_Click ERROR: {ex.Message}");
                MessageBox.Show("Could not open Windows Settings.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Export LinkRouter Data",
                    Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"LinkRouter_Backup_{DateTime.Now:yyyy-MM-dd}"
                };

                if (dialog.ShowDialog() == true)
                {
                    DataExportService.Export(dialog.FileName);

                    MessageBox.Show(
                        $"Data exported successfully!\n\nFile: {dialog.FileName}\n\n" +
                        "This backup includes:\n" +
                        "- All URL rules\n" +
                        "- URL groups and overrides\n" +
                        "- Application settings",
                        "Export Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"ExportData_Click ERROR: {ex.Message}");
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Import LinkRouter Data",
                    Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                    DefaultExt = "json"
                };

                if (dialog.ShowDialog() == true)
                {
                    // Validate the file first
                    var (valid, message, package) = DataExportService.ValidateExportFile(dialog.FileName);

                    if (!valid)
                    {
                        MessageBox.Show($"Cannot import this file:\n\n{message}", "Invalid File",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Confirm import
                    var confirmResult = MessageBox.Show(
                        $"{message}\n\n" +
                        "This will replace your current data.\n" +
                        "A backup will be created automatically before importing.\n\n" +
                        "Do you want to continue?",
                        "Confirm Import",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirmResult != MessageBoxResult.Yes)
                        return;

                    // Perform import
                    var result = DataExportService.Import(dialog.FileName, replaceExisting: true);

                    if (result.Success)
                    {
                        MessageBox.Show(result.Message, "Import Complete",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh the page
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show($"Import failed:\n\n{result.Message}", "Import Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"ImportData_Click ERROR: {ex.Message}");
                MessageBox.Show($"Import failed: {ex.Message}", "Import Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
