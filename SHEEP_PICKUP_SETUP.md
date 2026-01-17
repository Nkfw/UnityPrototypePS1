# Sheep Pickup System - Setup Guide

## Overview
Simple sheep pickup/carry/drop system using E key. Follows component-based architecture with separate scripts for interaction and sheep behavior.

---

## Scripts Created

### 1. PlayerInteraction.cs
- Handles E key pickup/drop logic
- Finds nearest sheep within range
- Manages carrying state
- Communicates with PlayerController for speed changes

### 2. Sheep.cs
- Basic sheep state (idle vs carried)
- Handles parenting to carry position
- Disables physics while being carried
- Re-enables physics when dropped

### 3. PlayerController.cs (Updated)
- Added `carrySpeedMultiplier = 0.7f` (70% speed when carrying)
- Added `isCarryingSheep` state
- Added `SetCarryingState()` method for PlayerInteraction to call
- GetCurrentSpeed() now includes carry modifier

---

## Unity Setup Instructions

### Step 1: Setup Player GameObject

1. **Select Player** in Hierarchy
2. **Add PlayerInteraction component**:
   - Click "Add Component"
   - Search for "Player Interaction"
   - Add it

3. **Configure PlayerInteraction** in Inspector:
   - **Carry Position**: Leave empty (script will auto-create it)
     - Or create empty child GameObject called "CarryPosition" at:
       - Position: (0, 1.5, 1) - in front and slightly up
   - **Pickup Range**: 2 (how close to sheep)
   - **Show Debug Gizmos**: ✓ (helpful for testing)

4. **Connect Input Action**:
   - Find **Player Input** component on Player
   - Expand **Events** → **Player** action map
   - Find **Use** event
   - Click `+` to add callback
   - Drag Player GameObject to field
   - Select `PlayerInteraction.OnUse` from dropdown

### Step 2: Setup Sheep GameObject

1. **Select your Sheep** GameObject in Hierarchy

