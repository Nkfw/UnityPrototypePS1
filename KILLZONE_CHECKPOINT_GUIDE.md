# Kill Zone & Checkpoint System - Implementation Guide

## Overview
Complete implementation of the death/respawn system with bridge restoration and proper state management.

## Implemented Features

### 1. Kill Zone (KillZone.cs)
- Large BoxCollider trigger beneath the level
- Detects when player or sheep falls off the map
- Triggers death via DeathManager singleton
- Visual gizmos in Scene view (red wireframe box)

### 2. Checkpoint System (Checkpoint.cs + CheckpointManager.cs)
- Save points throughout the level
- Stores player position, rotation, sheep states, and destructible object states
- Initial checkpoint auto-saved at level start
- New checkpoints save on player entry (one-time activation)

### 3. Death Manager (DeathManager.cs)
- Handles all death events with screen fade transitions
- Freezes player input during respawn sequence
- Coordinates with ScreenFader for smooth transitions
- Calls CheckpointManager to restore game state

### 4. Bridge Restoration (BridgeCollapse.cs)
- Bridges use `SetActive(false)` instead of `Destroy()` for restoration
- CheckpointManager tracks which destructibles were active at save time
- Bridges restore when checkpoint loads
- Safety delay (0.5s) prevents immediate re-collapse after restoration

## Critical Bug Fixes

### Bug #1: Bridge Not Restoring After Death
**Problem**: Bridge was permanently destroyed using `Destroy(gameObject)`

**Solution**:
- Changed to `gameObject.SetActive(false)` in BridgeCollapse.cs:109
- Added `destructibleObjects` list to CheckpointManager
- Checkpoints save/restore destructible active states

### Bug #2: Bridge Immediately Re-collapsing After Respawn
**Problem**: Player spawned inside bridge trigger, causing instant collapse

**Solution**:
- Added `OnEnable()` to BridgeCollapse.cs to reset state variables
- Implemented 0.5s safety delay after bridge re-enables
- Prevents collapse checks during safety window

### Bug #3: Bridge Collapsing When Player Alone (NOT Carrying Sheep)
**Problem**: When checkpoint was saved while carrying sheep, PlayerInteraction kept stale references after respawn

**Root Cause**:
1. Player saves checkpoint while carrying sheep
2. Player dies and respawns
3. CheckpointManager restores sheep to ground position
4. **But PlayerInteraction still had `currentSheep` reference and `currentItemType = Sheep`**
5. Bridge checked `IsCarryingSheep` → returned `True` even though sheep was on ground
6. Bridge collapsed incorrectly

**Solution**:
- Added `ForceDropCarriedItem()` method to PlayerInteraction.cs:265-293
- CheckpointManager calls this BEFORE restoring sheep positions (CheckpointManager.cs:211)
- Clears all carrying references: `currentSheep`, `currentLettuce`, `currentItemType`
- Notifies PlayerController to reset carrying state

## System Flow

### Save Checkpoint Flow
```
Player enters Checkpoint trigger
  → Checkpoint.OnTriggerEnter() detects Player tag
  → Checkpoint.ActivateCheckpoint()
  → CheckpointManager.SaveCheckpoint()
    → Saves player position/rotation
    → Saves sheep positions/states
    → Saves which destructibles are active (bridges, walls, etc.)
```

### Death & Respawn Flow
```
Player/Sheep falls into KillZone
  → KillZone.OnTriggerEnter() detects fall
  → DeathManager.OnDeath(cause)
  → DeathManager.DeathSequenceCoroutine()
    1. Freeze player input
    2. Fade screen to black (wait for complete)
    3. CheckpointManager.LoadLastCheckpoint()
       → Restore player position/rotation
       → ForceDropCarriedItem() to clear stale references ⭐
       → Restore sheep positions/states
       → Restore destructible objects (re-enable bridges)
    4. Fade screen from black (wait for complete)
    5. Unfreeze player input
```

### Bridge Collapse Flow
```
Player or Sheep enters bridge trigger
  → BridgeCollapse.OnTriggerEnter()
  → BridgeCollapse.CheckCollapse()
    → Check safety delay (< 0.5s? skip)
    → Check if player carrying sheep? → Collapse
    → Check if player + separate sheep both on bridge? → Collapse
  → BridgeCollapse.CollapseBridge()
    → gameObject.SetActive(false)
```

## Setup Instructions

### 1. Create Kill Zone
1. Create empty GameObject: "KillZone"
2. Position beneath level (e.g., Y = -10)
3. Add BoxCollider component
   - Check "Is Trigger"
   - Size to cover entire level area (e.g., 100 x 10 x 100)
