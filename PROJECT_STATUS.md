# Sheep Stealth Puzzle Game - Current Project Status

**Last Updated:** 2026-01-25

---

## 🎯 Project Vision

Unity 3D stealth puzzle game inspired by "Sheep, Dog 'n' Wolf" (2001) (Player controls a wolf who must steal sheep from a guarded flock and deliver them to a goal zone while avoiding detection by a patrolling guard dog).
This prototype heavily inspired by other 3D platformers from PS1 era as well.

**Core Loop:** Scout Level → Collect Gadgets → Lure Sheep → Avoid Guard → Deliver to Goal
Some levels have different loop, but it all comes to delivering the sheep or special object to Goal

---

## ✅ COMPLETED FEATURES

### 1. Player Movement System ✅
**Files:** `PlayerController.cs`

**Implemented:**
- Camera-relative third-person movement (WASD)
- Manual gravity system for CharacterController
- Jump mechanics (Spacebar)
- Smooth character rotation toward movement direction
- Cinemachine FreeLook camera with mouse orbit

**States:**
- Walking (default speed)
- Crouching (Ctrl) - slower, quieter
- Running (Shift) - faster, louder
- Carrying - reduced speed when holding sheep (70%)

**Animation System:**
- Animator Controller with Speed (Float) and IsJumping (Bool) parameters
- Idle animation when Speed = 0
- Walk/Run blend tree based on Speed value (0.3 = walk, 1.0 = run)
- Jump animation triggered by IsJumping = true, resets to idle/walk on landing
- Smooth transitions with minimal Exit Time for responsive controls
- Animation states properly sync with movement and jump mechanics

**Working As Expected:** ✅

---

### 2. Player Interaction System ✅
**Files:** `PlayerInteraction.cs`

**Implemented:**
- Pickup/carry/drop sheep with E key
- Pickup/carry/drop lettuce with E key
- Ground detection via raycast for accurate drop positioning
- **Collider-aware drop positioning**: Calculates sheep's collider offset from dimensions, not current position
- **Trigger-ignoring raycasts**: Uses `QueryTriggerInteraction.Ignore` to only hit walkable surfaces
- Three-tier fallback system for ground detection
- Detects nearest sheep in front of player (within pickup range)
- CarryPosition transform for held items
- Speed modifier applied when carrying sheep
- **Scale preservation**: Sheep maintains correct size when carried regardless of parent scale

**Working As Expected:** ✅

---

### 3. Sheep System ✅
**Files:** `Sheep.cs`, `SheepAttraction.cs`

**Implemented:**
- Sheep pickup/carry/drop mechanics
- Kinematic Rigidbody for trigger detection
- Collider disabled when carried, re-enabled when dropped
- Rotation constraints (X/Z locked) to prevent tilting
- **Advanced ground positioning system**:
  - Uses `collider.bounds` for scale-aware offset calculation
  - Calculates offset in `Awake()` based on actual collider dimensions
  - Accounts for non-uniform scales automatically
  - Works correctly on flat ground, bridges, and varied terrain
- **Trigger-ignoring raycasts**: All ground detection ignores trigger colliders (bridge triggers, etc.)
- Ground detection system with emergency fallback
- Falling physics when ground disappears
- **Scale preservation**: Stores and restores original scale when picked up/dropped
- State machine: Idle, Walking, Sniffing, Eating, Returning

**Attraction System:**
- Detects closest lettuce within configurable radius
- Sniffing delay before movement (configurable)
- Walks toward lettuce using Rigidbody.MovePosition()
- **Smooth ground following**: Raycast-based positioning keeps sheep flush with surfaces
- Eats lettuce after 2-second eating duration (configurable)
- Destroys lettuce after eating
- Returns to Idle state or Returning state (if inside territory)
- Handles multiple lettuce sources (always picks closest)
- Ignores carried lettuce
- Complete state machine: Idle → Sniffing → Walking → Eating → Idle/Returning

**Territory/Return System:**
- SheepFollowingZone.cs manages territory timer
- 5-second thinking timer after eating (configurable)
- Sheep returns to home position if inside territory and no new attractions
- Territory exit permanently "frees" sheep
- Return cancelled automatically if new lettuce appears
- Uses same moveSpeed for returning

