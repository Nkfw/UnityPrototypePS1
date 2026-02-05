# How to Set Up Destructible Types

## What This Means

When I added the new checkpoint system, I added a **dropdown menu** to the BridgeCollapse script. This dropdown lets you tell Unity whether each destructible should:
- **Respawn on death** (like bridges - you need them to come back)
- **Stay destroyed** (like walls you break - they should stay broken as progress)

## Step-by-Step Instructions

### 1. Find Your Destructible Objects

In your Unity Hierarchy, find objects with the `BridgeCollapse.cs` component:
- Bridges
- Breakable walls
- Collapsing floors
- Any other destructible objects

### 2. Select the Object

Click on the object in the Hierarchy (e.g., "Bridge")

### 3. Look at the Inspector Panel

You'll now see the `BridgeCollapse` component with a **NEW section at the top**:

```
╔════════════════════════════════════╗
║ Bridge Collapse (Script)           ║
╠════════════════════════════════════╣
║ Destructible Settings              ║  ← NEW SECTION!
║ ┌────────────────────────────────┐ ║
║ │ Destructible Type  [Hazard ▼] │ ║  ← THIS DROPDOWN!
║ └────────────────────────────────┘ ║
║                                    ║
║ Debug - Read Only                  ║
║ Player On Bridge: false            ║
║ Sheep On Bridge: false             ║
╚════════════════════════════════════╝
```

### 4. Choose the Type from Dropdown

Click the dropdown that says `[Hazard ▼]` and you'll see 3 options:

**Option 1: Hazard** (Default)
- Use for: Bridges, spike traps, collapsing floors
- Behavior: **Respawns every time player dies**
- Why: So player can try the challenge again

**Option 2: Progress**
- Use for: Breakable walls, obstacles you need to remove
- Behavior: **Stays destroyed once broken**
- Why: It's progress - player shouldn't have to break it again

**Option 3: Consumable** (Future use)
- Use for: Crates, barrels (when you add them later)
- Behavior: Respawns on checkpoint save, not on every death
- Not implemented yet - leave this for future

## Examples

### Example 1: Bridge Over Lava
```
Object: "Bridge1"
Component: BridgeCollapse.cs
Setting: Destructible Type → Hazard

Reason: Bridge should respawn every time player dies,
        otherwise they'd be stuck unable to cross.
```

### Example 2: Breakable Wall Blocking Path
```
Object: "BreakableWall"
Component: BridgeCollapse.cs
Setting: Destructible Type → Progress

Reason: Once player breaks through this wall to access
        a new area, it should STAY broken. That's progress!
```

### Example 3: Collapsing Floor Trap
```
Object: "CollapsibleFloor"
Component: BridgeCollapse.cs
Setting: Destructible Type → Hazard

Reason: Floor should respawn so player can attempt
        the jump/puzzle again after dying.
```

## What Happens When You Set This

### Hazard Type (Bridges)
1. Player crosses bridge → Falls → Dies
2. Checkpoint loads
3. **Bridge is ACTIVE again** (respawned)
4. Player can try crossing again

### Progress Type (Walls)
1. Player breaks wall → Continues through
2. Player reaches checkpoint
3. Player dies later
4. Checkpoint loads
5. **Wall stays BROKEN** (progress saved)
6. Player doesn't have to break it again

## How to Check Your Settings

For each destructible in your scene:

**Bridges/Traps** (should respawn):
- [ ] Bridge over gap → Hazard ✅
- [ ] Collapsing floor → Hazard ✅
- [ ] Spike trap → Hazard ✅

**Obstacles/Progress** (should stay destroyed):
- [ ] Breakable wall → Progress ✅
- [ ] Removable obstacle → Progress ✅

## Visual Reference

Here's what the Inspector looks like:

**BEFORE (Old System):**
```
Bridge Collapse (Script)
  Debug - Read Only
    Player On Bridge: false
    Sheep On Bridge: false
```

**AFTER (New System):**
```
Bridge Collapse (Script)
  Destructible Settings         ← NEW!
    Destructible Type: [Hazard ▼]  ← NEW DROPDOWN!

  Debug - Read Only
    Player On Bridge: false
    Sheep On Bridge: false
```

## Common Mistakes

❌ **WRONG:** Setting a bridge to "Progress"
- Problem: Bridge won't respawn, player gets stuck
- Fix: Change to "Hazard"

❌ **WRONG:** Setting a breakable wall to "Hazard"
- Problem: Wall respawns every death, player loses progress
- Fix: Change to "Progress"

✅ **CORRECT:**
- Bridges = Hazard (respawn)
- Walls = Progress (stay broken)

## Need Help?

If you don't see the "Destructible Type" dropdown:
1. Check that BridgeCollapse.cs has been saved
2. Unity might need to recompile - wait a moment
3. Try closing and reopening Unity
4. Make sure you're looking at BridgeCollapse component (not another script)

## Testing

After setting types, test it:

1. Break a wall (if Progress type)
2. Cross a bridge (if Hazard type)
3. Save checkpoint
4. Die somehow
5. Check results:
   - Hazard bridges should be back
   - Progress walls should still be broken

## Default Setting

If you don't change anything, **all destructibles default to "Hazard"** (respawn behavior).

This is safe - it prevents soft-locking. But you should change walls to "Progress" to give players a sense of achievement!
