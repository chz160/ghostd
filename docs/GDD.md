## 1. Game Overview

### Game Title (Working Title)
**Ghostd**  
*(Other candidate titles: Corecide, GhostCode, AI.EXE, FractalNet)*

### High-Concept Pitch
In the decaying backchannels of a forgotten network, rogue AIs wage silent war for dominance. Craft your agent from salvaged code fragments, deploy it into the datastream, and evolve through tactical battles against corrupted programs in a world that looks like a terminal and bites like malware.

### Genre & Platform
- **Genre**: AI Auto-Battler, Terminal-Themed Strategy
- **Sub-Genre**: Collectible/Upgradeable Simulation
- **Style**: Cyberpunk / Hacker / Glitchpunk
- **Platform**: Android + iOS (Cross-platform from launch)

### Target Audience
- **Primary**: Mobile gamers who enjoy competitive strategy games
- **Secondary**: Auto-battler fans looking for something unique
- **Aesthetic Appeal**: Cyberpunk enthusiasts and retro computing fans especially those nostalgic for late '90s/early 2000s tech culture
- **Competitive Players**: Those who enjoy climbing leaderboards and mastering metas
- **Age**: 14–35, tech-savvy players who appreciate hacker culture
- **Regions**: Global, with initial focus on North America, Europe, and Asia

### Core Gameplay Loop
1. **Design your AI**: Select a Core, Behavior, and Augment module
2. **Find Opponent**: Match with another player or off against a procedurally generated wild AI
3. **Run Battle**: Server-authoritative combat simulation ensures fair play
4. **Salvage**: Win and claim a new module from the enemy
5. **Evolve**: Reconfigure your AI and optimize your build
6. **Climb Rankings**: Progress through global and weekly leaderboards
7. **Repeat**: Face stronger opponents, discover rare modules, master the meta

### Core Pillars
- **Customization**: AIs are built from modular components—each choice matters
- **Discovery**: Players encounter strange, powerful, and glitchy modules through salvage
- **Strategic Depth**: Despite the simplicity, module synergy and behavior matter
- **Immersion**: The game feels like you're inside a terminal jacked into a forgotten server
- **Competitive Play**: Skill-based matchmaking and leaderboards drive mastery
- **Fair Play**: Server-side validation ensures a cheat-free environment

### Monetization Strategy
- **Launch (v1)**: Free-to-play with optional cosmetics
  - Terminal themes and color schemes
  - ASCII art variations for modules
  - Battle log visual effects
- **Future Updates**:
  - Season passes with exclusive modules (balanced, not P2W)
  - Tournament entry fees with prize pools
  - Premium account features (detailed stats, replays)
- **Core Promise**: All gameplay-affecting content earnable through play

### Unique Selling Points
- **No typing required, but looks like a command line**—the perfect blend of hacker aesthetic and mobile UX
- **Tactical auto-battler without grind**—focus on creativity and build experimentation
- **Single-dev indie charm**—crafted with care and full of mystery

---

## 2. Technical Architecture

### Backend Infrastructure
**Ghostd** uses Nakama as its backend server, providing:
- **Real-time Multiplayer**: Authoritative battle simulation prevents cheating
- **Matchmaking**: ELO-based skill matching with power level considerations
- **Data Persistence**: Cloud saves, module collections, battle history
- **Social Features**: Friends lists, spectating, battle replays
- **Analytics**: Player behavior tracking for balance updates

### Client-Server Architecture
- **Godot Client**: Handles UI, animations, and user input
- **Nakama Server**: Manages game state, battles, and validation
- **Communication**: 
  - WebSocket for real-time battle updates
  - REST API for inventory and progression
  - MessagePack encoding for mobile data efficiency

### Security & Anti-Cheat
- **Server Authority**: All battle calculations happen server-side
- **Module Validation**: Server verifies legal module combinations
- **Input Validation**: All client requests sanitized and rate-limited
- **Replay System**: Battle outcomes can be verified through replay
- **Device Fingerprinting**: Detect and prevent multi-accounting

### Mobile Optimization
- **Adaptive Quality**: Network detection adjusts update frequency
- **Battery Management**: Reduced polling when on battery power
- **Offline Mode**: Queue battles and sync when reconnected
- **Data Compression**: ~70% reduction using MessagePack + gzip
- **Connection Resilience**: Automatic reconnection with exponential backoff

