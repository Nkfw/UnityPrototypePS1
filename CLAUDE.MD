# Sheep Stealth Puzzle Game - Project Reference

**Last Updated:** 2026-01-28

## Quick Status Overview

**What's Playable Right Now:**
- ✅ Player movement (walk, run, crouch, jump) with camera-relative controls
- ✅ Player animations (idle, walk, run, jump) fully implemented with Animator Controller
- ✅ Pick up and carry sheep/lettuce, drop them
- ✅ Sheep AI attracts to lettuce, walks to it, eats it
- ✅ Lettuce spawner maintains resource availability with predefined spawn points
- ✅ Bridge hazards collapse when conditions met
- ✅ Stationary guard with vision cone detection, chase, and HEAD TRACKING
- ✅ Guard head rotates to scan during patrol, locks onto player during chase
- ✅ Win zone completes level when sheep enters
- ✅ Territory/return system (sheep return home if inside territory and no new attractions)
- ✅ Kill zones and checkpoints (full death/respawn system with guard position reset)

**What's Missing for Core Gameplay:**
- ⏳ Patrolling guards (currently only stationary)
- ⏳ Advanced gadgets (Salad Gun, Fan, Perfume)
- ⏳ UI polish and visual feedback

**Project Completion:** ~78% (core mechanics complete, guard AI polished, needs patrolling guards)

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
| Sheep | Rigidbody (Kinematic) | MovePosition for smooth movement, trigger detection |
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

**Animation System (COMPLETE ✅):**
- **Animator Controller** with Speed (Float) and IsJumping (Bool) parameters
- **Idle Animation**: Plays when Speed = 0
- **Walk/Run Animation**: Blend tree based on Speed value (0.3 = walk, 1.0 = run)
- **Jump Animation**: Triggered when IsJumping = true, returns to idle/walk when landed
- Smooth transitions with minimal Exit Time for responsive controls
- All animations integrated with PlayerController movement states

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
- **Scale preservation**: Stores original scale before parenting, restores after to prevent distortion

**Physics Setup:**
- Uses **kinematic Rigidbody** for:
  - Trigger detection (OnTriggerEnter in WinZone)
  - Smooth movement via `Rigidbody.MovePosition()`
  - Arcade-style physics (no bouncing/sliding)
- Rotation constraints: X/Z locked to prevent tilting
- **Advanced ground positioning**:
  - Bounds-based offset calculation in `Awake()` for scale independence
  - All raycasts use `QueryTriggerInteraction.Ignore` to avoid trigger colliders
  - Smooth terrain following with configurable skin width
  - Works correctly on flat ground, bridges, and varied terrain heights
- Ground detection with emergency fallback
- Falls when ground disappears

**Territory/Return System (COMPLETE):**
- Sheep returns to home position if inside territory zone and no new attractions
- 5-second thinking timer after eating
- Territory exit permanently "frees" sheep

### Guard States (Stationary Version)

Currently implemented as a **stationary guard** with full 3D model and head tracking:

- **Guarding (Idle)**: Head slowly rotates left/right scanning area (Y-axis rotation), vision cone active
- **Alert**: Player detected in vision cone → Head stops scanning and **locks onto player** → Brief 0.5s delay before chase
- **Chasing**: CharacterController-based pursuit of player, **head continuously tracks player** during movement, catches player on contact
- **Death/Respawn**: Guard returns to **initial spawn position** (not checkpoint position) and resets to Guarding state

**Guard Components:**
- `GuardStationary.cs`: Main state machine (Guarding/Alert/Chasing) with head rotation and player tracking
  - `RotateHead()`: Oscillating scan rotation during Guarding state
  - `LookAtPlayer()`: Smooth head tracking during Alert/Chasing states
  - `CancelInvoke()`: Prevents chase restart after death/respawn
- `GuardChase.cs`: Handles CharacterController-based pursuit and player catching
  - Integrates with DeathManager for death sequence
  - Position restored by CheckpointManager (not self-managed)
- `GuardVisionCone.cs`: Cone-shaped detection using angle check + raycast for line-of-sight
- `GuardHearingZone.cs`: Spherical trigger for hearing detection (not yet connected to player states)
- `FollowingZone.cs`: General detection zone trigger

**Head Tracking System:**
- Guard has separate head Transform (part of 3D model hierarchy)
- During Guarding: Head rotates left/right within min/max angle range
- During Alert/Chase: Head smoothly tracks player position using Quaternion.Slerp
- Rotation in local space (relative to guard body)
- Configurable tracking speed (default: 5)

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
- Guarding: Scans area with rotating head (oscillates between minAngle and maxAngle), checks vision cone continuously
- Alert: Head stops scanning and locks onto player position, 0.5s delay before transitioning to chase
- Chasing: Active pursuit via CharacterController, head continuously tracks player during movement

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
   - **Predefined spawn points using Transform array (level designer control)**
   - Dictionary-based spawn tracking to prevent double-spawning at same location
   - Ground detection for accurate placement via raycast
   - Visual gizmos showing X marks at spawn points for level design
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

