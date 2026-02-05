# Guard Head Tracking & Death System - Changelog

**Date:** 2026-01-28

## Summary

Implemented guard head tracking system with 3D model support and fixed all guard respawn bugs. The guard now has realistic head movement that scans during patrol and locks onto the player during chase. All death/respawn issues have been resolved.

---

## Features Implemented

### 1. Guard Head Tracking System

**Files Modified:**
- `GuardStationary.cs`

**New Features:**
- Added `Transform player` field for player reference (auto-finds by tag)
- Added `headTrackingSpeed` parameter for smooth tracking (default: 5)
- Added `LookAtPlayer()` method that:
  - Calculates direction from head to player (horizontal plane only)
  - Smoothly rotates head using Quaternion.Slerp
  - Works in local space relative to guard body
  - Updates continuously during Alert and Chasing states

**Behavior:**
- **Guarding State**: Head rotates left/right scanning (existing behavior)
- **Alert State**: Head stops scanning and locks onto player
- **Chasing State**: Head continuously tracks player during movement

**Unity Setup:**
- Assign separate head Transform in Inspector (from 3D model hierarchy)
- Player auto-detected by "Player" tag
- Configurable tracking speed

---

### 2. Guard Respawn Bug Fixes

**Problem 1: Pending Invoke() Restart**
- **Issue**: Guard would restart chase after respawn due to pending `Invoke(nameof(StartChasing))` call
- **Fix**: Added `CancelInvoke(nameof(StartChasing))` to `StopChasing()` method in `GuardStationary.cs`

**Problem 2: Wrong Checkpoint Position**
- **Issue**: Guards were saved at current position when checkpoint activated (during chase), not spawn position
- **Fix**: Modified `CheckpointManager.cs` to:
  - Store initial guard positions in `initialGuardPositions` dictionary (permanent, never changes)
  - Skip saving guard positions in `SaveCheckpoint()` (only save on level start)
  - Always restore guards to initial positions in `LoadLastCheckpoint()`

**Problem 3: CharacterController State Interference**
- **Issue**: CharacterController internal state caused guard to drift away after position restore
- **Fix**: Disable CharacterController before setting position, re-enable after
  ```csharp
  controller.enabled = false;
  guard.transform.position = data.position;
  guard.transform.rotation = data.rotation;
  controller.enabled = true;
  ```

---

## Files Modified

### GuardStationary.cs
**New Fields:**
```csharp
[SerializeField] private Transform player; // Auto-finds by tag
[SerializeField] private float headTrackingSpeed = 5f;
```

**New Methods:**
```csharp
private void LookAtPlayer()
{
    // Calculate direction to player (horizontal only)
    // Smoothly rotate head using Quaternion.Slerp
    // Works in local space
}
```

**Modified Methods:**
- `Awake()`: Added player auto-detection
- `Update()`: Added `LookAtPlayer()` calls for Alert/Chasing states
- `StopChasing()`: Added `CancelInvoke(nameof(StartChasing))`

---

### CheckpointManager.cs
**New Fields:**
```csharp
private Dictionary<GameObject, GuardData> initialGuardPositions = new Dictionary<GameObject, GuardData>();
```

**Modified Methods:**
- `SaveInitialCheckpoint()`: Stores guard positions in both `initialGuardPositions` and `currentCheckpoint`
- `SaveCheckpoint()`: Removed guard position saving (no longer updates guard positions)
- `LoadLastCheckpoint()`:
  - Now restores guards from `initialGuardPositions` instead of checkpoint data
  - Disables/enables CharacterController during position change
  - Calls `StopChasing()` and `StopChase()` to reset state

---

### GuardChase.cs
**Changes:**
- Removed manual position reset logic (now handled by CheckpointManager)
- Simplified `CatchPlayer()` to call `DeathManager.OnDeath()`

---

## Design Decisions

### Why Guards Restore to Initial Position (Not Checkpoint Position)

**Problem:** If guards save position when checkpoint activates, and player activates checkpoint during chase, the guard would be saved at the "chased" position. On respawn, guard would be blocking the checkpoint.

**Solution:** Guards ALWAYS restore to their starting position, creating:
- Predictable gameplay (guards always reset to known locations)
- No "guard camping checkpoint" scenarios
- Cleaner puzzle design

**Implementation:**
- `initialGuardPositions` stores spawn positions at level start
- Never modified after level start
- `SaveCheckpoint()` doesn't update guard positions
- `LoadLastCheckpoint()` always uses initial positions

---

### Why Disable CharacterController During Teleport

**Problem:** CharacterController has internal state (velocity buffers, movement) that persists when you change `transform.position`. This causes the entity to "rubber-band" or drift away from the teleport destination.

**Solution:** Standard Unity pattern:
1. Disable CharacterController (clears internal state)
2. Set position/rotation
3. Re-enable CharacterController (fresh state)

This ensures clean teleportation without movement carryover.

---

### Why Cancel Invoke Calls

**Problem:** `Invoke(nameof(StartChasing), alertDelay)` schedules a delayed method call. If player dies during alert/chase, this call remains scheduled and executes after respawn, restarting the chase.

**Solution:** Call `CancelInvoke(nameof(StartChasing))` when stopping chase to clear all pending delayed calls.

---

## Testing Checklist

- [x] Guard head rotates during Guarding state
- [x] Guard head locks onto player during Alert state
- [x] Guard head tracks player during Chase state
- [x] Guard catches player → death sequence → respawn at checkpoint
- [x] Guard returns to initial spawn position (not chase position)
- [x] Guard stays at spawn position (no drifting)
- [x] Guard resumes Guarding state after respawn
- [x] No chase restart after respawn
- [x] Works with checkpoints activated during chase

---

## Unity Inspector Setup

### Guard GameObject
1. **GuardStationary Component:**
   - Assign `Head` transform (from 3D model hierarchy)
   - Optional: Assign `Player` transform (or let it auto-find)
   - Set `Head Tracking Speed` (default: 5, range: 3-7 recommended)
   - Set head rotation angles (`Min Angle`, `Max Angle`)

2. **GuardChase Component:**
   - No changes needed (position restoration handled by CheckpointManager)

### CheckpointManager GameObject
1. **CheckpointManager Component:**
   - Add guards to `Guards` list (size = number of guards)
   - Drag each guard GameObject into the list

---

## Known Limitations

- Head tracking only works on Y-axis (horizontal rotation)
- Assumes guard model faces forward (Z-axis)
- Works with CharacterController-based guards (not NavMeshAgent)

---

## Future Enhancements

- Add head rotation limits during tracking (prevent 180° snapping)
- Add smooth transition when switching from scanning to tracking
- Add head "anticipation" (look toward player slightly before alert)
- Support vertical head rotation (looking up/down)
