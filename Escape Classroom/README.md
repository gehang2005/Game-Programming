# Escape Classroom — What Did I Miss?

**What Did I Miss?** is an observation-based puzzle game inspired by *The Exit 8* and similar "endless classroom" experiences. Walk through a repeating classroom and corridor, spot anything out of place, and leave through the correct door. Clear 5 classes in a row to win.

---

## Overview

At the start of each class, the scene may contain **zero or one** anomaly. Your goal is not combat or traditional puzzles — it is to **memorize what the classroom normally looks like** and make the right call when something feels off:

- **Anomaly detected** → Leave through the **back door**
- **No anomaly** → Leave through the **front door**

A wrong choice resets your progress back to Class 1. After completing all 5 classes, head to the bed at the end of the corridor to trigger the ending.

---

## Controls

| Action | Input |
|--------|-------|
| Move | `W` `A` `S` `D` |
| Look around | Mouse movement |
| Adjust camera distance | Mouse scroll wheel |
| Interact with doors | `F` |

Click **What Did I Miss ?** on the main menu for the in-game how-to-play panel.

---

## How to Run

### Requirements

- [Unity 2022.3.62f3c1](https://unity.com/) (or a compatible 2022.3 LTS release)
- Windows (current development platform)

### Play in the Editor

1. Open this project folder in Unity Hub (the directory containing `Escape Classroom.sln`)
2. Open `Assets/Scenes/MainMenu.unity`
3. Press **Play** and select **Class Begins !** from the main menu

Build scene order (`File → Build Settings`):

1. `Assets/Scenes/MainMenu.unity`
2. `Assets/Scenes/SampleScene.unity`

---

## Gameplay Flow

```
Main Menu → Enter classroom scene
    ↓
Round starts: random anomaly spawns (or none)
    ↓
Player observes the classroom and corridor
    ↓
Exit through the front or back door
    ↓
  ├─ Correct choice → advance to the next class
  ├─ Wrong choice   → progress resets to Class 1
  └─ All 5 classes cleared → reach the bed → ending screen
```

### Decision Rules

| Round state | Correct exit |
|-------------|--------------|
| Anomaly present | Back door |
| No anomaly | Front door |

Current progress is shown in the UI (`Class 1` through `Class 5`).

---

## Anomalies

All anomaly objects live under the `Anomalies` scene node and are auto-collected by `ClassroomAnomalyRandomizer`. The current build includes:

| Name | Effect type | What changes |
|------|-------------|--------------|
| **Chihaya** | Texture swap | Quad image switches to an anomaly version |
| **nailongLaugh** | Proximity trigger | Texture changes and laughter plays when the player walks nearby |
| **frame** | Material swap | Painting inside the frame uses an anomaly material |
| **catmeme** | Texture swap | Multiple cat meme textures become anomalous |
| **cxk** | Model swap | Normal model is replaced with an anomaly model |

Each round has roughly a **90%** chance of spawning an anomaly and a **10%** chance of a completely normal classroom. The system also tries to avoid repeating the same anomaly on consecutive rounds.

---

## Project Structure

```
Escape Classroom/
├── Assets/
│   ├── Scenes/
│   │   ├── MainMenu.unity      # Main menu
│   │   └── SampleScene.unity   # Main game scene
│   ├── Scripts/                # Game logic (see table below)
│   │   └── Editor/             # Editor-only builder scripts
│   ├── Broadcast/              # Models, textures, audio, etc.
│   │   └── 24HC_Speakers/      # Anomaly resources and props
│   ├── Character/              # Player character model & animations
│   ├── Classroom/              # Classroom environment model & materials
│   ├── Office Tile Kit/        # Office/corridor environment kit
│   ├── SimpleSky/              # Skybox system
│   └── TextMesh Pro/           # UI text rendering
├── Packages/
├── ProjectSettings/
└── README.md
```

### Core Scripts

| Script | Responsibility |
|--------|----------------|
| `ClassroomGameFlowController` | Class progress, exit validation, win logic |
| `ClassroomAnomalyRandomizer` | Anomaly pool collection and per-round randomization |
| `TeleportTrigger` | Front/back door teleportation, triggers validation and new rounds |
| `IAnomalyEffect` | Shared anomaly interface (`Activate` / `Deactivate`) |
| `QuadTextureAnomaly` | Texture-based anomalies (Chihaya, catmeme) |
| `MaterialSwapAnomaly` | Material-based anomalies (frame) |
| `ModelSwapAnomaly` | Model-swap anomalies (cxk) |
| `ProximityTextureAnomaly` | Proximity-triggered texture + audio anomaly (nailongLaugh) |
| `SimplePlayerMove` | Third-person character movement with footstep audio |
| `CameraFollow` | Third-person follow camera with mouse look and scroll zoom |
| `DoorController` | Interactive door open/close with `F` key |
| `MainMenuController` | Main menu scene management |
| `MenuButtonHover` | Button hover animation effects |
| `GameEndTrigger` | Bed trigger after clearing all classes |
| `GameClearScreen` | Ending UI fade-in and buttons |
| `LaughTriggerForwarder` | Forwards proximity trigger events for laugh anomaly |

---

## Adding New Anomalies

1. Add a new child under the `Anomalies` node in `SampleScene` (preferably as a **direct child**)
2. Attach a script that implements `IAnomalyEffect`, or rely on simple `SetActive` visibility
3. Make sure **Auto Collect On Awake** is enabled on `ClassroomAnomalyRandomizer` (`AnomalyManager`)

You do not need to manually edit the `anomalies` list — placing objects under `Anomalies` adds them to the random pool automatically.

---

## Asset Sources & Credits

### Environment & Props

| Asset | Source | Description |
|-------|--------|-------------|
| **SimpleSky** | [Unity Asset Store](https://assetstore.unity.com/) | Skybox system with clouds and sky dome |
| **Office Tile Kit** | [爱给网](https://www.aigei.com/) | Office/corridor modular environment kit (walls, floors, doors, windows, etc.) |
| **Classroom** | [爱给网](https://www.aigei.com/) | Classroom environment model (`classroom.fbx`) and all PBR materials/textures |
| **frame (画框)** | [爱给网](https://www.aigei.com/) | Picture frame model (`frame.fbx`) used in the frame anomaly |
| **bed (床)** | [爱给网](https://www.aigei.com/) | Bunk bed model at the end of the corridor |
| **bin (垃圾桶)** | [爱给网](https://www.aigei.com/) | Trash bin prop |
| **Speakers** | Broadcast / 24HC_Speakers | Speaker props (various materials and LODs) |

### Character

| Asset | Source | Description |
|-------|--------|-------------|
| **keqing (刻晴) model** | [爱给网](https://www.aigei.com/) | Player character model with bone rigging |
| **Character textures** | [爱给网](https://www.aigei.com/) | Face, hair, body, clothing, and shadow textures |
| **Standing Idle** | [Mixamo](https://www.mixamo.com/) | Idle animation |
| **woman——Walking02** | [Mixamo](https://www.mixamo.com/) | Walking animation |

### Anomaly Assets

| Asset | Source | Description |
|-------|--------|-------------|
| **Chihaya (千早爱音) images** | 网络 (Internet) | Character images including `Chihaya Laugh.png` and `chihayaCry.png` |
| **Chihaya Laugh audio** | [爱给网](https://www.aigei.com/) | Laugh sound effect |
| **Hachimi (cat meme) textures** | 网络 (Internet) | 6 meme-style images: `catcheff.png`, `catSpider.gif`, `catTrump.png`, `dogLoading.gif`, `Huh.jpg`, `Oh.jpg`, `rose.gif`, `stare.jpg`, `memes.jpg` |
| **nailong (奶龙) images** | 网络 (Internet) | Character images `nailong.png` and `nailongNotLaugh.jpg` |
| **nailong laugh audio** | [爱给网](https://www.aigei.com/) | Laugh sound effect |
| **frame anomaly material** | 网络 (Internet) | Two alternative painting materials used as anomaly variants |
| **frame normal material** | 网络 (Internet) | Default painting material |
| **cxk models** | [爱给网](https://www.aigei.com/) | Both normal (`cxk.fbx`) and anomaly (`basketball.fbx`) 3D models |
| **Hachimi audio** | [爱给网](https://www.aigei.com/) | Associated sound effects (`hachimi.mp3`, `Northeast.mp3`) |

### Audio

| Asset | Source | Description |
|-------|--------|-------------|
| **Footstep sounds** | [爱给网](https://www.aigei.com/) | Walking (`行走.mp3`) footstep audio |
| **门外竖着** | [爱给网](https://www.aigei.com/) | Background / ambient audio |

### UI & Framework

| Asset | Source | Description |
|-------|--------|-------------|
| **TextMesh Pro** | Unity Technologies | UI text rendering (built-in Unity package) |
| **Quad prefab** | Custom | Simple quad mesh used for texture-based anomaly displays |

---

## License

This project is intended for coursework and learning purposes. If you plan to distribute it publicly, verify the license terms of all third-party assets listed above. Most assets from 爱给网 are designated for personal/educational use; Mixamo animations are royalty-free for personal and commercial use under Adobe's terms; Unity Asset Store assets follow their respective individual licenses.
