# Sheep Stealth Puzzle Game - Project Reference

**Last Updated:** 2026-01-21

## Quick Status Overview

**What's Playable Right Now:**
- ✅ Player movement (walk, run, crouch, jump) with camera-relative controls
- ✅ Pick up and carry sheep/lettuce, drop them
- ✅ Sheep AI attracts to lettuce, walks to it, eats it
- ✅ Lettuce spawner maintains resource availability
- ✅ Bridge hazards collapse when conditions met
- ✅ Stationary guard with vision cone detection and chase
- ✅ Win zone completes level when sheep enters

**What's Missing for Core Gameplay:**
- ⏳ Territory/return system (sheep go back home if not rescued quickly)
- ⏳ Kill zones and checkpoints (fail states for falling)
- ⏳ Patrolling guards (currently only stationary)
- ⏳ Advanced gadgets (Salad Gun, Fan, Perfume)
- ⏳ UI polish and visual feedback

**Project Completion:** ~60% (core mechanics done, needs systems integration and polish)

---

## Project Overview

This is a Unity 3D stealth puzzle game inspired by "Sheep, Dog 'n' Wolf" (2001). The player controls a wolf-like character who must steal sheep from a guarded flock and deliver them to a goal zone while avoiding detection by a patrolling guard dog.

## Core Gameplay Loop

```
Scout Level → Collect Gadgets → Lure Sheep → Avoid Guard → Deliver to Goal
```

## Key Design Principles

1. **Deterministic Puzzles**: Each level has intended solutions, no random elements
2. **No Combat**: Player cannot defeat the guard, only evade or distract
3. **Gadget Synergies**: Items combine for emergent effects (e.g., Fan + Perfume)
4. **Cartoon Logic**: Physics and interactions follow "Looney Tunes" rules

## Technical Architecture

### Code Organization Philosophy

**IMPORTANT ARCHITECTURAL DECISION**: We use **component-based separation** to keep scripts independent, granular, and maintainable.

#### Script Separation Strategy:
- **Each script has ONE clear responsibility** (Single Responsibility Principle)
- Scripts should be **independent** and communicate via references
- This approach enables easier debugging, testing, and collaboration
- Prevents monolithic "God scripts" that do everything

#### Player Script Structure:
```
Player (GameObject)
├── CharacterController
├── PlayerController.cs        ← Movement, jump, gravity, crouch, run
├── PlayerInteraction.cs       ← Pickup/drop sheep with E key
└── PlayerInput                ← Unity Input System component
```

#### Guard Script Structure:
```
Guard (GameObject)
├── NavMeshAgent
├── GuardStationary.cs         ← Main state machine: Idle, Alert, Chasing
├── GuardChase.cs              ← Chase behavior and player catching
├── GuardVisionCone.cs         ← Vision detection with raycast
├── GuardHearingZone.cs        ← Spherical hearing trigger
└── FollowingZone.cs           ← Detection zone trigger
```

**Why this works:**
- If interaction breaks → check `PlayerInteraction.cs` only
- If movement feels wrong → check `PlayerController.cs` only
- Can disable/enable features independently during testing
- Each script stays small, focused, and readable

### Movement Components

| Entity | Component | Reason |
|--------|-----------|--------|
| Player | CharacterController | Precise control, no physics sliding |
| Sheep | CharacterController | Consistent behavior, predictable movement |
| Guard | NavMeshAgent | Pathfinding for patrol and chase |

### Gravity Handling

Player and Sheep use **manual gravity** applied in code:

```csharp
if (controller.isGrounded)
    velocity.y = -2f;
else
    velocity.y += gravity * Time.deltaTime;

controller.Move((movement + velocity) * Time.deltaTime);
```

### Camera System

Uses **Cinemachine FreeLook** for third-person camera:
- Orbits around player with mouse input
- Player movement is **camera-relative** (W = forward toward camera view)
- Player smoothly rotates to face movement direction
- Camera can rotate independently while player stands still

## Entity Systems

### Player States

