# Interface System Guide - Sheep Attraction Architecture

**Last Updated:** 2026-01-23

---

## What is an Interface?

An **interface** is like a **contract** or **job description** that says: "If you want to be this type of thing, you MUST implement these methods."

### Real-World Analogy

Think of it like applying for a job:
- **Interface = Job Description**: "To be a sheep attraction, you must have these 6 abilities"
- **Class Implementation = Job Application**: "Yes, I can do all those things!"

### Why Can't You Attach Interfaces in Unity?

Interfaces are **pure code contracts** - they have no data, no behavior, just requirements. Unity can only attach **MonoBehaviour scripts** to GameObjects because those contain actual logic and data.

However, when a MonoBehaviour **implements** an interface, it automatically gets the interface's "powers" and can be found by systems looking for that interface type.

---

## How The Sheep Attraction System Works

### Architecture Overview

```
ISheepAttraction (Interface)
        ↑ implements
        |
    ┌───┴────┬─────────┬──────────┐
    |        |         |          |
  Lettuce  Button   Flower   (Future types...)
```

### Step-by-Step Flow

#### Step 1: The Interface Contract (`ISheepAttraction.cs`)

The interface defines **6 required methods** that any attraction must implement:

```csharp
public interface ISheepAttraction
{
    Vector3 GetPosition();                      // Where are you?
    bool IsAvailable();                         // Can I interact with you?
    float GetPriority();                        // How attractive are you?
    void OnSheepInteract(Sheep sheep);          // I'm interacting with you!
    bool ShouldDestroyAfterInteraction();       // Should you disappear?
    GameObject GetGameObject();                 // What's your GameObject?
}
```

#### Step 2: Classes Implement the Interface

```csharp
public class Lettuce : MonoBehaviour, ISheepAttraction
//                                     ↑
//                            "I promise to implement
//                             all ISheepAttraction methods!"
{
    // ... pickup/drop code ...

    // ISheepAttraction implementation
    public Vector3 GetPosition() => transform.position;
    public bool IsAvailable() => !isBeingCarried;
    public float GetPriority() => 1.0f;
    // ... etc
}
```

#### Step 3: Sheep Searches Via Interface (NOT Specific Types)

**OLD WAY (Hardcoded - Bad ❌):**
```csharp
// Only finds lettuce - can't detect buttons or flowers!
GameObject[] lettuces = GameObject.FindGameObjectsWithTag("Lettuce");
Lettuce lettuceComponent = lettuceObj.GetComponent<Lettuce>();
```

**NEW WAY (Flexible - Good ✅):**
```csharp
// Finds ANY MonoBehaviour that implements ISheepAttraction
ISheepAttraction[] allAttractions = FindObjectsByType<MonoBehaviour>()
    .OfType<ISheepAttraction>()
    .ToArray();
```

This finds **Lettuce, Button, Flower, or ANY future attraction type** automatically!

#### Step 4: Sheep Interacts Through Interface

```csharp
foreach (ISheepAttraction attraction in allAttractions)
{
    // Sheep doesn't care what TYPE it is - just asks interface questions

    if (!attraction.IsAvailable())
        continue; // Skip unavailable attractions

    float distance = Vector3.Distance(position, attraction.GetPosition());
    float priority = attraction.GetPriority();

    // Choose best attraction...

    // Later, when sheep reaches it:
    attraction.OnSheepInteract(sheep);

    if (attraction.ShouldDestroyAfterInteraction())
        Destroy(attraction.GetGameObject());
}
```

**The sheep NEVER needs to know** if it's talking to Lettuce, Button, or Flower!

---

## Current Implementation: Lettuce

**File:** `Lettuce.cs`

```csharp
public class Lettuce : MonoBehaviour, ISheepAttraction
{
    private bool isBeingCarried = false;

    // ... pickup/drop mechanics ...

    // ===== ISheepAttraction Interface Implementation =====

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public bool IsAvailable()
    {
        // Lettuce is available if not being carried
        return !isBeingCarried;
    }

    public float GetPriority()
    {
        // Default priority (1.0 = baseline)
        return 1.0f;
    }

    public void OnSheepInteract(Sheep sheep)
    {
        Debug.Log($"Sheep {sheep.name} is eating lettuce {name}");
    }

    public bool ShouldDestroyAfterInteraction()
    {
        // Lettuce gets eaten (destroyed)
        return true;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
```

**Behavior:**
- Priority: 1.0 (baseline)
- Available: When NOT being carried by player
- Destroyed: YES (gets eaten)

---

## Example 1: Adding a Button Attraction

Buttons attract sheep and trigger actions (like opening doors), but stay in the level.

