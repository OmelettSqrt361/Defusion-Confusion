# Defusion Confusion — Script & Object Reference

Unity 2D project (GMTK 2026 Game Jam). A "bomb-defusal" style game where a player runs around a room performing timed tasks (wires, screws, keypads, USB drives, locks, a horse-picking minigame...) while one or more bombs count down.

This doc maps **what talks to what**, **what each script needs to run**, and **how objects get created**, based on a read-through of every `.cs` file in the repo (`GMTK2026/Assets/`).

---

## 1. Architecture at a glance

There is no formal singleton — instead almost every script reaches the game's "hub" objects at `Start()`/`Awake()` via `GameObject.FindWithTag(...)`. That means **scene setup (tags) is the real dependency graph**, not just serialized references.

```
GameManager (hub)
 ├─ PlayerControler        (movement, item pickup, task interaction)
 ├─ MusicManager            (persists across scenes via DontDestroyOnLoad)
 ├─ SpriteOutlineManager    (visual highlight of interactables)
 └─ List<Bomb>               (win/lose timing)

Task (attached to every interactable "station": bomb, door, box, computer, lockAndKey, text)
 ├─ spawns a TaskMenuMain-rooted UI prefab (taskMenuPrefab) on Start()
 ├─ Bomb / Door / ComputerTask / ScrewCover / WireHolder / HorseBox / Lock
 │   all hang off that spawned menu and read `TaskMenuMain.controler` to get back to their owning Task
 └─ toolNames[] / toolsToActivate[] — items the player must be holding (matched by Item.attribute)
     to unlock certain sub-tools inside the task menu

Item (pickup, e.g. screwdriver/key/USB/scissors)
 └─ picked up by PlayerControler, carries an `attribute` string that Task/Lock/Screw/Wire/UsbPlug
     compare against to decide whether their button is currently usable
```

**Required GameObject tags** (scripts call `GameObject.FindWithTag(...)` for these — the scene will throw `NullReferenceException` at runtime if any are missing):

| Tag | Expected on | Used by |
|---|---|---|
| `GameController` | the `GameManager` object | almost every script (`gm = ...GetComponent<GameManager>()`) |
| `Player` | the player object (needs `PlayerControler`, `Rigidbody2D`, `Animator`, `AudioSource`) | `GameManager`, `Task`, `Item`, `InteractiveAudio` |
| `Main Audio` | the `MusicManager` object (persistent) | `GameManager`, `MusicManager` |
| `MainVCamera` | the main Cinemachine virtual camera | `Task` |
| `Task Canvas` | canvas that spawned task menus get parented under | `Task` |
| `BlackScreen` | the fullscreen fade `Image` used for teleport transitions | `Door` |
| `Hideable Overlay` | the HUD overlay hidden while a task is open | `GameManager` |
| `Death Screen` | inactive-by-default lose screen | `GameManager` (found via `FindDisabledWithTag`, so it can be inactive) |
| `Win Screen` | inactive-by-default win screen | `GameManager` (same, `FindDisabledWithTag`) |
| `Time Text` | inactive-by-default `TextMeshProUGUI` for the win timer | `GameManager` (same) |

`FindDisabledWithTag` is a custom helper (`GameManager.cs`) that scans `Resources.FindObjectsOfTypeAll` because Unity's normal `FindWithTag` skips inactive objects.

---

## 2. Core hub scripts

### `GameManager` (`Scripts/GameManager.cs`)
The central controller. One per scene, tagged `GameController`.

