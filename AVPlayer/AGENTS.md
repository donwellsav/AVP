# AVPlayer Architecture & Development Guidelines

This document outlines the architectural decisions and coding standards for the AVPlayer project.

## 1. Technical Stack

*   **Framework:** .NET 9 (C# 13)
*   **UI Framework:** WPF (Windows Presentation Foundation)
*   **Architecture Pattern:** MVVM (Model-View-ViewModel) using `CommunityToolkit.Mvvm`.
*   **Dependency Injection:** `Microsoft.Extensions.Hosting` (Generic Host).
*   **UI Library:** `WPF-UI` (lepo.co) for Windows 11 Fluent Design.
*   **Logging:** `Serilog` (File sink).

## 2. Media Engine Strategy

*   **Library:** `LibVLCSharp` (LGPL).
    *   **Reasoning:** Robust, cross-platform capable core (LibVLC), extensive format support, and hardware acceleration.
*   **Abstraction:** All media operations must be performed through `IMediaPlayerService` to decouple the UI from the specific media engine.

## 3. Transparency & Hardware Acceleration (The "Airspace" Solution)

The application requires a **transparent Output Window** for lower-third overlays (Alpha Channel video) while maintaining **hardware acceleration**.

*   **Problem:**
    *   Standard WPF `AllowsTransparency="True"` forces software rendering for the entire window, killing performance for 4K playback.
    *   Hosting a child HWND (e.g., `VideoView` in LibVLCSharp) creates an "Airspace" issue where the video is always on top of WPF content, and transparency/alpha blending with WPF controls is impossible.

*   **Solution: Direct3D 11 Interop (`D3DImage`)**
    *   We will use a **D3D11-based rendering pipeline**.
    *   **Mechanism:**
        1.  LibVLC (or the media engine) decodes video frames to a Direct3D 11 texture (using `libvlc_video_set_output_callbacks` or shared handle).
        2.  We implement a custom WPF Control that uses `D3DImage`.
        3.  The D3D11 texture from the media engine is shared with the `D3DImage` back buffer.
        4.  WPF composites the `D3DImage` (which contains the video with alpha) with the rest of the WPF visual tree.
    *   **Benefit:** This allows full hardware acceleration for video decoding and rendering *within* the WPF composition graph, enabling per-pixel alpha blending of the video over other WPF elements or the desktop (if the window uses valid transparency methods that don't force software rendering, or simpler: if the window is TopMost and the video *is* the content).

## 4. Deployment

*   **Strategy:** **Single-File Self-Contained**.
    *   **Profile:** `win-x64`.
    *   **Settings:**
        *   `PublishSingleFile=true`
        *   `SelfContained=true`
        *   `IncludeNativeLibrariesForSelfExtract=true`
        *   `PublishTrimmed=false` (To avoid reflection issues with WPF/DI)

## 5. Project Structure

*   `ViewModels/`: ViewModel classes (ObservableObject).
*   `Views/`: WPF Windows and UserControls.
*   `Services/`: Logic and external system wrappers (IMediaPlayerService).
*   `Models/`: Data objects.
*   `Helpers/`: Utility classes.

## 6. Coding Standards

*   **Async/Await:** Use `async` / `await` for I/O bound operations. Avoid `void` async methods except for event handlers.
*   **Thread Safety:** UI updates MUST be marshaled to the UI thread using `Application.Current.Dispatcher` or `MainThread` helpers. Media events (Timecode, VU) fire on background threads.
*   **Nullability:** Enable Nullable Reference Types (`<Nullable>enable</Nullable>`).