**Working As Expected:** ✅

---

### 4. Lettuce System ✅
**Files:** `Lettuce.cs`, `LettuceGarden.cs`

**Implemented:**
- Pickup/carry/drop mechanics matching sheep pattern
- Rigidbody with gravity for realistic dropping
- Physics disabled when carried (kinematic + no gravity)
- Physics enabled when dropped (non-kinematic + gravity)
- Collider disabled when carried to prevent sheep attraction

**Spawner System:**
- `LettuceGarden.cs` - Maintains fixed number of lettuces per level (default: 4)
- Automatically respawns destroyed/eaten lettuces
- **Predefined spawn points using Transform array (level designer control)**
- Dictionary-based spawn tracking to prevent double-spawning
- Ground detection for accurate placement via raycast
- Visual gizmos showing X marks at spawn points for level design
- Prevents soft-locking (player always has resources)

**Working As Expected:** ✅

---

### 5. Level Hazards ✅
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

**Working As Expected:** ✅

---

### 6. Win Zone System ✅
**Files:** `WinZoneCheck.cs`

**Implemented:**
- Trigger volume at level exit
- Detects when sheep enters the zone
- Triggers level complete event
- Debug logging for victory condition

**Future Enhancements (Planned):**
- Victory UI display
- Level transition/loading
- Victory sound effects
- Player movement freeze on win

**Working As Expected:** ✅

---

### 7. Sheep Territory & Return System ✅
**Files:** `TerritoryZone.cs`, `SheepFollowingZone.cs`, modified `SheepAttraction.cs`

**Implemented:**
- TerritoryZone.cs - BoxCollider trigger detecting sheep enter/exit
- SheepFollowingZone.cs - Manages territory timer and triggers return
- Added "Returning" state to SheepAttraction.SheepState enum
- Added `StartReturningHome(Vector3 home)` public method
- Added `ReturnToHome()` private method with attraction checking
- Added `ResetToIdleState()` for checkpoint restoration

**Territory Behavior:**
- Inside Territory: After eating, 5-second thinking timer starts (configurable)
- If timer expires and no new attractions → Sheep returns to home position
- If new lettuce appears → Return cancelled, sheep goes to lettuce
- If player picks up sheep → Timer resets
- Outside Territory: Sheep permanently freed, no returning behavior

**Return Movement:**
- Uses same `moveSpeed` as walking to lettuce (consistent behavior)
- Smooth rotation toward home position
- Auto-cancels if new attraction detected during return

**Working As Expected:** ✅

---

### 8. Kill Zone & Checkpoint System ✅
**Files:** `KillZone.cs`, `Checkpoint.cs`, `CheckpointManager.cs`, `DeathManager.cs`, `ScreenFader.cs`

**Implemented:**

**Kill Zone:**
- Large BoxCollider trigger beneath entire level
- Detects player OR sheep falling off map
- Player falls → Death sequence → Respawn
- Sheep falls → Instant fail → Level reload
- Debug gizmos showing red semi-transparent kill zone
- Detailed debug logging for fall detection

**Death Manager (Singleton):**
- Centralized death handling for all death types
- DeathCause enum: PlayerFell, SheepFell, GuardCaught, Explosion, Trap
- Coroutine-based death sequence with proper timing
- Freezes player input during death/respawn
- Calls ScreenFader and CheckpointManager in sequence
- Waits for fade animations to complete before continuing
- Extensible for future death types

**Checkpoint System:**
- Multiple checkpoint trigger zones throughout level
- First checkpoint auto-activates on level start
- Saves full game state: player position/rotation, all sheep positions/rotations
- Prevents double-activation with state tracking
- Visual gizmos: Yellow (inactive) → Green (activated)
- Sphere icon marker 2 units above trigger for easy identification

