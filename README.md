# 🛡️ Elden Ring Custom Moveset Mod – “X Mod Name”
*A full-featured Elden Ring moveset overhaul with real-time Lua logic, custom animations, and a WPF overlay for transformation states.*

---

## 🔧 Tools & Technologies Used
- **Smithbox** – map, parameter, and text editing
- **WitchyBND** – for unpacking/packing game archives
- **FLVER Editor 2.0** – model and hitbox editing
- **DSAnimStudio** – animation/TAE editing
- **FXR Playground** – FX/visual effect editing
- **Lua scripting** – weapon logic & transformation conditions
- **WPF (C#)** – custom overlay UI for transformation HP bar
- **ERClipEditor**, **HKLib** – clip and helper libraries

---

## 🎮 Features
- 🔁 **Custom Transformation Mechanic**:
  - A special moveset state triggered by HP conditions or weapon use.
  - Tracked in real-time using a custom **WPF overlay** showing transformation status and HP bar.

- 🗡️ **Unique Weapon Logic**:
  - Lua-based logic scripts determine attack behavior, transformation triggers, and cooldowns.

- 🧬 **Animation Overhaul**:
  - Edited TAE files using DSAnimStudio to create fluid, thematic movesets.

- 🧠 **Reverse Engineering & Tooling**:
  - Manual adjustment of FLVER models and hitboxes.
  - Packed/unpacked BND files with WitchyBND.
  - Custom FXR edits for visual flair.

---

## 📸 Screenshots / Demo
*(Insert GIFs, images, or YouTube links here)*  
- [ ] Gameplay showcase (YouTube)
- [ ] Lua script snippet
- [ ] Overlay UI preview
- [ ] Before/After animations

---

## 🧠 Challenges Overcome
- Worked with **undocumented binary formats** and community-made tools.
- Mapped transformation states to real-time UI with WPF & memory tracking.
- Balanced gameplay around a completely custom weapon logic system.

---

## 📁 Folder Structure (Example)
```
/MovesetMod
│
├── /LuaScripts             # Custom weapon logic
├── /Animations             # TAE/DSAnimStudio data
├── /Models                 # FLVER edits
├── /OverlayApp             # WPF project (HP Bar UI)
├── /FXR                    # Visual effects
├── /Screenshots            # Media for demo
├── README.md
```

---

## 💡 Future Plans
- Add transformation audio/voice lines using SoundBanks.
- Expand overlay to show weapon cooldowns or form timers.
- Package as a user-friendly mod installer.

---

## 🧑‍💻 About Me
I’m passionate about systems-level gameplay, reverse engineering, and technical design. This project was born from a love of Elden Ring and a desire to push the engine in creative ways using Lua scripting, C# overlays, and FromSoftware’s unique toolchain.
