# Setup Guide - Camera & Player Configuration

## Camera-Relative Movement (COMPLETED ✅)

The player now moves relative to the camera direction, just like modern 3rd-person games (Zelda, Dark Souls, etc.).

### How It Works:
- Press **W** → Move toward where camera is looking
- Press **S** → Move away from camera
- Press **A/D** → Strafe left/right relative to camera
- Player smoothly rotates to face the movement direction
- You can rotate the camera while standing still

### Code Changes Made:
1. Added `cameraTransform` cache to store main camera reference
2. Created `GetCameraRelativeMovement()` method that:
   - Uses camera's forward/right vectors
   - Projects them onto horizontal plane (ignores camera pitch)
   - Calculates movement direction based on WASD input
3. Updated `RotatePlayer()` to use camera-relative direction
4. Added smooth rotation using `Quaternion.Slerp()` (adjustable via `rotationSpeed`)

### Inspector Settings:
- **Rotation Speed**: `10f` (how fast player turns to face movement direction)
  - Higher = snappier rotation
  - Lower = smoother, more momentum-based feel

---

## Cinemachine FreeLook Y-Axis Inversion

To invert the Y-axis (move mouse up = look up), follow these steps:

### Method 1: Using Cinemachine Input Axis Controller (Recommended)

1. **Select your Cinemachine FreeLook camera** in the Hierarchy
2. **Find the "Axis Control" section** in the Inspector
3. Locate the **Y Axis** settings (controls vertical orbit)
4. **Change the "Invert Input" option**:
   - Check the box for inverted controls (flight sim style)
   - Or multiply "Input Axis Value" by `-1`

### Method 2: Modify Input Actions (Alternative)

If using the new Input System with Cinemachine:

1. Open `GameControls.inputactions`
2. Find the **Look** action
3. Add a **"Invert Vector2" processor** to the binding
4. Configure it to invert only the Y component

### Method 3: In Cinemachine Input Provider

If you're using a CinemachineInputProvider component:

1. Select the GameObject with `CinemachineInputProvider`
2. In the Inspector, find the Y Axis mapping
3. Add a negative multiplier or invert flag

---

## Cinemachine FreeLook Settings Recommendations

For a smooth third-person camera experience:

### Body Settings:
- **Binding Mode**: World Space (camera independent of player rotation)
- **Y Axis**:
  - Value: `0.5` (middle rig by default)
  - Min: `-30°` to `0°` (low angle)
  - Max: `50°` to `80°` (high angle)
- **X Axis**:
  - Speed: `200-300` (horizontal rotation speed)
  - Acceleration/Deceleration Time: `0.1-0.2` (smooth feel)

### Aim Settings:
- **Tracked Object Offset**: `(0, 1.5, 0)` (look at player's chest/head height)
- **Dead Zone**: `0.1` (small center area with no camera movement)
- **Soft Zone**: `0.6` (area where camera starts to move)

### Rigs (Top/Middle/Bottom):
Each rig controls camera position at different heights:
- **Top Rig**: Height `4`, Radius `3` (bird's eye view)
- **Middle Rig**: Height `2`, Radius `4` (standard gameplay view) ← Most used
- **Bottom Rig**: Height `0.5`, Radius `3` (low angle view)

### Noise (Optional):
Add subtle camera shake for cinematic feel:
- Use `Basic Multi Channel Perlin` noise profile
- Amplitude Gain: `0.5-1.0`
- Frequency Gain: `0.5-1.0`

---

## Testing Your Setup

### Camera Movement:
1. Press **Play**
2. Move mouse to rotate camera around player
3. Player should stand still while camera orbits
4. Check Y-axis inversion works as expected

### Player Movement:
1. Rotate camera to face a direction
2. Press **W** - player should move toward camera view
3. While moving, rotate camera - player adjusts movement direction
4. Press **Space** - player should jump
5. Press **A/D** - player should strafe left/right

### Expected Behavior:
- Camera rotates smoothly around stationary player
- Player faces movement direction when WASD is pressed
- Movement is always relative to camera view
- Player rotation is smooth, not instant snap

---

## Troubleshooting

### Issue: Camera doesn't follow player
**Solution**: Check Cinemachine FreeLook's "Follow" and "Look At" targets are set to Player

### Issue: Movement feels wrong
**Solution**: Adjust `rotationSpeed` in PlayerController (default: 10)
- Too fast? Lower to 5-7
- Too slow? Increase to 12-15

### Issue: Player moves in world space, not camera-relative
**Solution**: Make sure Main Camera has the "MainCamera" tag

### Issue: Y-axis inversion not working
**Solution**:
1. Check Cinemachine version (should be 2.9+)
2. Try Method 1 from the Y-axis inversion section above
3. Verify Input System is properly configured

### Issue: Camera clips through ground/walls
**Solution**:
- Enable "Collider" component on Cinemachine FreeLook
- Set layers to collide with (typically "Default" + "Environment")
- Adjust "Damping" when occluded

---

## Next Steps

Now that camera and movement are working:

1. **Test different Cinemachine settings** to find your preferred feel
2. **Adjust player speed** in Inspector if movement feels too fast/slow
3. **Add animations** (idle, walk, run, jump) that match movement
4. **Implement sneaking/running** states as per CLAUDE.MD design doc
5. **Create test level geometry** to test camera behavior in tight spaces

---

## Additional Resources

- [Cinemachine Documentation](https://docs.unity3d.com/Packages/com.unity.cinemachine@2.9/manual/index.html)
- [Unity Input System Guide](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/index.html)
- [Third Person Controller Best Practices](https://learn.unity.com)

---

*Last Updated: January 2026*
*Status: Camera-relative movement implemented ✅*