**Checkpoint Manager:**
- Singleton managing save/load operations
- Auto-finds all sheep with "Sheep" tag if not manually assigned
- Saves initial checkpoint at level start
- Dictionary-based storage for multiple sheep states
- Restores player position and rotation
- Restores all sheep positions and rotations
- **Resets sheep Rigidbody velocity to prevent infinite falling**
- **Sets sheep Rigidbody to kinematic for proper movement**
- **Resets sheep state machine to Idle via ResetToIdleState()**
- Re-enables sheep GameObject and components if disabled

**Screen Fader:**
- Canvas with CanvasGroup alpha control
- FadeToBlack() and FadeFromBlack() coroutines
- Smooth Lerp-based alpha transitions
- Configurable fade durations and black screen hold time
- IsFading property for timing synchronization
- Utility methods for instant black/transparent

**Death Sequence Flow:**
1. Death source calls `DeathManager.OnDeath(cause)`
2. DeathManager starts coroutine
3. Freeze player input
4. Fade to black (wait for completion)
5. Load checkpoint (teleport while screen is black)
6. Fade from black (wait for completion)
7. Unfreeze player input
8. Gameplay continues

**Working As Expected:** ✅

---

## ⏳ IN PROGRESS / PARTIALLY IMPLEMENTED

_No systems currently in progress. All core features are complete and working._

---

## ❌ NOT YET IMPLEMENTED

---

### Attraction Priority System
**Priority:** Low (polish feature for later)

**Purpose:**
Allow different attraction sources to have priority levels when sheep detects multiple options simultaneously.

**Example Use Cases:**
- Fresh food > stale food
- Perfume scent > physical lettuce
- Favorite food > regular food
- Closer low-priority item vs farther high-priority item

**Current Behavior:**
Sheep always chooses closest attraction (distance-based only)

**Status:** Deferred until core gameplay complete

---

### Guard AI System ✅ (COMPLETE - Stationary Guard)
**Status:** Implemented and tested
**Files:** `GuardStationary.cs`, `GuardChase.cs`, `GuardVisionCone.cs`, `GuardHearingZone.cs`, `FollowingZone.cs`

**What's Working:**
- Stationary guard with head rotation (ping-pong pattern)
- Vision cone detection with line-of-sight raycasting
- Alert state with configurable delay before chase
- Chase behavior with NavMesh movement
- Player catching and state reset
- Chase cancellation when player escapes line-of-sight
- Debug gizmos for vision cone visualization

**Needs Integration:**
- Guard catch death type (modify GuardChase.cs to call DeathManager.OnDeath)
- Visual feedback for guard states (material colors)
- Parameter tuning based on playtesting
- Hearing zone integration with player movement states

---

#### **Guard Type 1: Stationary Guard** (IMPLEMENTED ✅)

**Behavior:**
- Stands in one position (does not move)
- Rotates in **ping-pong pattern** (e.g., -90° to +90°, then back)
- Configurable rotation speed and angle limits
- Creates predictable blind spot when facing away

**Detection Zones:**

1. **Vision Cone (Trapezoid)**
   - Forward-facing field of view
   - Requires line-of-sight raycast (blocked by obstacles)
   - **Instant alert** when player detected (no suspicious state)
   - Angle: ~60-90° (configurable)
   - Range: ~10-15 units (configurable)

2. **Hearing Zone (Shape TBD)**
   - 360° detection around guard (no blind spot)
   - Only triggers on **sprinting** (loud player actions)
   - **Instant alert** when heard
   - Range: ~5-8 units (smaller than vision)
   - Does NOT detect walking/crouching

**Chase Behavior:**
When player detected (seen OR heard):
1. Guard runs toward player **very fast**
2. Catches player within range → "beats" them
3. Player respawns at checkpoint
4. Guard returns to original position
5. Resumes rotation pattern

**State Machine:**
- Idle → Rotating (ping-pong between angles)
- Detected → Chasing (fast movement toward player)
- Caught → Returning (walk back to start position)
- Returning → Idle (resume rotation)

**Relationship to Territory Zone:**
- Guard's rotation position typically inside Territory Zone
- Territory Zone (black circle) often encompasses guard's detection range
- Sheep return home within this zone

---

#### **Future Guard Types (Planned)**
- **Patrol Guard**: Walks between waypoints using NavMeshAgent
- **Alert Guard**: Has suspicious state, investigates sounds
- **Fast Guard**: Quick patrols, short detection range

