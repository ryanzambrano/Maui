# Amazon MAUI Application

A cross-platform application built with .NET MAUI that appears to be an Amazon-like shopping application with inventory management capabilities.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (17.8 or later) with the following workloads:
  - Mobile development with .NET
  - Universal Windows Platform development (for Windows builds)
- For Mac development:
  - [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/) 
  - Xcode (latest version)

## Getting Started

### Clone the Repository

```bash
git clone <repository-url>
cd Maui-App
```

### Build and Run the Application

#### Using Visual Studio 2022

1. Open the `Amazon.sln` solution file in Visual Studio 2022
2. Select your target platform in the dropdown menu
3. Click the "Run" button or press F5

#### Using Command Line

Build the application:

```bash
dotnet build
```

Run the application (specify your target platform):

```bash
dotnet run -f <target-framework>
```

Where `<target-framework>` is one of:
- `net9.0-windows10.0.19041.0` (Windows)
- `net9.0-android` (Android - requires connected device or emulator)
- `net9.0-ios` (iOS - requires Mac and connected device/simulator)
- `net9.0-maccatalyst` (macOS)

Example:
```bash
dotnet run -f net9.0-maccatalyst
```

## Project Structure

- `ViewModels/`: Contains MVVM view models
- `Views/`: Contains MAUI UI views
- `Models/`: Data models
- `Services/`: Service classes for data handling
- `Converters/`: Value converters
- `Resources/`: Application resources (images, fonts, etc.)
- `Platforms/`: Platform-specific code

## Dependencies

- CommunityToolkit.Mvvm (8.4.0)
- Microsoft.Maui.Controls
- Microsoft.Extensions.Logging.Debug (9.0.0)

## Features

- Shop functionality
- Inventory management
- Shopping cart
- Configuration settings

## Troubleshooting

If you encounter build issues:

1. Ensure you have the correct .NET SDK version installed
2. Try cleaning the solution: `dotnet clean`
3. Delete the `bin` and `obj` folders and rebuild
4. For platform-specific issues, check that you have the required platform SDKs installed