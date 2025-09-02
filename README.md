# Image Enhancer AI (.NET Edition)

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9-blue.svg" alt=".NET 9" />
  <img src="https://img.shields.io/badge/UI-Avalonia-purple" alt="Avalonia UI" />
  <img src="https://img.shields.io/badge/API-ASP.NET%20Core-blueviolet" alt="ASP.NET Core" />
  <img src="https://img.shields.io/badge/platform-windows%20%7C%20macos%20%7C%20linux-lightgrey.svg" alt="Platforms" />
  <a href="https://github.com/raulb/ia_image_enhancer_csharp/blob/main/LICENSE.md">
    <img src="https://img.shields.io/github/license/raulb/ia_image_enhancer_csharp" alt="License" />
  </a>
</p>
<p align="center">
  <a href="https://github.com/raulb/ia_image_enhancer_csharp/stargazers">
    <img src="https://img.shields.io/github/stars/raulb/ia_image_enhancer_csharp?style=social" alt="GitHub Stars" />
  </a>
  <a href="https://github.com/raulb/ia_image_enhancer_csharp/network/members">
    <img src="https://img.shields.io/github/forks/raulb/ia_image_enhancer_csharp?style=social" alt="GitHub Forks" />
  </a>
  <a href="https://github.com/raulb/ia_image_enhancer_csharp/issues">
    <img src="https://img.shields.io/github/issues/raulb/ia_image_enhancer_csharp" alt="GitHub Issues" />
  </a>
</p>

A cross-platform desktop application and web API for enhancing images using Real-ESRGAN AI models and converting them to modern, efficient formats like WebP and AVIF.

## Table of Contents

