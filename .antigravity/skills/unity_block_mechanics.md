---
name: unity_block_mechanics
description: Guidelines for manipulating grid logic, piece geometry, undo actions, and level loading.
---

# Unity 2D Block Puzzle Mechanics Guide

## Grid Calculations
- Start world coordinates are derived from `cellSize = 1.0f` and centered using:
  `startX = -(width - 1) * cellSize / 2f`
  `startY = -(height - 1) * cellSize / 2f + verticalOffset`
- `verticalOffset` is typically `0.5f` to `0.8f` to leave room for top UI panels.

## Draggable Pieces & Slots
- Pieces spawn within bottom inventory docks (`PieceStackSlot`).
- Spawn bounds: `maxAvailableWidth = 3.4f`, `spawnY = -3.2f`.
- When modifying `TryPlacePiece`:
  - Verify bounds via `IsValidCoord()`.
  - Check occupancy via `isCellOccupied[,]`.
  - Maintain the offset alignment to preserve snap accuracy regardless of piece rotation.

## Undo Integrity
- Every successful move MUST push to `moveHistory` with a `PlacementAction` struct containing:
  - `piece` reference
  - `occupiedCoords` array
  - `sourceSlot` reference
- Undoing restores the exact piece count in the slot via `sourceSlot.RestorePiece()` and subtracts penalty score.