- **Walking**: Normal speed, some noise
- **Sneaking**: Slow speed, silent (hold Shift/L1)
- **Running**: Fast speed, loud (guard hears)
- **Carrying**: Holding sheep, reduced speed

### Sheep States

**Files:** `Sheep.cs`, `SheepAttraction.cs`

**Currently Implemented States:**
- **Idle**: Standing still, waiting for attractions
- **Sniffing**: Detected attraction, brief delay before moving (configurable)
- **Walking**: Moving toward attraction source using `Rigidbody.MovePosition()`
- **Eating**: At lettuce location, 2-second eating duration (configurable), destroys lettuce
- **Carried**: Being held by player

**Attraction System (COMPLETE):**
- Detects closest lettuce within configurable radius
- Handles multiple lettuce sources (always picks closest)
- Ignores carried lettuce
- Walks to lettuce → Eats it → Destroys it → Returns to Idle
- Complete state machine: Idle → Sniffing → Walking → Eating → Idle

**Pickup Mechanics:**
- When picked up: collider disabled, parented to player's carry position
- When dropped: unparented, collider re-enabled, kinematic physics

**Physics Setup:**
- Uses **kinematic Rigidbody** for:
  - Trigger detection (OnTriggerEnter in WinZone)
  - Smooth movement via `Rigidbody.MovePosition()`
  - Arcade-style physics (no bouncing/sliding)
- Rotation constraints: X/Z locked to prevent tilting
- Ground detection with emergency fallback
- Falls when ground disappears

**Missing Features:**
- **Territory/Return System** (planned): Sheep should return to home position if inside territory zone and no new attractions appear within 5 seconds after eating

### Guard States (Stationary Version)

Currently implemented as a **stationary guard** that watches over sheep:

- **Idle**: Head slowly rotates left/right scanning area, vision cone active
- **Alert**: Player detected in vision cone, brief delay before chase
- **Chasing**: NavMesh pursuit of player, catches player on contact
- **Return**: (Not yet implemented - guard continues chasing until player escapes)

**Guard Components:**
- `GuardStationary.cs`: Main state machine managing Idle/Alert/Chasing states with head rotation
- `GuardChase.cs`: Handles NavMesh-based pursuit and player catching
- `GuardVisionCone.cs`: Cone-shaped detection using angle check + raycast for line-of-sight
- `GuardHearingZone.cs`: Spherical trigger for hearing detection (not yet connected to player states)
- `FollowingZone.cs`: General detection zone trigger

**Future Guard Types:**
- **Patrol**: Walking between waypoints (planned)
- **Return**: Going back to patrol route after losing player (planned)

## Detection System

### Current Implementation

**Vision Cone Detection** (`GuardVisionCone.cs`):
- Uses angle check to see if player is within field of view
- Performs raycast for line-of-sight verification
- Ignores detection if obstacles block view
- Configurable vision angle and range
- Debug visualization with colored rays

**Hearing Zone** (`GuardHearingZone.cs`):
- Spherical trigger around guard
- Detects player entry/exit
- Not yet integrated with player movement states (running/sneaking)

**State-Based Detection** (`GuardStationary.cs`):
- Idle: Scans area with rotating head, checks vision cone continuously
- Alert: Brief delay when player spotted before transitioning to chase
- Chasing: Active pursuit via NavMesh

### Planned Detection Features

- **Movement Noise**: Running alerts guard, sneaking is silent
- **Visual Indicator UI**: Show player when they're being detected
  1. **None**: No indicator (too far)
  2. **Near**: Green icon (player in hearing range)
  3. **Hidden**: Orange icon (in detection zone but concealed)
  4. **Detected**: Red icon (guard sees player, chase begins)

## Gadget System

### Currently Implemented Items

1. **Lettuce** ✅
   **Files:** `Lettuce.cs`
   - Pickup/carry/drop mechanics (E key via PlayerInteraction)
   - Rigidbody with gravity for realistic dropping
   - Physics disabled when carried (kinematic + no gravity)
   - Physics enabled when dropped (non-kinematic + gravity)
   - Collider disabled when carried to prevent sheep attraction
   - Attracts sheep within configurable radius
   - Sheep walks to it, eats it (2-second eating duration), destroys it

