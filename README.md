# Ghostd – Godot Project (v1)

Welcome to the core Godot project for **Ghostd**, a terminal-themed rogue AI battler for Android and iOS.

This repository contains the entire source code and assets for version 1 (MVP) of the game.

---

## 📱 Game Overview

**Ghostd** is a fast, strategic auto-battler set in a decaying digital world. Players design modular AIs using Core, Behavior, and Augment modules, then deploy them in stylized terminal battles against wild AIs. Victory grants salvaged code fragments used to evolve and optimize builds.

* **Genre**: Auto-battler / Strategy
* **Aesthetic**: Hacker, cyberpunk, terminal UI (no typing required)
* **Target Platforms**: Android & iOS (mobile only, MVP is offline-first)

---

## 🔧 Project Structure

* `/Assets`

  * Game scripts, UI prefabs, module data, and battle logic
* `/Audio`

  * Ambient loops, glitch effects, retro UI sounds
* `/Resources`

  * Config files, ASCII module definitions, wild AI schematics
* `/Scenes`

  * `BootScene`, `MainTerminal`, `BattleRunner`, `ModuleLab`
* `/Scripts`

  * `Core`, `Behavior`, `Augment`, `BattleSystem`, `ArchiveManager`, etc.

---

## 🚀 Getting Started

1. **Godot Version**: This project uses **Godot 4**
2. Clone the repo
3. Open the project in Godot GDScript
4. Set the target platform (Android or iOS)
5. Run `MainTerminal` scene to begin

---

## ✅ MVP Feature Checklist

* [x] Tap-based terminal UI
* [x] Modular AI construction (3-part system)
* [x] Procedural wild AI generation
* [x] Turn-based, auto-resolved battle system
* [x] Salvage and archive progression system
* [x] Offline save/load system
* [x] Audio feedback and ambient loops

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
**Engine**: Godot 4
**Audio**: Freesound.org, custom design, retro archive samples
**Font**: Terminal-inspired bitmap font (public domain or licensed)

---

## 📌 License

This project is proprietary and not open source. All assets and code are for internal use during development unless otherwise stated.

---

*Welcome to the datastream. Run your AI. Salvage the code. Evolve.*