### Step 1: Create `ButtonAttraction.cs`

```csharp
using UnityEngine;

// This script goes on Button GameObjects
// Buttons attract sheep and trigger actions when pressed
public class ButtonAttraction : MonoBehaviour, ISheepAttraction
{
    [Header("Button Settings")]
    [SerializeField] private bool isPressed = false;
    [SerializeField] private float priority = 0.8f; // Slightly less attractive than lettuce

    [Header("Button Action")]
    [SerializeField] private GameObject doorToOpen; // What happens when pressed?

    // ===== ISheepAttraction Interface Implementation =====

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public bool IsAvailable()
    {
        // Button only available if not pressed yet
        return !isPressed;
    }

    public float GetPriority()
    {
        // Buttons are slightly less attractive than lettuce
        return priority;
    }

    public void OnSheepInteract(Sheep sheep)
    {
        // Mark as pressed
        isPressed = true;

        // Trigger button action
        if (doorToOpen != null)
        {
            doorToOpen.SetActive(false); // Open the door!
            Debug.Log($"Button pressed by {sheep.name}! Door opened!");
        }

        // Visual feedback: sink the button down
        transform.position += Vector3.down * 0.1f;

        Debug.Log($"Sheep {sheep.name} pressed button {name}");
    }

    public bool ShouldDestroyAfterInteraction()
    {
        // Buttons stay in the level after being pressed
        return false;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
```

### Step 2: Unity Setup

1. **Create Button GameObject:**
   - Hierarchy → Right-click → Create Empty → Name: "Button"
   - Add child Cube for visuals
   - Position it in your level

