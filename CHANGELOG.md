# Changelog

All notable changes to LinkRouter will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-02-10

### Added

- **Intelligent URL Routing** (`e94fd40`)
  - Route URLs to specific browsers and profiles based on domain patterns
  - Wildcard pattern matching (e.g., `*.google.com`, `github.com/*`)
  - Default browser fallback when no rules match

- **Multi-Browser Support** (`9838ca0`, `eb435b6`)
  - Works with Chrome, Edge, Firefox, Brave, Opera, and Opera GX
  - BrowserDetector service for automatic browser discovery
  - BrowserIconService for centralized browser icon handling
  - Browser-specific color coding throughout the UI

- **Profile Management** (`f53ef35`, `d931b50`)
  - Automatic detection of all browser profiles (personal, work, dev)
  - Profile avatar display in dropdowns with async loading from Chrome/Edge Preferences
  - ProfileAvatarService with disk/memory caching for performance
  - Match profiles by stable `ProfilePath` instead of display name (which can change)

- **URL Groups** (`9838ca0`, `fc9ec18`)
  - Group related URL patterns with shared browser/profile settings
  - Search/filter patterns within groups
  - "Move to Rule" feature to convert group patterns to individual rules
  - Empty state displays for new groups and no search results

- **Built-in Templates** (`4f805e6`, `d19d6e1`)
  - Pre-configured groups for Microsoft 365 (including Engage and Yammer) and Google Suite
  - Built-in groups are preserved (disabled, not deleted) when clearing all groups

- **Multi-Profile Rules** (`f53ef35`)
  - Rules can offer multiple browser/profile choices via picker dialog
  - BrowserProfileMultiSelector UserControl with Simple/Advanced modes
  - RuleProfilePickerWindow supports both UrlRule and UrlGroup

- **Pattern Validation System** (`4e22f99`, `fb34777`)
  - ValidationService with pattern validation, conflict detection, and normalization
  - Real-time pattern conflict detection when adding patterns to groups
  - Validates: empty patterns, wildcards, duplicate rules, domain conflicts, browser paths
  - ValidationMessagePanel control for inline validation display
  - ValidationWarningDialog for warning confirmation before save
  - Source group exclusion for validation when moving patterns

- **Clipboard Monitoring** (`f875a98`, `7acfaa3`)
  - ClipboardMonitorService for detecting URLs in clipboard
  - ClipboardNotificationWindow for URL detection notifications
  - Toggle in Settings page to enable/disable monitoring
  - Clipboard toggle disabled when global monitoring is off

- **System Tray Integration** (`f875a98`)
  - TrayIconService for system tray icon with context menu
  - Minimize to system tray with quick access menu
  - Tray icon activates existing SettingsWindow instead of creating new one

- **Single Instance Support** (`1ed6318`)
  - SingleInstanceManager using Mutex + Named Pipe for instance detection and IPC
  - Prevents multiple app instances from running
  - Forwards URLs/commands to existing instance
  - Fixed named pipe security

- **Toast Notifications** (`9838ca0`, `7acfaa3`)
  - NotificationHelper with modern dark theme UI
  - Quick rule creation from notification when no rule matches
  - "Show Unmatched URL Notifications" toggle in Settings page
  - HasDisabledMatchingRule/Group methods to suppress notifications for disabled rules

- **Data Protection & Backup** (`fdac07c`)
  - Multi-version backup retention (keeps last 5 backups)
  - Pre-migration safety backups before schema changes
  - Graceful corrupt data handling with user notification
  - Schema version tracking to AppSettings
  - JsonStorageService creates timestamped backups on each save
  - Automatic recovery from timestamped backups if main file corrupts

- **Import/Export** (`fdac07c`)
  - DataExportService for full data backup/restore
  - Export/Import feature in Settings page
  - Migration trigger after importing rules to convert old formats

- **Windows Integration** (`b3b8b90`, `7ac608d`)
  - Registers as a Windows browser handler for http/https protocols
  - ApplicationIcon in Capabilities (required for Windows Default Apps list)
  - Startmenu subkey with StartMenuInternet value
  - Auto-registration on app startup if not registered
  - Windows 11 2025+ UserChoiceLatest registry check for default browser

- **Tabbed Settings UI** (`98f1435`)
  - Tabbed navigation (Home, Rules, Settings, Docs)
  - Windows 11-style theme with modern controls
  - SettingsCard control for consistent settings layout
  - ConfirmationDialog for delete confirmations
  - Frame-based page navigation

- **Navigation Status Indicator** (`959c34b`, `d931b50`)
  - 3-state status indicator in SettingsWindow navigation bar (Active/Not Default/Not Registered)
  - Fixed sidebar status to check IsSystemDefaultBrowser() not just IsEnabled
  - Shows "Not Default" (orange) when rules enabled but not default browser

- **Edit Mode Change Detection** (`7acfaa3`)
  - Save button visibility toggle in edit mode for rules and URL groups
  - Only shows Save when changes are detected

- **Date/Time Tooltips** (`46a4c98`, `0a79de1`)
  - Full date/time tooltip on hover for Updated column (e.g., "February 9, 2026 at 3:45 PM")
  - Full date/time when hovering over pattern dates in URL group editor

- **Command-line Options** (`2a19611`)
  - Support for `--manage`, `--register`, `--unregister`, `--startup`

- **Installer** (`04d9b94`, `210d1e9`)
  - Inno Setup installer script (LinkRouterSetup.iss)
  - AppConfig.cs for centralized configuration
  - Unique GUID and publisher info

