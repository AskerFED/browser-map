# Contributing to LinkRouter

Thank you for your interest in contributing to LinkRouter! This document provides guidelines and information for contributors.

## Getting Started

### Prerequisites

- Windows 10 (1607+) or Windows 11
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extensions
- (Optional) Inno Setup 6 for building the installer

### Setting Up the Development Environment

1. Fork and clone the repository:
   ```bash
   git clone https://github.com/YOUR-USERNAME/link-router.git
   cd link-router
   ```

2. Build the project:
   ```bash
   dotnet build BrowserSelector.csproj
   ```

3. Run the application:
   ```bash
   dotnet run --project BrowserSelector.csproj
   ```

## How to Contribute

### Reporting Bugs

1. Check if the bug has already been reported in [Issues](https://github.com/AskerFED/link-router/issues)
2. If not, create a new issue with:
   - Clear, descriptive title
   - Steps to reproduce
   - Expected vs actual behavior
   - Windows version and app version
   - Relevant log entries from `D:\BrowserSelector\log.txt`

### Suggesting Features

1. Check existing issues and discussions for similar suggestions
2. Create a new issue with the "feature request" label
3. Describe the feature and its use case

### Submitting Code

1. **Fork the repository** and create a feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following the code style guidelines below

3. **Test thoroughly**:
   - Test with multiple browsers (Chrome, Edge, Firefox)
   - Test URL routing rules
   - Test import/export functionality

4. **Commit your changes**:
   ```bash
   git commit -m "Add: brief description of your change"
   ```

5. **Push and create a Pull Request**:
   ```bash
   git push origin feature/your-feature-name
   ```

## Code Style Guidelines

### C# Conventions

- Use C# 12 features where appropriate
- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation for public APIs

### XAML Conventions

- Use consistent indentation (4 spaces)
- Reuse styles from `Themes/Generic.xaml` and `Themes/Windows11Theme.xaml`
- Use icon geometries from Window.Resources (see CLAUDE.md)

### Architecture

- **Models/** - Data classes
- **Services/** - Business logic and external integrations
- **Pages/** - UI pages for the main window
- **Controls/** - Reusable custom controls
- **Converters/** - XAML value converters

### Important Patterns

1. **Profile Matching**: Always match by `ProfilePath`, not `Name` (names can change)
2. **Data Persistence**: Use `JsonStorageService` with atomic saves
3. **Validation**: Use `ValidationService` for URL pattern validation
4. **Logging**: Use `Logger.Log()` for debugging

## Pull Request Process

1. Ensure your code builds without warnings
2. Update documentation if needed
3. Add a clear PR description explaining your changes
4. Link any related issues
5. Wait for review - maintainers will review and provide feedback

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Focus on the code, not the person
- Help others learn and grow

## Questions?

- Open a [Discussion](https://github.com/AskerFED/link-router/discussions) for questions
- Check existing issues for common problems

Thank you for contributing!