2. **Set Tag**:
   - At top of Inspector, click "Tag" dropdown
   - Select "Sheep" (or create "Sheep" tag if it doesn't exist)
   - **CRITICAL**: Tag must be exactly "Sheep"

3. **Add Sheep component**:
   - Click "Add Component"
   - Search for "Sheep"
   - Add it

4. **Add Physics** (if not already present):
   - Add **Rigidbody** component:
     - Mass: 1
     - Use Gravity: ✓
     - Is Kinematic: ✗ (unchecked)

   - Add **Collider** (Capsule or Box):
     - Is Trigger: ✗ (unchecked - sheep should be solid)
     - Adjust size to match sheep model

5. **Verify Sheep.cs** component shows:
   - State: Is Being Carried = ✗ (unchecked)

### Step 3: Create Sheep Tag (If Needed)

If "Sheep" tag doesn't exist:
1. Top menu: **Edit → Project Settings**
2. Select **Tags and Layers**
3. Click `+` under Tags
4. Type: `Sheep`
5. Save

---

## How It Works

### Pickup Flow:
1. Player walks near sheep (within 2 units)
2. PlayerInteraction finds nearest sheep using `GameObject.FindGameObjectsWithTag("Sheep")`
3. Press **E key**
4. PlayerInteraction calls `sheep.OnPickedUp(carryPosition)`
5. Sheep:
   - Parents to carryPosition transform
   - Disables physics (becomes kinematic)
   - Disables collider
6. PlayerInteraction calls `playerController.SetCarryingState(true)`
7. Player speed reduces to 70% (3.5 units/sec from 5 units/sec)

### Drop Flow:
1. Press **E key** again while carrying
2. PlayerInteraction calls `sheep.OnDropped(dropPosition)`
3. Sheep:
   - Unparents from player
   - Moves to position in front of player
   - Re-enables physics
   - Re-enables collider
4. PlayerInteraction calls `playerController.SetCarryingState(false)`
5. Player speed returns to normal

---

## Testing Checklist

### Basic Functionality:
- [ ] Walk near sheep → Yellow wire sphere visible (pickup range)
- [ ] Green sphere visible at carry position (in front of player)
- [ ] Cyan line connects player to nearest sheep
- [ ] Press E near sheep → Sheep attaches to carry position
- [ ] Sheep held in front of player (position: 0, 1.5, 1)
- [ ] Player moves slower while carrying (70% speed)
- [ ] Press E again → Sheep drops in front of player
- [ ] Sheep falls to ground with physics
- [ ] Player speed returns to normal

### Edge Cases:
- [ ] Press E far from sheep → "No sheep nearby" debug message
- [ ] Multiple sheep → Picks up closest one
- [ ] Carry sheep + crouch → Speed = 5 * 0.7 * 0.5 = 1.75 units/sec
- [ ] Carry sheep + run → Speed = 5 * 0.7 * 2.2 = 7.7 units/sec
- [ ] Can't pick up same sheep twice (should already be carried)

### Debug Console Messages:
- `"Picked up sheep: [name]"` when picking up
- `"Dropped sheep: [name]"` when dropping
- `"No sheep nearby to pick up!"` when E pressed too far

---

## Current Speed Modifiers (All Stack!)

| State | Multiplier | Speed (from base 5) |
|-------|------------|---------------------|
| Normal | 1.0x | 5 units/sec |
| Crouching | 0.5x | 2.5 units/sec |
| Running | 2.2x | 11 units/sec |
| Carrying | 0.7x | 3.5 units/sec |
| **Crouch + Carry** | 0.5 * 0.7 = 0.35x | **1.75 units/sec** |
| **Run + Carry** | 2.2 * 0.7 = 1.54x | **7.7 units/sec** |

---

## Debug Visualization (Gizmos)

When **Show Debug Gizmos** is enabled on PlayerInteraction:

- **Yellow wire sphere** = Pickup range (2 units radius)
- **Green wire sphere** = Carry position (where sheep will be held)
- **Cyan line** = Connection to nearest sheep (when in range)
- **White wire sphere on sheep** = Sheep is idle (not carried)
- **Green wire sphere on sheep** = Sheep is being carried

---

## Troubleshooting

### Issue: E key doesn't work
**Solution**: Check PlayerInput component has "Use" event connected to `PlayerInteraction.OnUse`

### Issue: "No sheep nearby" but I'm right next to it
**Solution**:
1. Check sheep has "Sheep" tag (case-sensitive!)
2. Check Pickup Range in PlayerInteraction (increase to 3-4 for testing)

### Issue: Sheep doesn't appear when picked up
**Solution**:
1. Check Carry Position exists (should auto-create if null)
2. Manually create CarryPosition child GameObject at (0, 1.5, 1)
3. Assign it to PlayerInteraction's "Carry Position" field

### Issue: Sheep falls through floor when dropped
**Solution**:
1. Ensure ground has a Collider component
2. Check Sheep's Rigidbody has Use Gravity enabled

### Issue: Can't pick up sheep after dropping
**Solution**: Check Sheep.cs component shows "Is Being Carried = false" after drop

### Issue: Player doesn't slow down when carrying
**Solution**:
1. Check Console for "Picked up sheep" message
2. Verify PlayerController has `carrySpeedMultiplier = 0.7f`
3. Check PlayerInteraction is calling `playerController.SetCarryingState(true)`

---

## Future Enhancements (Not Implemented Yet)

From CLAUDE.MD design doc, future sheep features:
- **Sheep AI States**: Idle, Following (attraction to lettuce), Sniffing, Returning
- **Sheep Attraction**: Sheep walks toward lettuce/scent beams
- **Multiple Sheep**: Track which sheep is carried, which are idle
- **Goal Zone**: Detect player carrying sheep entering goal
- **Animations**: Sheep idle/carried animations
- **Audio**: Sheep "baa" sounds

---

## Architecture Notes

Following component-based separation:
- **PlayerController.cs** = Movement ONLY (speed, jump, gravity)
- **PlayerInteraction.cs** = Interaction logic (pickup, drop, E key)
- **Sheep.cs** = Sheep state (idle vs carried)

Benefits:
- If pickup breaks → check PlayerInteraction only
- If movement feels wrong → check PlayerController only
- Can test sheep behavior independently
- Easy to add more interactable objects later (items, gadgets)

---

## Next Steps

Once pickup is working:
1. **Test with multiple sheep** - Place 3-4 sheep, test picking up different ones
2. **Add Goal Zone** - Create trigger that detects player carrying sheep
3. **Add Sheep Attraction** - Implement lettuce that sheep walks toward
4. **Polish** - Add animations, sounds, particle effects

---

*Last Updated: January 2026*
*Status: Basic pickup/carry/drop complete ✅*
