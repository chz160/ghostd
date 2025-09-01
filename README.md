# Ghostd – Godot Project (v1)

Welcome to the core Godot project for **Ghostd**, a terminal-themed rogue AI battler for Android and iOS.

This repository contains the entire source code and assets for version 1 (MVP) of the game.

---

## 📱 Game Overview

**Ghostd** is a fast, strategic auto-battler set in a decaying digital world. Players design modular AIs using Core, Behavior, and Augment modules, then deploy them in stylized terminal battles against other players or wild AIs. Victory grants salvaged code fragments used to evolve and optimize builds.

* **Genre**: Auto-battler / Strategy with Online Multiplayer
* **Aesthetic**: Hacker, cyberpunk, terminal UI (no typing required)
* **Target Platforms**: Android & iOS (mobile only)
* **Backend**: Nakama server for multiplayer, leaderboards, and cloud saves

---

## 🔧 Project Structure

### Current Structure
* `/src` - **Godot project root**
  * `/Scenes`: `BootScene`, `MainTerminal`, `AIAssembly`, `ModuleViewer`
  * `/Scripts`: Game logic organized by functionality
    * `/Modules`: Core module system classes
    * `/UI`: Terminal UI components
  * `/Resources`: Module definitions, fonts, shaders, themes

* `/docs` - **Game documentation**
  * Game Design Document (GDD)
  * Gameplay flowchart
  * One-pager concept

### Backend Structure (Nakama)
* `/nakama` - **Server infrastructure**
  * `/docker-compose.yml`: Docker setup for Nakama, PostgreSQL, and Prometheus
  * `/modules`: TypeScript modules directory (ready for game logic)
  * `/data`: Server configuration and runtime data
* `/src/Scripts/Network` - Client-side networking (to be implemented)

---

## 🔌 Backend Requirements

**Ghostd** uses Nakama as its backend server for:
- **Real-time multiplayer battles**: Server-authoritative combat simulation
- **Matchmaking**: Skill-based matching for fair battles
- **Leaderboards**: Global and weekly rankings
- **Cloud saves**: Sync progress across devices
- **Module validation**: Server-side verification to prevent cheating

The game includes an offline mode for single-player battles against AI opponents.

---

## 🚀 Getting Started

### Prerequisites
1. **Godot Version**: This project uses **Godot 4.4.1**
2. **Android SDK**: Required for Android builds
3. **Docker & Docker Compose**: Will be required for Nakama integration (planned)

### Current Setup
1. Clone the repo
2. **Start Nakama Server** (optional for local testing):
   ```bash
   cd nakama
   docker-compose up -d
   ```
3. Open the project in Godot from the `src/` directory
4. Set the target platform (Android or iOS)
5. Run `BootScene` to start the game

### Nakama Server Access
- **Console**: http://localhost:7351
- **HTTP API**: http://localhost:7350
- **gRPC**: localhost:7349

---

## ✅ MVP Feature Checklist

### Implemented
* [ ] Tap-based terminal UI
* [ ] Modular AI construction (3-part system)
* [ ] Procedural wild AI generation
* [ ] Turn-based, auto-resolved battle system (local only)
* [ ] Salvage and archive progression system
* [ ] Local save system
* [ ] Audio feedback and ambient loops

### Planned (Nakama Integration)
* [ ] Online multiplayer battles (PvP)
* [ ] Global and weekly leaderboards
* [ ] Cloud save synchronization
* [ ] Server-authoritative battle validation
* [ ] Matchmaking system
* [ ] Social features (friends, spectating)

---

## 🎨 Design Direction

* Minimal, ASCII-inspired UI
* No keyboard input (except optional text fields like AI name)
* Strong feedback loops: sound, blinking cursors, flickers
* Accessible UX for mobile

---

## 🧠 Documentation

* [Game Design Document (GDD)](docs/GDD.md)
* [Gameplay Flowchart](docs/gameplay_flowchart.md)
* [One-Pager Concept](docs/game_one_pager.md)

---

## 🙌 Credits

**Developer**: \[Your Name or Handle]
**Engine**: Godot 4.4.1
**Backend**: Nakama Server
**Audio**: Freesound.org, custom design, retro archive samples
**Font**: Terminal-inspired bitmap font (public domain or licensed)

---

## 📌 License

This project is proprietary and not open source. All assets and code are for internal use during development unless otherwise stated.

---

*Welcome to the datastream. Run your AI. Salvage the code. Evolve.*
