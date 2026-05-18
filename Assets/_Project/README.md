# Holy Mackerel — Project Notes

Working title for a portrait-mode 2D mobile fishing game built in Unity 6.4 with URP 2D.
Target platforms: iOS and Android. Reference resolution: 1170 × 2532 (iPhone portrait).

---

## Project Structure

All first-party game code and assets live under `Assets/_Project/`. The underscore prefix
keeps this folder sorted to the top of the Project window, above any imported packages.

```
Assets/_Project/
  Scripts/
    StartScreen/   # Scripts specific to the title screen
    Game/          # Scripts specific to the in-game fishing scene
    Core/          # Cross-cutting utilities (scene loading, save data, etc.)
  Scenes/          # .unity scene files (StartScreen, GameScene)
  Sprites/
    StartScreen/   # Backgrounds, logo, "tap to start" art
    Game/          # Fish, hook, water, environment art
    UI/            # Buttons, icons, HUD elements
  Fonts/           # TMP font assets and source TTF/OTF files
  Prefabs/         # Reusable game objects (fish variants, UI panels, etc.)
  Audio/
    SFX/           # Short sound effects
    Music/         # Looping music tracks
```

C# code uses the `HolyMackerel.*` namespace, mirroring the folder hierarchy
(e.g. `HolyMackerel.Core`, `HolyMackerel.StartScreen`).

---

## Current State

- **StartScreen scene** — title art with a blinking "-TAP TO START-" prompt.
  A tap anywhere on the screen transitions to the game scene.
- **GameScene** — currently empty; placeholder for the fishing gameplay loop.

Scripts implemented so far:

| Script | Purpose |
| --- | --- |
| `Core/SceneLoader.cs` | Static helpers for switching scenes by name. |
| `StartScreen/StartScreenController.cs` | Listens for any tap/click and loads `GameScene`. Has a 0.5s grace period to swallow stray taps carried over from a previous scene. |
| `StartScreen/PressToStartBlink.cs` | Ping-pong fade on a TMP text for the "TAP TO START" prompt. |

---

## Naming Conventions

- **Scenes:** `StartScreen`, `GameScene` (exact strings — `SceneLoader` references them as constants).
- **Scripts / Classes:** PascalCase, namespace matches subfolder under `Scripts/`.
- **Prefabs:** PascalCase, no spaces (e.g. `FishCommon`, `HookRig`).
- **Sprites / Audio:** PascalCase or snake_case is fine; keep consistent within a folder.

---

## Next Steps For The Editor

_Manual steps to be performed in the Unity Editor (to be filled in following the assistant's
walkthrough)._ Rough outline:

- [ ] Create `StartScreen.unity` and `GameScene.unity` inside `Assets/_Project/Scenes/`.
- [ ] Add both scenes to **File → Build Profiles → Scene List** in the correct order
      (StartScreen at index 0, GameScene at index 1).
- [ ] In the StartScreen scene:
  - [ ] Set up a Canvas (Screen Space – Camera or Overlay) configured for 1170×2532 portrait.
  - [ ] Add a TMP text reading "-TAP TO START-", attach `PressToStartBlink`,
        and assign the text component to its `targetText` field.
  - [ ] Add an empty GameObject called `StartScreenController` with the
        `StartScreenController` script attached.
- [ ] In the GameScene: leave empty for now (a single camera + URP 2D renderer is fine).
- [ ] Configure Player Settings:
  - [ ] Default Orientation: Portrait
  - [ ] Auto-Rotation: disabled (or Portrait-only allowed)
  - [ ] iOS bundle ID, Android package name set to placeholders.
- [ ] Confirm Input System setting: **Edit → Project Settings → Player → Active Input Handling**
      should be either "Input System Package (New)" or "Both".

---

## Future Plans

Upcoming systems (not yet implemented):

- **Hook physics** — line + lure controlled by tilt/touch.
- **Fish spawning** — pools of fish types with rarity weights and behaviors.
- **Upgrade shop** — rods, lines, lures, and bait progression.
- **Energy system** — limits play sessions, refills over time (or via ads/IAP).
- **Ads & IAP integration** — rewarded ads for energy, currency, and revives.
- **Save data** — persistent progress, currency, inventory.
- **Audio mixer** — separate SFX/Music buses with mute toggles.
