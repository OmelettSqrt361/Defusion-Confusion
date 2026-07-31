# 💣 Defusion Confusion

A top-down 2D "bomb-defusal" game made in Unity for the **GMTK Game Jam 2026**. Run around a room, pick up tools, and complete a string of frantic mini-tasks (cut the right wires, unscrew a panel, crack a keypad, plug in a USB, pick a lock...) before the timer on one or more bombs hits zero.

---

## Table of contents

- [About the project](#about-the-project)
- [Requirements](#requirements)
- [Getting started](#getting-started)
- [Controls](#controls)
- [Project structure](#project-structure)
- [Scenes](#scenes)
- [Documentation](#documentation)
- [Design notes](#design-notes)
- [Roadmap / ideas](#roadmap--ideas)
- [Contributing](#contributing)

---

## About the project

Defusion Confusion is built around a simple loop: one or more **bombs** are ticking down in a room, and the only way to stop them is to walk up to **tasks** (wires, screws, a computer, a lock, a box...) and solve each one, usually while holding the correct **item** (screwdriver, key, scissors, USB stick). Solve all the tasks tied to a bomb and it defuses; run out of time on any bomb and it's game over.

The tighter the timer gets, the faster the player character moves — the game leans into rising tension rather than punishing precision alone.

This repository contains the full Unity project (scenes, scripts, art, animations, audio) as well as the jam's working notes and level-design brainstorms.

## Requirements

- **Unity 2020.3.29f1** (LTS) — this is the exact editor version the project was authored in (see `GMTK2026/ProjectSettings/ProjectVersion.txt`). Using a different 2020.3.x patch will usually still open the project, but the closer to this version the better.
- A machine capable of running the Unity Editor (Windows/macOS/Linux all work — the project itself is 2D and has no platform-specific code).
- No external/paid assets are required; all package dependencies are pulled automatically from Unity's package registry the first time the project opens (Cinemachine, 2D Tilemap/Animation/SpriteShape, TextMeshPro, etc. — see `GMTK2026/Packages/manifest.json`).

## Getting started

1. Install **Unity Hub** and, through it, **Unity 2020.3.29f1** (or the closest available 2020.3 LTS patch).
2. Clone or download this repository.
3. In Unity Hub, choose **Add project** and point it at the `GMTK2026/` folder (that's the actual Unity project root — not the repository root).
4. Open the project. Let Unity import assets and resolve packages on first load (this can take a few minutes).
5. In the **Project** window, open `Assets/Scenes/NonlevelPlayables/Menu.unity` and press **Play** to start from the main menu, or open any scene under `Assets/Scenes/LevelsOriginal/` to jump straight into a level.

> New to the codebase? Start with [`docs/DOCUMENTATION.md`](docs/DOCUMENTATION.md) — it explains how all the scripts fit together and walks through building a level and a new task from scratch.

## Controls

| Input | Action |
|---|---|
| `W` `A` `S` `D` / Arrow keys | Move |
| `X` or `E` | Context action: pick up a nearby item, drop a held item, or open a nearby task |
| Mouse | Interact with on-screen task UI (buttons, sliders, keypad, drag-tools) once a task is open |
| `Esc` | Quit (main menu) |

## Project structure

```
Defusion-Confusion-main/
├── README.md                          – you are here
├── Defusion-Confusion-Documentation.md – original script/architecture reference
├── notes.txt                          – art direction + "how to build a level" checklist
├── level ideas.txt                    – brainstormed levels & gimmicks
├── otherIdeas.txt                     – misc feature ideas (tutorial, settings menu, etc.)
└── GMTK2026/                          – the actual Unity project
    ├── Assets/
    │   ├── Scripts/                   – all gameplay C# scripts
    │   ├── Editor/                    – custom Inspector code (GameManager, Task)
    │   ├── Animation/                 – animator controllers + clips (Player, Items, Tasks, Menus)
    │   ├── Scenes/
    │   │   ├── LevelsOriginal/        – the jam's playable levels (Warehouse, BombSchool, Office)
    │   │   ├── LevelsRemakes/         – reworked/updated level(s)
    │   │   ├── NonlevelPlayables/     – Menu + Story1‑5 cutscene scenes
    │   │   └── TestScenes/            – scratch/prototype scenes
    │   └── TextMesh Pro/              – TMP runtime assets
    ├── Packages/                      – Unity Package Manager manifest
    └── ProjectSettings/               – Unity project configuration
```

## Scenes

Scenes registered in Build Settings, in play order:

1. `NonlevelPlayables/Menu` — main menu
2. `NonlevelPlayables/Story1`
3. `LevelsOriginal/BombSchool` — tutorial-style first level
4. `NonlevelPlayables/Story2`
5. `LevelsOriginal/Warehouse`
6. `NonlevelPlayables/Story3`
7. `LevelsOriginal/Office`
8. `NonlevelPlayables/Story4`
9. `NonlevelPlayables/Story5`

`Assets/Scenes/TestScenes/` contains prototyping scenes (camera test, player movement test, horse minigame test, tileset test) that are useful references but aren't part of the shipped flow. `Assets/Scenes/LevelsRemakes/WarehouseRemake` is a work-in-progress redo of the Warehouse level.

## Documentation

This repo ships two documentation files, aimed at different needs:

| File | What it's for |
|---|---|
| [`Defusion-Confusion-Documentation.md`](Defusion-Confusion-Documentation.md) | Deep-dive **reference**: every script, what it depends on, and the required GameObject tags. Best when you already know Unity and just need to know "what calls what." |
| [`docs/DOCUMENTATION.md`](docs/DOCUMENTATION.md) | **Guide + tutorials**: the same architecture explained conversationally, plus step-by-step walkthroughs for building a new level and a new task type from scratch. Best starting point for new contributors. |

## Design notes

From `notes.txt`:

- **Art style**: simplistic, Flash-inspired, top-down "cubist" perspective. Sprites are 256×256; interactable objects get a 10px pure-black outline, non-interactables a lighter 10px outline, fine details a 5px outline. Tiles are 256×208 (16:13 aspect ratio).
- **Known quirk**: task hitboxes are not designed to overlap/collide with each other — this is a deliberate scope limitation, not a bug to chase.

## Roadmap / ideas

`level ideas.txt` and `otherIdeas.txt` capture the team's brainstorming and are a good source of "what's next":

- **Full level concepts**: Metro (bomb tied to a train's arrival/departure), Manor (entertain guests or lose + cook an omelette task), Hospital, Power Plant (light management), Magic School (moving platforms).
- **Gimmick levels**: a level with many small bombs, a "long walk, trivial defuse" tunnel level, a story/lore level at the antagonists' HQ.
- **Standalone feature ideas**: an in-between-levels textbox/dialogue system (also usable for tutorialization), a settings menu (outline thickness, audio, key rebinding, mouse-only mode for mobile), a dedicated tutorial level, and a custom raster font for UI text.

## Contributing

This started as a solo/small-team game jam project built under a strict time limit, so expect some rough edges (a hardcoded keypad code, some duplicated logic between `GameManager.Update()` and `GameManager.DefuseBomb()`, etc. — see the "Notable quirks" section of the documentation). Before adding a new task type or level, read [`docs/DOCUMENTATION.md`](docs/DOCUMENTATION.md) — the tag-based wiring between scripts means a scene can compile fine but fail at runtime if a required tag is missing.
