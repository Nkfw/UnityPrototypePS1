# Sheep Stealth Puzzle Game - Project Reference

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
├── PlayerController.cs        ← Movement, jump, gravity ONLY
├── PlayerInteraction.cs       ← Pickup, use items, press E (future)
├── PlayerInventory.cs         ← Manage held items, switching (future)
├── PlayerStateManager.cs      ← Walking/sneaking/running states (future)
└── PlayerInput                ← Unity Input System component
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

- **Idle**: Standing in flock
- **Following**: Walking toward attraction source (food/scent)
- **Carried**: Being held by player
- **Sniffing**: At attraction source, temporary (~5 sec)
- **Returning**: Walking back to original position

**Key Mechanic**: When picked up, sheep's collider is disabled and parented to player. When dropped, it's unparented and collider re-enabled.

**IMPORTANT**: Sheep requires a **kinematic Rigidbody** for:
- Trigger detection (OnTriggerEnter in WinZone)
- Smooth movement via `Rigidbody.MovePosition()`
- Arcade-style physics (no bouncing/sliding)

### Guard States

- **Patrol**: Walking between waypoints
- **Suspicious**: Player nearby but not seen
- **Chase**: Pursuing detected player
- **Return**: Going back to patrol route

## Detection System

### Detection Levels

1. **None**: No indicator (too far)
2. **Near**: Green icon (player in hearing range)
3. **Hidden**: Orange icon (in detection zone but concealed)
4. **Detected**: Red icon (guard sees player, chase begins)

Detection uses:
- **Vision Cone**: Angle check + raycast for line-of-sight
- **Hearing Range**: Sphere around guard
- **Movement Noise**: Running alerts guard

## Gadget System

### Core Items

1. **Lettuce**
   - Placed on ground, attracts sheep within radius (~5 units)
   - Sheep walks to it, eats it, then returns after sniffing

2. **Salad Gun**
   - Shoots lettuce at distant locations via raycast
   - Limited ammo per level

3. **Fan**
   - Creates wind zone (BoxCollider trigger)
   - Interacts with items implementing `IFanInteractable` interface
   - Passes wind direction to affected items

4. **Perfume**
   - Sits on ground, idle by default
   - When hit by fan wind, creates a growing **scent beam**
   - Beam is a directional BoxCollider that attracts sheep from distance
   - Beam grows while wind is active (~2 sec to max length)
   - Beam shrinks when wind stops

### Gadget Synergy Example

```
Fan (active) → Perfume (idle) → Scent Beam (extends in wind direction) → Sheep (attracted from far away)
```

## Sheep Attraction System

### How Sheep Follow Attractions

1. Sheep has `SphereCollider` trigger as "AttractionSensor"
2. When attraction source (lettuce, scent beam) enters sensor:
   - Sheep transitions to **Following** state
   - Walks toward attraction position
3. When sheep reaches target:
   - Transitions to **Sniffing** state (5 sec)
   - If target is lettuce, eats it (destroys object)
4. After sniffing:
   - Transitions to **Returning** state
   - Walks back to original position
   - Returns to **Idle** state

**Special Case**: Scent beam has no physical item to eat, sheep just sniffs the perfume's position.

## Level Components

### Goal Zone
- Trigger volume at level exit
- Detects player carrying sheep
- Triggers level complete when both conditions met

### Kill Zone
- Respawns player if they fall into pits/hazards
- Can also respawn sheep if needed

### Patrol Path
- Empty GameObject with Transform waypoints as children
- Guard follows waypoints in sequence
- Can loop or ping-pong

## Project Structure Pattern

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

### Current Status (✅ = Complete)
1. ✅ **Core Movement**: Player movement with camera-relative controls and jumping
2. ✅ **Basic Interaction**: Player can pickup/carry/drop sheep with E key
3. ✅ **Player States**: Crouching (Ctrl) and Running (Shift) with speed modifiers
4. ⏳ **Sheep Behavior**: Basic sheep AI and attraction system (next phase)
5. ⏳ **Guard AI**: Patrol and chase with detection
6. ⏳ **Basic Items**: Lettuce and attraction mechanics
7. ⏳ **Advanced Synergies**: Fan + Perfume interaction
8. ⏳ **Level Design & Polish**: Build test level, add juice

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
- Arcade-style (no Rigidbody physics needed)

**Phase 3: Sheep Behavior (NEXT)**
- Add sheep attraction system (lettuce lures sheep)
- Implement sheep states: Following, Sniffing, Returning
- CharacterController-based movement for sheep
- Attraction radius detection

**Phase 4: Guard AI**
- Implement NavMesh patrol system
- Create detection system (vision cone + hearing)
- Add chase behavior
- Integrate with player states

**Phase 5: Items & Gadgets**
- Create item base classes
- Implement lettuce attraction
- Add gadget synergies (Fan + Perfume)
- Test interaction flows

## Future Considerations

- Multiple guards per level?
- Multiple sheep to collect?
- Checkpoint system for mid-level respawns?
- Inventory capacity limits?
- Guard distraction mechanics (throw items to create noise)?

---

**Core Loop in Action**: Player scouts level → places lettuce to lure sheep away from guard → uses fan + perfume to create long-range scent beam → sheep follows scent → player picks up sheep → sneaks past guard to goal zone → level complete!
