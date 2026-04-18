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

To make the code easier to navigate for reviewers, here is how it's organized:

* **`App.xaml / .cs`**: The main entry point that starts the application.
* **`MainWindow.xaml`**: Defines the visual layout and design of the window.
* **`MainWindow.xaml.cs`**: Handles the user interaction and UI logic.
* **`MainWindow.Data.cs`**: A dedicated file containing the data for all Skyrim perks and skills.
* **`Models.cs`**: Defines the basic data structures for the application.
* **`SkyrimCalculator.cs`**: Contains the mathematical formulas used for the build calculations.

## 🛠️ How to Build from Source

To verify that the binary provided on Nexus Mods matches this source code, you can build the application yourself. This ensures full transparency and security.

### Prerequisites
* **.NET 10.0 SDK** (Ensure you have the latest version installed).
* **Visual Studio 2026** (with the ".NET Desktop Development" workload).

### Option 1: Command Line
1. Open a terminal (PowerShell or CMD) in the project's root folder.
2. Run the following command to create a production-ready build:
   ```bash
   dotnet build --configuration Release
   ```
3. Once finished, you will find the compiled executable in:  
   `\bin\Release\net10.0-windows\`

### Option 2: Visual Studio 2026
1. Open the solution file (`.sln`) in Visual Studio.
2. Select **Release** from the solution configurations dropdown menu at the top.
3. Go to the menu: **Build > Build Solution**.
4. The output will be generated in the same `\bin\Release\` folder mentioned above.

## 👤 About the Project

Developed by **Syvric**. This tool was created out of a passion for Skyrim and as a project to learn modern C# development. It aims to provide a simple but effective way for the community to experiment with new character ideas.

---
*Note: This tool is a standalone WPF application and does not require any external assets or database setups to compile.*
