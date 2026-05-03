# Mosaic Backend

The high-performance ASP.NET Core API powering the Mosaic music experience.

## Overview
This backend handles authentication, music discovery via YouTube, and media streaming proxying. It is designed with a modular architecture to ensure scalability and ease of maintenance.

## Core Features
- **Firebase Authentication**: Secure user management and session handling.
- **Music Streaming Proxy**: Robust audio streaming with Range support for mobile devices.
- **YouTube Integration**: Real-time track search and metadata extraction.

## Technology Stack
- **Framework**: ASP.NET Core 10.0
- **Database/Auth**: Firebase (Firestore & Auth)
- **External API**: YoutubeExplode
- **Architecture**: Modular Monolith

## Getting Started
1. **Configure Environment**: Create a `.env` file with your Firebase and API settings.
2. **Restore Dependencies**: `dotnet restore`
3. **Run the API**: `dotnet run`

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
