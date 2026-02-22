# AVP - High-Performance Windows 11 AV Show Control

Welcome to the AVP project. This document provides an in-depth analysis of our roadmap, current capabilities, and technical architecture.

## Overview

AVP is a high-performance audio/video playback and show control application designed for live event environments. Built on the modern Windows 11 platform using .NET 9 and WPF, it aims to provide a robust, reliable, and controllable media server solution.

---

## Part 1: User & Stakeholder Roadmap Analysis

This section outlines the strategic vision for AVP, detailing where we are today and where we are heading. Our goal is to evolve from a single-channel media player into a comprehensive show control system.

### Phase 1: Foundation (Current Status)
**Focus:** Stability, Core Engine, and Basic Integration.

We have successfully established the core architecture required for high-performance media playback. The current version includes:
*   **Robust Media Engine:** Powered by **LibVLC**, ensuring compatibility with virtually all modern video and audio codecs (H.264, H.265, ProRes, MP4, MKV, MOV, WAV, FLAC).
*   **Hardware Acceleration:** Utilization of GPU resources for smooth playback of high-resolution content.
*   **Remote Control (OSC):** Implementation of Open Sound Control (OSC) allows AVP to be triggered by industry-standard consoles and software (e.g., QLab, Isadora, GrandMA, EOS).
    *   *Supported Commands:* `/avp/play`, `/avp/pause`, `/avp/stop`, `/avp/volume`, `/avp/seek`.
*   **Dedicated Video Output:** A separate `VideoWindow` for full-screen presentation, keeping the control interface distinct from the audience view.

### Phase 2: Expansion (Next Steps)
**Focus:** Flexibility and Advanced Control.

The immediate roadmap focuses on expanding the capabilities of the playback engine to support more complex show requirements:
*   **Multiple Video Outputs:** Support for multiple physical displays and projectors, allowing for independent routing of content to different screens.
*   **Playlist & Queue Management:** Implementation of a playlist system to sequence media files, rather than loading them one by one.
*   **Enhanced Transport Controls:** Precision seeking, frame-stepping, and loop controls for rigorous rehearsal and cueing workflows.
*   **Configuration Interface:** A dedicated settings UI to configure OSC ports, audio device routing, and display preferences without editing configuration files.

### Phase 3: Integration (Future Vision)
**Focus:** Full Show Control and Automation.

The long-term vision for AVP is to become a central node in a show control network:
*   **Timeline & Cues:** A visual timeline for programming complex sequences of media events.
*   **Advanced Networking:** Support for additional protocols (potentially Art-Net/sACN for DMX control) to synchronize lighting and video.
*   **Failover & Redundancy:** Features designed for mission-critical environments, such as main/backup synchronization.

---

## Part 2: Developer Technical Analysis

This section provides a technical deep-dive into the architecture and implementation details for contributors and developers.

### Architecture Overview
The project follows a strict **MVVM (Model-ViewModel-View)** pattern to ensure separation of concerns and testability.

*   **Framework:** .NET 9 (Windows)
*   **UI System:** Windows Presentation Foundation (WPF)
*   **Dependency Injection:** `Microsoft.Extensions.Hosting` is used for bootstrapping the application and managing service lifecycles.

### Key Components

#### 1. Media Engine (`IMediaPlayerService`)
*   **Implementation:** `LibVlcPlayerService` wraps the `LibVLCSharp` library.
*   **Rationale:** LibVLC provides a battle-tested, cross-platform media engine that supports a vast array of codecs and hardware acceleration methods, avoiding the limitations of the native WPF `MediaElement`.
*   **Threading:** Media loading and initialization are handled asynchronously (`Task.Run`) to prevent blocking the UI thread during heavy I/O operations.

#### 2. OSC Integration (`IOscService`)
*   **Implementation:** `OscService` utilizes the `CoreOSC` / `Rug.Osc` libraries for UDP communication.
*   **Functionality:** Listens on a configurable port (default: 8000) for incoming OSC bundles and messages.
*   **Thread Safety:** Incoming messages are dispatched to the UI thread or relevant service contexts to ensure thread-safe updates to the ViewModel and Player.

#### 3. Logging & Diagnostics
*   **Library:** `Serilog`
*   **Configuration:** Structured logging is configured via `SerilogFactory`. Logs are written to `%APPDATA%\AVP\Logs` for post-show analysis and debugging.

### Project Structure
*   **`AVP/Services/`**: Contains business logic and hardware integration (Player, OSC, Logging).
*   **`AVP/ViewModels/`**: Contains `MainViewModel` and other view models that bridge the UI and Services.
*   **`AVP/Views/`**: Contains XAML definitions for `MainWindow` (Control Surface) and `VideoWindow` (Output).

### Build Instructions

1.  **Prerequisites:**
    *   Visual Studio 2022 (v17.12+)
    *   .NET 9 SDK
    *   Windows 10/11

2.  **Build:**
    ```bash
    git clone https://github.com/donwellsav/AVP.git
    cd AVP
    dotnet restore
    dotnet build
    ```

3.  **Run:**
    Launch `AVP.exe` from the build output directory or via Visual Studio.

---

*This roadmap is a living document and will evolve as development progresses. Check the GitHub Issues for the most granular task tracking.*
