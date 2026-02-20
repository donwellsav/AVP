# Compilation & Deployment Instructions

## Prerequisites
*   .NET 9 SDK
*   Windows 10/11 x64

## Build for Development
```bash
dotnet build AVPlayer/AVPlayer.csproj
```

## Publish (Production)
To create a single-file, self-contained executable for Windows x64:

```bash
dotnet publish AVPlayer/AVPlayer.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:PublishTrimmed=false \
    -p:UseAppHost=true
```

The output will be located in `AVPlayer/bin/Release/net9.0-windows/win-x64/publish/`.

## Architecture Notes
*   **Media Engine:** LibVLCSharp (LGPL). Native LibVLC binaries are embedded.
*   **Video Output:** Custom `VideoViewD3D` control using `WriteableBitmap` (Phase 3 implementation) for compatibility. Future optimization to `D3DImage` is possible for higher performance 4K rendering.
*   **OSC:** Listens on UDP Port 8000. Commands: `/take`, `/stop`, `/panic`, `/load`.
*   **Logs:** Logs are stored in `logs/` directory next to the executable.

## Troubleshooting
*   **Black Screen:** Ensure the secondary monitor is connected. The Output Window defaults to the secondary display.
*   **Crash on Startup:** Check `logs/` for details. Common issues include missing VC++ Redistributables (though self-contained usually handles dependencies, some system DLLs might be needed).
