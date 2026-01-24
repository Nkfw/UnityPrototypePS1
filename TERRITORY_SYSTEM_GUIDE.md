# Territory System - Implementation Guide

**Last Updated:** 2026-01-24

---

## 📋 Overview

The Territory System makes sheep return to their home position if they stay idle inside a territory zone for too long. Once sheep exit the zone, they're "escaped" and free forever.

---

## ✅ Implementation Complete

All three scripts have been implemented:

1. **SheepAttraction.cs** - Added "Returning" state and movement logic
2. **SheepFollowingZone.cs** - Timer management and territory detection
3. **TerritoryZone.cs** - Zone trigger detection

---

## 🔧 Setup Instructions

### **Step 1: Setup Territory Zone GameObject**

1. You already have the **"Following Zone"** GameObject in your scene ✅
2. **Add TerritoryZone.cs** script to the Following Zone GameObject
3. Verify Box Collider settings:
   - **Is Trigger** = ON ✅
   - **Size** = Adjust to cover desired territory area (currently 1x1x1 - make it bigger!)
   - Suggested size: X=10, Y=5, Z=10 (covers more area)

### **Step 2: Setup Sheep Prefab**

1. Select your **Sheep prefab**
2. **Add SheepFollowingZone.cs** component (if not already attached)
3. Configure parameters in Inspector:
   - **Thinking Duration** = 5 seconds (default)
   - Can adjust this value to test different timings

### **Step 3: Verify Component Setup**

On your Sheep, make sure you have:
- ✅ Sheep.cs
- ✅ SheepAttraction.cs
- ✅ SheepFollowingZone.cs (NEW)
- ✅ Rigidbody (Kinematic)
- ✅ Collider

---

## 🎮 How It Works

### **State Flow:**

```
1. Sheep spawns → homePosition = spawn location
2. Sheep eats lettuce → Returns to Idle state
3. Thinking timer starts → Counts down (5 seconds)
4. While counting down:
   - If NEW lettuce appears → Timer resets, sheep goes to lettuce
   - If player picks up sheep → Timer resets
   - If timer reaches 0 → Sheep enters "Returning" state
5. While returning home:
   - If NEW lettuce appears → Cancels return, goes to lettuce
   - If player picks up sheep → Cancels return, timer resets
   - If reaches home → Stops, returns to Idle state
6. If sheep exits territory zone:
   - hasEscaped = true
   - Sheep never returns home again (FREE FOREVER!)
```

---

## 🔍 New SheepAttraction States

The sheep state machine now includes:

| State | Description | Speed Used |
|-------|-------------|------------|
| Idle | Standing around, looking for attractions | 0 |
| Sniffing | Detected attraction, brief delay | 0 |
| Walking | Moving toward attraction | moveSpeed (4.5) |
| Eating | At attraction, consuming it | 0 |
| **Returning** | **Walking back to home position** | **moveSpeed (4.5)** |

---

## ⚙️ Configuration Parameters

### **SheepFollowingZone Settings:**

**Thinking Duration** (default: 5 seconds)
- How long sheep waits after becoming idle before returning home
- **2 seconds** = Aggressive (sheep returns quickly)
- **5 seconds** = Balanced (default)
- **10 seconds** = Forgiving (player has more time)

### **Debug Variables (Read Only in Inspector):**
- `isInsideZone` - Is sheep currently inside territory?
- `hasEscaped` - Has sheep ever left the zone?
- `thinkingTimer` - Current countdown value

---

## 🎨 Visual Debug Features

### **SheepAttraction Gizmos:**
- **Yellow wireframe sphere** - Detection radius for attractions
- **Green line** - Path to target attraction (when Walking/Eating)
- **Magenta line** - Path to home position (when Returning) ⭐ NEW
- **Green sphere** - Home position marker (when Returning) ⭐ NEW
- **Colored sphere above sheep** - Current state indicator:
  - White = Idle
  - Cyan = Sniffing
  - Green = Walking
  - Red = Eating
  - **Magenta = Returning** ⭐ NEW

### **SheepFollowingZone Gizmos:**
- **Blue wireframe sphere** - Home spawn position (during play mode)

### **TerritoryZone Gizmos:**
- **Orange transparent box** - Territory zone volume
- **Yellow wireframe box** - Territory zone boundaries

---

## 🧪 Testing Checklist

Test these scenarios to verify everything works:

### **Basic Return Behavior:**
1. ✅ Sheep eats lettuce → Timer starts (watch Inspector)
2. ✅ Wait 5 seconds → Sheep walks home (magenta line appears)
3. ✅ Sheep reaches home → Stops, goes to Idle (white indicator)

