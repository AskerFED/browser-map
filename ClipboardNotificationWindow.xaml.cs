using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace BrowserSelector
{
    /// <summary>
    /// Toast notification window for clipboard URL detection.
    /// Shows options to Open or Don't show again for the matched rule.
    /// </summary>
    public partial class ClipboardNotificationWindow : Window
    {
        private readonly string _url;
        private readonly string _domain;
        private readonly MatchResult _matchResult;
        private readonly DispatcherTimer _autoCloseTimer;
        private const int NOTIFICATION_TIMEOUT = 8000; // 8 seconds
        private DateTime _animationStartTime;
        private double _remainingTime;

        public event EventHandler<ClipboardNotificationResult>? NotificationClosed;

        public ClipboardNotificationWindow(string url, string domain, MatchResult matchResult)
        {
            InitializeComponent();

            _url = url;
            _domain = domain;
            _matchResult = matchResult;
            _remainingTime = NOTIFICATION_TIMEOUT;

            // Set UI content
            UrlText.Text = domain;
            SetMatchInfo();

            // Setup auto-close timer
            _autoCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(NOTIFICATION_TIMEOUT)
            };
            _autoCloseTimer.Tick += (s, e) => CloseWithAction(ClipboardNotificationAction.Timeout);

            Loaded += OnLoaded;
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
        }

        private void SetMatchInfo()
        {
            if (_matchResult.Type == MatchType.NoMatch)
            {
                MatchInfoText.Text = "No matching rule found";
                BrowserInfoText.Text = "Will open in default browser";
            }
            else
            {
                string matchSource = _matchResult.Type == MatchType.IndividualRule ? "rule" : "group";
                MatchInfoText.Text = $"Matches: \"{_matchResult.GetRuleName()}\" {matchSource}";

                var browserName = _matchResult.GetBrowserName();
                var profileName = _matchResult.GetProfileName();

                if (!string.IsNullOrEmpty(browserName))
                {
                    BrowserInfoText.Text = string.IsNullOrEmpty(profileName)
                        ? $"Opens in: {browserName}"
                        : $"Opens in: {browserName} - {profileName}";
                }
                else
                {
                    BrowserInfoText.Text = "Will open in default browser";
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Position at bottom-right of screen
            var workingArea = SystemParameters.WorkArea;
            Left = workingArea.Right - Width - 20;
            Top = workingArea.Bottom - ActualHeight - 20;

            AnimateIn();
            _autoCloseTimer.Start();
            _animationStartTime = DateTime.Now;
        }

        private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _autoCloseTimer.Stop();
            _remainingTime -= (DateTime.Now - _animationStartTime).TotalMilliseconds;
            if (_remainingTime < 0) _remainingTime = 0;
        }

        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_remainingTime > 0)
            {
                _autoCloseTimer.Interval = TimeSpan.FromMilliseconds(_remainingTime);
                _autoCloseTimer.Start();
                _animationStartTime = DateTime.Now;
            }
        }

        private void AnimateIn()
        {
            var workingArea = SystemParameters.WorkArea;
            var targetLeft = workingArea.Right - Width - 20;

            // Slide in from right
            var slideIn = new DoubleAnimation
            {
                From = workingArea.Right,
                To = targetLeft,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            BeginAnimation(LeftProperty, slideIn);
            BeginAnimation(OpacityProperty, fadeIn);
        }

        private void CloseWithAnimation(Action? onComplete = null)
        {
            _autoCloseTimer.Stop();

            // Slide out to right
            var slideOut = new DoubleAnimation
            {
                To = SystemParameters.WorkArea.Right,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            fadeOut.Completed += (s, e) =>
            {
                onComplete?.Invoke();
                Close();
            };

            BeginAnimation(LeftProperty, slideOut);
            BeginAnimation(OpacityProperty, fadeOut);
        }

        private void CloseWithAction(ClipboardNotificationAction action)
        {
            CloseWithAnimation(() =>
            {
                NotificationClosed?.Invoke(this, new ClipboardNotificationResult
                {
                    Action = action,
                    Url = _url,
                    Domain = _domain,
                    MatchResult = _matchResult
                });
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWithAction(ClipboardNotificationAction.Dismissed);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWithAction(ClipboardNotificationAction.Open);
        }

        private void DontShowAgainButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWithAction(ClipboardNotificationAction.DontShowAgain);
        }
    }

    public enum ClipboardNotificationAction
    {
        Open,
        DontShowAgain,
        Dismissed,
        Timeout
    }

    public class ClipboardNotificationResult
    {
        public ClipboardNotificationAction Action { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public MatchResult MatchResult { get; set; } = new();
    }
}
