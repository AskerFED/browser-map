using System.Windows;

namespace BrowserSelector.Services
{
    /// <summary>
    /// Centralized service for controlling application lifecycle (shutdown).
    /// All shutdown calls should go through this service for easier control and debugging.
    /// </summary>
    public static class AppLifecycleService
    {
        /// <summary>
        /// Request application shutdown. Centralized control point.
        /// </summary>
        public static void RequestShutdown(string reason)
        {
            Logger.Log($"AppLifecycleService.RequestShutdown: {reason}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }

        /// <summary>
        /// Shutdown or stay in background based on clipboard monitoring state.
        /// If clipboard monitoring is enabled, the app will stay running with tray icon.
        /// </summary>
        public static void ShutdownOrStayInBackground(string reason)
        {
            var settings = SettingsManager.LoadSettings();
            if (settings.ClipboardMonitoring.IsEnabled)
            {
                Logger.Log($"Clipboard monitoring enabled - staying in background ({reason})");
                EnsureBackgroundServicesRunning();
                return;
            }

            RequestShutdown(reason);
        }

        /// <summary>
        /// Ensure clipboard monitoring and tray icon are running.
        /// Called when app should stay in background.
        /// </summary>
        public static void EnsureBackgroundServicesRunning()
        {
            if (!ClipboardMonitorService.Instance.IsRunning)
            {
                Logger.Log("AppLifecycleService: Starting clipboard monitor service");
                ClipboardMonitorService.Instance.Start();
            }
            if (!TrayIconService.Instance.IsVisible)
            {
                Logger.Log("AppLifecycleService: Showing tray icon");
                TrayIconService.Instance.Show();
                TrayIconService.Instance.UpdateState();
            }
        }
    }
}