2. **Add Script:**
   - Select Button GameObject
   - Add Component → Scripts → ButtonAttraction
   - Set Priority = 0.8 (slightly less than lettuce's 1.0)
   - Drag a Door GameObject into "Door To Open" field (optional)

3. **Done!** Sheep will automatically detect it.

### Step 3: Test It

Press Play and watch:
- Sheep sniffs button
- Sheep walks to button
- Console: `"Sheep Sheep1 pressed button Button"`
- Console: `"Button pressed by Sheep1! Door opened!"`
- Console: `"Sheep: Finished interacting! Attraction remains."`
- Button stays in level (not destroyed)
- Button becomes unavailable (won't attract again)

---

## Example 2: Adding a Flower Attraction

Flowers smell nice but don't get eaten. Sheep can sniff them repeatedly with a cooldown.

### Create `FlowerAttraction.cs`

```csharp
using UnityEngine;

// This script goes on Flower GameObjects
// Flowers attract sheep for sniffing (reusable with cooldown)
public class FlowerAttraction : MonoBehaviour, ISheepAttraction
{
    [Header("Flower Settings")]
    [SerializeField] private float priority = 0.5f; // Less attractive than food
    [SerializeField] private float cooldownTime = 5f; // Time before sheep can sniff again

    private float nextAvailableTime = 0f;

    // ===== ISheepAttraction Interface Implementation =====

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public bool IsAvailable()
    {
        // Flower available if cooldown has passed
        return Time.time >= nextAvailableTime;
    }

    public float GetPriority()
    {
        return priority; // Lower priority than lettuce
    }

    public void OnSheepInteract(Sheep sheep)
    {
        // Sheep sniffs flower
        Debug.Log($"Sheep {sheep.name} sniffs flower {name}. Ahhh, lovely!");

        // Start cooldown
        nextAvailableTime = Time.time + cooldownTime;
    }

    public bool ShouldDestroyAfterInteraction()
    {
        // Flowers don't get destroyed - sheep just sniffs them
        return false;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
```

**Features:**
- Lower priority (0.5) - sheep prefer lettuce
- Cooldown system - unavailable for 5 seconds after sniffing
- Never destroyed - reusable attraction

---

## Comparison Table: Attraction Types

| Type | Priority | Available When? | Destroyed After Use? | Use Case |
|------|----------|-----------------|---------------------|----------|
| **Lettuce** | 1.0 (High) | Not carried | ✅ YES | Food - gets eaten |
| **Button** | 0.8 (Medium) | Not pressed | ❌ NO | Trigger mechanisms |
| **Flower** | 0.5 (Low) | Cooldown passed | ❌ NO | Decorative distraction |

### Selection Logic

Sheep chooses attractions based on:
1. **Distance**: Must be within detection radius
2. **Availability**: Must return `true` from `IsAvailable()`
3. **Priority**: If similar distance, higher priority wins

**Example Scenario:**
```
Sheep detects:
- Lettuce (Priority 1.0) at 3 units away
- Button (Priority 0.8) at 2.5 units away
- Flower (Priority 0.5) at 2 units away

Result: Sheep goes to Button
(Closest available attraction with decent priority)
```

---

## Visual Example: How Sheep "Asks" Attractions

```
Sheep: "Who implements ISheepAttraction?"

Unity finds:
   ✅ Lettuce    (implements ISheepAttraction)
   ✅ Button     (implements ISheepAttraction)
   ✅ Flower     (implements ISheepAttraction)
   ❌ Rock       (doesn't implement it - ignored)
   ❌ Tree       (doesn't implement it - ignored)

Sheep: "Lettuce, are you available?"
Lettuce: "Yes! I'm not being carried."

Sheep: "Button, are you available?"
Button: "No, I was already pressed."

Sheep: "Flower, are you available?"
Flower: "No, I'm on cooldown for 3 more seconds."

Sheep: "Okay, I'll go to Lettuce then!"
```

---

## Key Benefits of Interface-Based Design

### 1. **Extensibility**
Add new attraction types without modifying `SheepAttraction.cs`:
```csharp
// Just create a new script:
public class PerfumeAttraction : MonoBehaviour, ISheepAttraction
{
    // Implement 6 methods...
}

// Sheep automatically detects it! No code changes needed.
```

### 2. **Decoupling**
Sheep code doesn't know about specific types:
```csharp
// Sheep never uses:
GetComponent<Lettuce>()
GetComponent<Button>()
GetComponent<Flower>()

// Sheep only uses:
GetComponent<ISheepAttraction>() // Works for ALL types!
```

### 3. **Polymorphism**
Same interface, different behaviors:
```csharp
// All attractions respond to same method:
attraction.OnSheepInteract(sheep);

// But each does something different:
// - Lettuce: Gets eaten
// - Button: Triggers door
// - Flower: Starts cooldown
```

---

## Common Questions

### Q: Do I attach the interface to GameObjects?
**A:** No! You attach the **class that implements the interface** (like `Lettuce.cs`, `ButtonAttraction.cs`). The interface is just a contract.

### Q: How does sheep find attractions without tags?
**A:** `FindObjectsByType<MonoBehaviour>().OfType<ISheepAttraction>()` searches ALL MonoBehaviours and filters for those implementing the interface.

### Q: Can I have multiple attraction types in one level?
**A:** Yes! That's the whole point. Sheep will detect all of them and choose based on distance + priority.

### Q: What if I want lettuce to have higher priority than buttons?
**A:** Already done! Lettuce returns `1.0`, Button returns `0.8`. Sheep prefers higher priority at similar distances.

### Q: Can one GameObject have multiple attraction types?
**A:** Technically yes, but not recommended. One attraction type per GameObject keeps things clean.

---

## Future Attraction Ideas

### Scent Beam (Planned)
```csharp
public class ScentBeamAttraction : MonoBehaviour, ISheepAttraction
{
    public float GetPriority() => 1.5f; // Very attractive!
    public bool IsAvailable() => fanIsBlowing; // Only when Fan is active
    // Long-range attraction created by Fan + Perfume synergy
}
```

### Salad Gun Projectile (Planned)
```csharp
public class SaladProjectile : MonoBehaviour, ISheepAttraction
{
    public float GetPriority() => 1.2f; // More interesting than regular lettuce
    public bool ShouldDestroyAfterInteraction() => true; // Gets eaten
    // Shot from Salad Gun gadget
}
```

### Danger Zone (Anti-Attraction)
```csharp
public class DangerZone : MonoBehaviour, ISheepAttraction
{
    public float GetPriority() => -1.0f; // Negative = repel!
    public bool IsAvailable() => true; // Always active
    // Sheep could avoid negative priority attractions
}
```

---

## Code Reference

### Files Involved

**Interface Definition:**
- `ISheepAttraction.cs` - The contract (6 methods)

**Current Implementations:**
- `Lettuce.cs` - Food attraction (destroyable)

**Sheep AI:**
- `SheepAttraction.cs` - Uses interface to find attractions

**Example Implementations (Not Yet Created):**
- `ButtonAttraction.cs` - Trigger attraction (persistent)
- `FlowerAttraction.cs` - Decorative attraction (cooldown)

---

## Summary

**The Magic of Interfaces:**
You **never need to modify `SheepAttraction.cs`** to add new attraction types. Just:

1. Create a new script (e.g., `ButtonAttraction.cs`)
2. Implement `ISheepAttraction` interface
3. Attach it to a GameObject
4. Done! Sheep automatically detect it.

This is **polymorphism** in action - treating different types the same way through a common interface.

---

**End of Interface Guide**