4. Add KillZone.cs script
5. Assign Layer (optional): "KillZone"

### 2. Create Checkpoint System
1. Create empty GameObject: "CheckpointManager"
2. Add CheckpointManager.cs script
3. Assign references in Inspector:
   - Player: Drag player GameObject
   - All Sheep: Add sheep to list
   - Destructible Objects: Add bridges, breakable walls, etc.

### 3. Create Checkpoints
1. Create empty GameObject: "Checkpoint 1"
2. Add BoxCollider component
   - Check "Is Trigger"
   - Size: Large enough for player to walk through (e.g., 3 x 3 x 3)
3. Add Checkpoint.cs script
4. For first checkpoint: Check "Is Starting Checkpoint"
5. Duplicate for additional checkpoints throughout level

### 4. Create Death Manager
1. Create empty GameObject: "DeathManager"
2. Add DeathManager.cs script
3. Assign references in Inspector:
   - Screen Fader: Drag ScreenFader GameObject
   - Checkpoint Manager: Drag CheckpointManager GameObject
4. Settings:
   - Freeze Player On Death: ✓ (recommended)
   - Show Debug Logs: ✓ (for testing)

### 5. Setup Bridges
1. Select bridge GameObject
2. Add **TWO** colliders:
   - Normal Collider (Is Trigger: OFF) - walkable surface
   - Trigger Collider (Is Trigger: ON) - detection zone
3. Add BridgeCollapse.cs script
4. Add bridge to CheckpointManager's "Destructible Objects" list

### 6. Create Screen Fader (if not exists)
1. Create Canvas: "ScreenFaderCanvas"
2. Add full-screen black Image: "FadeImage"
3. Add ScreenFader.cs script to FadeImage
4. Configure fade duration (e.g., 1 second)

## Testing Checklist

- [ ] Player falls into KillZone → Death triggered, screen fades, respawns at checkpoint
- [ ] Sheep falls into KillZone → Death triggered, level resets
- [ ] Bridge collapses when player carrying sheep walks on it
- [ ] Bridge collapses when player + separate sheep both on bridge
- [ ] Bridge does NOT collapse when only player on bridge
- [ ] Bridge does NOT collapse when only sheep on bridge
- [ ] Bridge restores after death/respawn
- [ ] Bridge does NOT immediately re-collapse after restoration
- [ ] Checkpoint saves when player enters for first time
- [ ] Checkpoint does NOT re-save when player re-enters
- [ ] Player carrying sheep at checkpoint → After respawn, sheep is on ground (not carried)
- [ ] All sheep positions restore correctly
- [ ] Multiple checkpoints work (saves latest one)

## Debug Logs

All systems include debug logs with prefixes:
- `[BRIDGE]` - Bridge collapse system
- `[PLAYER]` - Player interaction state
- `KillZone:` - Fall detection
- `CheckpointManager:` - Save/load operations
- `DeathManager:` - Death sequence
- `Checkpoint 'name':` - Individual checkpoint events

Enable/disable via Inspector checkboxes in each script.

## Known Limitations

1. **Single sheep support**: System handles one sheep, but multiple sheep in list (for future expansion)
2. **No mid-air saves**: Checkpoints only trigger on ground entry
3. **Bridge doesn't animate**: Instant disable (no collapse animation)
4. **No victory condition yet**: System only handles failures

## Future Enhancements

- [ ] Add bridge collapse animation
- [ ] Add sound effects for death/respawn
- [ ] Add respawn particle effects
- [ ] Support multiple sheep in level
- [ ] Add checkpoint activation feedback (particles, sound)
- [ ] Add autosave intervals
- [ ] Add manual save/load menu
- [ ] Track death statistics (falls, catches, etc.)

## Files Modified/Created

**Created**:
- `Assets/Script/KillZone.cs` - Fall detection
- `Assets/Script/Checkpoint.cs` - Individual checkpoint triggers
- `Assets/Script/CheckpointManager.cs` - Save/load system
- `Assets/Script/DeathManager.cs` - Death sequence coordinator
- `Assets/Script/ScreenFader.cs` - Screen fade transitions

**Modified**:
- `Assets/Script/BridgeCollapse.cs` - Added restoration support, safety delay, state reset
- `Assets/Script/PlayerInteraction.cs` - Added ForceDropCarriedItem() method
- `Assets/Script/CheckpointManager.cs` - Added destructible tracking, force drop on load

**Updated**:
- `CLAUDE.md` - Added checkpoint system to project reference
- `KILLZONE_CHECKPOINT_GUIDE.md` - This file