---

**Files Needed:**
- `GuardStationary.cs` - Main stationary guard behavior
- `GuardVisionCone.cs` - Vision detection component
- `GuardHearingZone.cs` - Sound detection component
- `GuardChase.cs` - Chase and catch behavior
- `PatrolPath.cs` - Waypoint system (for future patrol guards)

---

### Detection System
**Planned Features:**
- Centralized detection manager (singleton)
- Four detection levels: None, Near, Hidden, Detected
- UI indicator showing detection state
- Sound-based detection for running player

**Files Needed:**
- `DetectionManager.cs` - Centralized system
- `DetectionIndicator.cs` - UI display

**Priority:** High (tied to Guard AI)

---

### Advanced Gadgets
**Planned Items:**

1. **Salad Gun** (Not Started)
   - Shoots lettuce at distant locations
   - Raycast-based projectile
   - Limited ammo per level

2. **Fan** (Not Started)
   - Creates wind zone (BoxCollider trigger)
   - IFanInteractable interface for affected items
   - Directional wind system

3. **Perfume** (Not Started)
   - Idle by default
   - Activated by Fan wind
   - Creates growing scent beam
   - Directional attraction for sheep

**Priority:** Medium (enhance puzzle complexity)

---

### Level Systems

See sections above for implemented Kill Zone & Checkpoint System ✅

---

### UI Systems (Not Started)

**Needed:**
- Inventory display (show held item)
- Detection indicator (guard awareness)
- Level objective display
- Timer/score system
- Pause menu

---

## 🛠️ TECHNICAL DEBT & IMPROVEMENTS

### Code Quality
- ✅ Component-based architecture maintained
- ✅ Single Responsibility Principle followed
- ✅ Debug logs cleaned from production code (bridge, sheep, player scripts)

### Performance
- No issues identified yet
- Need to test with multiple sheep/guards

### Polish Needed
- ✅ Player animations complete (idle, walk, run, jump)
- ⏳ Sheep animations (planned)
- ⏳ Guard animations (planned)
- No sound effects
- No particle effects
- No UI polish

---

## 📁 CURRENT FILE STRUCTURE

```
Assets/
├── Script/
│   ├── PlayerController.cs ✅
│   ├── PlayerInteraction.cs ✅
│   ├── Sheep.cs ✅
│   ├── SheepAttraction.cs ✅
│   ├── ISheepAttraction.cs ✅
│   ├── BridgeCollapse.cs ✅
│   ├── WinZoneCheck.cs ✅
│   ├── Lettuces/
│   │   ├── Lettuce.cs ✅
│   │   └── LettuceGarden.cs ✅
│   └── Guard Scripts/
│       ├── GuardStationary.cs ✅
│       ├── GuardChase.cs ✅
│       ├── GuardVisionCone.cs ✅
│       ├── GuardHearingZone.cs ✅
│       └── FollowingZone.cs ✅
│
├── Scenes/
│   └── [Level scenes]
│
└── Prefabs/
    ├── Player.prefab ✅
    ├── Sheep.prefab ✅
    ├── Lettuce.prefab ✅
    ├── Bridge.prefab ✅
    └── Guard.prefab ✅
```

**Missing Folders:**
- `AI/` - Guard behavior scripts
- `Systems/` - Detection, game manager
- `UI/` - HUD elements
- `Items/` - Advanced gadgets

---

## 🎮 PLAYABLE FEATURES (Current Build)

**What You Can Do Right Now:**
1. Move player with WASD with smooth animations (idle, walk, run)
2. Run (Shift) and Crouch (Ctrl)
3. Jump with Spacebar (animated jump with proper landing)
4. Pick up sheep with E
5. Carry sheep (moves slower)
6. Drop sheep with E
7. Pick up lettuce with E
8. Drop lettuce (falls with gravity)
9. Sheep detects and walks to closest lettuce
10. Sheep eats lettuce (destroys it after 2 seconds)
11. Sheep returns home if inside territory and no new attractions
12. Sheep freed permanently if it exits territory
13. Walk on bridge → collapses if carrying sheep
14. Bridge collapses if player + separate sheep both on it
15. Stationary guard rotates and chases if player detected
16. Player/sheep falls off map → Death/respawn system activates
17. Screen fades to black → Reload checkpoint → Fade from black
18. Multiple checkpoints save game state
19. Deliver sheep to win zone → Level complete

