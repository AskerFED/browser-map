using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using BrowserSelector.Models;
using Forms = System.Windows.Forms;

namespace BrowserSelector.Services
{
    /// <summary>
    /// Service that manages the system tray icon for background clipboard monitoring.
    /// </summary>
    public class TrayIconService : IDisposable
    {
        #region Singleton

        private static TrayIconService? _instance;
        private static readonly object _lock = new object();

        public static TrayIconService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new TrayIconService();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Fields

        private Forms.NotifyIcon? _notifyIcon;
        private Forms.ContextMenuStrip? _contextMenu;
        private Forms.ToolStripMenuItem? _statusMenuItem;
        private Forms.ToolStripMenuItem? _pauseMenuItem;
        private DispatcherTimer? _pauseTimer;
        private bool _disposed;
        private bool _isVisible;

        #endregion

        #region Events

        public event EventHandler? SettingsRequested;
        public event EventHandler? ExitRequested;

        #endregion

        #region Properties

        public bool IsVisible => _isVisible;

        #endregion

        #region Constructor

        private TrayIconService()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows the tray icon.
        /// </summary>
        public void Show()
        {
            Logger.Log($"TrayIconService.Show() called. Already visible: {_isVisible}");

            if (_isVisible) return;

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CreateNotifyIcon();
                    if (_notifyIcon != null)
                    {
                        _notifyIcon.Visible = true;
                        _isVisible = true;
                        Logger.Log($"TrayIconService: Tray icon shown. Icon null: {_notifyIcon.Icon == null}");
                    }
                    else
                    {
                        Logger.Log("TrayIconService: NotifyIcon is null after creation!");
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Log($"TrayIconService.Show() ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Hides the tray icon.
        /// </summary>
        public void Hide()
        {
            if (!_isVisible) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _isVisible = false;
                    Logger.Log("TrayIconService: Tray icon hidden");
                }
            });
        }

