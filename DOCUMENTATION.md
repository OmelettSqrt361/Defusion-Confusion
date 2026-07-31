# Defusion Confusion — Guide & Tutorials

This is the "start here" companion to [`Defusion-Confusion-Documentation.md`](../Defusion-Confusion-Documentation.md). That file is a dense script-by-script reference; this one explains the same ideas conversationally and then walks you through actually **doing** things: setting up the project, building a level, and adding a new task.

If you just want to know "what does script X call / need," jump to the reference doc instead. If you want to understand the game well enough to build something new, start here.

## Table of contents

1. [How the game works, conceptually](#1-how-the-game-works-conceptually)
2. [The tag system — the project's real wiring diagram](#2-the-tag-system--the-projects-real-wiring-diagram)
3. [Core concepts glossary](#3-core-concepts-glossary)
4. [Tutorial: setting up the project locally](#4-tutorial-setting-up-the-project-locally)
5. [Tutorial: playing through a level](#5-tutorial-playing-through-a-level)
6. [Tutorial: building a level from scratch](#6-tutorial-building-a-level-from-scratch)
7. [Tutorial: adding a new pickup item](#7-tutorial-adding-a-new-pickup-item)
8. [Tutorial: adding a new task (a simple button-mash task)](#8-tutorial-adding-a-new-task-a-simple-button-mash-task)
9. [Tutorial: wiring a task into a bomb's win condition](#9-tutorial-wiring-a-task-into-a-bombs-win-condition)
10. [Debugging tips](#10-debugging-tips)
11. [Common pitfalls](#11-common-pitfalls)
12. [Where to go next](#12-where-to-go-next)

---

## 1. How the game works, conceptually

Every level follows the same loop:

1. A **`GameManager`** sits at the center of the scene. It tracks every bomb's timer, whether the player has won or lost, and holds references to the HUD, death screen, and win screen.
2. One or more **`Bomb`** components count down. As soon as **any** bomb hits zero, the player loses.
3. Scattered around the room are **`Task`** objects — the bomb casing itself, a locked door, a box, a computer, a lock-and-key panel. Walking up to one and pressing **X/E** "zooms the camera in" on a UI panel spawned for that task.
4. Inside that UI panel are one or more **mini-games** (cut wires, unscrew a cover, type a passcode, plug in a USB, pick a lock...). Some of these require the player to be holding a specific **`Item`** (screwdriver, key, scissors, USB stick) before the relevant button becomes clickable.
5. Completing a mini-game increments two counters: the level-wide `GameManager.winConditions` and (if it belongs to a bomb) that `Bomb`'s own `bombCoditions`. When a bomb's condition count is met, it defuses. When the level's total win-condition count is met, `GameManager.Win()` fires.

The tension knob is `GameManager.currentBombFactor` — derived every frame from whichever bomb has the least time left relative to the level's longest timer. `PlayerControler` reads this value to speed the player's run speed up as things get closer to exploding.

## 2. The tag system — the project's real wiring diagram

Unlike a project built around singletons or a dependency-injection framework, Defusion Confusion's scripts find each other at runtime with `GameObject.FindWithTag(...)`. This is convenient, but it means **the actual "wiring diagram" of the game lives in the Tags column of the Inspector, not in code.** If a scene is missing a required tag, the game will compile fine and then throw a `NullReferenceException` the moment it hits Play.

Keep this table nearby whenever you build a new scene:

| Tag | Put it on... | Why |
|---|---|---|
| `GameController` | The object holding `GameManager` | Every gameplay script fetches the `GameManager` through this tag |
| `Player` | The player object (`PlayerControler` + `Rigidbody2D` + `Animator` + `AudioSource`) | Items/Tasks detect proximity via trigger colliders tagged `Player` |
| `Main Audio` | The object holding `MusicManager` | Keeps music alive across scene loads |
| `MainVCamera` | The main Cinemachine virtual camera | `Task` swaps priority to this when zooming back out |
| `Task Canvas` | The canvas that task-menu UIs get parented under | `Task.Start()` instantiates the menu here |
| `BlackScreen` | The fullscreen fade `Image` | `Door.Teleport()` fades through this |
| `Hideable Overlay` | The HUD canvas/group hidden while a task is open | `GameManager` toggles this |
| `Death Screen` | The (inactive by default) lose screen | Found via a special "find even if inactive" helper |
| `Win Screen` | The (inactive by default) win screen | Same as above |
| `Time Text` | The (inactive by default) win-timer `TextMeshProUGUI` | Same as above |

> `Death Screen`, `Win Screen`, and `Time Text` are allowed to start **disabled** in the scene — `GameManager` uses a custom `FindDisabledWithTag()` helper (because Unity's built-in `FindWithTag` skips inactive objects) specifically so these can be hidden by default.

## 3. Core concepts glossary

- **`GameManager`** — one per scene; the hub. Only does gameplay bookkeeping (bombs, win/lose, HUD) when `sceneType == Level`.
- **`Task`** — attached to any interactable "station" in the world (bomb, door, box, computer, lock panel). Owns the camera zoom and spawns a UI menu prefab.
- **`TaskMenuMain`** — a one-field bridge component sitting at the root of every spawned task-menu prefab; every sub-widget inside the menu climbs back up to its owning `Task` through this.
- **`Item`** — a pickup (screwdriver, key, scissors, USB stick). Its `attribute` string is the "key" that task sub-widgets check against.
- **Sub-widgets** (`Wire`, `Screw`, `Lock`, `UsbPlug`, `Computer`, `HorseBox`, ...) — live inside a task menu, each implementing one mini-game, and each reporting progress back up to a `Bomb` and/or `GameManager.winConditions`.
- **`attribute` matching** — the whole "you need the right tool" mechanic is just string comparison: an `Item.attribute` (e.g. `"screwdriver"`) has to match a `Screw.attribute`/`Lock.attribute`/etc. for that button to become interactable.

## 4. Tutorial: setting up the project locally

1. Install **Unity Hub**.
2. In Unity Hub → **Installs** → **Install Editor**, install **Unity 2020.3.29f1** (check `GMTK2026/ProjectSettings/ProjectVersion.txt` if a newer patch has replaced it — match the `2020.3.x` line as closely as possible).
3. Download/clone this repository to your machine.
4. In Unity Hub → **Projects** → **Add** → **Add project from disk**, select the **`GMTK2026`** folder specifically (not the repository root — that's just where the extra docs/notes live).
5. Click the project to open it. The first open will take longer than usual while Unity imports every asset and resolves packages listed in `GMTK2026/Packages/manifest.json` (Cinemachine, 2D Tilemap, 2D Animation, TextMeshPro, etc.).
6. Once the Editor is open, check the **Console** window for errors. A clean project should compile with zero errors; warnings are fine.

## 5. Tutorial: playing through a level

1. In the **Project** window, navigate to `Assets/Scenes/NonlevelPlayables/Menu.unity` and double-click to open it.
2. Press **Play** at the top of the Editor.
3. Use the main menu to start a level (this calls `sceneHopper.Hop(int)`, which loads a scene by its Build Settings index — see [Scenes](../README.md#scenes) in the README for the index order).
4. In a level:
   - Move with **WASD**/arrow keys.
   - Walk next to an item and press **X** or **E** to pick it up.
   - Walk next to a task (with or without an item in hand) and press **X**/**E** to open it — the camera will zoom in on that task's UI.
   - Solve the mini-game with the mouse.
   - Repeat until every bomb is defused, or run out of time and watch the death screen.

If you'd rather skip straight to a level for testing, you can also just open `Assets/Scenes/LevelsOriginal/BombSchool.unity` (or `Warehouse.unity` / `Office.unity`) directly and press Play — each `Level`-type scene is self-contained.

## 6. Tutorial: building a level from scratch

This follows the same steps the team used themselves (see `notes.txt`), fleshed out.

### Step 1 — Scaffolding & tags

1. Create a new scene (**File → New Scene**, 2D template).
2. Create an empty GameObject, name it `GameManager`, add the `GameManager` component, tag it `GameController`.
3. In its Inspector, set **Scene Type** to `Level`.
4. Create two more objects: `Death Screen` and `Win Screen` (each typically a full-screen `Image`/`Canvas` group), tag them `Death Screen` and `Win Screen` respectively, and set them **inactive** in the Hierarchy (uncheck the box next to their name).
5. Create a `TextMeshProUGUI` object for the win timer, tag it `Time Text`, and set it inactive too.
6. Add three Canvases: one tagged `Hideable Overlay` (the normal HUD, hidden while a task is open), one for a non-hideable overlay (always-on HUD elements, no tag required unless another script needs it), and one tagged `Task Canvas` (where task menus get spawned as children).
7. Create an empty `mainWorld` GameObject to act as the parent for your level's floor/wall/decoration art — this keeps the Hierarchy organized but has no code behavior on its own.

### Step 2 — Building the room

1. Under `mainWorld`, build your floor and wall tilemaps (256×208 tiles, 16:13 aspect ratio, top-down "cubist" perspective per the art direction notes).
2. Add decoration sprites in their own sub-group.
3. Add a `Wall` object (or several) with `Collider2D`s (non-trigger) to define the level boundaries — these are what stop the player from walking off the map.

### Step 3 — Player & camera

1. Drag in the `Player` prefab (or build one: `Rigidbody2D`, `Animator`, `AudioSource`, `PlayerControler`, tag `Player`).
2. Add the main Camera plus a Cinemachine confiner so the camera doesn't show empty space past the level boundary (give it a little padding so out-of-bounds areas are still textured, not blank).
3. Tag your Cinemachine virtual camera `MainVCamera`.

### Step 4 — Tasks & bombs

1. Place a bomb object in the room; give it a `Task` component with `taskType = bomb` and a `Bomb` component.
2. Set `Bomb.initTimer` (seconds) and `Bomb.bombConditionCount` (how many sub-tasks must be completed to defuse it).
3. Assign `Task.taskMenuPrefab` to a UI prefab whose root has a `TaskMenuMain` component — this is what gets spawned into `Task Canvas` when the player opens the task.
4. Add your other tasks (door, box, computer, lock) the same way — see the [task tutorial](#8-tutorial-adding-a-new-task-a-simple-button-mash-task) below for a concrete worked example.
5. On `GameManager`, add every `Bomb` in the scene to its `bombs` list, and set `winConditionCount` to the total number of win-condition increments across every task sub-widget in the level (see [§9](#9-tutorial-wiring-a-task-into-a-bombs-win-condition) — this number has to be tallied by hand, there's no automatic validation).
6. Press Play and test. Use the custom `Task` Inspector's **Defuse Bomb** debug button (visible when `taskType == bomb`) to skip straight to a defused state while iterating on level layout.

## 7. Tutorial: adding a new pickup item

Let's say you want to add a **wrench**.

1. Create a sprite/prefab for the wrench with a `SpriteRenderer` and an `Animator` (it needs `Idle`, `Held`, and typically `Interactable`/`Near` states/animations — copy an existing item like `Screwdriver` as a starting template).
2. Add a **trigger** `Collider2D` (`Is Trigger` checked) so the player's proximity triggers fire.
3. Add the `Item` component. Set:
   - `sr` → the sprite renderer
   - `animator` → the animator
   - `attribute` → a unique string identifying this tool, e.g. `"wrench"`
4. Place it in the level. When the player walks close and presses X/E, `PlayerControler.ItemPickup()` picks the *nearest* item in range, calls `Item.ItemGrabbed()`, and mirrors its `attribute` into `PlayerControler.attribute` — this is the value every task sub-widget will compare against.

## 8. Tutorial: adding a new task (a simple button-mash task)

This walks through adding a brand-new task type end-to-end: a "hold this bolt and click it N times" task, similar in spirit to `Screw`/`ScrewCover` but simplified. It doubles as a template for any new mini-game.

### Step 1 — The task menu prefab

1. Duplicate an existing task menu prefab (e.g. the Screw task's menu) as a starting point, or build a new UI prefab from scratch.
2. Its **root object must have a `TaskMenuMain` component** — every script inside will find its owning `Task` through `GetComponentInParent<TaskMenuMain>().controler`.
3. Inside, add a `Button` for the bolt.

### Step 2 — The mini-game script

Create a new script, e.g. `Bolt.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;

public class Bolt : MonoBehaviour
{
    public string attribute = "wrench";     // tool required to interact
    public int clicksNeeded = 5;
    int clicks = 0;

    Button button;
    TaskMenuMain menu;

    void Start()
    {
        button = GetComponent<Button>();
        menu = GetComponentInParent<TaskMenuMain>();
        button.onClick.AddListener(OnClick);
    }

    void Update()
    {
        // only clickable while the player holds the matching item
        button.interactable = (menu.controler.gm.taskItem == attribute
                                || menu.controler.gm.playerControler.attribute == attribute);
    }

    void OnClick()
    {
        clicks++;
        if (clicks >= clicksNeeded)
        {
            // report progress up — see §9 for wiring this into a bomb
            menu.controler.gameObject.GetComponent<Bomb>().bombCoditions++;
        }
    }
}
```

> This mirrors the pattern every existing sub-widget uses: check the held item's `attribute` before allowing interaction, then increment a counter on `Bomb`/`GameManager` when the mini-game is solved. Look at `Screw.cs` and `Wire.cs` side-by-side with this if anything is unclear — they're the closest real analogues.

4. Attach `Bolt` to the button object in your menu prefab, and set `attribute` in the Inspector to match the `Item.attribute` of the tool that should unlock it (e.g. `"wrench"`).

### Step 3 — The task itself

1. In the world, create the object your bolt task lives on (e.g. an engine panel). Add:
   - A `Task` component, `taskType` set appropriately (use `box`/`text`/whichever fits, or add a new enum case if it's structurally different — see `Task.cs`'s `taskType` enum).
   - `taskMenuPrefab` → the prefab you built in Step 1.
   - A trigger `Collider2D` so the player can be detected as "near" this task.
2. In `Task.toolNames[]` / `toolsToActivate[]` (edited via the custom Inspector's tools table), list the child GameObject name(s) inside your menu prefab that should only activate once the right tool is held — **the name here must exactly match the child's actual name in the prefab hierarchy**, matching is case-insensitive but otherwise exact.
3. Press Play, walk up to the task, press X/E, and confirm the camera zooms in and the button becomes clickable once you're holding the right item.

## 9. Tutorial: wiring a task into a bomb's win condition

This is the step that's easiest to get subtly wrong, because **nothing in the project validates it automatically.**

Two counters have to agree by the end of the level:

- `GameManager.winConditionCount` — set once, by hand, in the Inspector on the `GameManager` object. This should equal the **total number of win-condition increments that exist across every task in the level.**
- Each `Bomb.bombConditionCount` — set once, by hand, in the Inspector on that specific bomb. This should equal the **number of sub-tasks tied to that particular bomb.**

Every time a mini-game is solved, its script typically does two things:
```csharp
gm.winConditions++;              // counts toward the level's overall win
bomb.bombCoditions++;            // counts toward this specific bomb's defusal
```
(`Wire`/`WireHolder`, `usbInputTask`, and `HorseBox` all follow this pattern — see the reference doc's [§3 table](../Defusion-Confusion-Documentation.md#3-task-sub-systems-spawned-inside-a-task-menu) for the exact call sites.)

**Checklist when adding or removing a mini-game from a level:**

1. Does completing it call `gm.winConditions++` (directly, or via a bomb's `Defuse()` which adds `bombConditionCount` to it)? If the mini-game is bomb-bound, it should generally go through the bomb.
2. Does completing it call `bomb.bombCoditions++` on the correct `Bomb`? (Note the project's existing typo — it's `bombCoditions`, not `bombConditions` — match it exactly if you're editing `Bomb.cs`-adjacent code.)
3. Re-count: does `GameManager.winConditionCount` in the Inspector equal the sum of every `gm.winConditions++` call site that will actually fire in this level?
4. Re-count: does each `Bomb.bombConditionCount` equal the number of sub-tasks feeding into that specific bomb?

If these numbers are wrong, the symptom is usually one of: the level never triggers `Win()` even though everything looks solved, or a bomb never visually defuses even though its sub-tasks are done.

## 10. Debugging tips

- **`GameManagerEditor`** (custom Inspector on `GameManager`) has a **Debug Mode** foldout that exposes normally-read-only runtime state (current win conditions, bomb factor, etc.) while the game is running, plus a **Null references** foldout that flags unassigned key fields before you even hit Play.
- **`TaskEditor`** (custom Inspector on `Task`) shows a **Defuse Bomb** debug button when `taskType == bomb`, letting you skip a bomb's mini-games instantly while testing level flow.
- If you get a `NullReferenceException` the moment you press Play, it's almost always a **missing tag** — check the table in [§2](#2-the-tag-system--the-projects-real-wiring-diagram) first.
- Console warnings about "no `Main Audio` object found" are expected on the very first scene load; `GameManager` will instantiate `musicManagerFallback` automatically in that case.

## 11. Common pitfalls

- **Renaming a child object inside a task menu prefab** without updating `Task.toolNames[]` in the Inspector — `Task.GetMatchingChildren` matches by name string, so this silently breaks that tool with no compile error.
- **Forgetting a trigger collider** on an `Item` or `Task` — proximity detection (`itemsNear`/`tasksNear` on `PlayerControler`) depends entirely on `OnTriggerEnter2D`/`OnTriggerExit2D` firing, which needs `Is Trigger` checked and a matching collider/rigidbody pair on the player.
- **Overlapping task hitboxes** — per `notes.txt`, this is a known, accepted limitation ("due to programming shenanigans, task hitboxes cannot collide"), not something to try to fix — just keep tasks spaced apart in your level layout.
- **Mismatched win-condition counts** — see [§9](#9-tutorial-wiring-a-task-into-a-bombs-win-condition).
- **The keypad passcode is hardcoded** (`"21885"` in `Computer.cs`) — if you add a second computer task expecting a different code, you'll need to make the code an Inspector field first rather than assuming it's configurable today.

## 12. Where to go next

- Read [`Defusion-Confusion-Documentation.md`](../Defusion-Confusion-Documentation.md) top to bottom once — it's short enough, and it's the authoritative map of every script's dependencies.
- Skim `level ideas.txt` and `otherIdeas.txt` in the repo root for a backlog of features and levels that haven't been built yet — a tutorial level, a settings menu, and additional full levels (Metro, Manor, Hospital, Power Plant, Magic School) are all still on the table.
- When in doubt about a specific script's exact fields and call sites, open the `.cs` file directly under `GMTK2026/Assets/Scripts/` — every script in this project is short enough to read in a couple of minutes.
