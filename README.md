# AVP - Audio Video Playback

A Windows Desktop Application for Audio/Video playback, built with .NET 9 and WPF.

## Build & Deployment

This project uses **GitHub Actions** for Continuous Integration (CI).
Every time code is pushed to the `main` branch or a Pull Request is opened, a workflow automatically builds the application.

### How to get the Build

1.  Go to the **Actions** tab in this GitHub repository.
2.  Click on the latest workflow run (e.g., "Build WPF App").
3.  Scroll down to the **Artifacts** section.
4.  Download the `AVP-Windows-Release` zip file.
5.  Extract and run `AVP.exe`.

## Render Configuration

**Important Note:** This is a Windows Desktop Application (WPF). It cannot run on Render, which is a platform for Linux-based web services and static sites.

If you have set up a Render service for this repository (e.g., as a Static Site), it will not function as expected for running the application.

**Recommendation:**
-   **Disable or Delete** the Render service for this repository to avoid confusion and unnecessary resource usage.
-   Use the **GitHub Actions** artifacts described above to distribute the application.
