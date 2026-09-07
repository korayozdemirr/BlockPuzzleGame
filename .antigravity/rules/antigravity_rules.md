# Project: 2D Block Puzzle Game (Unity)

## Core Architecture & Conventions
- **Unity Version:** 2022.3 LTS (URP 2D).
- **Architecture Pattern:** Clean Singleton managers (`GridManager`, `AudioManager`, `UIManager`, `LevelManager`, `SaveManager`, `LevelGenerator`).
- **Target Platform:** Mobile Portrait (9:16 aspect ratio). Reference resolution: 1080x1920.
- **Input System:** 2D Raycast & Collider based touch interaction (`DraggablePiece`).

## Critical Coding Constraints
1. **Pivot & Centering:** Never assume block root pivots are centered. Always respect `CenterChildrenPivot()` logic when creating or modifying draggable pieces.
2. **Layering (Sorting Order):**
   - Background Sprite: Order `-10`
   - Board Tray Background: Order `-1`
   - Grid Slots / Cells: Order `0`
   - Placed Blocks: Order `2`
   - Picked / Dragged Blocks: Order `10`
   - Floating Score Popups: Order `20`
3. **UI Standard:** Always use TextMeshPro (`TextMeshProUGUI`). Canvas Render Mode must stay `Screen Space - Overlay` with `Scale With Screen Size (Match 0.5)`. Use `LilitaOne-Regular SDF` casual game font asset.
4. **Game Feel (Juice):**
   - Audio calls must route strictly through `AudioManager.Instance`.
   - Never play `sfx_pick` on tap; separate tap (Rotate) and drag (Pick) using established `DragThreshold` (0.25f) and `ClickDurationLimit` (0.3f).
   - Use dynamic pitch variation for multi-pop animations.

## Memory & Git Synchronization Rules
1. **Memory Tracking:** Maintain `PROJECT_MEMORY.md` at the project root. Whenever a feature, bugfix, or refactor is completed:
   - Log the change concisely under the "Change Log" section.
   - Update the "Active State" and "Pending Tasks" sections.
   - Do not re-read entire legacy scripts if `PROJECT_MEMORY.md` already specifies their current state (Token Optimization).
2. **Automated Commits:** After completing any functional change and updating `PROJECT_MEMORY.md`, run or propose a conventional Git commit (e.g., `feat: ...`, `fix: ...`) to keep version control synchronized.

## Git & File Discipline
- Do not modify or delete `.meta` files manually.
- Exclude `Library/`, `Temp/`, and `Logs/` from any proposed Git operations.