# Changelog

All notable changes to LinkRouter will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-02-10

### Added

- **Intelligent URL Routing** - Route URLs to specific browsers and profiles based on domain patterns
- **Multi-Browser Support** - Works with Chrome, Edge, Firefox, Brave, Opera, and Opera GX
- **Profile Management** - Automatic detection of all browser profiles (personal, work, dev)
- **URL Groups** - Group related URL patterns with shared browser/profile settings
- **Built-in Templates** - Pre-configured groups for Microsoft 365 and Google Suite
- **Multi-Profile Rules** - Rules can offer multiple browser/profile choices via picker
- **Pattern Validation** - Real-time validation with conflict detection and warnings
- **Default Browser Fallback** - Set a fallback browser when no rules match
- **Toast Notifications** - Quick rule creation from notification when no rule matches
- **Clipboard Monitoring** - Monitor clipboard for URLs and route them
- **System Tray Integration** - Minimize to system tray with quick access menu
- **Single Instance** - Prevents multiple instances from running
- **Import/Export** - Full backup and restore functionality
- **Atomic Saves** - Safe file operations with automatic rolling backups
- **Windows Integration** - Registers as a Windows browser handler for http/https protocols
- **Command-line Options** - Support for `--manage`, `--register`, `--unregister`, `--startup`

### Browser Support

- Google Chrome (full profile support)
- Microsoft Edge (full profile support)
- Mozilla Firefox (full profile support)
- Brave Browser (full profile support)
- Opera (full profile support)
- Opera GX (full profile support)

---

## [Unreleased]

### Planned

- Dark mode support
- Keyboard shortcuts
- Rule import from browser extensions
