# Bastra

Bastra is a cross-platform card game built with **.NET MAUI**. It implements the traditional "Bastra" (or "Basra") card game with real-time online multiplayer using Firebase.

## Features

- **Cross‑platform**: targets Android, iOS, macOS (Catalyst) and Windows.
- **Firebase integration**: user authentication, Firestore for realtime game state and data storage.
- **Multiplayer gameplay** with create/join lobby, waiting room and real‑time updates via snapshot listeners.
- **Bilingual instructions**: the instructions page can toggle between English and Hebrew.
- **Audio & Text‑to‑Speech**: sound effects and spoken game actions for a richer experience.
- **Local notifications**: Android implementation for reminders if a game is left mid‑session.
- **User profile**: statistics, win streak tracking and optional profile image stored locally.

## Project Structure

```
Bastra.sln              Solution file
Bastra/                 .NET MAUI project containing all source code
  Models/               Shared data models
  ModelsLogic/          Game logic, Firebase and utility classes
  ViewModels/           MVVM view models for pages and popups
  Views/                XAML pages and popups
  Utilities/            Helper utilities and constants
  Platforms/            Platform‑specific code (Android, iOS, Windows, macOS)
  Resources/            Images, fonts, sounds and styles
```

## Firebase Setup

Firebase is required for authentication and multiplayer. The project expects `google-services.json` under `Resources/Raw/` and `Platforms/Android/Resources/`. The repository only contains placeholder files. Replace them with your own Firebase configuration downloaded from the Firebase console.

## Building and Running

1. **Install the .NET 8 SDK** and .NET MAUI workloads. On Windows/Mac you can use Visual Studio 2022 or later with the MAUI workloads installed.
2. Restore packages and build:
   ```bash
   dotnet build Bastra.sln
   ```
3. Run on a specific platform (example for Android):
   ```bash
   dotnet build Bastra/Bastra.csproj -t:Run -f net8.0-android
   ```
   You can also open the solution in Visual Studio and deploy to your emulator or device.

## Gameplay Overview

- Launch the app and sign up or sign in with email/password.
- From the Home page you can create a new game or join an existing one.
- A waiting room shows connected players and starts the game when all players join.
- The game board supports selecting cards, throwing to the table and collecting according to Bastra rules. Jack cards trigger special animations.
- Scores are synced through Firebase. When a player wins or leaves, a win popup is displayed and statistics are updated.

## Statistics & Profile

The Profile page displays information stored in local preferences:
- Account creation date
- Games played and won
- Win streak and longest streak
- Optional profile picture

## Cleaning Old Game Documents

The Android project includes a background `DeleteOldDocsService` that periodically deletes Firestore game documents older than two days to keep the database tidy.

## Dependencies

Key NuGet packages used:
- `CommunityToolkit.Maui` and `CommunityToolkit.Mvvm`
- `Plugin.CloudFirestore` for Firestore access
- `FirebaseAuthentication.net` for authentication
- `Plugin.Maui.Audio` for playing sound effects

## Contributing

Contributions are welcome! Feel free to open issues or pull requests.

## License

This project is provided as-is under the repository's license.