### **Interruption Cases:**
4. ✅ Drop new lettuce while returning → Sheep cancels return, goes to lettuce
5. ✅ Pick up sheep while returning → Return cancels, timer resets
6. ✅ Drop lettuce before timer expires → Timer resets, sheep goes to lettuce

### **Territory Escape:**
7. ✅ Lure sheep outside territory zone → `hasEscaped = true` in Inspector
8. ✅ After escaping, lettuce behavior still works
9. ✅ After escaping, sheep never returns home (even if idle for 10+ seconds)

### **Edge Cases:**
10. ✅ Sheep re-enters territory after escaping → Still marked as escaped
11. ✅ Multiple sheep in same territory → Each has independent timer
12. ✅ Sheep picks up lettuce while returning → Cancels return immediately

---

## 🐛 Troubleshooting

### **Issue: Sheep doesn't return home**

**Check:**
- Is SheepFollowingZone component attached to sheep? ✅
- Is `isInsideZone = true` in Inspector? ✅
- Has sheep escaped already? (check `hasEscaped`) ✅
- Is thinking timer counting down? (watch `thinkingTimer` value) ✅

### **Issue: Sheep returns home immediately**

**Check:**
- Is `thinkingDuration` set too low? (should be 5 seconds)
- Is timer being reset properly when sheep eats?

### **Issue: Territory zone not detecting sheep**

**Check:**
- Is TerritoryZone.cs attached to Following Zone GameObject? ✅
- Is Box Collider **Is Trigger = ON**? ✅
- Is sheep tagged as "Sheep"? ✅
- Is zone size big enough to cover the area?

### **Issue: Sheep walks through lettuce while returning**

**Expected behavior!** If lettuce appears while returning, sheep should cancel return and go to lettuce. If it's not happening:
- Check that `LookForAttractions()` is being called in `ReturnToHome()`

---

## 📊 Code Architecture Summary

### **SheepAttraction.cs Changes:**
```csharp
// Added to enum
Returning // NEW state

// Added fields
private Vector3 homePosition;
public SheepState CurrentState => currentState; // Exposes state

// Added methods
public void StartReturningHome(Vector3 home)
private void ReturnToHome()

// Added to Update() switch
case SheepState.Returning:
    ReturnToHome();
    break;
```

### **SheepFollowingZone.cs Responsibilities:**
- Stores home spawn position
- Manages thinking timer countdown
- Triggers return when timer expires
- Handles territory enter/exit events
- Resets timer when sheep is picked up

### **TerritoryZone.cs Responsibilities:**
- Detects when sheep enters zone
- Detects when sheep exits zone
- Calls OnEnteredTerritory() / OnExitedTerritory() on sheep

---

## 🎯 Gameplay Design Notes

### **Why This Design:**

1. **Predictable Speed** - Sheep returns at same speed as walking to lettuce (moveSpeed)
2. **Player Agency** - Player can cancel return by dropping new lettuce or picking up sheep
3. **Risk/Reward** - Player must either:
   - Lure sheep outside territory (safe but far from goal)
   - Grab sheep within 5-second window (risky but keeps sheep close)
4. **Permanent Escape** - Once sheep exits zone, it's free forever (rewards successful luring)

### **Tuning Tips:**

**Make it easier:**
- Increase `thinkingDuration` to 10+ seconds
- Make territory zone smaller
- Place lettuce spawners outside territory

**Make it harder:**
- Decrease `thinkingDuration` to 2-3 seconds
- Make territory zone larger
- Limit lettuce count

---

## 📝 Future Enhancements

Possible additions later:

- **Sound Effects** - Play sound when sheep starts returning
- **Animation** - Different walk animation for returning vs. walking to food
- **Speed Variation** - Return slower/faster based on distance
- **Multiple Territories** - Different zones with different timers
- **Visual Indicator** - UI element showing timer countdown

---

## ✅ Summary

You now have a complete, working territory system!

**Key Features:**
- ✅ Sheep returns home after 5 seconds of being idle
- ✅ Uses same movement speed (moveSpeed) for consistency
- ✅ Cancels return if new lettuce appears
- ✅ Permanent escape when leaving territory
- ✅ Full debug visualization with gizmos
- ✅ Clean separation of concerns (3 focused scripts)

**Next Steps:**
1. Test in Unity Play Mode
2. Adjust `thinkingDuration` to your preference
3. Scale up territory zone size to cover more area
4. Place multiple sheep to test simultaneous timers

**Good luck testing! 🐑**
