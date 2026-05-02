# Backend Documentation

This document provides an overview of the modules and libraries used in the Mosaic backend.

## Modules

The backend is organized into several modules, each responsible for a specific domain of the application:

- **Auth**: Handles user authentication, token validation, and communication with Firebase Auth.
- **Music**: The core module for music-related functionality.
    - **Search**: Integrates with YouTube to provide music search results.
    - **Streaming**: Extracts audio stream URLs from YouTube videos.
    - **Tracks**: Manages track metadata and details.
- **Playlists**: Manages user-created playlists, including adding/removing tracks and playlist CRUD operations.
- **History**: Tracks the user's recently played music for quick access.
- **Users**: Manages user profile information and preferences.
- **Liked**: (Managed via Firestore) Handles the user's collection of liked songs.

## Infrastructure & Core Services

- **FirebaseService**: A singleton service that initializes the Firebase Admin SDK and provides access to Firestore.
- **apiClient**: (Shared) Standardized Axios configuration for internal and external requests.

## Libraries & Frameworks

- **ASP.NET Core (v10.0)**: The primary framework used for building the RESTful API.
- **YoutubeExplode (v6.6.0)**: Used for metadata extraction and finding high-quality audio streams from YouTube.
- **FirebaseAdmin (v3.5.0)**: The official SDK for interacting with Firebase services from a server-side environment.
- **Google.Cloud.Firestore (v4.2.0)**: Provides a high-level API for interacting with the Firestore database.
- **DotNetEnv (v3.2.0)**: Loads configuration from `.env` files.
- **JwtBearer (v10.0.7)**: Middleware for validating Firebase-issued JWT tokens.
- **EntityFrameworkCore**: Used for database modeling (currently utilizing In-Memory and SQL Server providers).
- **Newtonsoft.Json (v13.0.4)**: The standard library for JSON serialization and deserialization.