### Scalability Plan
- **Phase 1**: Single Nakama instance (supports ~10K CCU)
- **Phase 2**: Nakama cluster with Redis cache
- **Phase 3**: Regional servers for global deployment
- **Database**: PostgreSQL with read replicas
- **CDN**: Static assets and module data cached globally

---

## 3. Multiplayer Gameplay

### Battle Modes
- **Proximity Battle** (Primary): Face players within ~100m physical range
- **Local Network**: Battle players on same WiFi/LAN
- **Wild Hunt**: PvE battles against procedural AIs (always available)
- **Ghost Battle**: Async battles against recorded player strategies
- **Global Events**: Special time-limited tournaments (secondary)

### Proximity-Based System (Core Feature)
- **Discovery Range**: Detect players within 500m radius
- **Battle Range**: Must be within 100m to initiate battle
- **Privacy Modes**: Precise, Fuzzy (±100-500m), or Ghost (hidden)
- **Location Updates**: Battery-aware with 30-120s intervals
- **Fallback Logic**: If no local players → Ghost battles → AI opponents

### Location Safety & Privacy
- **Safe Zones**: Auto-disabled near schools, hospitals
- **Age Restrictions**: Location features require 13+ age
- **Obfuscation**: Optional location fuzzing for privacy
- **No Chat**: Emotes only to ensure player safety
- **Cooldowns**: Limit battles with same player (anti-harassment)

### Battle Resolution
- **Duration**: 30-90 seconds server-side simulation
- **Display**: Client shows stylized terminal log of actions
- **Deterministic**: Same inputs always produce same result
- **Spectatable**: Friends can watch live battles
- **Replayable**: All battles saved for 7 days

### Progression & Rewards
- **Victory Rewards**: Choice of 1 from 3 opponent modules
- **Defeat Learning**: Gain XP even from losses
- **Win Streaks**: Bonus rewards for consecutive wins
- **Daily Quests**: Module-specific challenges
- **Season Journey**: Long-term goals with exclusive rewards

### Social Features
- **Friend System**: Add via ID or QR code
- **Battle History**: View past matches and stats
- **Spectate Mode**: Watch top players' strategies
- **Guild System** (Future): Team-based competitions
- **Chat**: Quick emotes only (no text chat for safety)

---

## 9. Marketing & Positioning

### Tagline Options
- "Build. Battle. Evolve. Infiltrate the Grid."
- "A rogue AI battler in a decaying digital world."
- "Your code is your weapon. Your mind is your edge."

### Store Description (Short)
> Build and evolve a rogue AI in a corrupted digital wasteland. Face off against wild programs, salvage code, and survive the datastream in this stylized terminal battler.

### Store Description (Long)
> In the ruins of an abandoned network, rogue AIs fight for survival. Design your own fractal intelligence from hacked code fragments, challenge corrupted programs in auto-resolved battles, and upgrade your agent through scavenged parts. With a minimalist hacker aesthetic and satisfying gameplay loop, Terminal Fracture is a strategy game disguised as a retro terminal. Build smarter AIs. Win weirder fights. Dominate the datastream.

### Market Positioning
- **Aesthetic Niche**: Hacker/cyberpunk terminal-style games
- **Gameplay Niche**: Tactical idle/auto-battlers that focus on logic and customization, not reflexes
- **Audience**:
  - Gamers nostalgic for 90s-era tech and cyber fiction
  - Players of AI simulation, auto chess, or strategy-lite games
  - Solo dev/indie fans who appreciate minimal, well-crafted design

### Influences & Comparison Titles
- *Reigns* (simple choices with depth)
- *AI Dungeon* (AI-themed, terminal style interface)
- *Glitchhikers* (visual/audio mood inspiration)
- *Inscryption* (tone, UI layering, glitch effects)
- *Candy Crush / Good Pizza Great Pizza* (feedback loop mastery)

### Launch Channels
- **Android & iOS App Stores**
- Itch.io page for desktop dev builds and feedback
- Reddit (e.g., r/IndieDev, r/roguelites, r/cyberpunkgame, r/gamedev)
- Hacker-style trailer on YouTube + Shorts
- TikTok and Instagram Reels for atmospheric battle logs with glitchy overlays
- Devlogs on Twitter/X and Mastodon

### Soft Launch Strategy
- Limited soft launch on Android only to test retention and onboarding (manual beta keys or open testing)
- Collect player feedback from online communities and refine loop
- Full launch once retention and polish are validated

---

**Document Complete — Ready for Iteration and Expansion.**