### Kill Zone & Checkpoint System ✅
**Files:** `KillZone.cs`, `Checkpoint.cs`, `CheckpointManager.cs`, `DeathManager.cs`, `ScreenFader.cs`

**Implemented:**
- Large plane trigger beneath level detects player/sheep falling
- Player falls → Death sequence → Respawn at checkpoint
- Sheep falls → Instant fail → Level reloads from checkpoint
- **Guard catches player** → Death sequence → Respawn at checkpoint
- Screen fades to black during death/respawn
- Full game state restoration (player + all sheep + **all guards**)
- Sheep Rigidbody velocity reset to prevent infinite falling
- Sheep state machine reset to Idle after respawn
- **Guards restore to INITIAL spawn positions** (not checkpoint positions)
- Multiple checkpoints per level supported
- First checkpoint auto-saves on level start

**Death Manager:**
- Singleton pattern managing all death events
- Event-based system with DeathCause enum (PlayerFell, SheepFell, **GuardCaught**, Explosion, Trap)
- Coroutine-based sequence: Freeze player → Fade to black → Load checkpoint → Fade from black → Unfreeze
- Extensible for future death types

**Checkpoint System:**
- Trigger zones throughout level
- Saves player position, rotation, all sheep positions
- **Does NOT save guard positions** (guards always return to spawn)
- **Guards stored separately in initialGuardPositions dictionary** (never changes after level start)
- Starting checkpoint auto-activates
- Visual gizmos: Yellow (inactive) → Green (activated)
- Prevents double-activation with state tracking

**Guard Restoration System:**
- Guards always restore to **initial spawn position**, not current position
- Prevents "guard camping checkpoint" scenarios
- CharacterController disabled during position restoration (prevents movement interference)
- Guard state machine reset to Guarding
- Pending Invoke() calls cancelled (prevents chase restart)
- Proper state synchronization between GuardStationary and GuardChase

**Screen Fader:**
- Canvas with CanvasGroup alpha control
- Smooth fade to/from black transitions
- Configurable fade durations and hold time
- Coroutine-based animation system

### Territory Zone System ✅
**Files:** `TerritoryZone.cs`, `SheepFollowingZone.cs`, modifications to `SheepAttraction.cs`

**Implemented:**
- Territory zone marks guard patrol area (BoxCollider trigger)
- Sheep has "Returning" state added to state machine
- Idle timer counts down after eating (default: 5 seconds)
- Sheep returns to home position if no new attractions appear
- Territory exit permanently "frees" sheep (no more returning)
- Return cancelled automatically if new lettuce appears

**Inside Territory Zone:**
1. Sheep follows attraction (lettuce/scent) normally
2. After eating, enters Idle state with 5-second timer
3. If timer expires → Returns to home spawn position
4. If player picks up sheep → Timer resets, sheep "rescued"
5. If new lettuce appears → Return cancelled, goes to lettuce

**Outside Territory Zone:**
1. Sheep permanently "freed" from territory memory
2. After eating → stays at current location
3. Only moves if new attraction appears
4. No returning behavior

**Puzzle Design:**
Player must either:
- Lure sheep completely out of territory (safe), OR
- Pick up sheep within 5-second window after eating (risky, near guards)

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
├── SheepAttraction.cs           ← Sheep AI: attraction, walking, eating, returning ✅
├── SheepFollowingZone.cs        ← Territory timer and return trigger ✅
├── ISheepAttraction.cs          ← Interface for all attraction types ✅
├── BridgeCollapse.cs            ← Bridge hazard detection ✅
├── WinZoneCheck.cs              ← Goal zone detection ✅
├── TerritoryZone.cs             ← Territory boundary detection ✅
├── DeathManager.cs              ← Central death handler (singleton) ✅
├── CheckpointManager.cs         ← Save/load game state ✅
├── Checkpoint.cs                ← Checkpoint trigger zones ✅
├── KillZone.cs                  ← Fall detection trigger ✅
├── ScreenFader.cs               ← Screen fade effects ✅
├── Lettuces/
│   ├── Lettuce.cs               ← Lettuce item (implements ISheepAttraction) ✅
│   └── LettuceGarden.cs         ← Lettuce spawner system ✅
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
- `TERRITORY_SYSTEM_GUIDE.md` - Territory/return system setup
- `KILLZONE_CHECKPOINT_GUIDE.md` - Death/checkpoint system setup

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
- **Scale-independent positioning**: Use collider bounds for reliable positioning regardless of scale
- **Trigger-aware raycasting**: Use `QueryTriggerInteraction.Ignore` for ground detection to avoid hitting detection triggers
- **Physics-based calculations**: Prefer collider dimensions over hardcoded offsets
- **CharacterController teleportation**: Disable/enable CharacterController when teleporting to clear internal state
- **Invoke cancellation**: Always cancel pending Invoke() calls when resetting state machines
- **Initial position storage**: Store entity spawn positions separately from checkpoint data for predictable resets
- **Head tracking in local space**: Calculate rotations relative to parent transform for proper hierarchy support
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
2. ✅ **Player Animations**: Idle, walk, run, jump animations with Animator Controller (Speed + IsJumping parameters)
3. ✅ **Player Interaction**: Pickup/carry/drop sheep and lettuce with E key
4. ✅ **Sheep Basic Mechanics**: Pickup/carry/drop, kinematic physics, ground detection
5. ✅ **Sheep Attraction System**: Detects lettuce, walks to it, eats it, returns home, state machine complete
6. ✅ **Lettuce System**: Pickup/drop mechanics, gravity physics, spawner with predefined points
7. ✅ **Level Hazards**: Bridge collapse system working
8. ✅ **Win Condition**: Win zone detects sheep and triggers victory
9. ✅ **Stationary Guard AI**: Vision cone, head rotation, chase, catch, and **head tracking**
10. ✅ **Guard Head Tracking**: Head scans during patrol, locks onto player during alert/chase
11. ✅ **Guard Death Integration**: Proper respawn to initial position, state machine reset, CharacterController handling
12. ✅ **Territory/Return System**: Sheep return home if inside territory and no new attractions
13. ✅ **Kill Zone & Checkpoints**: Full death/respawn system with screen fades and guard restoration

