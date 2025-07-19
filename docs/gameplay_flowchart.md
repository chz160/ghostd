## Gameplay Flowchart & Narrative Flow

### Summary of Game Design So Far

**Working Title:** TBD (Candidates include *Ghostd*, *Blackline*, *VOIDRUN*, *EXE*, *0DAY*)

**Core Concept:** A terminal-themed rogue AI auto-battler where players design modular AIs and pit them against procedurally generated opponents in a retro cyberpunk setting. Victory allows players to salvage modules and evolve their agent.

**Aesthetic:**
- Terminal/command line UI (tap-only, no required typing)
- Cyberpunk / glitchcore / hacker themes
- Ambient and ASMR-inspired audio feedback (modem tones, glitch pulses)
- ASCII art UI and module visuals

**Gameplay Loop:**
1. Build AI from 3 parts: Core, Behavior, Augment
2. Battle a wild AI (auto-resolved, turn-based)
3. Salvage a new part if victorious
4. Rebuild, optimize, and repeat

**Modules (v1):**
- ~5 Cores
- ~5 Behaviors
- ~5 Augments
- 5+ Wild AI opponents with unique combinations

**Battle Details:**
- Fully automated
- Short duration (3–6 rounds)
- Outcome decided by stat resolution and module synergy

**Progression:**
- Local module collection (Archive)
- Salvage screen post-victory
- Unlock new modules through victories

**Planned MVP Features:**
- Local gameplay only (no network or cloud)
- Terminal-style UI with touch-friendly interaction
- Small but diverse module pool
- Sound and ambient design
- Save/load system

**Post-MVP Ideas:**
- AI ghost sharing or passive PvP
- Cosmetic terminal themes
- Rare event encounters and glitches

---

### Core Game Loop

1. **Build AI** – Assemble an AI using available Core, Behavior, and Augment modules from the Archive.
2. **Run Battle** – Deploy AI into a terminal-simulated auto-battle against a procedurally generated Wild AI.
3. **Resolve Outcome**:
   - Victory: Salvage 1 of 3 offered modules and add it to Archive.
   - Defeat: No reward, but keep current AI and modules.
4. **Evolve** – Reconfigure AI with newly acquired modules to test new strategies.
5. **Repeat** – Continue battling, collecting, and evolving in short play sessions.

---

### 1. Game Launch
- Player sees animated terminal boot sequence (flickering, ASCII art, CRT sound)
- Fake terminal prompt blinks: `> _`
- Tap anywhere brings up "Your Terminal"

---

### 2. Your Terminal (Main Hub)
**Main Options:**
- `[ RUN AI ]` → Starts a battle sequence
- `[ RECONFIGURE ]` → Opens AI builder screen
- `[ ARCHIVE ]` → View collected modules
- `[ SETTINGS ]` → Sound, Theme, Accessibility

First-time user:
- Walkthrough triggers with a faux AI voice log: “Initializing rogue instance…”

---

### 3. AI Creation / Reconfigure
- Choose 1 of each:
  - CORE
  - BEHAVIOR
  - AUGMENT
- Modules displayed with ASCII art + stats
- Save configuration to exit back to terminal

---

### 4. Run AI (Battle)
**Flow:**
- Screen shows entering corrupted datastream...
- Match found: Wild AI schematic appears
- Battle log animates each round in fake-terminal style
- Battle ends → Victory or Defeat

Victory:
- Salvage screen (choose 1 of 3 modules)
- Auto-saves new module to Archive
- Option to immediately equip or store

Defeat:
- AI survives but gains no modules
- Option to retry or reconfigure

---

### 5. Archive
- Scrollable list/grid of collected modules
- View stats, lore logs, boot fragments
- Long-press to mark favorites or delete corrupted ones

---

### 6. Meta Progression (Future)
- Win streak unlocks special modules (v1.5+)
- Rare events (glitched AIs, encrypted logs)
- Cosmetic unlocks (cursor shapes, color themes)

---

### 7. Optional Mid-Term Goals (Post-MVP)
- Online opponent pool (upload ghost AIs)
- Passive scanning (background auto-battles)
- Tournament ladder or seasonal resets

---

This flowchart supports both fast loops and long-term build experimentation while keeping the terminal UX intuitive and addictive.

