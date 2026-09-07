# PROJECT MEMORY - 2D Block Puzzle Game (Unity)

## 📌 Architecture Summary & Technical Overview
- **Unity Version:** 2022.3 LTS (URP 2D).
- **Target Resolution:** Mobile Portrait 9:16 (1080x1920 reference resolution).
- **Design Pattern:** Clean Singleton Managers (`GridManager`, `AudioManager`, `UIManager`, `LevelManager`, `SaveManager`, `LevelGenerator`).
- **UI Standard:** TextMeshPro (`TextMeshProUGUI`) with `LilitaOne-Regular SDF` casual game font asset and embedded Texture2D atlas sub-asset.

---

## 🎮 Core Mechanics & Script Breakdown

### 1. Grid & Drag-Drop Interaction (`GridManager.cs` & `DraggablePiece.cs`)
- `DraggablePiece.cs`:
  - `CenterChildrenPivot()`: Calculates average local position of child sub-blocks and centers them at `(0,0,0)`.
  - Input Handling: `OnMouseDown`, `OnMouseDrag`, `OnMouseUp`.
  - Tap vs Drag distinction: If drag distance `< 0.25f` (`DragThreshold`) and hold duration `< 0.3f` (`ClickDurationLimit`), executes clockwise -90° rotation (`RotatePiece()`). Otherwise, triggers drag (`PlayPick()`) and sets sorting order to `10`.
- `GridManager.cs`:
  - `BuildGrid()`: Centered world position formula `startX = -(width-1)*cellSize/2f`, `verticalOffset = 1.5f`.
  - `TryPlacePiece()`: Validates distance (`< cellSize * 0.6f`) and cell occupancy (`!isCellOccupied`). Pushes successful move to `moveHistory` stack with `PlacementAction` struct.
  - `UndoLastMove()`: Pops `PlacementAction`, clears occupied cells, destroys placed piece, restores slot count (`sourceSlot.RestorePiece()`), subtracts score (-25 pts), and clears Game Over overlay if open.

### 2. Level System & AI Level Generator (`LevelManager.cs` & `LevelGenerator.cs`)
- `LevelManager.cs`: Handcrafted level assets (Levels 1 & 2). Beyond index 1, calls `LevelGenerator.Instance.GenerateNextLevel(targetLevelNumber)`.
- `LevelGenerator.cs`: Endless procedural level generator (3x4, 4x4, 4x5 grids) utilizing 2x2 Square, L, I3, and I2 blocks with 100% board-fill verification loop. Dynamic level numbers.

### 3. Data Persistence (`SaveManager.cs`)
- **Lazy Initialization:** `SaveManager.Instance` automatically instantiates `new GameObject("SaveManager").AddComponent<SaveManager>()` with `DontDestroyOnLoad` if absent from scene.
- **PlayerPrefs Keys:** `Saved_CurrentLevelIndex`, `Saved_HighScore`, `Saved_SFXMuted`, `Saved_VibrationEnabled`.
- **API:** `GetCurrentLevelIndex()`, `SaveCurrentLevelIndex()`, `GetHighScore()`, `TryUpdateHighScore()`, `IsSFXMuted()`, `SetSFXMuted()`, `IsVibrationEnabled()`, `SetVibrationEnabled()`, `ResetAllData()`.

### 4. Audio & Haptics (`AudioManager.cs`)
- SFX clips: `pickSound`, `rotateSound`, `snapSound`, `failSound`, `undoSound`, `popSound`, `victorySound`.
- `PlayPop()`: Dynamic pitch variation (`0.92f - 1.08f`).
- `PlaySnap()` & `PlayVictory()`: Triggers `TriggerVibration()` (`Handheld.Vibrate()`) if enabled.
- Mute Check: All SFX playback checks `SaveManager.Instance.IsSFXMuted()`.

### 5. UI System, Juice & Animations (`UIManager.cs` & `FloatingScoreText.cs`)
- **LilitaOne Google Game Font:** Embedded SDF Font Asset at `Assets/Resources/Fonts/LilitaOne-Regular SDF.asset`.
- **UI Sprites:** Loaded from `Assets/Resources/Sprites/` (`btn_next_level.png`, `btn_undo_icon.png`).
- **Floating Score Popups (`FloatingScoreText.cs`):** Spawns world-space TextMeshPro popups (`+50` yellow on drop, `+200 EXCELLENT!` green on win, `NO MORE MOVES!` red on loss) at sorting order `20` drifting upwards, scaling, and fading out.
- **Micro-Animations (`UIManager.cs`):**
  - Score Pulse: `1.0 -> 1.25 -> 1.0` scale pulse on score change.
  - Elastic Bounce: `0.0 -> 1.15 -> 1.0` scale bounce for panel popups.
  - Button Scale: `1.0 -> 0.88 -> 1.0` press feedback.

### 6. Game Over & Move Solver (`GridManager.cs`)
- `CanAnyPieceBePlaced()`: Simulates all available slot pieces across all 4 rotation angles (0°, 90°, 180°, 270°) against all free grid cells.
- If no valid move exists, triggers `TriggerGameOver()` -> `NO MORE MOVES!` popup, `PlayFail()` SFX, and opens Game Over Overlay Panel.
- Options: `RETRY LEVEL` (restarts level) or `UNDO LAST MOVE` (takes back last move, clears Game Over overlay, continues play).

