# Sheep Stealth Puzzle Game - Current Project Status

**Last Updated:** 2026-01-17

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

**Working As Expected:** ✅

---

### 2. Player Interaction System ✅
**Files:** `PlayerInteraction.cs`

**Implemented:**
- Pickup/carry/drop sheep with E key
- Pickup/carry/drop lettuce with E key
- Ground detection via raycast for accurate drop positioning
- Three-tier fallback system for ground detection
- Detects nearest sheep in front of player (within pickup range)
- CarryPosition transform for held items
- Speed modifier applied when carrying sheep

**Working As Expected:** ✅

---

### 3. Sheep System ✅
**Files:** `Sheep.cs`, `SheepAttraction.cs`

**Implemented:**
- Sheep pickup/carry/drop mechanics
- Kinematic Rigidbody for trigger detection
- Collider disabled when carried, re-enabled when dropped
- Rotation constraints (X/Z locked) to prevent tilting
- Ground detection system with emergency fallback
- Falling physics when ground disappears
- State machine: Idle, Walking, Sniffing, Eating

**Attraction System:**
- Detects closest lettuce within configurable radius
- Sniffing delay before movement (configurable)
- Walks toward lettuce using Rigidbody.MovePosition()
- Eats lettuce after 2-second eating duration (configurable)
- Destroys lettuce after eating
- Returns to Idle state
- Handles multiple lettuce sources (always picks closest)
- Ignores carried lettuce
- Complete state machine: Idle → Sniffing → Walking → Eating → Idle

**Working As Expected:** ✅

**Missing Features:** Territory system (see "NOT YET IMPLEMENTED" section)

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
- Random spawn positions within configurable radius
- Ground detection for accurate placement
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

## ⏳ IN PROGRESS / PARTIALLY IMPLEMENTED

_No systems currently in progress. All basic features are complete and working._

---

## ❌ NOT YET IMPLEMENTED

### Sheep Territory & Return System
**Priority:** High (core puzzle mechanic)

**Concept:**
A "Territory Zone" (large BoxCollider trigger) marks the guard patrol area. Sheep behavior changes based on whether they're inside or outside this zone.

**Inside Territory Zone:**
1. Sheep follows attraction (lettuce/scent) normally
2. After eating, waits 5 seconds for new attractions
3. If no new attraction appears → **returns to home spawn position**
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

**Implementation Needs:**
- `TerritoryZone.cs` - BoxCollider trigger component
- `SheepAttraction.cs` modifications:
  - `Vector3 homePosition` - stored on Start()
  - `bool isInTerritory` - updated via OnTriggerEnter/Exit
  - `ReturnHome()` state - walks back to spawn position
  - 5-second idle timer after eating before returning
- Territory zone debug visualization (Gizmos)

**Files Needed:** Modify `SheepAttraction.cs`, create `TerritoryZone.cs`

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

### Guard AI System
**Priority:** High (core gameplay mechanic)

**Overview:**
Multiple guard types with different behaviors. Start with Type 1: Stationary Guard.

---

#### **Guard Type 1: Stationary Guard**

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

#### Kill Zone & Checkpoint System (Not Started)
**Priority:** High (core fail state mechanic)

**Concept:**
Large plane collider beneath the entire level that detects if player or sheep falls off the map.

**Kill Zone Behavior:**
- Positioned beneath level (e.g., Y = -10)
- Large BoxCollider trigger covering entire level area
- Detects Player OR Sheep falling through

**Fail Conditions:**
- Player falls → **FAIL STATE** → reload checkpoint
- Any sheep falls → **FAIL STATE** → reload checkpoint
- Death = level failure (not just respawn)

**Checkpoint System:**
- Save points throughout level
- Stores player position, carried items, sheep positions
- On fail: Reload level state from last checkpoint
- First checkpoint = level start position

**Implementation Needs:**
- `KillZone.cs` - Large plane trigger, detects Player/Sheep tags
- `CheckpointManager.cs` - Singleton, stores/loads game state
- `Checkpoint.cs` - Individual checkpoint trigger volumes
- Level state data: player position, inventory, sheep positions