- **Logging** (`e2f5da9`)
  - Log file moved to %APPDATA%\LinkRouter
  - Log rotation (1MB max, keeps one backup as log.old.txt)

### Fixed

- **Rule Priority Order** (`7b73fd9`)
  - **Issue**: URL Groups were incorrectly taking priority over individual rules, causing specific rules to be ignored when a matching group existed.
  - **Fix**: Reversed the matching order so individual rules are now evaluated first, allowing users to create specific overrides for URLs that would otherwise match a group pattern.
  - **Change**: Updated validation dialog title to "Confirm Save" with clearer message.

- **Notification Suppression for Built-in Groups** (`2cc639e`)
  - **Issue**: "Create Rule" notifications were being suppressed for built-in URL groups (like Microsoft 365, Google Suite) that were disabled by default, even though the user never explicitly disabled them.
  - **Fix**: Added `HasBeenConfigured` flag to track whether a user has actually interacted with a group. Notifications are now only suppressed for groups the user explicitly disabled.

- **Browser Registration** (`b3b8b90`)
  - **Issue**: App wasn't appearing in Windows Default Apps list and was writing to protected system registry keys.
  - **Fix**: Added ApplicationIcon to Capabilities, added Startmenu subkey, reverted broken changes that wrote to `Software\Classes\http/https`, removed dangerous deletion of system protocol handler keys.

- **Profile Matching** (`b3b8b90`)
  - **Issue**: Profile names with duplicate suffixes like "Person 2 (Profile 3)" wouldn't match after Chrome profile changes.
  - **Fix**: Match profiles by stable `ProfilePath` instead of display `Name` which can change.

- **Chrome Profile Names** (`e2f5da9`)
  - **Issue**: Profile names showed internal names instead of user-configured custom names.
  - **Fix**: Read Chrome/Edge profile names from Local State file instead of individual Preferences files.

- **Window Shutdown Bugs** (`08a7124`)
  - **Issue**: Event handler leaks caused crashes and unexpected behavior on window close.
  - **Fix**: Added event handler cleanup in SettingsWindow.Closing to unsubscribe from page events. Fixed notification flow to properly shutdown app when AddRuleWindow is cancelled or SettingsWindow closes.

- **Default Browser Status Detection** (`d931b50`)
  - **Issue**: Sidebar showed incorrect status by checking IsEnabled instead of IsSystemDefaultBrowser.
  - **Fix**: Settings page now uses IsSystemDefaultBrowser() instead of IsRegistered().

- **Import Migration** (`fb34777`)
  - **Issue**: Imported rules in old single-profile format weren't being converted.
  - **Fix**: Added migration trigger after importing rules to ensure immediate conversion to multi-profile format.

- **Build Command** (`1bb224b`)
  - **Issue**: MSB1011 error when multiple .csproj files exist.
  - **Fix**: Specify project file explicitly in build command.

### Changed

- **Landing Page Redesign** (`cc49da8`, `7113ccf`)
  - Complete redesign with modern 2025 UI/UX featuring dark gradient theme and Inter font
  - Animated browser icons orbiting in hero section
  - Replaced emojis with professional SVG icons throughout
  - Sticky navigation with mobile-responsive hamburger menu
  - Comprehensive SEO: meta tags, Open Graph, Twitter Cards, JSON-LD structured data
  - Accessibility improvements including focus states and reduced motion support
  - Fixed responsive layout: changed feature grid from `auto-fit` to fixed 3-column with breakpoints for tablets (768px) and mobile (480px)

- **Browser Icons on Landing Page** (`2e22640`, `2c5315e`, `0f70983`)
  - Replaced placeholder icons with official browser brand logos from browser-logos CDN
  - Added fallback inline SVGs when CDN failed to load
  - High-quality icons for Chrome, Edge, Firefox, Brave, and Opera

- **App Icon Branding** (`fe39b69`, `ebefac1`, `913ba2d`)
  - Replaced generic SVG icons with actual LinkRouter icon in navigation, footer, and hero section
  - Upgraded orbit center icon from blurry 32x32 to sharp 128x128 resolution

- **Documentation** (`746e9bd`, `b512ce7`, `6d9a044`)
  - Added 14 screenshots documenting all major features
  - Reorganized README structure to be feature-focused with contextual screenshots
  - Comprehensive documentation covering features, architecture, edge cases, validation, and troubleshooting
  - Added missing features to README: Clipboard Monitoring, System Tray, Single Instance, Toast Notifications

- **Rules Page UX** (`c0c80f9`, `8246662`)
  - Search placeholder text and clear (X) button in search boxes
  - Disabled delete button for built-in groups with tooltip explanation
  - Click-to-edit functionality on group cards
  - Resizable EditUrlGroupWindow without nested scroll
  - Clear buttons moved next to add buttons (right-aligned)
  - Tab-specific clear functionality (rules vs groups)
  - Reduced toggle switch size with softer colors
  - Adjusted column widths for better readability

- **Download Links** (`734fb9a`, `e6d9395`, `e348c51`)
  - Updated all download URLs to point to installer package
  - Fixed portable ZIP filename to match release asset
  - Download links now open in new tab

- **Removed Browser Policy Feature** (`959c34b`)
  - Removed ineffective browser policy suppression feature

### Browser Support

- Google Chrome (full profile support with avatars)
- Microsoft Edge (full profile support with avatars)
- Mozilla Firefox (full profile support)
- Brave Browser (full profile support)
- Opera (full profile support)
- Opera GX (full profile support)

---