- **`sceneType`** enum (`MainMenu`, `Storyboard`, `Level`) gates most logic — only `Level` scenes wire up gameplay refs (player, overlay, death/win screens, timer text, outline manager).
- **On `Start()`**: finds `Player`, `Hideable Overlay`, `Death Screen`, `Win Screen`, `Time Text` (Level scenes only); finds-or-instantiates a `MusicManager` (`musicManagerFallback` prefab) if no `Main Audio`-tagged object exists yet; computes `maxBombTime` from all `bombs` in the list; optionally swaps music (`hasNewSong`); optionally builds the outline list from every `Item` and `Task` in the scene via reflection-style `GetObjectsWithScript`.
- **`Update()`**: drives the "how scared is the room" audio feel — `currentBombFactor` is derived from the *lowest* remaining bomb timer vs `maxBombTime`, and is read by `PlayerControler` to speed up player movement as bombs get close to exploding. Also fires `Win()` once `winConditions == winConditionCount`.
- **`Win()` / `Lose()`**: flip `hasEnded`, disable/enable relevant screens, stop/play audio.
- **`DefuseBomb(Bomb)`**: removes a bomb from `bombs` and recalculates `maxBombTime`/`currentBombFactor` — called by `Bomb.Defuse()`.
- **`AddOutlinedObject(type, obj)`**: lets runtime-spawned objects (e.g. `BoxOpener.SpawnItem`) register themselves with `SpriteOutlineManager`.
- **Instantiation requirement**: needs `AudioSource` on the same object; needs `musicManagerFallback` (a prefab with `MusicManager` + `AudioSource`) assigned in Inspector; needs `bombs` list populated (or left empty for non-bomb levels) in Inspector.
- Has a custom Inspector: `Editor/GameManagerEditor.cs` (grouped fields, debug-mode toggle, null-ref warnings).

### `PlayerControler` (`Scripts/PlayerControler.cs`)
Attached to the `Player`-tagged object. Requires `Rigidbody2D`, `Animator`, `AudioSource` on itself.

- Reads `Input.GetAxisRaw` for movement; uses a `windupTime` to ramp velocity in rather than snap to max speed; `runVelocityIncrease * gm.currentBombFactor` makes the player run faster as the bomb timer runs low.
- Tracks nearby interactables via two lists (`itemsNear`, `tasksNear`) that are populated/depopulated by `Item`/`Task` trigger callbacks (`OnTriggerEnter2D`/`Exit2D`) — **so Items and Tasks must have a `Collider2D` set to `isTrigger` and the Player needs a matching `Collider2D`/`Rigidbody2D` to generate those events.**
- `X`/`E` key handles: pick up / drop item, or open nearest task, depending on what's near and whether the player is holding something.
- `attribute` (string) mirrors the currently held `Item.attribute` — this is what `Task.toolNames[]`, `Lock.attribute`, `Screw.attribute`, `UsbPlug.attribute` compare against.
- **Depends on**: `GameManager` (tag `GameController`) for `currentBombFactor`.

### `Task` (`Scripts/Task.cs`)
Attached to every interactable "station" object (bomb casing, computer, box, door frame, lock-and-key panel, text sign). This is the single biggest hub script besides `GameManager`.