### 7. Overlays & Main Menu (`UIManager.cs`)
- **Otonom Main Menu Screen (`EnsureAutoMainMenuPanel`):** Title *"BLOCK PUZZLE"*, `LEVEL X`, `BEST SCORE: Y`, and large `PLAY ▶` button.
- **Otonom Settings Screen (`EnsureAutoSettingsPanel`):** Title *"SETTINGS"*, `SFX SOUNDS: ON/OFF`, `VIBRATION: ON/OFF`, `RESET PROGRESS`, `CLOSE`. Top-left `⚙` (OPT) gear button.
- **Otonom Game Over Screen (`EnsureAutoGameOverPanel`):** Title *"GAME OVER"*, `RETRY LEVEL`, `UNDO LAST MOVE`.

---

## 🟢 Active State
- **Status:** All core systems, AI level generation, SaveManager persistence, UI animations, Google LilitaOne font asset, move validation solver, Main Menu, Game Over overlay, and Settings panel are 100% operational.
- **Scene Objects Required:** Minimal. All managers, overlays, and font assets are self-creating and self-contained via code and lazy initialization.

---

## 📝 Change Log

### [2026-09-07] - Initial Project Setup & Architecture Analysis
- Analyzed codebase & architectural rules in `.antigravity/rules/` and `.antigravity/skills/`.
- Verified 2D raycast touch interaction, pivot centering logic, sorting orders, and reference resolution.

### [2026-09-07] - SaveManager Integration & PlayerPrefs Persistence
- Created `SaveManager.cs` following Singleton pattern with `DontDestroyOnLoad`.
- Integrated `Saved_CurrentLevelIndex` and `Saved_HighScore` with `LevelManager.cs`, `GridManager.cs`, and `UIManager.cs`.
- Refactored `SaveManager.Instance` to **Lazy Initialization** (auto-instantiates `new GameObject("SaveManager")` if absent from scene).

### [2026-09-07] - AI Level Generator Fixes
- Fixed issue where `generatedLevelCounter` reset to 3 on game reload. Updated `LevelGenerator.GenerateNextLevel(targetLevelNumber)` and `LevelManager` to pass `currentLevelIndex + 1`.

### [2026-09-07] - UI Polish & Micro-Animations (Juice)
- Created `FloatingScoreText.cs` for world-space animated popups (`+50`, `+200 EXCELLENT!`).
- Implemented `AnimateScorePulse`, `AnimatePanelElasticBounce`, and `AnimateButtonClick` coroutines in `UIManager.cs`.

### [2026-09-07] - Smart Game Over System & Move Solver
- Implemented `CanAnyPieceBePlaced()` in `GridManager.cs` testing all 4 piece rotations against grid occupancy matrix.
- Implemented `TriggerGameOver()`, `RestartCurrentLevel()`, and integrated Undo reset state.
- Created `EnsureAutoGameOverPanel()` in `UIManager.cs` with `RETRY LEVEL` and `UNDO LAST MOVE` options.

### [2026-09-07] - Settings Panel & Audio/Haptic Controls
- Added `IsSFXMuted()` and `IsVibrationEnabled()` to `SaveManager.cs`.
- Integrated SFX mute checks and `Handheld.Vibrate()` in `AudioManager.cs`.
- Created top-left Settings gear button (`⚙`) and dynamic `EnsureAutoSettingsPanel()` with SFX toggle, Vibration toggle, and Reset Progress features.

### [2026-09-07] - Main Menu / Start Screen
- Updated `LevelManager.cs` to show Main Menu first instead of auto-starting grid.
- Created `EnsureAutoMainMenuPanel()` in `UIManager.cs` featuring Title *"BLOCK PUZZLE"*, Level & Best Score overview, and `PLAY ▶` button.

### [2026-09-07] - LilitaOne Google Font & UI Sprites
- Downloaded `LilitaOne-Regular.ttf` from Google Fonts into `Assets/Fonts/`.
- Generated and saved embedded TMP Font Asset at `Assets/Resources/Fonts/LilitaOne-Regular SDF.asset` with embedded Texture2D atlas sub-asset.
- Applied `LilitaOne-Regular SDF` font automatically to all TextMeshPro components in `UIManager.cs` and `FloatingScoreText.cs`.
- Replaced missing raw Unicode glyphs (`▶`, `⚙`) with clean text labels and applied real UI button sprites (`btn_next_level.png`, `btn_undo_icon.png`).

---

## 📋 Pending Tasks & Potential Next Steps
1. **Line / Column Clearing System (Woodoku / Blockudoku mechanic):** Full row/column detection, line clear animations, and combo multipliers (`+150 COMBO!`).
2. **Power-ups & Boosters:** Hammer/Bomb booster (destroys selected 2x2 or 1x1 area), Refresh booster (re-rolls current bottom piece options).
3. **Visual Particles & Sparkles:** Enhanced particle textures for victory celebrations and line clears.