**What's Missing for Full Gameplay:**
- Patrolling guards (only stationary implemented)
- Guard catch death integration
- Advanced gadgets for puzzles
- Victory UI and level transitions
- Detection indicator UI

---

## 📋 RECOMMENDED NEXT STEPS

### Immediate Priorities (Next Phase)
1. **Integrate Guard Death Type**
   - Modify GuardChase.cs to call `DeathManager.OnDeath(DeathManager.DeathCause.GuardCaught)` when catching player
   - Test guard catch → death → respawn flow
   - Verify screen fade and checkpoint restoration works with guard deaths

2. **Implement Patrolling Guard**
   - Create PatrolPath.cs with waypoint system
   - Modify guard to walk between waypoints
   - Implement patrol → chase → return-to-patrol flow
   - Multiple guard variations (fast/slow, short/long routes)

### Short-Term Goals
3. **Guard Polish & Testing**
   - Visual feedback for guard states (material colors)
   - Parameter tuning based on playtesting
   - Hearing zone integration with player movement states
   - Build test level with cover and obstacles

### Medium-Term Goals (Phase 5)
4. **Add Advanced Gadgets**
   - Salad Gun
   - Fan + Perfume synergy

5. **Build Test Level**
   - Full puzzle layout
   - Patrol path for guard
   - Multiple sheep and gadgets

### Long-Term Polish
6. **Add Juice**
   - Animations
   - Sound effects
   - Particle effects
   - UI polish

---

## 🐛 KNOWN ISSUES

### Current Issues (To Address Later):

**Sheep Behavior:**
- ⚠️ Sniff behavior needs more testing
- Need to verify sniffing delay works correctly in various scenarios
- Priority: Medium (affects gameplay feel)

**Lettuce Spawner:**
- ✅ **FIXED** - LettuceGarden now uses predefined Transform[] spawn points
- ✅ **FIXED** - Dictionary tracking prevents double-spawning at same location
- ✅ Visual gizmos show spawn point locations for level design

### Recently Fixed:
- ✅ Kinematic Rigidbody linearVelocity error
- ✅ Sheep flying to Y=80 (ground layer misconfiguration)
- ✅ Sheep falling when carried near edge (logic order)
- ✅ Bridge not destroying (trigger collider height)
- ✅ Sheep tilting during movement (rotation constraints)
- ✅ Lettuce floating when dropped (Rigidbody missing)
- ✅ Lettuce falling when carried (physics not disabled)
- ✅ Guard vision cone raycast distance bug (checked full viewDistance instead of actual distance)
- ✅ Guard state desync when catching player (now properly calls StopChasing)
- ✅ Head rotation logic inverted (comparison operators fixed)
- ✅ Sheep infinite falling after death (Rigidbody velocity reset + kinematic restore)
- ✅ Sheep not moving after respawn (state machine reset to Idle)
- ✅ Sheep not in checkpoint save data (missing from allSheep list)
- ✅ Sheep sinking into ground when dropped (now uses collider dimensions, not current position)
- ✅ Sheep floating above bridge (raycasts now ignore trigger colliders)
- ✅ Sheep scaling distorted when carried (stores and restores original scale)
- ✅ Ground positioning affected by scale (now uses bounds-based calculation)

---

## 📊 COMPLETION ESTIMATE

**Overall Project Progress:** ~75%