2. **Lettuce Garden** ✅
   **Files:** `LettuceGarden.cs`
   - Spawner system that maintains fixed number of lettuces per level (default: 4)
   - Automatically respawns destroyed/eaten lettuces
   - Random spawn positions within configurable radius
   - Ground detection for accurate placement
   - Prevents soft-locking (player always has resources)

### Planned Items

3. **Salad Gun** (Not Started)
   - Shoots lettuce at distant locations via raycast
   - Limited ammo per level

4. **Fan** (Not Started)
   - Creates wind zone (BoxCollider trigger)
   - Interacts with items implementing `IFanInteractable` interface
   - Passes wind direction to affected items

5. **Perfume** (Not Started)
   - Sits on ground, idle by default
   - When hit by fan wind, creates a growing **scent beam**
   - Beam is a directional BoxCollider that attracts sheep from distance
   - Beam grows while wind is active (~2 sec to max length)
   - Beam shrinks when wind stops

### Gadget Synergy Example (Planned)

```
Fan (active) → Perfume (idle) → Scent Beam (extends in wind direction) → Sheep (attracted from far away)
```

## Level Components

### Win Zone ✅
**Files:** `WinZoneCheck.cs`

**Implemented:**
- Trigger volume at level exit
- Detects when sheep enters the zone (checks "Sheep" tag)
- Triggers level complete event
- Debug logging for victory condition

**Planned Enhancements:**
- Victory UI display
- Level transition/loading
- Victory sound effects
- Player movement freeze on win

### Level Hazards ✅
**Files:** `BridgeCollapse.cs`

**Implemented:**
- Bridge collapses when player carrying sheep walks on it
- Bridge collapses when player AND separate sheep are both on bridge
- Trigger detection for player and sheep
- Debug visualization showing detection state

**Configuration:**
- Requires TWO colliders on bridge:
  - Normal collider (Is Trigger OFF) - provides walkable surface
  - Trigger collider (Is Trigger ON) - detects entities
- Trigger collider must be tall enough to intersect floating player capsule

### Kill Zone & Checkpoint System (Planned - High Priority)
**Concept:**
Large plane collider beneath the entire level that detects if player or sheep falls off the map.

**Kill Zone Behavior:**
- Positioned beneath level (e.g., Y = -10)
- Large BoxCollider trigger covering entire level area
- Detects Player OR Sheep falling through
- Player falls → FAIL STATE → reload checkpoint
- Any sheep falls → FAIL STATE → reload checkpoint

**Checkpoint System:**
- Save points throughout level
- Stores player position, carried items, sheep positions
- On fail: Reload level state from last checkpoint
- First checkpoint = level start position

**Files Needed:** `KillZone.cs`, `Checkpoint.cs`, `CheckpointManager.cs`

### Territory Zone System (Planned - High Priority)
**Concept:**
A "Territory Zone" (large BoxCollider trigger) marks the guard patrol area. Sheep behavior changes based on whether they're inside or outside this zone.

**Inside Territory Zone:**
1. Sheep follows attraction (lettuce/scent) normally
2. After eating, waits 5 seconds for new attractions
3. If no new attraction appears → returns to home spawn position
4. If player picks up sheep during 5-second window → sheep "rescued"

**Outside Territory Zone:**
1. Sheep permanently "freed" from territory memory
2. After eating → stays at current location
3. Only moves if new attraction appears
4. No returning behavior

**Puzzle Design:**
Player must either:
- Lure sheep completely out of territory (safe), OR
- Pick up sheep within 5-second window after eating (risky, near guards)

**Files Needed:** `TerritoryZone.cs`, modifications to `SheepAttraction.cs`

### Patrol Path (Planned)
- Not yet implemented (using stationary guard for now)
- Empty GameObject with Transform waypoints as children
- Guard follows waypoints in sequence with loop or ping-pong

**Files Needed:** `PatrolPath.cs`

## Actual Project Structure

