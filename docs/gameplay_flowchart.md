## Gameplay Flowchart & Narrative Flow

### Summary of Game Design So Far

**Working Title:** TBD (Candidates include *Ghostd*, *Blackline*, *VOIDRUN*, *EXE*, *0DAY*)

**Core Concept:** A proximity-based terminal-themed auto-battler where players design modular AIs and battle other players within physical range (~100m). The game encourages local, in-person competition with a retro cyberpunk aesthetic. Victory allows players to salvage modules and evolve their agent.

**Aesthetic:**
- Terminal/command line UI (tap-only, no required typing)
- Cyberpunk / glitchcore / hacker themes
- Ambient and ASMR-inspired audio feedback (modem tones, glitch pulses)
- ASCII art UI and module visuals

**Gameplay Loop:**
1. Build AI from 3 parts: Core, Behavior, Augment
2. Scan for nearby players (proximity-based)
3. Challenge & battle detected players (server-validated)
4. Salvage a new part if victorious
5. Rebuild, optimize, and climb local leaderboards

**Modules (v1):**
- ~5 Cores
- ~5 Behaviors
- ~5 Augments
- 5+ Wild AI opponents with unique combinations

**Battle Details:**
- Fully automated, server-authoritative
- Short duration (30-90 seconds)
- Proximity validation prevents spoofing
- Outcome decided by module synergy

**Location & Privacy:**
- 100m battle range, 500m discovery
- Privacy modes: Precise/Fuzzy/Ghost
- Battery-aware GPS updates
- Safe zones auto-disable battles

**Progression:**
- Module collection synced to cloud
- Local & global leaderboards
- Area-based rankings
- Weekly tournaments

**MVP Features:**
- Proximity-based PvP battles
- Fallback: LAN → Ghost → AI battles
- Terminal-style UI with touch-friendly interaction
- Small but diverse module pool
- Sound and ambient design
- Location privacy controls
- Cloud save with offline mode

**Post-MVP Ideas:**
- AI ghost sharing
- Cosmetic terminal themes
- Rare event encounters and glitches

**Safety Features:**
- No text chat (emotes only)
- Battle cooldowns per player
- Age-gated location features
- Report/block functionality

---

### Core Game Loop

1. **Build AI** – Assemble an AI using Core, Behavior, and Augment modules from your collection.
2. **Scan Area** – Detect nearby players within 500m discovery radius, with possible general direction indicator.
3. **Challenge Player** – Select opponent within 100m battle range (or use fallback mode). Distance could be dynamic based on the density of local players. For instance NYC might have a smaller battle range that 100m due to more players being in the area.
4. **Battle** – Server-validated combat with terminal-style visualization.
5. **Resolve Outcome**:
   - Victory: Salvage 1 of 3 opponent modules
   - Defeat: Gain XP, learn from loss
6. **Evolve** – Reconfigure AI with new modules and strategies.
7. **Climb Rankings** – Progress through local area and global leaderboards.

---

### 1. Game Launch
- Player sees animated terminal boot sequence (flickering, ASCII art, CRT sound)
- Fake terminal prompt blinks: `> _`
- Tap anywhere brings up "Your Terminal"

---

### 2. Your Terminal (Main Hub)
**Main Options:**
- `[ SCAN AREA ]` → Detect nearby players
- `[ RECONFIGURE ]` → Opens AI builder screen
- `[ ARCHIVE ]` → View collected modules
- `[ RANKINGS ]` → Local/Global leaderboards
- `[ SETTINGS ]` → Privacy, Sound, Theme

First-time user:
- Location permission request
- Privacy mode selection (Precise/Fuzzy/Ghost)
- Tutorial: "Detecting local network nodes..."

---

### 3. AI Creation / Reconfigure
- Choose 1 of each:
  - CORE
  - BEHAVIOR
  - AUGMENT
- Modules displayed with ASCII art + stats
- Save configuration to exit back to terminal

---

### 4. Scan Area (Proximity Detection)
**Flow:**
- Terminal shows: "Scanning corrupted datastream..."
- Radar-style UI shows nearby players
- List displays:
  - Players within 100m (can battle)
  - Players 100-500m (approaching)
  - Privacy-respecting display names

**Fallback Options:**
- No players nearby → Show Ghost battles
- No Ghosts → Generate AI opponent
- Offline mode → Local AI only

### 5. Battle Sequence
**Initiation:**
- Select opponent from scan results
- Send challenge (with cooldown)
- Both players must accept within 60s

**Battle Flow:**
- "Establishing secure connection..."
- Server validates proximity
- Terminal shows real-time battle log
- 30-90 second automated combat
- Results validated server-side

**Victory:**
- Choose 1 of 3 opponent modules
- Ranking points awarded
- Win streak counter updates

**Defeat:**
- XP gained for participation
- Battle replay available
- Rematch cooldown enforced

---

### 6. Archive & Collection
- Scrollable list/grid of collected modules
- View stats, lore logs, boot fragments
- Long-press to mark favorites or delete corrupted ones
- Filter by type, rarity, source
- Module origins tracked (opponent names)
- Cloud sync with offline cache

---

### 6.5. Meta Progression (Future)
- Win streak unlocks special modules (v1.5+)
- Rare events (glitched AIs, encrypted logs)
- Cosmetic unlocks (cursor shapes, color themes)
---

### 7. Rankings & Leaderboards
**Local Area Rankings:**
- City/region-based leaderboards
- "Dominant AI in [Area Name]"
- Weekly local tournaments

**Global Rankings:**
- Overall ranking points
- Module diversity score
- Win/loss ratios

### 8. Privacy & Safety Settings
**Location Privacy:**
- **Precise**: Exact location for friends
- **Fuzzy**: ±100-500m randomization
- **Ghost**: Hidden from all scanning

**Battle Controls:**
- Block list management
- Battle availability toggle
- Safe zone detection
- Parental controls (age 13+)

### 9. Special Events
**Location-Based Events:**
- Landmark battles (parks, malls)
- Community day gatherings
- Regional exclusive modules

**Timed Events:**
- Weekend tournaments
- Holiday module drops
- Beta test new features

---

### Technical Flow: Proximity Battle Validation

1. **Client Request:**
   - Player A requests battle with Player B
   - GPS coordinates + accuracy sent
   - Privacy mode applied

2. **Server Validation:**
   - Distance calculation (<100m)
   - Safe zone check
   - Cooldown verification
   - Anti-spoof checks

3. **Battle Execution:**
   - Server runs simulation
   - Results sent to both clients
   - Replay data stored
   - Rankings updated

---

This proximity-focused flowchart creates a unique local competitive experience while maintaining player safety and privacy.

