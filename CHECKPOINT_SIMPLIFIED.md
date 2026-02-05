# Simplified Checkpoint System

## What Changed

The checkpoint system has been simplified to follow **Single Responsibility Principle**:

- ❌ **Removed:** DestructibleType enum from BridgeCollapse.cs
- ❌ **Removed:** Dropdown menu for destructible types
- ✅ **Simplified:** BridgeCollapse only handles bridges (always respawns on death)
- ✅ **Future-ready:** When you add walls, create BreakableWall.cs as a separate script

## Current System

### Bridges (BridgeCollapse.cs)
- **Behavior:** Always respawn on death
- **Why:** Prevents soft-locking (player needs bridge to cross)
- **CheckpointManager field:** `bridges` list

### Future: Breakable Walls (Not Yet Implemented)
When you add breakable walls, you'll create:
- **New script:** `BreakableWall.cs`
- **Behavior:** Stay destroyed once broken (progress)
- **CheckpointManager field:** Add `breakableWalls` list
- **Restoration logic:** Separate handling in LoadLastCheckpoint()

## Setup Instructions

### 1. Checkpoint Setup (Same as Before)
```
Checkpoint GameObject
├── BoxCollider (trigger)
├── Checkpoint.cs
│   ├── Spawn Point: [Optional child GameObject]
│   └── Spawn Height: 2.0
└── SpawnPoint (optional child)
```

### 2. Bridge Setup
```
Bridge GameObject
├── Collider (Is Trigger: OFF) - walkable surface
├── Collider (Is Trigger: ON) - detection zone
└── BridgeCollapse.cs
```

### 3. CheckpointManager Setup
```
CheckpointManager GameObject
└── CheckpointManager.cs
    ├── Player: [Player GameObject]
    ├── All Sheep: [List of sheep]
    └── Bridges: [List of bridge GameObjects]  ← Changed from "Destructible Objects"
```

## Code Architecture

### Current Implementation (Bridges Only)

**BridgeCollapse.cs:**
- Single purpose: Handle bridge collapse
- No enum, no classification
- Always treated as "hazard" (respawns)

**CheckpointManager.cs:**
- Saves which bridges were active
- On death: Restores bridges to checkpoint state

### Future Implementation (When You Add Walls)

**Step 1: Create BreakableWall.cs**
```csharp
using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    public void Break()
    {
        // Wall destruction logic
        gameObject.SetActive(false);
    }
}
```

**Step 2: Update CheckpointManager.cs**
```csharp
[SerializeField] private List<GameObject> bridges;        // Respawn on death
[SerializeField] private List<GameObject> breakableWalls; // Stay destroyed

// In CheckpointData:
public List<GameObject> activeBridges;
public List<GameObject> destroyedWalls; // Walls that have been broken

// In SaveCheckpoint:
// Save bridges (active ones)
// Save walls (destroyed ones)

// In LoadLastCheckpoint:
// Bridges: Restore to checkpoint state (usually active)
// Walls: Keep destroyed if in destroyedWalls list
```

**Step 3: Different Restoration Logic**
```csharp
// Bridges always respawn
foreach (GameObject bridge in bridges)
{
    bool shouldBeActive = activeBridges.Contains(bridge);
    bridge.SetActive(shouldBeActive);
}

// Walls stay destroyed if broken
foreach (GameObject wall in breakableWalls)
{
    bool wasDestroyed = destroyedWalls.Contains(wall);
    wall.SetActive(!wasDestroyed);
}
```

## Why This Is Better

### Before (With Enum)
❌ BridgeCollapse.cs handles bridges, walls, crates
❌ Confusing dropdown for "bridge types"
❌ Violates single responsibility
❌ Hard to extend (what if walls have different behavior?)

### After (Simplified)
✅ BridgeCollapse.cs only handles bridges
✅ No confusing dropdown
✅ Each destructible type gets its own script
✅ Easy to extend (just add new script + list)

## Benefits

1. **Cleaner code:** Each script has one clear purpose
2. **No confusion:** "Bridge" means bridge, not "destructible that might be a bridge"
3. **Easier testing:** Test bridges separately from walls
4. **Better organization:** Inspector shows separate lists for different types
5. **Future-proof:** Adding new destructible types doesn't modify existing scripts

## Migration from Old System

If you previously used the enum system:

1. ✅ Enum removed - no action needed
2. ✅ "Destructible Objects" field → "Bridges" field
3. ✅ Re-assign bridges in Inspector to new "Bridges" list
4. ✅ All bridges now always respawn (no dropdown to configure)

## Current Features

- ✅ Checkpoints save while carrying sheep
- ✅ Portal spawn effect (player/sheep fall from above)
- ✅ Bridges respawn on death
- ✅ Sheep spawn scattered near checkpoint
- ✅ Save/load player position from checkpoint spawn point

## Future Features (When You Add Walls)

- ⏳ Create BreakableWall.cs script
- ⏳ Add `breakableWalls` list to CheckpointManager
- ⏳ Track destroyed walls in checkpoint data
- ⏳ Walls stay destroyed after checkpoint load

## Testing Checklist

- [ ] Checkpoint saves while carrying sheep ✅
- [ ] Player spawns from above at checkpoint ✅
- [ ] Sheep spawn scattered near checkpoint ✅
- [ ] Bridges respawn when player dies ✅
- [ ] Multiple checkpoints work correctly ✅
- [ ] Bridges field in CheckpointManager has all bridges assigned

## Example: Adding Breakable Walls Later

When you're ready to add walls:

**1. Create the script:**
```bash
Assets/Script/BreakableWall.cs
```

**2. Implement basic destruction:**
```csharp
public class BreakableWall : MonoBehaviour
{
    public void Break()
    {
        Debug.Log($"Wall {name} destroyed!");
        gameObject.SetActive(false);
    }
}
```

**3. Update CheckpointManager:**
- Add `[SerializeField] private List<GameObject> breakableWalls;`
- In `CheckpointData`, add `public List<GameObject> destroyedWalls;`
- In `SaveCheckpoint()`, track destroyed walls
- In `LoadLastCheckpoint()`, keep destroyed walls disabled

**4. Assign in Inspector:**
- Add all wall GameObjects to CheckpointManager's "Breakable Walls" list

That's it! Clean separation of concerns.