**Files Needed:** `KillZone.cs`, `Checkpoint.cs`, `CheckpointManager.cs`

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
- ⚠️ Debug logs still active in production code (should be removed later)

### Performance
- No issues identified yet
- Need to test with multiple sheep/guards

### Polish Needed
- No animations yet (all gameplay is functional but lacks visual feedback)
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
│   ├── SheepAttraction.cs ✅ (partial)
│   ├── Lettuce.cs ✅
│   └── BridgeCollapse.cs ✅
│
├── Scenes/
│   └── [Level scenes]
│
└── Prefabs/
    ├── Player.prefab ✅
    ├── Sheep.prefab ✅
    ├── Lettuce.prefab ✅
    └── Bridge.prefab ✅
```

**Missing Folders:**
- `AI/` - Guard behavior scripts
- `Systems/` - Detection, game manager
- `UI/` - HUD elements
- `Items/` - Advanced gadgets

---

## 🎮 PLAYABLE FEATURES (Current Build)

**What You Can Do Right Now:**
1. Move player with WASD
2. Run (Shift) and Crouch (Ctrl)
3. Jump with Spacebar
4. Pick up sheep with E
5. Carry sheep (moves slower)
6. Drop sheep with E
7. Pick up lettuce with E
8. Drop lettuce (falls with gravity)
9. Sheep detects and walks to closest lettuce
10. Sheep eats lettuce (destroys it after 2 seconds)
11. Sheep handles multiple lettuces correctly
12. Walk on bridge → collapses if carrying sheep
13. Bridge collapses if player + separate sheep both on it

**What's Missing for Full Gameplay:**
- Territory/return system for sheep
- Guard to avoid
- Advanced gadgets for puzzles
- Victory UI and level transitions

---

## 📋 RECOMMENDED NEXT STEPS

### Immediate Priorities (Phase 4)
1. **Implement Territory & Return System**
   - Create TerritoryZone.cs trigger zone
   - Add return-to-home behavior in SheepAttraction.cs
   - 5-second idle timer after eating
   - Territory exit detection

### Short-Term Goals (Phase 4)
2. **Implement Guard AI**
   - NavMesh patrol system
   - Basic detection (vision cone)
   - Chase behavior

3. **Add Win Condition**
   - Create WinZone.cs
   - Level complete trigger
   - Victory UI

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

**Current Issues:** None

**Recently Fixed:**
- ✅ Kinematic Rigidbody linearVelocity error
- ✅ Sheep flying to Y=80 (ground layer misconfiguration)
- ✅ Sheep falling when carried near edge (logic order)
- ✅ Bridge not destroying (trigger collider height)
- ✅ Sheep tilting during movement (rotation constraints)
- ✅ Lettuce floating when dropped (Rigidbody missing)
- ✅ Lettuce falling when carried (physics not disabled)

---

## 📊 COMPLETION ESTIMATE

**Overall Project Progress:** ~46%

| System | Progress | Status |
|--------|----------|--------|
| Player Movement | 100% | ✅ Complete |
| Player Interaction | 100% | ✅ Complete |
| Sheep Basic Mechanics | 100% | ✅ Complete |
| Sheep AI Attraction | 100% | ✅ Complete |
| Sheep Territory System | 0% | ❌ Not Started |
| Lettuce System | 100% | ✅ Complete |
| Level Hazards (Bridge) | 100% | ✅ Complete |
| Win Zone (Basic) | 100% | ✅ Complete |
| Guard AI | 0% | ❌ Not Started |
| Detection System | 0% | ❌ Not Started |
| Victory UI & Transitions | 0% | ❌ Not Started |
| Advanced Gadgets | 0% | ❌ Not Started |
| UI Systems | 0% | ❌ Not Started |
| Polish/Juice | 0% | ❌ Not Started |

---

## 💡 DESIGN DECISIONS LOG

### Recent Decisions:
1. **Sheep Use Rigidbody (Kinematic)**
   - Reason: Needed for trigger detection and smooth movement
   - Constraints: X/Z rotation locked to prevent tilting

2. **Lettuce Uses Dynamic Rigidbody**
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