| System | Progress | Status |
|--------|----------|--------|
| Player Movement | 100% | ✅ Complete |
| Player Animations | 100% | ✅ Complete |
| Player Interaction | 100% | ✅ Complete |
| Sheep Basic Mechanics | 100% | ✅ Complete |
| Sheep AI Attraction | 100% | ✅ Complete |
| Sheep Territory System | 100% | ✅ Complete |
| Lettuce System | 100% | ✅ Complete |
| Level Hazards (Bridge) | 100% | ✅ Complete |
| Win Zone (Basic) | 100% | ✅ Complete |
| Guard AI (Stationary) | 100% | ✅ Complete |
| Kill Zone & Checkpoints | 100% | ✅ Complete |
| Death/Respawn System | 100% | ✅ Complete |
| Screen Fade Effects | 100% | ✅ Complete |
| Guard AI (Patrolling) | 0% | ❌ Not Started |
| Guard Catch Death Integration | 0% | ❌ Not Started |
| Detection Indicator UI | 0% | ❌ Not Started |
| Victory UI & Transitions | 0% | ❌ Not Started |
| Advanced Gadgets | 0% | ❌ Not Started |
| Sheep/Guard Animations | 0% | ❌ Not Started |
| Sound/Particle Effects | 0% | ❌ Not Started |

---

## 💡 DESIGN DECISIONS LOG

### Recent Decisions:
1. **Sheep Use Rigidbody (Kinematic)**
   - Reason: Needed for trigger detection and smooth movement
   - Constraints: X/Z rotation locked to prevent tilting

2. **Collider Bounds-Based Ground Positioning**
   - Reason: Scale-independent positioning that works regardless of transform scale
   - Method: Calculate offset as `transform.position.y - collider.bounds.min.y`
   - Benefits: Handles non-uniform scales automatically, works on any terrain height

3. **Trigger-Ignoring Raycasts**
   - Reason: Bridge trigger colliders were causing sheep to float above walkable surface
   - Solution: Use `QueryTriggerInteraction.Ignore` on all ground detection raycasts
   - Benefits: Only hits actual walkable surfaces, not detection triggers

4. **Scale Preservation on Pickup**
   - Reason: Unity parent-child scale inheritance was distorting sheep size
   - Solution: Store `lossyScale` before parenting, restore as adjusted `localScale` after
   - Benefits: Sheep maintains correct visual size regardless of parent scale

5. **Collider-Dimension Drop Positioning**
   - Reason: Using current position while carried gave wrong offset (sheep in air)
   - Solution: Calculate from collider properties: `(height/2) - center.y`
   - Benefits: Consistent positioning whether sheep is carried or on ground

6. **Lettuce Uses Dynamic Rigidbody**
   - Reason: Needs realistic falling physics when dropped
   - Switches to kinematic when carried

3. **Bridge Requires Two Colliders**
   - Reason: One for physics surface, one for trigger detection
   - Trigger must be tall enough for floating player capsule

4. **Ground Detection Uses LayerMask**
   - Reason: Prevents false positives with other objects
   - Must be configured in Inspector per script

5. **Component-Based Architecture**
   - Each script has single responsibility
   - Independent scripts communicate via GetComponent
   - Enables easier debugging and testing

---

## 🎯 FEATURE ROADMAP

### Phase 3: Sheep Behavior (COMPLETE ✅)
- [x] Sniffing timer (configurable delay)
- [x] Lettuce eating/destruction
- [x] Multiple attraction handling (picks closest)
- [x] Ignores carried lettuce

### Phase 3.5: Territory System (CURRENT)
- [ ] Create TerritoryZone.cs
- [ ] Add homePosition storage
- [ ] Implement ReturnHome state
- [ ] 5-second idle timer after eating
- [ ] Territory enter/exit detection

### Phase 4: Guard AI
- [ ] NavMesh setup
- [ ] Patrol waypoint system
- [ ] Vision cone detection
- [ ] Chase behavior
- [ ] Return to patrol

### Phase 5: Win Conditions
- [ ] WinZone trigger
- [ ] Victory screen
- [ ] Level restart

### Phase 6: Advanced Gadgets
- [ ] Salad Gun
- [ ] Fan
- [ ] Perfume
- [ ] Fan + Perfume synergy

### Phase 7: Level Design
- [ ] Test level layout
- [ ] Multiple sheep placement
- [ ] Gadget placement
- [ ] Difficulty balancing

### Phase 8: Polish
- [ ] Animations
- [ ] Sound effects
- [ ] Particle effects
- [ ] UI design
- [ ] Tutorial level

---

**End of Project Status Document**
