# AVP - High-Performance Windows 11 AV Show Control

AVP is a high-performance audio/video playback and show control application built for Windows 11 using .NET 9 and WPF. It leverages the power of LibVLC for robust media handling and follows modern architectural patterns for maintainability and scalability.

## Architecture

The project follows a strict MVVM (Model-ViewModel-View) architecture:

- **MVVM Framework**: [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) for observable objects and commands.
- **Dependency Injection**: [Microsoft.Extensions.Hosting](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host) for bootstrapping the application, managing services, and DI.
- **Logging**: [Serilog](https://serilog.net/) for structured logging, configured via `SerilogFactory`.
- **Media Engine**: [LibVLCSharp](https://github.com/videolan/libvlcsharp) (VLC) for hardware-accelerated media playback across various formats.
- **UI Framework**: Windows Presentation Foundation (WPF) targeting .NET 9.

## Getting Started

### Prerequisites

- **Visual Studio 2022** (17.12 or newer recommended)
- **.NET 9 SDK**
- Windows 10/11 (Windows 11 recommended for the best experience)

### Installation

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/donwellsav/AVP.git
    cd AVP
    ```
2.  **Restore dependencies**:
    ```bash
    dotnet restore
    ```
3.  **Build the project**:
    ```bash
    dotnet build
    ```

### Usage

1.  **Launch the application**: Run `AVP.exe` from the build output directory or via Visual Studio.
2.  **Select Media**: Enter the full path to a media file (video or audio) in the input text box.
3.  **Playback Control**:
    -   Click **Play** to start or resume playback.
    -   Click **Pause** to pause the current playback.
    -   Click **Stop** to end playback and reset the position.

## Build & Deployment

This project uses **GitHub Actions** for Continuous Integration (CI). Every time code is pushed to the `main` branch or a Pull Request is opened, a workflow automatically builds the application.

### How to get the Build

1.  Go to the **Actions** tab in this GitHub repository.
2.  Click on the latest workflow run (e.g., "Build WPF App").
3.  Scroll down to the **Artifacts** section.
4.  Download the `AVP-Windows-Release` zip file.
5.  Extract and run `AVP.exe`.

## Render Configuration

**Important Note:** This is a Windows Desktop Application (WPF). It cannot run on Render, which is a platform for Linux-based web services and static sites.

If you have set up a Render service for this repository, it will not function as expected. We recommend using GitHub Actions for distribution.

## Roadmap

Feature tracking and future plans are managed via GitHub Issues. Key upcoming areas include:
- OSC (Open Sound Control) Support
- Multiple Video Windows
- Enhanced Media Controls

See the [Issues](https://github.com/donwellsav/AVP/issues) tab for more details.
