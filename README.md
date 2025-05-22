# Elden Ring Custom Moveset Mod – “Soul Weapon Transformation”
*A full-featured Elden Ring moveset overhaul with real-time Lua logic, custom animations, and a WPF overlay for transformation states.*

---

## Tools & Technologies Used
- **Smithbox** – parameter, and text editing
- **WitchyBND** – for unpacking/packing game archives
- **FLVER Editor 2.0** – model and hitbox editing
- **DSAnimStudio** – animation/TAE editing
- **FXR Playground** – FX/visual effect editing
- **Lua scripting** – weapon logic & transformation conditions
- **WPF (C#)** – custom overlay UI for transformation HP bar
- **ERClipEditor**, **HKLib** – clip and helper libraries, opening behavior files

---

## Features
- **Custom Transformation Mechanic**:
  - A special moveset state triggered by weapon use.
  - Tracked in real-time using a custom **WPF overlay** showing transformation status and HP bar.

- **Unique Weapon Logic**:
  - Lua-based logic scripts determine attack behavior, transformation triggers, and cooldowns.

- **Animation Overhaul**:
  - Edited TAE files using DSAnimStudio to create fluid, thematic movesets.

- **Reverse Engineering & Tooling**:
  - Manual adjustment of FLVER models and hitboxes.
  - Packed/unpacked BND files with WitchyBND.
  - Custom FXR edits for visual flair.

---

## Demo
Can be found on my youtube channel: https://www.youtube.com/@techbot911/videos
A specific one that leverages modding not only the player but summons aswell and adding them to the game can be found here: https://youtu.be/yYVQKOa1qjU?si=tJft7Jvn6krzJq9V

---

## Challenges Overcome
- Worked with **undocumented binary formats** and community-made tools.
- Mapped transformation states to real-time UI with WPF & memory tracking.
- Balanced gameplay around a completely custom weapon logic system.

---

## Folder Structure (Core parts)
```
/Soul Weapon Transformation
│
├── /action                 # Custom weapon logic
├── /chr                    # TAE/DSAnimStudio data
├── /msg                    # Game text
├── /parts                  # FLVER and texture edits
├── /Resources              # WPF assets (HP Bar UI)
├── /sfx                    # Visual effects
├── regulation.bin          # Parameters
├── EldenOverlay.exe        # WPF overlay that auto starts elden ring
├── Other Files             # Includes helper files
```

---

## Future Plans
- Add more transformations
- Expand and imrpove overlay implementation.
- Package as a user-friendly mod installer.

---

## About Me
I’m passionate about systems-level gameplay, reverse engineering, and technical design. This project was born from a love of Elden Ring and a desire to push the engine in creative ways using Lua scripting, C# overlays, and FromSoftware’s unique toolchain.