- [Features](#features)
- [Solution Structure](#solution-structure)
- [How it Works](#how-it-works)
- [API Endpoints](#api-endpoints)
- [Screenshots](#screenshots)
- [Processing Options Explained](#processing-options-explained)
- [Prerequisites and Downloads](#prerequisites-and-downloads)
- [Required Files](#required-files)
- [Getting Started](#getting-started)
- [Technologies Used](#technologies-used)
- [Contributing](#contributing)
- [License](#license)
- [Disclaimer](#disclaimer)

## Features

- **AI-Powered Upscaling:** Enhance image resolution by 4x using state-of-the-art Real-ESRGAN models.
- **Multiple AI Models:** Choose from a selection of pre-configured Real-ESRGAN models for different types of images (e.g., photos, anime).
- **Modern Format Conversion:** Convert your images to high-efficiency formats like `.webp` and `.avif` to save disk space.
- **Batch Processing:** Process entire folders of images, including subfolders, in one go.
- **Cross-Platform:** Runs on Windows, macOS, and Linux.
- **User-Friendly GUI:** A simple and intuitive graphical interface built with Avalonia UI.
- **Web API:** A headless web API for programmatic access to the image processing functionality.
- **Dark & Light Themes:** Switch between themes to match your preference.
- **Optional Source File Deletion:** Automatically delete original files after processing to save space.
- **Processing Log:** Keep track of all processed images, including details like original and processed sizes, and space savings.

## Solution Structure

The solution is divided into three projects:

- **`ImageProcessor.Core`:** A .NET library project that contains the core logic for image processing. It orchestrates the command-line tools to perform image enhancement and conversion.
- **`ImageProcessor.Api`:** An ASP.NET Core web API that exposes the image processing functionality as a web service.
- **`ImageProcessor.UI`:** A desktop application built with Avalonia UI that provides a graphical user interface.

## How it Works

The application uses a pipeline of command-line tools to process images:

1. **Upscaling (Optional):** If enabled, the source image is first passed to `realesrgan-ncnn-vulkan` to be upscaled.
2. **Conversion (Optional):** The (potentially upscaled) image is then converted to either `.webp` using `cwebp.exe` or `.avif` using `ffmpeg.exe`.
3. **File Handling:** The application manages the creation of output folders and the deletion of intermediate and source files.

The API uses a job-based system to handle processing requests asynchronously.

## API Endpoints

The API provides endpoints for starting and monitoring processing jobs.

### `POST /api/Processing/start`

Starts a new image processing job. The request body must be a JSON object with the processing options. The response will contain a `jobId` that can be used to track the status of the job.

**Example JSON Body:**

```json
{
  "inputFolder": "C:\\path\\to\\your\\images",
  "outputFolder": "C:\\path\\to\\output",
  "model": "realesrgan-x4plus",
  "processSubfolders": true,
  "convertToWebP": true,
  "convertToAvif": false,
  "applyUpscale": true,
  "deleteSourceFile": false,
  "includeWebPFiles": false,
  "includeAvifFiles": false
}
```

**Example `curl` command:**

```bash
curl -X POST "https://localhost:7131/api/Processing/start" -H "Content-Type: application/json" -d "{\"inputFolder\": \"C:\\\\path\\\\to\\\\your\\\\images\", \"outputFolder\": \"C:\\\\path\\\\to\\\\output\", \"model\": \"realesrgan-x4plus\", \"processSubfolders\": true, \"convertToWebP\": true, \"applyUpscale\": true}" --insecure
```

*Note: The `--insecure` flag is used here to bypass SSL certificate verification for the self-signed development certificate. On Windows Command Prompt, you may need to adjust the escaping of quotes and backslashes.*

**Example Response:**

```json
{
  "jobId": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
}
```

### `GET /api/Processing/{jobId}/status`

Gets the current status of a processing job.

**Example `curl` command:**

```bash
curl "https://localhost:7131/api/Processing/a1b2c3d4-e5f6-7890-1234-567890abcdef/status" --insecure
```

**Example Response:**

```json
{
  "id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "status": "Running",
  "lastUpdate": {
    "message": "Processing file 7 of 25...",
    "currentFile": "image07.jpg",
    "overallProgress": 0.28
  }
}
```

### `GET /api/Processing/{jobId}/history`

Gets the complete progress history of a processing job.

**Example `curl` command:**

```bash
curl "https://localhost:7131/api/Processing/a1b2c3d4-e5f6-7890-1234-567890abcdef/history" --insecure
```

**Example Response:**

```json
[
  {
    "message": "Found 25 images in 3 folders."
  },
  {
    "message": "Processing folder...",
    "currentFolderName": "Subfolder1"
  },
  {
    "message": "Processing file 1 of 25...",
    "currentFile": "image01.jpg"
  }
]
```

## Screenshots

![IMAGE](https://github.com/user-attachments/assets/65fad462-f0ec-4f5e-b2a6-9ca15cb5af77)

## Processing Options Explained

The main window provides several options to customize the image processing workflow.

| Option                | Description                                                                                                                            |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| **AI Model**          | Selects the Real-ESRGAN AI model to use for the upscaling process. Different models are trained for specific types of content (e.g., photos, anime). |
| **Apply Upscale**     | If checked, the AI-powered upscaling process is applied to the images. If unchecked, the application will only perform format conversion. |
| **Convert to WebP**   | If checked, the final image will be converted to the `.webp` format. This option is mutually exclusive with `Convert to Avif`.          |
| **Convert to Avif**   | If checked, the final image will be converted to the `.avif` format using `ffmpeg`. This option is mutually exclusive with `Convert to WebP`. |
| **Process Subfolders**| If checked, the application will search for images in all subdirectories of the selected input folder.                                  |
| **Delete Source File**| **(Use with caution!)** If checked, the original source image will be permanently deleted after it has been successfully processed.      |
| **Include WebP Files**| If checked, existing `.webp` files in the source folder will be included in the processing queue.                                      |
| **Include Avif Files**| If checked, existing `.avif` files in the source folder will be included in the processing queue.                                      |

## Prerequisites and Downloads
>
> 💡 **Note**  
> For now, all Microsoft Windows prerequisites are included in the release package, available inside the compressed file: [https://github.com/specializeddevel/ia_image_enhancer_csharp/releases]

To build and run this project from source, you will need the **.NET 9 SDK**.

- **Download:** [Download .NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Required Files

For the application to function correctly, you need to either build from source and acquire the files listed below, or use a pre-compiled version.

### 1. External Binaries

This application relies on external command-line tools. These must be placed in the main application directory.

- **Real-ESRGAN (realesrgan-ncnn-vulkan):**
  - **Repository:** [https://github.com/xinntao/Real-ESRGAN-ncnn-vulkan](https://github.com/xinntao/Real-ESRGAN-ncnn-vulkan)
  - **Download:** Download the latest release for your platform from the [releases page](https://github.com/xinntao/Real-ESRGAN-ncnn-vulkan/releases).

> ⚠️ **Important**  
> **GPU Requirement:** AI upscaling requires a graphics card that supports **Vulkan 1.1** or higher.

- **cwebp (WebP Converter):**
  - **Repository:** [https://developers.google.com/webp]
  - **Download:** Download the `libwebp` package for your platform from the [downloads repository](https://storage.googleapis.com/downloads.webmproject.org/releases/webp/index.html).

- **FFmpeg (AVIF Converter):**
  - **Website:** [https://ffmpeg.org/](https://ffmpeg.org/)
  - **Download:** Download a release build for your platform from the [official download page](https://ffmpeg.org/download.html).

> 💡 **Note**  
> **Linux/macOS Permissions:** On Linux and macOS, you must grant execution permissions to the binaries.
>
> ```bash
> chmod +x ./realesrgan-ncnn-vulkan
> chmod +x ./cwebp
> chmod +x ./ffmpeg
> ```

### 2. Models Folder

You must have a folder named `models` in the application's root directory. This folder must contain the `.bin` and `.param` files for the Real-ESRGAN models you intend to use.

### 3. Pre-compiled Application (Optional)

> 💡 **Note**  
> I will soon provide links to download ZIP files containing the compiled binaries for each platform (Windows, macOS, Linux), along with all the necessary dependencies.

- **Windows:** (Link will be here)
- **macOS:** (Link will be here)
- **Linux:** (Link will be here)

## Getting Started

1. **Clone the repository:**

    ```bash
    git clone https://github.com/your-username/image_enhancer_csharp.git
    cd image_enhancer_csharp
    ```

2. **Add Required Files:**
    Follow the instructions in the [Required Files](#required-files) section to download and place the necessary binaries and models.
3. **Build the project:**

    ```bash
    dotnet build
    ```

4. **Run the application:**
    - **To run the Desktop UI:**

        ```bash
        dotnet run --project ImageProcessor.UI
        ```

    - **To run the Web API:**

        ```bash
        dotnet run --project ImageProcessor.Api
        ```

## Technologies Used

- **.NET 9 / C#:** The core application logic and UI are built with the latest version of .NET.
- **Avalonia UI:** A cross-platform UI framework for creating the desktop application.
- **ASP.NET Core:** Used to create the Web API for headless processing.
- **MVVM Pattern:** The Model-View-ViewModel pattern is used to structure the UI code, powered by the **CommunityToolkit.Mvvm** library.
- **Swagger / OpenAPI:** Used for API documentation and testing in the `ImageProcessor.Api` project via the **Swashbuckle.AspNetCore** library.

## Contributing

Contributions are welcome! If you have a feature request, bug report, or pull request, please feel free to open an issue or submit a PR.

## License

This project is licensed under the MIT License - see the `LICENSE.md` file for details.

## Disclaimer

This project uses external, pre-compiled binaries for image processing. These binaries are provided by their respective authors and are subject to their own licenses. Please ensure you comply with their terms of use.