```
Assets/Script/
├── PlayerController.cs          ← Player movement (walk, run, crouch, jump) ✅
├── PlayerInteraction.cs         ← Pickup/drop sheep/lettuce with E key ✅
├── Sheep.cs                     ← Basic sheep state (idle/carried) ✅
├── SheepAttraction.cs           ← Sheep AI: attraction, walking, eating ✅
├── Lettuce.cs                   ← Lettuce item pickup/drop mechanics ✅
├── LettuceGarden.cs             ← Lettuce spawner system ✅
├── BridgeCollapse.cs            ← Bridge hazard detection ✅
├── WinZoneCheck.cs              ← Goal zone detection ✅
└── Guard Scripts/
    ├── GuardStationary.cs       ← Main guard state machine ✅
    ├── GuardChase.cs            ← Chase and catch behavior ✅
    ├── GuardVisionCone.cs       ← Vision detection ✅
    ├── GuardHearingZone.cs      ← Hearing detection ✅
    └── FollowingZone.cs         ← General detection zone ✅
```

**Also in project root:**
- `CLAUDE.md` - This file (project reference and architecture)
- `PROJECT_STATUS.md` - Detailed status tracking and completion estimates
- `SETUP_GUIDE.md` - Setup instructions
- `SHEEP_PICKUP_SETUP.md` - Sheep pickup configuration guide

### Future Organization
```
Scripts/
├── Player/          (Movement, interaction, inventory)
├── AI/              (Guard behavior, patrol, detection)
├── Sheep/           (Behavior, attraction system)
├── Items/           (Base class, individual gadgets, synergies)
├── Systems/         (Detection, goal zone, game manager)
└── UI/              (Detection indicator, inventory display)
```

## Implementation Philosophy

- **Component-based architecture**: Each script has one clear responsibility
- **Granular separation**: Independent scripts that communicate via references
- **Use ScriptableObjects** for entity settings (guard ranges, sheep speeds)
- **Centralized Detection System** (singleton for simplicity in prototype)
- **Interface-based synergies** (IFanInteractable for clean gadget interactions)
- **Event-driven level completion** (UnityEvents on GoalZone)
- **Gizmos for debug visualization** (patrol paths, vision cones, attraction radii)

### Script Communication Pattern

Scripts reference each other via `GetComponent<>()`:

```csharp
// Example: PlayerInteraction.cs needs to check if player is moving
public class PlayerInteraction : MonoBehaviour
{
    private PlayerController controller;
    private PlayerInventory inventory;

    void Start()
    {
        controller = GetComponent<PlayerController>();
        inventory = GetComponent<PlayerInventory>();
    }

    public void OnUse()
    {
        // Can check controller state
        if (controller.moveInput.magnitude > 0)
            return; // Can't use while moving

        // Can access inventory
        inventory.UseCurrentItem();
    }
}
```

## Development Priorities

### Current Status (✅ = Complete, 🔨 = In Progress, ⏳ = Not Started)

**Core Systems:**
1. ✅ **Player Movement**: Camera-relative controls, jump, crouch, run, carry speed modifier
2. ✅ **Player Interaction**: Pickup/carry/drop sheep and lettuce with E key
3. ✅ **Sheep Basic Mechanics**: Pickup/carry/drop, kinematic physics, ground detection
4. ✅ **Sheep Attraction System**: Detects lettuce, walks to it, eats it, state machine complete
5. ✅ **Lettuce System**: Pickup/drop mechanics, gravity physics, spawner system
6. ✅ **Level Hazards**: Bridge collapse system working
7. ✅ **Win Condition**: Win zone detects sheep and triggers victory
8. ✅ **Basic Guard AI**: Stationary guard with vision cone, head rotation, chase, and catch

**In Progress:**
9. 🔨 **Guard Testing & Polish**: Needs playtesting, visual feedback, parameter tuning

**High Priority - Not Started:**
10. ⏳ **Territory/Return System**: Sheep return home if inside territory and no new attractions
11. ⏳ **Kill Zone & Checkpoints**: Fail states for falling off map
12. ⏳ **Patrolling Guard**: Moving guard with waypoint system

