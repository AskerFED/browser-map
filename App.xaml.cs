using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using BrowserSelector.Models;
using BrowserSelector.Services;

namespace BrowserSelector
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Logger.Log("========== APPLICATION STARTUP ==========");
            Logger.Log($"Arguments count: {e.Args.Length}");
            for (int i = 0; i < e.Args.Length; i++)
            {
                Logger.Log($"  Arg[{i}]: {e.Args[i]}");
            }

            // --- SINGLE INSTANCE CHECK (must be first) ---
            if (!SingleInstanceManager.Instance.TryAcquireLock())
            {
                // Another instance exists - forward args and exit
                Logger.Log("Forwarding arguments to existing instance");

                if (e.Args.Length > 0)
                {
                    SingleInstanceManager.SendArgumentsToExistingInstance(e.Args);
                }
                else
                {
                    // No args = user wants to open settings
                    SingleInstanceManager.SendArgumentsToExistingInstance(new[] { "--manage" });
                }

                // Exit immediately (don't initialize anything)
                Shutdown(0);
                return;
            }

            // This is the first instance - start pipe server to receive args from future instances
            SingleInstanceManager.Instance.ArgumentsReceived += OnArgumentsReceived;
            SingleInstanceManager.Instance.StartPipeServer();

            // Pre-load profile avatars in background for better UX
            ProfileAvatarService.LoadAllAvatarsAtStartup();

            // Initialize built-in templates and apply any updates
            try
            {
                UrlGroupManager.EnsureBuiltInGroupsExist();
            }
            catch (Exception ex)
            {
                Logger.Log($"Warning: Failed to initialize built-in templates: {ex.Message}");
            }

            // Auto-register if not already registered
            if (!RegistryHelper.IsRegistered())
            {
                Logger.Log("App not registered - auto-registering...");
                try
                {
                    RegistryHelper.RegisterAsDefaultBrowser();
                    Logger.Log("Auto-registration completed successfully");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Auto-registration failed: {ex.Message}");
                }
            }

            // Always set up clipboard monitoring event handlers
            // This ensures notifications work whether enabled from Settings or via --startup
            SetupClipboardEventHandlers();

            if (e.Args.Length > 0)
            {
                var url = e.Args[0];
                Logger.Log($"Processing argument: {url}");

                // Handle startup parameter (silent start or background monitoring)
                if (url.Equals("--startup", StringComparison.OrdinalIgnoreCase))
                {
                    var clipboardSettings = SettingsManager.LoadSettings().ClipboardMonitoring;

                    if (clipboardSettings.IsEnabled)
                    {
                        Logger.Log("Command: --startup - Starting clipboard monitoring in background");
                        StartBackgroundMonitoring();
                        return; // Keep app running in background
                    }
                    else
                    {
                        Logger.Log("Command: --startup (silent start) - clipboard monitoring disabled, exiting silently");
                        AppLifecycleService.RequestShutdown("--startup with monitoring disabled");
                        return;
                    }
                }

                // Handle background monitoring command
                if (url.Equals("--background", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Log("Command: --background - Starting clipboard monitoring");
                    StartBackgroundMonitoring();
                    return;
                }

                // Handle manage rules command
                if (url.Equals("--manage", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Log("Command: --manage - opening SettingsWindow");
                    try
                    {
                        var settingsWindow = new SettingsWindow();
                        Logger.Log("SettingsWindow created, showing dialog...");
                        settingsWindow.ShowDialog();
                        Logger.Log("SettingsWindow closed");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"FATAL: Failed to create SettingsWindow: {ex}");
                        MessageBox.Show($"Error starting application:\n\n{ex.Message}\n\nCheck log at D:\\LinkRouter\\log.txt",
                            "LinkRouter Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    AppLifecycleService.ShutdownOrStayInBackground("--manage completed");
                    return;
                }

                // Handle register command - register and show settings with App tab
                if (url.Equals("--register", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Log("Command: --register - registering as default browser");
                    try
                    {
                        Logger.Log("Calling RegistryHelper.RegisterAsDefaultBrowser()...");
                        RegistryHelper.RegisterAsDefaultBrowser();
                        Logger.Log("Registration complete, opening SettingsWindow");
                        var settingsWindow = new SettingsWindow();
                        settingsWindow.SelectAppTab();
                        Logger.Log("SettingsWindow created, showing dialog...");
                        settingsWindow.ShowDialog();
                        Logger.Log("SettingsWindow closed");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"FATAL: Failed to create SettingsWindow: {ex}");
                        MessageBox.Show($"Error starting application:\n\n{ex.Message}\n\nCheck log at D:\\LinkRouter\\log.txt",
                            "LinkRouter Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    AppLifecycleService.ShutdownOrStayInBackground("--register completed");
                    return;
                }
                else if (url.Equals("--unregister", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Log("Command: --unregister - unregistering as default browser");
                    RegistryHelper.UnregisterAsDefaultBrowser();
                    Logger.Log("Unregistration complete, shutting down");
                    AppLifecycleService.RequestShutdown("--unregister complete");
                    return;
                }

                // Normal operation - process URL and open browser
                else
                {
                    Logger.Log("Normal operation - processing URL");

                    // Validate URL - treat empty/whitespace as no URL
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        Logger.Log("URL is empty/whitespace - treating as no URL, showing SettingsWindow");
                        try
                        {
                            var settingsWindow = new SettingsWindow();
                            settingsWindow.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"FATAL: Failed to create SettingsWindow: {ex}");
                            MessageBox.Show($"Error starting application:\n\n{ex.Message}",
                                "LinkRouter Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        AppLifecycleService.ShutdownOrStayInBackground("Empty URL - settings closed");
                        return;
                    }

                    Logger.Log($"Valid URL received: {url}");
                    Logger.Log("Creating MainWindow...");

                    MainWindow? mainWindow = null;
                    try
                    {
                        mainWindow = new MainWindow();
                        Logger.Log("MainWindow created successfully");
                        Logger.Log("Calling mainWindow.SetUrl()...");
                        mainWindow.SetUrl(url);
                        Logger.Log("SetUrl() completed - flow continues in MainWindow");
                        // Note: If SetUrl() finds a match and auto-opens, it calls Shutdown()
                        // If no match, HandleNoMatch() calls Show() on the window
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error processing URL: {ex}");

                        // Try to show the window as fallback for manual selection
                        Logger.Log("Attempting fallback - showing window for manual selection");
                        try
                        {
                            if (mainWindow != null)
                            {
                                Logger.Log("MainWindow exists - showing it");
                                mainWindow.Show();
                                mainWindow.Activate();
                            }
                            else
                            {
                                // MainWindow creation failed - show error and shutdown
                                Logger.Log("MainWindow is null - showing error and shutting down");
                                MessageBox.Show($"Error processing URL:\n\n{ex.Message}",
                                    "LinkRouter Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                AppLifecycleService.RequestShutdown("MainWindow creation failed");
                            }
                        }
                        catch (Exception fallbackEx)
                        {
                            // Complete failure - shutdown
                            Logger.Log($"Fallback also failed: {fallbackEx}");
                            MessageBox.Show($"Error starting application:\n\n{ex.Message}",
                                "LinkRouter Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            AppLifecycleService.RequestShutdown("Fallback failed");
                        }
                    }
                }
            }
            else
            {
                // No URL argument - show SettingsWindow directly
                Logger.Log("No arguments provided - showing SettingsWindow");
                try
                {
                    var settingsWindow = new SettingsWindow();
                    Logger.Log("SettingsWindow created, showing dialog...");
                    settingsWindow.ShowDialog();
                    Logger.Log("SettingsWindow closed");
                }
                catch (Exception ex)
                {
                    Logger.Log($"FATAL: Failed to create SettingsWindow: {ex}");
                    MessageBox.Show($"Error starting application:\n\n{ex.Message}\n\nCheck log at D:\\LinkRouter\\log.txt",
                        "LinkRouter Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                AppLifecycleService.ShutdownOrStayInBackground("No arguments - settings closed");
            }
        }

        private bool _clipboardHandlersSetup = false;

        private void SetupClipboardEventHandlers()
        {
            if (_clipboardHandlersSetup) return;

            // Set up clipboard monitoring events (once)
            ClipboardMonitorService.Instance.UrlDetected += OnClipboardUrlDetected;

            // Set up tray icon events
            TrayIconService.Instance.SettingsRequested += OnTraySettingsRequested;
            TrayIconService.Instance.ExitRequested += OnTrayExitRequested;

            _clipboardHandlersSetup = true;
            Logger.Log("SetupClipboardEventHandlers: Event handlers registered");
        }

        private void StartBackgroundMonitoring()
        {
            Logger.Log("StartBackgroundMonitoring: Initializing services");

            // Event handlers are already set up in Application_Startup via SetupClipboardEventHandlers()

            // Start services
            ClipboardMonitorService.Instance.Start();
            TrayIconService.Instance.Show();
            TrayIconService.Instance.UpdateState();

            Logger.Log("StartBackgroundMonitoring: Services started, app running in background");
        }

        private void OnClipboardUrlDetected(object? sender, ClipboardUrlEventArgs e)
        {
            Logger.Log($"OnClipboardUrlDetected: {e.Url}");

            Dispatcher.Invoke(() =>
            {
                // Get match result
                var matchResult = UrlRuleManager.FindMatch(e.Url);

                // Show notification
                var notification = new ClipboardNotificationWindow(e.Url, e.Domain, matchResult);
                notification.NotificationClosed += OnNotificationClosed;
                notification.Show();
            });
        }

        private void OnNotificationClosed(object? sender, ClipboardNotificationResult result)
        {
            Logger.Log($"OnNotificationClosed: Action={result.Action}, URL={result.Url}");

            switch (result.Action)
            {
                case ClipboardNotificationAction.Open:
                    OpenUrlWithMatch(result.Url, result.MatchResult);
                    break;

                case ClipboardNotificationAction.DontShowAgain:
                    // Disable clipboard notifications for this rule
                    DisableClipboardForRule(result.MatchResult);
                    Logger.Log($"Disabled clipboard notifications for matched rule");
                    break;

                case ClipboardNotificationAction.Dismissed:
                case ClipboardNotificationAction.Timeout:
                    // No action needed
                    break;
            }
        }

        private void DisableClipboardForRule(MatchResult match)
        {
            if (match.Type == MatchType.IndividualRule && match.Rule != null)
            {
                match.Rule.ClipboardNotificationsEnabled = false;
                UrlRuleManager.UpdateRule(match.Rule);
                Logger.Log($"Disabled clipboard for rule: {match.Rule.Pattern}");
            }
            else if ((match.Type == MatchType.UrlGroup || match.Type == MatchType.GroupOverride) && match.Group != null)
            {
                match.Group.ClipboardNotificationsEnabled = false;
                UrlGroupManager.UpdateGroup(match.Group);
                Logger.Log($"Disabled clipboard for group: {match.Group.Name}");
            }
        }

        private void OpenUrlWithMatch(string url, MatchResult matchResult)
        {
            try
            {
                var browserPath = matchResult.GetBrowserPath();
                var profileArgs = matchResult.GetProfileArguments();

                if (matchResult.Type != MatchType.NoMatch && !string.IsNullOrEmpty(browserPath))
                {
                    var args = string.IsNullOrEmpty(profileArgs)
                        ? $"\"{url}\""
                        : $"{profileArgs} \"{url}\"";

                    Logger.Log($"Opening URL with profile: {browserPath} {args}");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = browserPath,
                        Arguments = args,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Use default browser
                    var settings = SettingsManager.LoadSettings();
                    if (!string.IsNullOrEmpty(settings.DefaultBrowserPath))
                    {
                        Logger.Log($"Opening URL with default browser: {settings.DefaultBrowserPath}");
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = settings.DefaultBrowserPath,
                            Arguments = $"\"{url}\"",
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        // Fallback to system default
                        Logger.Log("Opening URL with system default browser");
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"OpenUrlWithMatch error: {ex.Message}");
            }
        }

        private void OnTraySettingsRequested(object? sender, EventArgs e)
        {
            Logger.Log("OnTraySettingsRequested: Opening settings window");
            ShowOrActivateSettingsWindow();
        }

        private void OnTrayExitRequested(object? sender, EventArgs e)
        {
            Logger.Log("OnTrayExitRequested: Shutting down");

            // Clean up services
            ClipboardMonitorService.Instance.Stop();
            ClipboardMonitorService.Instance.Dispose();
            TrayIconService.Instance.Hide();
            TrayIconService.Instance.Dispose();

            AppLifecycleService.RequestShutdown("Tray exit requested");
        }

        #region Single Instance Support

        private void OnArgumentsReceived(object? sender, string[] args)
        {
            Logger.Log($"OnArgumentsReceived: {string.Join(", ", args)}");
            ProcessReceivedArguments(args);
        }

        private void ProcessReceivedArguments(string[] args)
        {
            if (args.Length == 0)
            {
                // No args - show/activate settings window
                ShowOrActivateSettingsWindow();
                return;
            }

            var arg = args[0];

            if (arg.Equals("--manage", StringComparison.OrdinalIgnoreCase))
            {
                ShowOrActivateSettingsWindow();
            }
            else if (arg.Equals("--startup", StringComparison.OrdinalIgnoreCase))
            {
                // Already running - just ensure background services
                var settings = SettingsManager.LoadSettings();
                if (settings.ClipboardMonitoring.IsEnabled)
                {
                    AppLifecycleService.EnsureBackgroundServicesRunning();
                }
            }
            else if (arg.Equals("--background", StringComparison.OrdinalIgnoreCase))
            {
                AppLifecycleService.EnsureBackgroundServicesRunning();
            }
            else if (arg.Equals("--register", StringComparison.OrdinalIgnoreCase))
            {
                RegistryHelper.RegisterAsDefaultBrowser();
                ShowOrActivateSettingsWindow(selectAppTab: true);
            }
            else if (arg.Equals("--unregister", StringComparison.OrdinalIgnoreCase))
            {
                // Probably shouldn't process from second instance
                Logger.Log("Ignoring --unregister from second instance");
            }
            else if (!string.IsNullOrWhiteSpace(arg))
            {
                // It's a URL - process it
                ProcessUrlFromSecondInstance(arg);
            }
        }

        private void ShowOrActivateSettingsWindow(bool selectAppTab = false)
        {
            Dispatcher.Invoke(() =>
            {
                // Check for existing settings window
                var existingWindow = Windows
                    .OfType<SettingsWindow>()
                    .FirstOrDefault();

                if (existingWindow != null)
                {
                    Logger.Log("Activating existing SettingsWindow");
                    existingWindow.WindowState = WindowState.Normal;
                    existingWindow.Activate();
                    existingWindow.Focus();

                    if (selectAppTab)
                    {
                        existingWindow.SelectAppTab();
                    }
                }
                else
                {
                    Logger.Log("Creating new SettingsWindow");
                    var settingsWindow = new SettingsWindow();

                    if (selectAppTab)
                    {
                        settingsWindow.SelectAppTab();
                    }

                    settingsWindow.Show();
                    settingsWindow.Activate();
                }
            });
        }

        private void ProcessUrlFromSecondInstance(string url)
        {
            Logger.Log($"ProcessUrlFromSecondInstance: {url}");

            Dispatcher.Invoke(() =>
            {
                try
                {
                    var mainWindow = new MainWindow();
                    mainWindow.SetUrl(url);
                }
                catch (Exception ex)
                {
                    Logger.Log($"ProcessUrlFromSecondInstance error: {ex.Message}");
                }
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Log("Application.OnExit: Cleaning up");
            SingleInstanceManager.Instance.Dispose();
            base.OnExit(e);
        }

        #endregion

    }

    public static class Logger
    {
        private static readonly string LogDirectory = AppConfig.AppDataFolder;
        private static readonly string LogFilePath = Path.Combine(LogDirectory, "log.txt");
        private static readonly string OldLogFilePath = Path.Combine(LogDirectory, "log.old.txt");
        private const long MaxLogSize = 1 * 1024 * 1024; // 1 MB

        public static void Log(string message)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);

                // Rotate log if it exceeds max size
                if (File.Exists(LogFilePath))
                {
                    var fileInfo = new FileInfo(LogFilePath);
                    if (fileInfo.Length > MaxLogSize)
                    {
                        if (File.Exists(OldLogFilePath))
                            File.Delete(OldLogFilePath);
                        File.Move(LogFilePath, OldLogFilePath);
                    }
                }

                File.AppendAllText(
                    LogFilePath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {message}{Environment.NewLine}"
                );
            }
            catch
            {
                // Never crash the app because of logging
            }
        }
    }
}