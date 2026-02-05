# Checkpoint Spawn & Progress System

## Overview
Enhanced checkpoint system that:
1. ✅ Allows saving while carrying sheep
2. ✅ Spawns player/sheep from above checkpoint (portal effect)
3. ✅ Distinguishes between Hazard and Progress destructibles
4. ✅ Persists progress (walls stay broken) while respawning hazards (bridges restore)

## Key Changes

### 1. Checkpoint Spawn Points

**New Fields in Checkpoint.cs:**
- `spawnPoint` (Transform) - Optional spawn location (if null, uses checkpoint position)
- `spawnHeight` (float) - How high above spawn point to drop from (default: 2m)

**Behavior:**
- Player spawns at checkpoint position + spawnHeight (falls into scene like a portal)
- Sheep spawn scattered around checkpoint (±1.5m offset) from same height
- Creates visual "portal drop" effect

**Setup:**
```
Checkpoint GameObject
├── BoxCollider (trigger) - Detection zone
├── Checkpoint.cs
└── SpawnPoint (child GameObject) - Optional spawn location
```

### 2. Destructible Classification System

**New Enum: DestructibleType**
```csharp
public enum DestructibleType
{
    Hazard,      // Respawns on death (bridges, traps)
    Progress,    // Stays destroyed once broken (walls, obstacles)
    Consumable   // Future: Respawns on checkpoint, not death
}
```

**BridgeCollapse.cs Changes:**
- Added `destructibleType` field (default: Hazard)
- Public property: `Type => destructibleType`

**Checkpoint Save Logic:**
- **Hazards**: Saves which ones are active (most are active)
- **Progress**: Saves which ones are destroyed (persists player progress)

**Checkpoint Load Logic:**
- **Hazards**: Restore to checkpoint state (usually active → bridge respawns)
- **Progress**: Keep destroyed if they were broken (wall stays gone)

### 3. Save While Carrying

**Old Behavior:** Checkpoint skipped if player carrying sheep

**New Behavior:**
- Checkpoint saves even when carrying sheep
- On death/respawn:
  1. Player drops carried item (via ForceDropCarriedItem)
  2. Sheep spawns near checkpoint from above
  3. Player spawns from above at checkpoint
  4. Both fall to ground together

**Why This Works:**
- Tracks progress: "Player reached checkpoint with sheep"
- Prevents soft-lock: Sheep isn't stuck in carried state
- Creates immersive "portal rescue" effect

## Example Scenarios

### Scenario 1: Carrying Sheep Across Bridge
```
1. Player picks up sheep
2. Player walks across bridge (holding sheep)
3. Player touches checkpoint → SAVES ✅
4. Bridge collapses (player + sheep fall)
5. Death triggered
6. Respawn:
   - Bridge RESPAWNS (Hazard type)
   - Player spawns at checkpoint from above
   - Sheep spawns near checkpoint from above
   - Both fall to ground
7. Player can try again
```

### Scenario 2: Breaking Through Wall
```
1. Player destroys wall (Progress type)
2. Player touches checkpoint → SAVES ✅
3. Player dies later
4. Respawn:
   - Wall STAYS BROKEN (Progress type)
   - Player spawns at checkpoint
   - Progress preserved
```

### Scenario 3: Mixed Destructibles
```
Level has:
- 2 bridges (Hazard)
- 1 breakable wall (Progress)
- 1 collapsing floor (Hazard)

Player breaks wall, crosses bridge 1, saves checkpoint.

Player dies on bridge 2.

Respawn:
- Wall: STAYS BROKEN ✅ (Progress)
- Bridge 1: RESPAWNS ✅ (Hazard)
- Bridge 2: RESPAWNS ✅ (Hazard)
- Floor: RESPAWNS ✅ (Hazard)
```

## Setup Instructions

### 1. Update Existing Checkpoints

For each checkpoint in your scene:

1. Select checkpoint GameObject
2. **Add spawn point (recommended):**
   - Create child empty GameObject: "SpawnPoint"
   - Position where you want player to land (on ground)
   - Drag to Checkpoint.cs → Spawn Point field
3. **Set spawn height:**
   - Default: 2m (good for short drop)
   - Increase for dramatic portal effect (e.g., 5m)

**Gizmos:**
- Cyan wire sphere = spawn landing position
- Magenta wire sphere = spawn drop position (above)
- Magenta line = drop path

### 2. Classify Destructibles

For each destructible (bridges, walls, etc.):

1. Select destructible GameObject
2. Find BridgeCollapse.cs component
3. Set **Destructible Type:**
   - **Hazard** - Bridges, spike traps, collapsing floors (respawn on death)
   - **Progress** - Breakable walls, obstacles to remove (stay destroyed)
   - **Consumable** - Crates, barrels (future: respawn on checkpoint)

### 3. Assign to CheckpointManager

1. Select CheckpointManager GameObject
2. Add ALL destructibles to "Destructible Objects" list
3. System will automatically classify by type

## Testing Checklist

- [ ] Checkpoint saves while carrying sheep ✅
- [ ] Player spawns from above at checkpoint (not frozen mid-air)
- [ ] Sheep spawns scattered near checkpoint from above
- [ ] Hazard destructibles (bridges) respawn on death
- [ ] Progress destructibles (walls) stay destroyed after death
- [ ] Player/sheep fall naturally from spawn height
- [ ] Multiple checkpoints work correctly
- [ ] Gizmos show spawn positions in Scene view

## Visual Feedback (Future Enhancement)

To make the "portal spawn" more obvious:

1. **Particle effect** at spawn position
2. **Sound effect** when spawning
3. **Brief invincibility** after spawn (0.5s grace period)
4. **Camera shake** on landing

## Debug Logs

Enable debug logs in CheckpointManager:
- `CheckpointManager: Checkpoint 'X' saved! Sheep tracked: Y, Active hazards: Z, Destroyed progress: W`
- `CheckpointManager: Using checkpoint 'X' spawn position: (coords)`
- `CheckpointManager: HAZARD 'Bridge' set active = true`
- `CheckpointManager: PROGRESS 'Wall' set active = false (was destroyed: true)`

## Code Files Modified

**Modified:**
- `Assets/Script/Checkpoint.cs` - Added spawn point and spawn height
- `Assets/Script/BridgeCollapse.cs` - Added DestructibleType enum and classification
- `Assets/Script/CheckpointManager.cs` - Spawn point logic, destructible type handling

**New:**
- `CHECKPOINT_SPAWN_SYSTEM.md` - This file

## Benefits

1. **Better gameplay flow**: No more "checkpoint skip" when carrying sheep
2. **Progress persistence**: Destroyed walls stay destroyed
3. **Hazard balance**: Bridges respawn (prevents soft-locking)
4. **Immersive respawn**: Portal drop effect feels intentional
5. **Designer control**: Can classify each destructible individually
6. **Future-proof**: Consumable type ready for crates/pickups

## Future Improvements

- [ ] Add Consumable destructible behavior (respawn on checkpoint, not death)
- [ ] Add visual/audio feedback for portal spawn
- [ ] Track rescued sheep count (for multi-sheep levels)
- [ ] Add inventory persistence when inventory system is added
- [ ] Create checkpoint activation animation (particles, sound)