**Medium/Low Priority:**
13. ⏳ **Advanced Gadgets**: Salad Gun, Fan + Perfume synergy
14. ⏳ **Level Design**: Complete test level with cover and obstacles
15. ⏳ **UI Systems**: Detection indicators, inventory display, victory screen
16. ⏳ **Polish**: Animations, sound effects, particle effects

### Implementation Phases

**Phase 1: Foundation (COMPLETE ✅)**
- PlayerController.cs with camera-relative movement
- Manual gravity system
- Jump mechanics
- Cinemachine FreeLook camera integration
- Crouch (Ctrl) and Run (Shift) states with speed multipliers

**Phase 2: Interaction (COMPLETE ✅)**
- PlayerInteraction.cs for E key pickup/drop
- Sheep.cs for basic sheep state (idle/carried)
- Pickup/carry/drop mechanics working
- Speed modifier when carrying sheep (70%)
- Arcade-style with kinematic Rigidbody

**Phase 3: Guard AI - Stationary (COMPLETE ✅)**
- GuardStationary.cs state machine (Idle/Alert/Chasing)
- GuardChase.cs for NavMesh pursuit and catching
- GuardVisionCone.cs for cone-based detection with raycast
- GuardHearingZone.cs for proximity detection
- Head rotation animation while in Idle state
- WinZoneCheck.cs for level completion

**Phase 4: Guard Testing & Polish (CURRENT)**
- Test guard in realistic gameplay scenario
- Add visual feedback for guard states (material colors, UI indicators)
- Tune detection parameters (angles, ranges, delays)
- Integrate hearing with player movement states (running = loud, sneaking = silent)
- Build simple test level with cover and obstacles

**Phase 5: Lettuce & Sheep Attraction (COMPLETE ✅)**
- Lettuce.cs for pickup/drop mechanics
- LettuceGarden.cs spawner system
- SheepAttraction.cs state machine (Idle/Sniffing/Walking/Eating)
- Attraction radius detection
- Closest lettuce selection
- Eating and destruction mechanics

**Phase 6: Level Hazards (COMPLETE ✅)**
- BridgeCollapse.cs trigger detection
- Detects player carrying sheep OR player + sheep separately
- Debug visualization

**Phase 7: Guard Testing & Polish (CURRENT)**
- Test guard in realistic gameplay scenario
- Add visual feedback for guard states (material colors, UI indicators)
- Tune detection parameters (angles, ranges, delays)
- Integrate hearing with player movement states (running = loud, sneaking = silent)
- Build simple test level with cover and obstacles

**Phase 8: Territory System (NEXT - High Priority)**
- Create TerritoryZone.cs trigger zone
- Modify SheepAttraction.cs for return-to-home behavior
- 5-second idle timer after eating
- Territory exit detection (sheep freed permanently)

**Phase 9: Kill Zones & Checkpoints (High Priority)**
- KillZone.cs for fall detection
- CheckpointManager.cs for save/load system
- Checkpoint.cs for individual save points
- Fail states and level restart

**Phase 10: Patrolling Guard**
- Create waypoint patrol system
- Implement patrol → chase → return-to-patrol flow
- Add multiple guard variations

**Phase 11: Advanced Gadgets**
- Create item base classes
- Implement Salad Gun (shoots lettuce)
- Implement Fan (wind zones)
- Implement Perfume (scent beams)
- Add gadget synergies (Fan + Perfume)
- Test interaction flows

**Phase 12: UI & Polish**
- Detection indicator UI
- Inventory display
- Victory screen and level transitions
- Animations
- Sound effects
- Particle effects

## Future Considerations

- Multiple guards per level?
- Multiple sheep to collect?
- Checkpoint system for mid-level respawns?
- Inventory capacity limits?
- Guard distraction mechanics (throw items to create noise)?

---

**Core Loop in Action**: Player scouts level → places lettuce to lure sheep away from guard → uses fan + perfume to create long-range scent beam → sheep follows scent → player picks up sheep → sneaks past guard to goal zone → level complete!
