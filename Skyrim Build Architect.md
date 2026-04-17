# Skyrim Build Architect ⚔️

**Skyrim Build Architect** is a desktop application developed to help players plan and optimize their character builds for *The Elder Scrolls V: Skyrim*. It allows you to simulate skill progression and calculate character stats before jumping into the game.

## 🚀 Main Features

* **Perk & Skill Planning:** Includes logic for all 18 skill trees to plan your character's path.
* **Live Stat Updates:** Automatically calculates Health, Magicka, and Stamina as you "level up" in the app.
* **Combat Calculations:** Provides estimates for armor rating and damage based on selected perks.
* **Modern UI:** A clean interface built with WPF (Windows Presentation Foundation) for a smooth user experience.
* **Standalone Design:** The app is self-contained and doesn't need external databases or complex installations.

## 🛠️ Tech Stack

| Component | Specification |
| :--- | :--- |
| **Language** | C# |
| **Runtime** | .NET 10.0 |
| **UI Framework** | WPF (XAML) |
| **IDE** | Visual Studio 2026 |

## 📁 Project Structure (File Overview)

To make the code easier to navigate, here is how it's organized:

* **`App.xaml / .cs`**: The main entry point that starts the application.
* **`MainWindow.xaml`**: Defines the visual layout and design of the window.
* **`MainWindow.xaml.cs`**: Handles the user interaction and UI logic.
* **`MainWindow.Data.cs`**: A dedicated file containing the data for all Skyrim perks and skills.
* **`Models.cs`**: Defines the basic data structures for the application.
* **`SkyrimCalculator.cs`**: Contains the mathematical formulas used for the build calculations.

## 👤 About the Project

Developed by **Syvric**. This tool was created out of a passion for Skyrim and as a project to learn modern C# development. It aims to provide a simple but effective way for the community to experiment with new character ideas.
