# Backend Documentation

This document provides an overview of the modules and libraries used in the Mosaic backend.

## Modules

The backend is organized into several modules, each responsible for a specific domain of the application:

- **Auth**: Handles user authentication, token generation, and communication with Firebase Auth. 
    - *Update*: Now uses a **Local JWT** system (Symmetric Security) to issue session tokens, ensuring reliable authentication while maintaining Firebase as the identity provider.
- **Users**: Manages user profile information, display names, and passwords.
    - *Update*: Integrated with **Cloudinary** for secure profile picture storage. Implemented automatic deletion of old assets when a new profile picture is uploaded.
- **Music**: The core module for music-related functionality.
    - **Search**: Integrates with YouTube to provide music search results.
    - **Streaming**: Extracts audio stream URLs from YouTube videos.
    - **Tracks**: Manages track metadata and details.
- **Playlists**: Manages user-created playlists, including adding/removing tracks and playlist CRUD operations.
- **History**: Tracks the user's recently played music for quick access.
- **Liked**: (Managed via Firestore) Handles the user's collection of liked songs.

## Infrastructure & Core Services

- **FirebaseService**: A singleton service that initializes the Firebase Admin SDK and provides access to Firestore and Auth.
- **CloudinaryService**: Manages media uploads, transformations (auto-cropping/resizing), and asset deletion.
- **EmailService**: Handles sending transactional emails such as verification codes and password reset links via SMTP.
- **apiClient**: (Shared) Standardized Axios configuration for internal and external requests.

## Libraries & Frameworks

- **ASP.NET Core (v10.0)**: The primary framework used for building the RESTful API.
- **CloudinaryDotNet**: The official SDK for managing media assets in Cloudinary.
- **YoutubeExplode (v6.6.0)**: Used for metadata extraction and finding high-quality audio streams from YouTube.
- **FirebaseAdmin (v3.5.0)**: The official SDK for interacting with Firebase services from a server-side environment.
- **Google.Cloud.Firestore (v4.2.0)**: Provides a high-level API for interacting with the Firestore database.
- **DotNetEnv (v3.2.0)**: Loads configuration from `.env` files.
- **JwtBearer (v10.0.7)**: Middleware for validating local JWT tokens using Symmetric Security Keys.
- **EntityFrameworkCore**: Used for database modeling (currently utilizing In-Memory and SQL Server providers).
- **Newtonsoft.Json / System.Text.Json**: Standard libraries for JSON serialization and deserialization.