- `taskType` enum: `bomb`, `box`, `door`, `computer`, `text`, `lockAndKey`.
- **On `Start()`**: finds `MainVCamera`, `Player`, `Task Canvas`, own `GameManager`; grabs its own child `CinemachineVirtualCamera` as `taskCam`; if `hasTaskMenu`, **instantiates `taskMenuPrefab`** as a child of `Task Canvas`, and immediately sets `taskMenu.GetComponent<TaskMenuMain>().controler = this` — this is the link every task sub-script (`Bomb`, `ScrewCover`, `WireHolder`, `Computer`, `Lock`, `HorseBox` via `bomb`) uses to reach back to its parent `Task`.
- `GetMatchingChildren`/`GetChildrenWithTag` scan the spawned menu by **name** (case-insensitive) against `toolNames[]`, and by **tag `"Zoom"`** for zoom buttons — so prefab child object naming must exactly match the `toolNames` array configured in the Inspector.
- `TurnOn()`/`TurnOff()`: swap Cinemachine camera priorities (`taskCam` vs `mainCam`) to "zoom into" the task view, disable player movement (`doingTask = true`), hide the HUD overlay, activate any tool GameObjects whose name matches the player's currently-held item attribute.
- `ZoomIn(cam)`/`ZoomOut()`: secondary camera swap for zooming further into a sub-element (e.g. into a lock's keyhole), hides/shows `zoomButtons`.
- `Defuse()`: only meaningful if `taskType == bomb`; forwards to `Bomb.Defuse(true)` (used by the custom editor's debug "Defuse Bomb" button).
- Special-cased for `taskType == door`: doesn't zoom in, just calls `Door.Teleport()` directly.
- Has a custom Inspector: `Editor/TaskEditor.cs` (conditional fields per `taskType`, a tools table editor, null-ref warnings, debug defuse button).
- **Instantiation requirements**: `taskMenuPrefab` must contain a `TaskMenuMain` component at its root; needs an `Animator` on the same object; if `hasAudio`, needs an `AudioSource`.

### `TaskMenuMain` (`Scripts/TaskMenuMain.cs`)
Trivial "access point" — just holds `public Task controler`, set by `Task.Start()` at spawn time. Every task-menu sub-script (see §3) fetches its owning `Task` via `GetComponentInParent<TaskMenuMain>().controler`, so **the spawned task menu prefab hierarchy must keep `TaskMenuMain` at or above every sub-widget**.

---

## 3. Task sub-systems (spawned inside a task menu)

These all assume they're a descendant of a `TaskMenuMain`-holding object and reach their parent `Task` through it.

| Script | Role | Key dependency / requirement |
|---|---|---|
| **`Bomb`** | Countdown timer + win-condition tally for a bomb-type Task. Ticks `timer` down each frame (unless `gm.notBegun`), updates a `Slider` + two `TextMeshProUGUI` fields, calls `GameManager.Lose()` on hitting 0, or `GameManager.DefuseBomb(this)` once `bombCoditions == bombConditionCount`. | Needs `slider`, `lCDScreen` assigned in Inspector; needs sibling `Task` component (`gameObject.GetComponent<Task>()`) on the *same* object (not the menu) for `TurnOff()` calls. |
| **`Wire` / `WireHolder`** | Wire-cutting minigame. Each `Wire` is a `Button` + `Image` that becomes interactable only when `gm.taskItem == attribute` (scissors held); cutting a "good" wire increments `WireHolder.goodWiresCut`, cutting a "bad" one calls `gm.Lose()` immediately. `WireHolder` checks each frame if all good wires are cut and, if so, increments both `gm.winConditions` and `bomb.bombCoditions`. | `WireHolder` requires a `Bomb` reachable via `GetComponentInParent<TaskMenuMain>().controler.gameObject.GetComponent<Bomb>()` — i.e. the wire task must be a bomb-type task. Each `Wire` needs `Button`, `Animator`, `Image` on itself, and a `WireHolder` in a parent. |
| **`Screw` / `ScrewCover`** | Unscrewing minigame. Each `Screw` button is interactable only when the player holds the matching `attribute` item (screwdriver); `Unscrew()` increments `ScrewCover.screwsUnsrewed` and fires an animator trigger. Once `screwsUnsrewed == numberOfScrews`, `ScrewCover.TakeOff()` plays an "open" animation. | `ScrewCover` needs `Animator`; each `Screw` needs `Button`, `Animator`, and a `ScrewCover` ancestor. |
| **`Computer` / `ComputerTask`** | Multi-state keypad/download minigame (`state` 0–5: password, message, downloading, connection lost, take-usb, blank). Correct 5-digit code `"21885"` (hardcoded) advances state and triggers animations; `NextIfUsb()` requires `usbPlugged` first. `UsbFull()` **instantiates** a `usbFull` prefab at `spawnpoint`. | `Computer` needs its parent `Task` reachable via `TaskMenuMain`; sets `ComputerTask.headComputer = this` on start, so a **`ComputerTask` component must exist on the parent `Task` object** for this wiring to succeed. Needs `AudioSource`, `inputField`, `downloadSlider`, `computerSR` assigned. |
| **`UsbPlug`** | A drop target for the USB item; becomes interactable when the player holds the matching `attribute`. `KillUsb()` drops/disables the player's held item and hides itself. | Needs `Button` on self; depends on `GameManager.playerControler`. |
| **`usbInputTask`** | Similar drop-target pattern but distinguishes a "right" vs "wrong" USB attribute; plays different SFX, and on success grants a win condition + a bomb condition and disables the used item. | Needs `AudioSource`, `usbButton` (`Button`), `diode` (`Image`), `bomb` reference assigned. |
| **`Lock` / `LockCaller`** | Lock-and-key minigame. `Lock`'s button is interactable only while the matching key `attribute` is held; `UnlockAnim()` plays an animation whose end calls (via an Animation Event → `LockCaller.AnimEnd()`) `Lock.Unlocked()` → `Door.Unlock()`. `LockCaller` also exposes `NoZoomOut`/`ZoomOutOkay` to lock/unlock the player's ability to back out of the zoomed lock view mid-animation. | `Lock` needs a `Door` reference assigned; `LockCaller` needs a `TaskMenuMain` ancestor and calls into `Task.DisableZoomOut`/`EnableZoomOut`. |
| **`Door`** | Not menu-bound — lives on the actual door/teleport-pad GameObject in the world. Wraps a `Task` (`taskType = door`). Starts `Unlock()`ed if `isLocked == false`. `Teleport()` fades to black (`BlackScreen`), moves the *player's* transform directly to `teleportDest`, and enforces a cooldown (`maxTeleportBuffer`). Triggered either by `Task.TurnOn()` (when `doorTask == true`) or directly by `OnCollisionEnter2D` with the player. | Needs `Animator`, `AudioSource`, sibling `Task`; needs the scene-wide `BlackScreen`-tagged object; needs `teleportDest` assigned. |
| **`HorseBox`** | A "find the horse among decoys" minigame with an escalating turn counter and a final "silly round" (squid-headed variant) at `maxTurns`. On success it calls `bombTask.ZoomOut()`, swaps to a win sprite, deactivates all its children, and grants a win + bomb condition. | Needs a `bomb` reference (drives `bombTask = bomb.gameObject.GetComponent<Task>()`), `Image` on self, `AudioSource`. |
| **`BoxOpener`** | Opens a box (`Animator` trigger) and, via an Animation Event calling `SpawnItem()`, **instantiates** `spawnedItem` at `spawnpoint` and registers it with `GameManager.AddOutlinedObject("item", ...)`. | Needs `Animator`, `SpriteRenderer`, `spawnedItem` prefab + `spawnpoint` assigned. |
| **`ComputerTask`** | Thin bridge living on the `Task` object for a computer-type task; exposes `DisableOnStartAnim`/`DisableZoom`/`EnableZoom` as Animation-Event-callable wrappers around `Computer`/`Task` methods. `headComputer` is set by `Computer.Start()`, not the Inspector. | Depends on `Computer` (menu-side) having run its `Start()` first. |

---

## 4. Item / interaction primitives

### `Item` (`Scripts/Item.cs`)
Attached to any pickupable object (screwdriver, key, USB stick, scissors...). Needs `SpriteRenderer` and `Animator` assigned, plus a trigger `Collider2D`.

- `OnTriggerEnter2D`/`Exit2D` (player only) call `PlayerControler.ItemAddProximity`/`ItemCloseProximity`.
- `attribute` (string, Inspector-set) is the identity token compared against by nearly every task sub-widget above.
- `ItemGrabbed()`/`ItemDropped()` toggle sorting order and an `Animator` "Hold" bool — called by `PlayerControler`, not by itself.

### `ToggleFollowCursor` (`Scripts/ToggleFollowCursor.cs`)
A UI-space drag-or-click "pick up and it follows the mouse" widget (used for something like a draggable in-menu tool). Requires `CanvasGroup`, `RectTransform`, a parent `Canvas` (uses `rootCanvas`) with a `worldCamera` set if not `ScreenSpaceOverlay`. While following, it writes `gm.taskItem = attribute` each frame (mirrors the "currently active tool" concept used by `Wire`/`usbInputTask`/etc., but for UI drag-tools rather than world items).

---

## 5. Audio

### `MusicManager` (`Scripts/MusicManager.cs`)
Tagged `Main Audio`. On `Awake()`, if no *other* `Main Audio` object already has a `GameManager` on it, it survives scene loads (`DontDestroyOnLoad`); otherwise it self-destructs — this is how music persists between the main menu and level scenes without duplicating. Holds the shared SFX clips (`beep`, `last10secs`, `boom`, `winSFX`) that `GameManager` copies references to at `Start()`.

### `AudioPlayer` (`Scripts/AudioPlayer.cs`)
Generic reusable "play this clip on demand" component (needs `AudioSource`); used as a UI-button callback target elsewhere in the project.

### `InteractiveAudio` (`Scripts/InteractiveAudio.cs`)
Distance-based volume/panning for ambient/positional sound sources relative to the `Player` tag — volume falls off linearly out to `hearingZenith`, optional stereo pan.

---

## 6. Visual/UI utility scripts

- **`SpriteOutlineManager`** (`Scripts/SpriteOutlineManager.cs`) — `[ExecuteAlways]`. Maintains a list of `(SpriteRenderer, Color)` pairs and pushes them into a shared `outlineMaterial` via `MaterialPropertyBlock` (so it doesn't create per-object material instances). Populated by `GameManager` at scene start and added-to at runtime via `GameManager.AddOutlinedObject`.
- **`BlackScreen`** (`Scripts/BlackScreen.cs`) — tagged `BlackScreen`; a full-screen `Image` fade used for `Door.Teleport()` transitions; `TurnOn(duration)` fades in and auto-clears after `duration` seconds.
- **`CountDown`** (`Scripts/CountDown.cs`) — the pre-level "3-2-1-Go" countdown; on animation completion (`OnCountdownEnd`, presumably an Animation Event) flips `gm.notBegun` and `playerControler.notBegun` to `false`, which is what actually unlocks player movement and bomb ticking.
- **`TilemapGrayscaleRecolor`** (`Assets/TilemapGrayscaleRecolor.cs`) — `[ExecuteAlways]`, `[RequireComponent(typeof(TilemapRenderer))]`. Remaps up to 16 source→target colors on a tilemap via shader properties (grayscale tileset + palette-swap shader), editable live in the editor.
- **`LCDScreen`** (`Scripts/LCDScreen.cs`) — one-line bridge: on `Start()`, finds the owning `Bomb` (via `TaskMenuMain`) and assigns its own `TextMeshProUGUI` as `bomb.lCDScreen`. Purely a wiring convenience so the LCD text object doesn't need a manual Inspector drag.

---

## 7. Misc / menu utility scripts

- **`quit.cs`** — `Escape` key or a UI button calls `Application.Quit()`.
- **`sceneHopper.cs`** — `Hop(int sceneID)` wraps `SceneManager.LoadScene(int)`; used by UI buttons (main menu → level, etc.). Scene indices must match Build Settings order.

---

## 8. Editor-only scripts

Located in `Assets/Editor/` — not compiled into builds.

- **`GameManagerEditor`** — custom Inspector for `GameManager`; groups fields by category, shows/hides bomb-list vs main-menu fields based on `sceneType`, has a "Debug Mode" foldout exposing normally-read-only runtime state, and a "Null references" foldout that warns about unassigned key refs.
- **`TaskEditor`** — custom Inspector for `Task`; shows different fields depending on `taskType` (e.g. only bomb tasks show a "Defuse Bomb" debug button; only door tasks show the `doorTask` toggle), and includes a small custom table UI (`DrawToolsTable`) for editing the parallel `toolNames[]`/`toolsToActivate[]` arrays with add/remove rows.

---

## 9. Notable quirks (from `notes.txt` + code)

- `notes.txt` states: player sprite size 256px, brush stroke size 10px, and **"due to programming shenanigans, task hitboxes cannot collide"** — i.e. overlapping task trigger zones are a known limitation, not a bug to chase.
- The computer password is hardcoded as the literal string `"21885"` in `Computer.Update()`.
- `Task.GetMatchingChildren` matches tool GameObjects by **name string**, case-insensitively — renaming a child under a `taskMenuPrefab` without updating `toolNames[]` in the Inspector will silently break that tool.
- `GameManager.Update()` and `GameManager.DefuseBomb()` duplicate the same "lowest bomb timer → audio cue" logic — if you ever change the beep/last-10-seconds logic, both copies need updating.
- Several scripts (`Bomb`, `Wire`, `usbInputTask`, `HorseBox`) independently increment both `gm.winConditions` **and** a per-bomb `bombCoditions` counter — a level's true "win" therefore depends on `winConditionCount` (Inspector-set on `GameManager`) matching the sum of all these increment points across every task in the scene. There's no central validation that these numbers agree.