**High Priority - Not Started:**
14. ⏳ **Patrolling Guard**: Moving guard with waypoint system
15. ⏳ **Guard Visual Polish**: Material changes for states, UI indicators

**Medium/Low Priority:**
14. ⏳ **Advanced Gadgets**: Salad Gun, Fan + Perfume synergy
15. ⏳ **Level Design**: Complete test level with cover and obstacles
16. ⏳ **UI Systems**: Detection indicators, inventory display, victory screen
17. ⏳ **Polish**: Sound effects, particle effects

### Implementation Phases

**Phase 1: Foundation (COMPLETE ✅)**
- PlayerController.cs with camera-relative movement
- Manual gravity system
- Jump mechanics
- Cinemachine FreeLook camera integration
- Crouch (Ctrl) and Run (Shift) states with speed multipliers
- Animator Controller with idle, walk, run, jump animations
- Animation parameters: Speed (Float) and IsJumping (Bool)
- Smooth animation transitions with minimal Exit Time

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

**Phase 4: Lettuce & Sheep Attraction (COMPLETE ✅)**
- Lettuce.cs for pickup/drop mechanics
- LettuceGarden.cs spawner system with predefined spawn points
- SheepAttraction.cs state machine (Idle/Sniffing/Walking/Eating)
- Attraction radius detection
- Closest lettuce selection
- Eating and destruction mechanics

**Phase 5: Level Hazards (COMPLETE ✅)**
- BridgeCollapse.cs trigger detection
- Detects player carrying sheep OR player + sheep separately
- Debug visualization

**Phase 6: Territory System (COMPLETE ✅)**
- Created TerritoryZone.cs trigger zone
- Created SheepFollowingZone.cs for timer management
- Modified SheepAttraction.cs with "Returning" state
- 5-second idle timer after eating (configurable)
- Return-to-home behavior using moveSpeed
- Territory exit detection (sheep freed permanently)
- Return cancelled if new lettuce appears

**Phase 7: Kill Zones & Checkpoints (COMPLETE ✅)**
- DeathManager.cs singleton for centralized death handling
- KillZone.cs for fall detection (player and sheep)
- CheckpointManager.cs for save/load game state
- Checkpoint.cs for individual checkpoint triggers
- ScreenFader.cs for screen fade effects
- Coroutine-based death sequence with proper timing
- Sheep velocity reset and state machine reset on respawn
- Full game state restoration (player + all sheep)

**Phase 8: Guard Head Tracking & Death System (COMPLETE ✅)**
- Added 3D model with separate head Transform
- Implemented head rotation during Guarding state (oscillating scan)
- Implemented LookAtPlayer() for Alert/Chase states (smooth tracking)
- Fixed guard respawn to use initial spawn positions (not checkpoint positions)
- Added CharacterController disable/enable during teleportation
- Fixed Invoke() cancellation to prevent chase restart after death
- Integrated guard with DeathManager and CheckpointManager

**Phase 9: Patrolling Guard (NEXT - High Priority)**
- Create waypoint patrol system
- Implement patrol → chase → return-to-patrol flow
- Add multiple guard variations

**Phase 10: Guard Visual Polish**
- Test guard in realistic gameplay scenario
- Add visual feedback for guard states (material colors, UI indicators)
- Tune detection parameters (angles, ranges, delays)
- Integrate hearing with player movement states (running = loud, sneaking = silent)
- Build simple test level with cover and obstacles

**Phase 10: Advanced Gadgets**
- Create item base classes
- Implement Salad Gun (shoots lettuce)
- Implement Fan (wind zones)
- Implement Perfume (scent beams)
- Add gadget synergies (Fan + Perfume)
- Test interaction flows

**Phase 11: UI & Polish**
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