        /// <summary>
        /// Updates the tray icon state based on current settings.
        /// </summary>
        public void UpdateState()
        {
            if (_notifyIcon == null || _statusMenuItem == null) return;

            var settings = SettingsManager.LoadSettings().ClipboardMonitoring;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (settings.IsPaused)
                {
                    _statusMenuItem.Text = $"Paused ({FormatRemainingTime(settings.RemainingPauseTime)})";
                    _statusMenuItem.Checked = false;
                    _notifyIcon.Text = "LinkRouter - Paused";
                }
                else if (settings.IsEnabled)
                {
                    _statusMenuItem.Text = "Monitoring Active";
                    _statusMenuItem.Checked = true;
                    _notifyIcon.Text = "LinkRouter - Monitoring";
                }
                else
                {
                    _statusMenuItem.Text = "Monitoring Disabled";
                    _statusMenuItem.Checked = false;
                    _notifyIcon.Text = "LinkRouter - Disabled";
                }
            });
        }

        /// <summary>
        /// Shows a brief balloon notification.
        /// </summary>
        public void ShowBalloon(string title, string text, Forms.ToolTipIcon icon = Forms.ToolTipIcon.Info, int timeout = 2000)
        {
            if (_notifyIcon == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _notifyIcon.ShowBalloonTip(timeout, title, text, icon);
            });
        }

        /// <summary>
        /// Pauses clipboard monitoring for the specified duration.
        /// </summary>
        public void PauseFor(TimeSpan duration)
        {
            var settings = SettingsManager.LoadSettings();
            settings.ClipboardMonitoring.PauseEndTime = DateTime.Now.Add(duration);
            SettingsManager.SaveSettings(settings);

            Logger.Log($"TrayIconService: Monitoring paused for {duration.TotalMinutes} minutes");

            // Start timer to auto-resume
            StartPauseTimer(duration);
            UpdateState();
        }

        /// <summary>
        /// Resumes clipboard monitoring.
        /// </summary>
        public void Resume()
        {
            var settings = SettingsManager.LoadSettings();
            settings.ClipboardMonitoring.PauseEndTime = null;
            SettingsManager.SaveSettings(settings);

            StopPauseTimer();
            UpdateState();

            Logger.Log("TrayIconService: Monitoring resumed");
        }

        #endregion

        #region Private Methods

        private void CreateNotifyIcon()
        {
            _notifyIcon = new Forms.NotifyIcon
            {
                Icon = LoadIcon(),
                Text = "LinkRouter",
                Visible = false
            };

            // Single click opens settings
            _notifyIcon.Click += (s, e) =>
            {
                // Only handle left-click (right-click shows context menu)
                if (e is Forms.MouseEventArgs mouseArgs && mouseArgs.Button == Forms.MouseButtons.Left)
                {
                    SettingsRequested?.Invoke(this, EventArgs.Empty);
                }
            };

            CreateContextMenu();
            _notifyIcon.ContextMenuStrip = _contextMenu;
        }

        private Icon LoadIcon()
        {
            // Try to load from WPF application resources (app.ico is defined as Resource in .csproj)
            try
            {
                var resourceUri = new Uri("pack://application:,,,/Assets/Icons/app.ico", UriKind.Absolute);
                var streamInfo = Application.GetResourceStream(resourceUri);
                if (streamInfo != null)
                {
                    Logger.Log("TrayIconService: Loaded icon from WPF resource");
                    return new Icon(streamInfo.Stream);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"TrayIconService: WPF resource load failed: {ex.Message}");
            }

            // Fallback: try to load from file in same directory as exe
            try
            {
                var iconPaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Icons", "app.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LinkRouter.ico")
                };

                foreach (var iconPath in iconPaths)
                {
                    if (File.Exists(iconPath))
                    {
                        Logger.Log($"TrayIconService: Loaded icon from file: {iconPath}");
                        return new Icon(iconPath);
                    }
                }

                Logger.Log("TrayIconService: No icon file found, using system default");
            }
            catch (Exception ex)
            {
                Logger.Log($"TrayIconService: File load failed: {ex.Message}");
            }

            // Return default icon
            Logger.Log("TrayIconService: Using system default icon");
            return SystemIcons.Application;
        }

        private void CreateContextMenu()
        {
            _contextMenu = new Forms.ContextMenuStrip();

            // Status item (toggleable)
            _statusMenuItem = new Forms.ToolStripMenuItem("Monitoring Active")
            {
                Checked = true,
                CheckOnClick = false
            };
            _statusMenuItem.Click += StatusMenuItem_Click;
            _contextMenu.Items.Add(_statusMenuItem);

            _contextMenu.Items.Add(new Forms.ToolStripSeparator());

            // Pause submenu
            _pauseMenuItem = new Forms.ToolStripMenuItem("Pause for...");
            _pauseMenuItem.DropDownItems.Add(CreatePauseItem("5 minutes", TimeSpan.FromMinutes(5)));
            _pauseMenuItem.DropDownItems.Add(CreatePauseItem("15 minutes", TimeSpan.FromMinutes(15)));
            _pauseMenuItem.DropDownItems.Add(CreatePauseItem("30 minutes", TimeSpan.FromMinutes(30)));
            _pauseMenuItem.DropDownItems.Add(CreatePauseItem("1 hour", TimeSpan.FromHours(1)));
            _pauseMenuItem.DropDownItems.Add(new Forms.ToolStripSeparator());
            var resumeItem = new Forms.ToolStripMenuItem("Resume now");
            resumeItem.Click += (s, e) => Resume();
            _pauseMenuItem.DropDownItems.Add(resumeItem);
            _contextMenu.Items.Add(_pauseMenuItem);

            _contextMenu.Items.Add(new Forms.ToolStripSeparator());

            // Settings
            var settingsItem = new Forms.ToolStripMenuItem("Settings...");
            settingsItem.Click += (s, e) => SettingsRequested?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(settingsItem);

            _contextMenu.Items.Add(new Forms.ToolStripSeparator());

            // Quit
            var quitItem = new Forms.ToolStripMenuItem("Quit");
            quitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
            _contextMenu.Items.Add(quitItem);
        }

        private Forms.ToolStripMenuItem CreatePauseItem(string text, TimeSpan duration)
        {
            var item = new Forms.ToolStripMenuItem(text);
            item.Click += (s, e) => PauseFor(duration);
            return item;
        }

        private void StatusMenuItem_Click(object? sender, EventArgs e)
        {
            var settings = SettingsManager.LoadSettings();

            if (settings.ClipboardMonitoring.IsPaused)
            {
                // Resume if paused
                Resume();
            }
            else
            {
                // Toggle enabled state
                settings.ClipboardMonitoring.IsEnabled = !settings.ClipboardMonitoring.IsEnabled;
                SettingsManager.SaveSettings(settings);

                if (settings.ClipboardMonitoring.IsEnabled)
                {
                    ClipboardMonitorService.Instance.Start();
                }
                else
                {
                    ClipboardMonitorService.Instance.Stop();
                }

                UpdateState();
            }
        }

        private void StartPauseTimer(TimeSpan duration)
        {
            StopPauseTimer();

            _pauseTimer = new DispatcherTimer
            {
                Interval = duration
            };
            _pauseTimer.Tick += (s, e) =>
            {
                StopPauseTimer();
                Resume();
            };
            _pauseTimer.Start();
        }

        private void StopPauseTimer()
        {
            if (_pauseTimer != null)
            {
                _pauseTimer.Stop();
                _pauseTimer = null;
            }
        }

        private string FormatRemainingTime(TimeSpan? remaining)
        {
            if (!remaining.HasValue) return "";

            if (remaining.Value.TotalMinutes < 1)
                return $"{remaining.Value.Seconds}s";
            else if (remaining.Value.TotalHours < 1)
                return $"{(int)remaining.Value.TotalMinutes}m";
            else
                return $"{(int)remaining.Value.TotalHours}h {remaining.Value.Minutes}m";
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;

            StopPauseTimer();

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            _contextMenu?.Dispose();
            _contextMenu = null;

            _disposed = true;
        }

        #endregion
    }
}
