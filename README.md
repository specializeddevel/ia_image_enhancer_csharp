# Image Enhancer AI (.NET Edition)

![.NET](https://img.shields.io/badge/.NET-9-blue.svg) ![Platforms](https://img.shields.io/badge/platform-windows%20%7C%20macos%20%7C%20linux-lightgrey.svg) ![License](https://img.shields.io/badge/license-MIT-green.svg)

A cross-platform desktop application and web API for enhancing images using Real-ESRGAN AI models and converting them to modern, efficient formats like WebP and AVIF.

## Table of Contents

- [Features](#features)
- [Solution Structure](#solution-structure)
- [How it Works](#how-it-works)
- [API Endpoints](#api-endpoints)
- [Screenshots](#screenshots)
- [Supported Platforms](#supported-platforms)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Technologies Used](#technologies-used)
- [Contributing](#contributing)
- [License](#license)
- [Disclaimer](#disclaimer)

## Features

-   **AI-Powered Upscaling:** Enhance image resolution by 4x using state-of-the-art Real-ESRGAN models.
-   **Multiple AI Models:** Choose from a selection of pre-configured Real-ESRGAN models for different types of images (e.g., general photos, anime).
-   **Modern Format Conversion:** Convert your images to high-efficiency formats like `.webp` and `.avif` to save disk space.
-   **Batch Processing:** Process entire folders of images, including subfolders, in one go.
-   **Cross-Platform:** Runs on Windows, macOS, and Linux.
-   **User-Friendly GUI:** A simple and intuitive graphical interface built with Avalonia UI.
-   **Web API:** A headless web API for programmatic access to the image processing functionality.
-   **Dark & Light Themes:** Switch between themes to match your preference.
-   **Optional Source File Deletion:** Automatically delete original files after processing to save space.
-   **Processing Log:** Keep track of all processed images, including details like original and processed sizes, and space savings.

## Processing Log

The application maintains a detailed log of all image processing operations. This log is stored in a plain text file named `processing_log.txt` located in your local application data folder (e.g., `%LOCALAPPDATA%\ImageProcessor\`).

### Log Details

Each entry in the log file records comprehensive information about a processed image:

-   **Date and Time:** When the processing occurred.
-   **Input File Path:** The full path to the original image file.
-   **Output File Path:** The full path to the processed image file.
-   **Original Size:** The size of the original image in bytes.
-   **Processed Size:** The size of the processed image in bytes.
-   **Input Folder:** The directory containing the original image.
-   **Output Folder:** The directory where the processed image was saved.
-   **Original File Name:** The name of the original image file.
-   **Processed File Name:** The name of the processed image file.
-   **Reduction Percentage:** The percentage of file size reduction achieved.

### Viewing the Log

You can view the processing log directly from the application's UI:

1.  Click the "View Log" button in the main application window.
2.  A new window will open, displaying the log entries grouped by date.
3.  Each daily entry shows the total original size, total processed size, and total reduction percentage for that day.
4.  Individual log entries within each day provide detailed information about each processed file.
5.  The log view includes a horizontal scrollbar to accommodate long file paths and ensure all data is visible.
6.  Columns and headers are aligned for improved readability, using a monospaced font.

### Exporting the Log

The log data can be exported to a CSV (Comma Separated Values) file for external analysis:

1.  In the log view window, click the "Export to CSV" button.
2.  Choose a location to save the CSV file.
3.  The exported CSV file will contain all the detailed log information, with values separated by semicolons (`;`).

## Space Saving Display

The main application window now provides real-time feedback on the efficiency of your image processing:

-   **Total Space Saved:** Displays the overall percentage of space saved across all processed images.
-   **Total Space Saved (MB):** Shows the total amount of disk space saved in megabytes, providing a clear, quantifiable measure of efficiency.

## Solution Structure


The solution is divided into three projects:

-   **`ImageProcessor.Core`:** A .NET library project that contains the core logic for image processing. It uses the `realesrgan-ncnn-vulkan.exe` and `cwebp.exe` command-line tools to perform image enhancement and conversion.
-   **`ImageProcessor.Api`:** An ASP.NET Core web API that exposes the image processing functionality as a web service. It provides an endpoint to upload an image, process it, and download the enhanced result.
-   **`ImageProcessor.UI`:** A desktop application built with Avalonia UI that provides a graphical user interface for the image processing functionality.
## How it Works

The application uses a pipeline of command-line tools to process images:

1.  **Upscaling (Optional):** If enabled, the source image is first passed to `realesrgan-ncnn-vulkan` to be upscaled.
2.  **Conversion (Optional):** The (potentially upscaled) image is then converted to either `.webp` using `cwebp.exe` or `.avif` using `ffmpeg.exe`.
3.  **File Handling:** The application manages the creation of output folders and the deletion of intermediate and source files.


The API uses a job-based system to handle processing requests. When a request is received, a new job is created and added to a queue. The job is then processed in the background, and the client can poll the API to get the status of the job.

## API Endpoints

The API provides the following endpoints:

-   `POST /api/Processing/start`: Starts a new image processing job. The request body should be a JSON object with the processing options. The response will contain a `jobId` that can be used to track the status of the job.
-   `GET /api/Processing/{jobId}/status`: Gets the status of a processing job. The response will contain the job's status, the last update message, and the overall progress.
-   `GET /api/Processing/{jobId}/history`: Gets the complete progress history of a processing job.
## Screenshots

**Main Application Window:**

\[Insert Screenshot of the main window here]

**Processing View:**

\[Insert Screenshot of the application during processing here]

## Supported Platforms

-   **Windows** (x64)
-   **macOS** (x64 & Apple Silicon)
-   **Linux** (x64)

## Prerequisites

### 1. .NET 9 SDK

You need to have the .NET 9 SDK installed to build and run this project.

-   **Download:** [Download .NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### 2. External Binaries

This application relies on external command-line tools for its core functionality. You need to download them and place them in the correct directory.

**Download Links:**

-   **Real-ESRGAN (realesrgan-ncnn-vulkan):**
    -   **Repository:** [https://github.com/xinntao/Real-ESRGAN-ncnn-vulkan](https://github.com/xinntao/Real-ESRGAN-ncnn-vulkan)
    -   **Download:** Download the latest release for your platform from the [releases page](https://github.com/xinntao/Real-ESRGAN-ncnn-vulkan/releases).

-   **cwebp (WebP Converter):**
    -   **Repository:** [https://developers.google.com/webp]
    -   **Download:** Download the `libwebp` package for your platform from the [downloads repository](https://storage.googleapis.com/downloads.webmproject.org/releases/webp/index.html).

-   **FFmpeg (AVIF Converter):**
    -   **Website:** [https://ffmpeg.org/](https://ffmpeg.org/)
    -   **Download:** Download a release build for your platform from the [official download page](https://ffmpeg.org/download.html).

**Installation:**

After downloading, place the executables in the `ImageProcessor.UI/bin/Debug/net9.0/` directory for the UI and `ImageProcessor.Api/bin/Debug/net9.0/` for the API.

## Getting Started

1.  **Clone the repository:**

    ```bash
    git clone https://github.com/your-username/image_enhancer_csharp.git
    cd image_enhancer_csharp
    ```

2.  **Install Dependencies:**

    Follow the instructions in the [Prerequisites](#prerequisites) section to download and install the required external binaries.

3.  **Build the project:**

    ```bash
    dotnet build
    ```

4.  **Run the application:**

    -   **To run the Desktop UI:**

        ```bash
        dotnet run --project ImageProcessor.UI
        ```

    -   **To run the Web API:**

        ```bash
        dotnet run --project ImageProcessor.Api
        ```

## Technologies Used

-   **.NET 9 / C#:** The core application logic and UI are built with the latest version of .NET.
-   **Avalonia UI:** A cross-platform UI framework for creating the desktop application.
-   **ASP.NET Core:** Used to create the Web API for headless processing.
-   **MVVM Pattern:** The Model-View-ViewModel pattern is used to structure the UI code for maintainability and testability.

## Contributing

Contributions are welcome! If you have a feature request, bug report, or pull request, please feel free to open an issue or submit a PR.

## License

This project is licensed under the MIT License - see the `LICENSE.md` file for details.

## Disclaimer

This project uses external, pre-compiled binaries for image processing. These binaries are provided by their respective authors and are subject to their own licenses. Please ensure you comply with their terms of use.
