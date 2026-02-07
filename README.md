# LinkRouter (BrowserSelector)

A powerful Windows desktop application that acts as an intelligent browser router. When you click any URL from applications like Teams, Outlook, Word, Slack, or emails, LinkRouter intercepts the link and routes it to the correct browser and profile based on your configured rules.

## Table of Contents

- [Features](#features)
- [How It Works](#how-it-works)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [URL Routing System](#url-routing-system)
- [Managing Rules](#managing-rules)
- [URL Groups](#url-groups)
- [Multi-Profile Rules](#multi-profile-rules)
- [Settings & Configuration](#settings--configuration)
- [Data Storage & Backup](#data-storage--backup)
- [Windows Integration](#windows-integration)
- [Validation System](#validation-system)
- [Keyboard & UI Interactions](#keyboard--ui-interactions)
- [Command-Line Interface](#command-line-interface)
- [Technical Architecture](#technical-architecture)
- [Browser Support](#browser-support)
- [Edge Cases & Error Handling](#edge-cases--error-handling)
- [Troubleshooting](#troubleshooting)
- [Project Structure](#project-structure)
- [Technologies Used](#technologies-used)
- [Contributing](#contributing)
- [License](#license)

---

## Features

### Core Features

- **Intelligent URL Routing** - Automatically routes URLs to the correct browser and profile based on configurable rules
- **Multi-Browser Support** - Detects and works with Chrome, Edge, Firefox, Brave, Opera, and Opera GX
- **Profile Management** - Access all your browser profiles (personal, work, development) and open links directly in the right profile
- **Multi-Profile Rules** - Single rules can have multiple browser/profile options with a picker UI
- **URL Groups** - Group URLs by domain patterns with auto-open behavior (e.g., all Microsoft 365 URLs open in Edge Work profile)
- **Built-in Groups** - Pre-configured groups for Microsoft 365 and Google Suite services (disabled by default)
- **Priority-Based Routing** - Clear priority order: Group Overrides > Individual Rules > URL Groups > Default
- **Default Browser Fallback** - Set a fallback browser for when no rules match

### Advanced Features

- **Windows Account Detection** - Auto-detect Azure AD/Office 365 accounts to match with browser profiles
- **Real-time Validation** - Pattern validation with conflict detection and warnings
- **Atomic Data Saves** - Safe file operations with automatic rolling backups
- **Import/Export** - Full backup and restore functionality
- **Toast Notifications** - Optional notifications when using default browser
- **Last Active Browser Memory** - Remember and auto-select recently used browser/profile (24-hour window)
- **Test Automation** - Built-in testing for all rules (Dev mode)

### Windows Integration

- **Default Browser Registration** - Registers as a Windows default browser handler
- **Startup Support** - Can start silently with Windows
- **Protocol Handling** - Handles http:// and https:// protocols
- **Single-File Deployment** - Self-contained executable with no external dependencies

---

## How It Works

```
URL clicked (from any app: Teams, Outlook, Slack, etc.)
        │
        ▼
Windows calls LinkRouter as default browser
        │
        ▼
┌─────────────────────────────────────────────┐
│ Priority 1: URL Group Overrides             │
│ (specific URL patterns within groups)       │
└─────────────────┬───────────────────────────┘
                  │ No match
                  ▼
┌─────────────────────────────────────────────┐
│ Priority 2: Individual URL Rules            │
│ (single-pattern rules with browser/profile) │
│ → If multiple profiles: Show picker         │
└─────────────────┬───────────────────────────┘
                  │ No match
                  ▼
┌─────────────────────────────────────────────┐
│ Priority 3: URL Groups                      │
│ (pattern collections like Microsoft 365)    │
│ → If multiple profiles: Show picker         │
└─────────────────┬───────────────────────────┘
                  │ No match
                  ▼
┌─────────────────────────────────────────────┐
│ Priority 4: Default Browser                 │
│ (fallback browser or show manual picker)    │
│ → Optional toast notification               │
└─────────────────────────────────────────────┘
```

### URL Matching Logic

1. **Pattern Normalization** - Patterns are normalized (lowercase, no protocol, no trailing slash)
2. **Substring Matching** - URL is checked if it contains the pattern
3. **Domain Matching** - URL domain is extracted and compared with pattern
4. **Subdomain Matching** - `sharepoint.com` matches `*.sharepoint.com`

---

## Installation

### System Requirements

- **Operating System**: Windows 10 (version 1607+) or Windows 11
- **Architecture**: 64-bit (x64)
- **Runtime**: None required (self-contained)

### Option 1: Use Pre-built Installer (Recommended)

1. Download `LinkRouterSetup-1.0.0.exe` from the Releases page
2. Run the installer
3. Choose installation options:
   - Create desktop shortcut
   - Create Start Menu shortcut
   - Register as browser handler
4. Complete installation

The installer places the application in `C:\Program Files\LinkRouter\`

### Option 2: Build from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/LinkRouter.git
cd LinkRouter

# Build the project
dotnet build -c Release

# Publish as single executable (self-contained)
dotnet publish -c Release -r win-x64 --self-contained

# Output: bin\Release\net8.0-windows\win-x64\publish\LinkRouter.exe
```

### Option 3: Portable Installation

1. Copy `LinkRouter.exe` to any folder
2. Run the application
3. It will auto-register and create data folder at `%APPDATA%\LinkRouter\`

---

## Getting Started

### First-Time Setup

1. **Launch LinkRouter** - The Settings window opens automatically on first run
2. **Register as Browser** - Go to Settings tab and click "Open Windows Settings"
3. **Set as Default** - In Windows Settings, set LinkRouter as default for HTTP/HTTPS
4. **Configure Default Browser** - On Home tab, select your fallback browser
5. **Enable Built-in Groups** (Optional) - Enable Microsoft 365 or Google Suite groups

### Quick Rule Creation

When you click a URL and no rule matches:

1. The browser picker window appears
2. Select your preferred browser and profile
3. Check **"Remember this choice"**
4. Click **Open** - A rule is automatically created for that domain

---

## URL Routing System

### Priority Order

| Priority | Type | Description |
|----------|------|-------------|
| 1 | **Group Overrides** | Specific URL patterns that override group defaults |
| 2 | **Individual Rules** | Single-pattern rules with browser/profile |
| 3 | **URL Groups** | Pattern collections (Microsoft 365, Google Suite, custom) |
| 4 | **Default Browser** | Fallback when no rules match |

### Pattern Matching

Patterns are matched using these methods:

1. **Contains Match**: URL contains the pattern string
2. **Domain Match**: Extracted URL domain matches pattern
3. **Subdomain Match**: Pattern `sharepoint.com` matches `contoso.sharepoint.com`

### Pattern Syntax

| Pattern | Matches |
|---------|---------|
| `github.com` | `github.com`, `www.github.com`, `gist.github.com` |
| `docs.google.com` | Only `docs.google.com` |
| `sharepoint.com` | All SharePoint sites: `*.sharepoint.com` |
| `outlook.office.com` | Outlook web app specifically |

**Note**: Wildcards (`*`) are not supported. Use domain patterns instead.

---

## Managing Rules

### Individual Rules

Individual rules are single-pattern rules that route specific URLs.

#### Creating a Rule

1. Go to **Settings** > **Manage Rules** tab
2. Click **+ Add Rule**
3. Enter URL pattern (e.g., `github.com`)
4. Select browser and profile (or multiple profiles)
5. Click **Save**

#### Rule Properties

| Property | Description |
|----------|-------------|
| Pattern | Domain or URL substring to match |
| Enabled | Toggle to enable/disable without deleting |
| Profiles | One or more browser/profile combinations |
| Created Date | When the rule was created |

#### Multi-Profile Rules

Rules can have multiple browser/profile options:
- **Single Profile**: Auto-opens in that browser/profile
- **Multiple Profiles**: Shows a picker to choose which profile

### Managing Rules

| Action | Description |
|--------|-------------|
| **Edit** | Modify pattern or profiles |
| **Delete** | Remove the rule |
| **Move to Group** | Convert rule to URL group pattern |
| **Enable/Disable** | Toggle without deleting |
| **Search** | Filter rules by pattern |

---

## URL Groups

URL Groups are collections of URL patterns that share the same browser/profile configuration.

### Built-in Groups

LinkRouter includes two built-in groups (disabled by default):

#### Microsoft 365

Patterns:
- `admin.microsoft.com`, `portal.azure.com`, `outlook.office.com`
- `outlook.office365.com`, `teams.microsoft.com`, `forms.office.com`
- `sharepoint.com`, `onedrive.com`, `office.com`, `microsoft365.com`
- `live.com`, `microsoftonline.com`, `azure.com`, `dynamics.com`
- `powerbi.com`, `powerapps.com`, `flow.microsoft.com`

#### Google Suite

Patterns:
- `mail.google.com`, `drive.google.com`, `docs.google.com`
- `sheets.google.com`, `slides.google.com`, `calendar.google.com`
- `meet.google.com`, `chat.google.com`, `keep.google.com`
- `contacts.google.com`, `admin.google.com`, `cloud.google.com`
- `console.cloud.google.com`

### Creating Custom Groups

1. Go to **Settings** > **Manage Rules** > **URL Groups** tab
2. Click **+ Add Group**
3. Enter group name and description
4. Add URL patterns
5. Configure browser/profile (single or multiple)
6. Set behavior:
   - **Use Default**: Auto-open with default browser/profile
   - **Show Profile Picker**: Always show picker (for multi-profile)
7. Click **Save**

### Group Properties

| Property | Description |
|----------|-------------|
| Name | Display name for the group |
| Description | Optional description |
| Is Built-In | Protected from deletion (can be modified) |
| Is Enabled | Toggle to enable/disable |
| URL Patterns | List of domain patterns |
| Default Browser | Browser to use for auto-open |
| Profiles | Multiple browser/profile options |
| Behavior | UseDefault or ShowProfilePicker |

### Group Overrides

Override specific URLs within a group to use different settings:

1. Within a group, certain URLs can have different browser/profile
2. Overrides have higher priority than group defaults
3. Example: Most M365 in Edge, but `personal.outlook.com` in Chrome

---

## Multi-Profile Rules

Rules and groups can have multiple browser/profile combinations.

### How It Works

| Profiles Count | Behavior |
|----------------|----------|
| 0 profiles | Falls back to manual selection |
| 1 profile | Auto-opens automatically |
| 2+ profiles | Shows profile picker window |

### Profile Picker

When a rule/group has multiple profiles:

1. A picker window appears showing all configured profiles
2. Each profile shows:
   - Browser icon and color
   - Profile name (or custom display name)
   - Browser name
3. Click a profile to open the URL
4. Selection is remembered as "last active" for future auto-selection

### Custom Display Names

Each profile in a rule can have a custom display name:
- Default: "Chrome - Work Profile"
- Custom: "Development Environment"

---

## Settings & Configuration

### Home Page

- **Fallback Browser**: Select default browser when no rules match
- **Statistics Dashboard**:
  - Total/Enabled/Disabled individual rules
  - Total/Enabled/Disabled URL groups
  - Installed browsers count

### Settings Page

#### Rules Processing

Toggle master switch to enable/disable all rule processing. When disabled:
- All URLs use the default browser
- Rules are preserved but not evaluated

#### Notifications

- **Show Notifications**: Toast notification when default browser is used
- Notification includes option to create a rule for the domain

#### Backup & Restore

- **Export**: Save all settings, rules, and groups to a JSON file
- **Import**: Restore from a backup file (creates pre-import backup automatically)

### Application Settings (settings.json)

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `isEnabled` | bool | `true` | Master rule processing toggle |
| `showNotifications` | bool | `true` | Show toast notifications |
| `useLastActiveBrowser` | bool | `true` | Remember last used browser |
| `autoDetectM365Profile` | bool | `false` | Windows account detection |
| `lastSelectedPage` | string | `"Home"` | Remember last settings page |

---

## Data Storage & Backup

### Data Location

All data is stored in: `%APPDATA%\LinkRouter\`

```
%APPDATA%\LinkRouter\
├── settings.json           # Application preferences
├── rules.json              # Individual URL rules
├── urlgroups.json          # URL groups
├── urlgroupoverrides.json  # Group overrides
└── backups/                # Automatic backups
    ├── rules.json.20250207_183000.bak.json
    ├── urlgroups.json.20250207_175000.bak.json
    └── pre-import_2025-02-07_120000.json
```

### Automatic Backups

LinkRouter creates automatic backups:

| Type | When | Location |
|------|------|----------|
| **Rolling Backups** | Every save | `backups/{file}.{timestamp}.bak.json` |
| **Pre-Import Backups** | Before import | `backups/pre-import_{timestamp}.json` |
| **Pre-Migration Backups** | Before schema changes | `backups/pre-migration_{name}_{timestamp}.json` |

**Retention**: Last 5 backups per file are kept (older ones auto-deleted)

### Atomic Saves

File operations use atomic writes:
1. Write to temporary file
2. Create timestamped backup of existing file
3. Atomically replace main file
4. Clean up old backups

### Data Recovery

If data files are corrupted, LinkRouter attempts recovery:
1. Try loading main file
2. Fall back to legacy `.bak` file
3. Try timestamped backups (newest first)
4. Show user notification if recovery fails

### Export Format

```json
{
  "exportVersion": 1,
  "appVersion": "1.0.0",
  "exportDate": "2025-02-07T19:00:00Z",
  "machineName": "DESKTOP-ABC123",
  "settings": { ... },
  "rules": [ ... ],
  "urlGroups": [ ... ],
  "urlGroupOverrides": [ ... ]
}
```

---

## Windows Integration

### Browser Registration

LinkRouter registers in Windows as a browser:

```
Registry Keys:
├── HKCU\Software\Clients\StartMenuInternet\LinkRouter
├── HKCU\Software\LinkRouter\Capabilities
│   ├── URLAssociations\http = LinkRouter
│   ├── URLAssociations\https = LinkRouter
│   └── FileAssociations\.htm, .html = LinkRouter
├── HKCU\Software\RegisteredApplications\LinkRouter
└── HKCU\Software\Classes\LinkRouter (URL Protocol handler)
```

### Setting as Default Browser

1. LinkRouter registers itself in Windows
2. Go to **Windows Settings** > **Apps** > **Default Apps**
3. Find **LinkRouter** in the list
4. Set as default for HTTP and HTTPS protocols

### Startup with Windows

When "Register as browser handler" is selected during installation:
- Adds entry to `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- Starts silently with `--startup` argument
- No window shown on startup

### Windows Account Detection

LinkRouter can detect Azure AD / Office 365 accounts:

**Detection Sources (Priority Order):**
1. Azure AD Join (domain-joined devices)
2. Workplace Join (non-domain-joined)
3. Office 365 Identity Cache

**Use Case**: Auto-match Windows account email domain with browser profile emails for Microsoft 365 URLs.

---

## Validation System

### Real-time Pattern Validation

When creating/editing rules, patterns are validated in real-time:

#### Errors (Block Save)

| Code | Message |
|------|---------|
| `PATTERN_EMPTY` | Pattern is required |
| `PATTERN_INVALID_FORMAT` | Invalid pattern format |
| `PATTERN_CONSECUTIVE_DOTS` | Pattern contains ".." |
| `PATTERN_UNSUPPORTED_WILDCARD` | Wildcards (*) not supported |
| `PATTERN_UNSUPPORTED_SCHEME` | Only http/https supported |
| `PATTERN_EXACT_DUPLICATE` | Pattern already exists |
| `PATTERN_SAME_DOMAIN_PROFILE` | Same domain + profile exists |

#### Warnings (Confirm to Proceed)

| Code | Message |
|------|---------|
| `PATTERN_CONFLICT_DIFFERENT_PROFILE` | Same domain with different profile |
| `PATTERN_PARENT_DOMAIN_EXISTS` | Parent domain rule exists |
| `PATTERN_SUBDOMAIN_EXISTS` | Subdomain rule exists |
| `PATTERN_EXISTS_IN_GROUP` | Pattern exists in a URL group |

### Group Validation

| Code | Message |
|------|---------|
| `GROUP_NAME_EMPTY` | Group name is required |
| `GROUP_NAME_DUPLICATE` | Group name already exists |
| `GROUP_PATTERNS_EMPTY` | At least one pattern required |
| `GROUP_PATTERN_EXISTS_AS_RULE` | Pattern exists as individual rule |
| `GROUP_PATTERN_OVERLAP` | Pattern overlaps with another group |

### Profile Validation

| Code | Message |
|------|---------|
| `PROFILE_REQUIRED` | At least one profile required |
| `BROWSER_NOT_FOUND` | Browser executable not found |
| `BROWSER_PATH_INVALID` | Browser path doesn't exist |

---

## Keyboard & UI Interactions

### Main Window (Browser Picker)

- **Drag**: Click and drag title bar to move window
- **ESC**: Close window (cancel)
- **Enter**: Open URL with selected browser

### Settings Window

- **Navigation**: Click tabs to switch pages (Home, Rules, Settings, Docs)
- **Search**: Type in search box to filter rules/groups
- **Clear Search**: Click X button in search box

### Profile Picker

- **Click**: Select profile and open URL
- **Cancel**: Close picker without action

---

## Command-Line Interface

### Arguments

| Argument | Description |
|----------|-------------|
| `<url>` | Process URL through routing rules |
| `--startup` | Silent launch (for Windows startup) |
| `--manage` | Open Settings window |
| `--register` | Register as browser and open Settings |
| `--unregister` | Unregister from Windows |

### Examples

```bash
# Open URL (normal usage when set as default browser)
LinkRouter.exe "https://github.com/user/repo"

# Open Settings window
LinkRouter.exe --manage

# Register and open Settings
LinkRouter.exe --register

# Silent startup (for Windows Run registry)
LinkRouter.exe --startup

# Unregister from Windows
LinkRouter.exe --unregister
```

---

## Technical Architecture

### Application Structure

```
┌─────────────────────────────────────────────────────────────┐
│                        App.xaml.cs                          │
│                    (Entry Point & Routing)                  │
└─────────────────────┬───────────────────────────────────────┘
                      │
        ┌─────────────┼─────────────┐
        ▼             ▼             ▼
┌───────────────┐ ┌──────────────┐ ┌───────────────────┐
│  MainWindow   │ │SettingsWindow│ │ ProfilePicker     │
│ (URL Picker)  │ │ (Management) │ │ (Multi-Profile)   │
└───────────────┘ └──────────────┘ └───────────────────┘
        │             │
        └──────┬──────┘
               ▼
┌─────────────────────────────────────────────────────────────┐
│                     Service Layer                           │
├─────────────────┬─────────────────┬─────────────────────────┤
│ UrlRuleManager  │ UrlGroupManager │ SettingsManager         │
│ (Rules CRUD)    │ (Groups CRUD)   │ (Preferences)           │
├─────────────────┴─────────────────┴─────────────────────────┤
│                   ValidationService                         │
│              (Pattern & Conflict Validation)                │
├─────────────────────────────────────────────────────────────┤
│                   JsonStorageService                        │
│           (Atomic Saves, Backups, Recovery)                 │
└─────────────────────────────────────────────────────────────┘
```

### Key Components

| Component | Purpose |
|-----------|---------|
| `App.xaml.cs` | Entry point, argument processing, auto-registration |
| `MainWindow` | Browser picker for manual URL selection |
| `SettingsWindow` | Main settings UI with tabbed navigation |
| `UrlRuleManager` | CRUD operations for individual rules |
| `UrlGroupManager` | CRUD operations for URL groups |
| `SettingsManager` | Application settings persistence |
| `ValidationService` | Pattern validation and conflict detection |
| `JsonStorageService` | Atomic file operations with backup |
| `BrowserDetector` | Browser and profile detection |
| `RegistryHelper` | Windows registry operations |
| `Logger` | Application logging |

### Data Flow

```
URL Received → UrlRuleManager.FindMatch()
                    │
    ┌───────────────┼───────────────┐
    ▼               ▼               ▼
 Check          Check           Check
Overrides       Rules          Groups
    │               │               │
    └───────────────┼───────────────┘
                    ▼
              MatchResult
                    │
    ┌───────────────┼───────────────┐
    ▼               ▼               ▼
 Single         Multiple         No
Profile         Profiles        Match
    │               │               │
    ▼               ▼               ▼
Auto-Open      Show Picker    Use Default
```

---

## Browser Support

### Supported Browsers

| Browser | Detection Method | Profile Support |
|---------|------------------|-----------------|
| **Google Chrome** | Registry + File System | Full (Preferences JSON) |
| **Microsoft Edge** | Registry + File System | Full (Preferences JSON) |
| **Mozilla Firefox** | Registry + File System | Full (profiles.ini) |
| **Brave Browser** | File System | Full (Preferences JSON) |
| **Opera** | Registry + File System | Full (Preferences JSON) |
| **Opera GX** | File System | Full (Preferences JSON) |

### Profile Detection Paths

| Browser | Profile Location |
|---------|------------------|
| Chrome | `%LOCALAPPDATA%\Google\Chrome\User Data\*\Preferences` |
| Edge | `%LOCALAPPDATA%\Microsoft\Edge\User Data\*\Preferences` |
| Firefox | `%APPDATA%\Mozilla\Firefox\Profiles\*` |
| Brave | `%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\*\Preferences` |
| Opera | `%APPDATA%\Opera Software\Opera Stable\*\Preferences` |
| Opera GX | `%LOCALAPPDATA%\Opera Software\Opera GX Stable\*\Preferences` |

### Profile Information Extracted

- **Profile Name** - From folder name or Preferences JSON
- **Account Email** - From `account_info[0].email` in Preferences
- **Email Domain** - Extracted from email for matching
- **Profile Path** - Full path for launch arguments
- **Launch Arguments** - Browser-specific profile flags

### Browser Colors (UI)

| Browser | Color |
|---------|-------|
| Google Chrome | `#4285F4` (Blue) |
| Microsoft Edge | `#0078D7` (Blue) |
| Mozilla Firefox | `#FF7139` (Orange) |
| Brave Browser | `#FB542B` (Orange) |
| Opera | `#FF1B2D` (Red) |
| Opera GX | `#FF006C` (Pink) |

---

## Edge Cases & Error Handling

### Browser Not Found

| Scenario | Handling |
|----------|----------|
| Rule's browser uninstalled | Falls back to manual selection |
| Profile path invalid | Warning only (still tries to launch) |
| Browser launch fails | Shows error message with option to select different browser |

### Data Corruption

| Scenario | Handling |
|----------|----------|
| Main file corrupted | Attempts recovery from backups |
| All backups corrupted | Shows notification, starts fresh |
| Fresh install (no files) | Creates default settings, no warning |

### URL Parsing

| Scenario | Handling |
|----------|----------|
| Invalid URL format | Uses full string for pattern matching |
| Empty URL | Shows manual picker |
| Malformed domain | Falls back to substring matching |

### Duplicate Profile Names

| Scenario | Handling |
|----------|----------|
| Same name in one browser | Appends directory name: "Default (Profile 3)" |
| Same name across browsers | Handled by browser + profile combination |

### Multi-Profile Edge Cases

| Profiles | Behavior |
|----------|----------|
| 0 profiles | Falls back to manual selection |
| 1 profile | Auto-opens automatically |
| 2+ profiles | Shows picker |
| Picker cancelled | Closes app without opening browser |

### Validation Edge Cases

| Pattern | Result |
|---------|--------|
| Empty string | Error: Pattern required |
| `*.example.com` | Error: Wildcards not supported |
| `ftp://example.com` | Error: Only http/https |
| `example..com` | Error: Consecutive dots |
| Duplicate pattern | Error: Already exists |

---

## Troubleshooting

### Common Issues

#### LinkRouter doesn't appear in Default Apps

1. Run `LinkRouter.exe --register`
2. Restart Windows Explorer or sign out/in
3. Check Windows Settings > Apps > Default Apps

#### URLs not being intercepted

1. Verify LinkRouter is set as default browser in Windows Settings
2. Check that Rules Processing is enabled (Settings page)
3. Verify the rule pattern matches the URL

#### Browser profiles not detected

1. Close the browser completely
2. Restart LinkRouter
3. Check that browser profile folders exist

#### Rules not matching

1. Check pattern format (no wildcards, lowercase)
2. Verify rule is enabled
3. Check priority order (overrides > rules > groups)

#### Data not saving

1. Check write permissions to `%APPDATA%\LinkRouter\`
2. Verify antivirus isn't blocking file writes
3. Check available disk space

### Logging

Logs are written to: `D:\LinkRouter\log.txt`

Log format:
```
2025-02-07 19:00:00.000 | Application starting with arguments: https://github.com
2025-02-07 19:00:00.050 | Found matching rule: github.com -> Chrome (Development)
2025-02-07 19:00:00.100 | Opening URL in Chrome with profile arguments
```

### Reset to Defaults

1. Close LinkRouter
2. Delete folder: `%APPDATA%\LinkRouter\`
3. Run `LinkRouter.exe --unregister`
4. Restart and run `LinkRouter.exe --register`

---

## Project Structure

```
LinkRouter/
├── App.xaml(.cs)                           # Application entry point
├── MainWindow.xaml(.cs)                    # Browser/profile picker UI
├── SettingsWindow.xaml(.cs)                # Main settings window
│
├── Pages/                                  # Settings window pages
│   ├── HomePage.xaml(.cs)                  # Dashboard with stats
│   ├── RulesPage.xaml(.cs)                 # Rules & groups management
│   ├── SettingsPage.xaml(.cs)              # Configuration options
│   └── DocsPage.xaml(.cs)                  # Documentation (Dev mode)
│
├── Windows/                                # Dialog windows
│   ├── AddRuleWindow.xaml(.cs)             # Add/edit rule dialog
│   ├── AddProfileDialog.xaml(.cs)          # Add profile to rule
│   ├── EditUrlGroupWindow.xaml(.cs)        # URL group editor
│   ├── RuleProfilePickerWindow.xaml(.cs)   # Multi-profile picker
│   ├── ValidationWarningDialog.xaml(.cs)   # Validation warnings
│   └── TestAutomationWindow.xaml(.cs)      # Test results (Dev)
│
├── Controls/                               # Custom WPF controls
│   ├── ValidationMessagePanel.xaml(.cs)    # Validation display
│   ├── BrowserProfileMultiSelector.xaml(.cs) # Profile selector
│   └── SettingsCard.xaml(.cs)              # Reusable settings card
│
├── Models/                                 # Data models
│   ├── UrlRule.cs                          # Rule model & matching
│   ├── UrlGroup.cs                         # Group model
│   ├── ValidationResult.cs                 # Validation models
│   ├── WindowsAccountInfo.cs               # Windows account model
│   └── ProfileMatchResult.cs               # Match confidence model
│
├── Services/                               # Business logic
│   ├── UrlRuleManager.cs                   # Rule CRUD & matching
│   ├── UrlGroupManager.cs                  # Group CRUD & matching
│   ├── SettingsManager.cs                  # Settings persistence
│   ├── ValidationService.cs                # Pattern validation
│   ├── JsonStorageService.cs               # Atomic file operations
│   ├── DataExportService.cs                # Import/export
│   ├── BrowserDetector.cs                  # Browser detection
│   ├── BrowserService.cs                   # Browser colors
│   ├── BrowserIconService.cs               # Browser icons
│   ├── WindowsAccountService.cs            # Windows account detection
│   ├── RegistryHelper.cs                   # Registry operations
│   ├── DefaultBrowserManager.cs            # Default browser handling
│   ├── NotificationHelper.cs               # Toast notifications
│   └── Logger.cs                           # Application logging
│
├── Themes/                                 # UI styling
│   ├── Windows11Theme.xaml                 # Main theme resources
│   ├── Colors.xaml                         # Color palette
│   └── Generic.xaml                        # Global implicit styles
│
├── Installer/                              # Inno Setup installer
│   └── LinkRouterSetup.iss                 # Installer script
│
├── BrowserSelector.csproj                  # Project configuration
├── BrowserSelector.sln                     # Solution file
├── app.ico                                 # Application icon
└── README.md                               # This file
```

---

## Technologies Used

### Framework & Runtime

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 8.0 | Target framework |
| **WPF** | (Windows-only) | User interface |
| **C#** | 12 | Programming language |

### Libraries

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Win32.Registry` | 5.0.0 | Registry operations |
| `System.Drawing.Common` | 8.0.0 | Icon extraction |
| `System.Text.Json` | (built-in) | JSON serialization |

### Build Configuration

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net8.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <IncludeAllContentForSelfExtract>true</IncludeAllContentForSelfExtract>
</PropertyGroup>
```

### Design Patterns

| Pattern | Usage |
|---------|-------|
| **Manager Pattern** | Static classes for CRUD operations |
| **Service Pattern** | Business logic encapsulation |
| **MVVM (Light)** | Data binding in WPF |
| **Atomic Operations** | Safe file writes with rollback |
| **Cascading Fallback** | Data recovery from backups |

---

## Contributing

Contributions are welcome! Please follow these guidelines:

### Getting Started

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make your changes
4. Run the application and test thoroughly
5. Commit your changes: `git commit -m "Add my feature"`
6. Push to the branch: `git push origin feature/my-feature`
7. Open a Pull Request

### Code Style

- Follow existing code conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and small

### Testing

- Test all new features manually
- Verify existing functionality still works
- Test edge cases and error scenarios

---

## License

MIT License - See [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- Windows 11 Fluent Design System for UI inspiration
- Inno Setup for installer creation
- All contributors and users

---

**LinkRouter** - Smart URL routing for Windows
