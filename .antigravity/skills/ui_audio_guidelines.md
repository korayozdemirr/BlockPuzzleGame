---
name: ui_audio_guidelines
description: Standards for Audio playback timing, UI animations, board backgrounds, and victory sequences.
---

# UI & Audio Integration Standards

## AudioManager Setup
- Singleton instance: `AudioManager.Instance`
- Implemented SFX hooks:
  - `PlayPick()`: Invoked only when drag distance exceeds threshold.
  - `PlayRotate()`: Invoked on quick tap release.
  - `PlaySnap()`: Triggered on valid grid drop.
  - `PlayFail()`: Triggered on invalid placement / return to dock.
  - `PlayUndo()`: Triggered on button click.
  - `PlayPop()`: Triggered per sub-block during win sequence (pitch randomized between 0.92f - 1.08f).
  - `PlayVictory()`: Triggered after all blocks pop.

## Win Sequence Flow
1. Block placement satisfies board completeness.
2. `ExplodeAllPiecesSequence` runs:
   - Loops sub-blocks with `0.06f` delay.
   - Instantiates particle effects tinted with block color.
   - Triggers `PlayPop()`.
3. Plays `PlayVictory()`.
4. Waits exactly `victorySound.length`.
5. Opens `Panel_Victory` via `UIManager.Instance.ToggleNextLevelPanel(true)`.

## Board Tray Framing
- `GridBoard_Background` scales dynamically:
  - `Width = (gridW * cellSize) + padding` (padding = 0.35f)
  - `Height = (gridH * cellSize) + padding`
  - Position: `(0, verticalOffset, 0)`