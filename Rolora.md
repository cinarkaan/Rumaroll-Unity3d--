# ROLORA - Version 2.0.3 (Is Live)

# Overview

**Rolora** is a mobile action‑puzzle adventure where you roll the either color or texture ‑ sided cube across procedurally generated platforms to reach an evacuation point. 
Each face of the cube must match the tile beneath it; stages grow longer and more challenging as you progress. 
The project is a solo commercial game in active development for Android with iOS planned next.

- **Dynamic Tiles**
*Caption: Dynamic tiles appear across all three maps, changing their properties — such as color and texture — every three seconds based on shader settings. These tiles alternate between dummy and original states, adding unpredictability to the gameplay. Their presence depends on the selected difficulty level, making higher difficulties more challenging and varied.*

### 🌟 Map & Stage Star Rating Thresholds

Players are awarded stars based on their completion time, relative to the thresholds defined for each map and stage.

#### Rating Rules:
* **3 Stars** 🌟🌟🌟 : Passed Time is less than or equal to Min Time
* **2 Stars** 🌟🌟 : Passed Time is less than or equal to Average Time
* **1 Star** 🌟 : Passed Time is less than or equal to Max Time

| Map | Stage | Min Time (s) | Average Time (s) | Max Time (s) |
| :--- | :---: | :---: | :---: | :---: |
| **Colorful** | 1 | 30s | 40s | 50s |
| | 2 | 40s | 60s | 70s |
| | 3 | 70s | 90s | 110s |
| | 4 | 140s | 160s | 180s |
| **Ancient** | 1 | 90s | 120s | 150s |
| | 2 | 175s | 215s | 245s |
| | 3 | 250s | 280s | 310s |
| | 4 | 300s | 330s | 360s |
| **Mystical** | 1 | 25s | 35s | 45s |
| | 2 | 80s | 95s | 105s |
| | 3 | 170s | 185s | 200s |

### 🏆 Performance-Based Reward System!

Your completion time now earns you 1 to 3 Stars per level. The better your time, the bigger your payout:

* 🪙 **Coins:** Earned coins are multiplied by your Star count (Stars × Base Coins).
* 💎 **Diamonds:** Earn 2 or more Stars to get Diamonds equal to your Star count.
* 🟢 **Emeralds:** Go for perfection! Complete a level with 3 Stars to score +3 Emeralds.

---

### Event Hours For Multiplayer
| Start-Time | End-Time | Spawn-Possibility | Status
| :---: | :---: | :---: | :--- |
| 09:00 AM | 07:00 PM | **55%** | Medium Chance 
| 08:00 PM | 00:00 AM | **90%** | High Chance 
| 00:00 AM | 09:00 AM | **20%** | Low Chance
| 19:00 PM | 20:00 PM | **20%** | Low Chance            

- *Caption : At the multiplayer scene, the event spawn rates depend on the time of the day; the shield item is to be spawned by the server.*
- *Caption : If the shield is not taken by any player, it will be destoryed itself within 10 seconds.*
- *Caption : The protected duration of the shield is setted 7 seconds at the multiplayer.*
- *Caption : The perfections that get from frames are not validated in the multiplayer, except from the "DarkIvy" frame, which is protected against the dwarf at the mystical map.*
- *Caption : Rest of the these day times , the shield spawn rates is to be settled as %20.*

## Rating Rules For Multiplayer
| Difficulty | Min Time (s) | Average Time (s) | Max Time (s) |
| :--- | :---: | :---: | :---: |
| **Easy** | 65s | 75s | 90s |
| **Normal** | 65s | 75s | 90s |
| **Hard** | 75s | 85s | 95s |

<p>The amounts of the reward cards that is taken from chest is to calculation with following formula <br><br></p>

<img src="https://latex.codecogs.com/svg.image?\color{white}\text{RewardAmount}=\text{Round}\left(\text{Amount}\times\frac{\text{PointScore}+\text{Point}}{200}\right)" alt="Reward Formula" />

# How To Play

### Android
As you can see, the movement buttons are placed on both the left and right sides of the screen. You can control movement and interact with UI elements by pressing these buttons.  
To change the point of view, simply slide your finger slightly from left to right or right to left. This allows you to rotate the camera and see the cube from different angles.

---

### Purpose
The colorful cube must reach the evacuation point by rolling onto tiles that match its bottom face color. Each new stage introduces longer platforms and more difficult obstacles, increasing the challenge as you progress.

## Gameplay

## 🎮 Rolora Gameplay Teaser
[![Rolora Gameplay Teaser](https://img.youtube.com/vi/ODF8o4Q7pYg/maxresdefault.jpg)](https://youtu.be/ODF8o4Q7pYg)

## Google Play Link
[![Get it on Google Play](https://play.google.com/intl/en_us/badges/static/images/badges/en_badge_web_generic.png)](https://play.google.com/store/apps/details?id=com.glorywindgames.rolora)

**Objective**  
Roll the cube to the evacuation point. Each time the cube rolls, the face that becomes the bottom must match the tile color or state beneath it. Rolling onto a nonmatching tile deducts time or points depending on the mode.

**Controls**  
- **Movement**: On‑screen left and right buttons.  
- **Camera**: Swipe left or right to rotate the view around the cube.  
- **UI**: Inventory, stats, and contextual action buttons are available in the HUD.

**Progression**  
- **Maps**: Colorful, Ancient, Mystical — each map contains four stages.  
- **Dynamic Tiles**: Tiles toggle between original and dummy states every 3 seconds; difficulty controls their presence and ratio.  
- **Customization**: Frames and materials provide gameplay modifiers (for example **+20% shield duration**, **+5% roll speed**). Preview materials in the Profile screen.

**License**  
This project is **not open source**. All rights reserved. Repository content is provided for viewing only. 
Unauthorized use, reproduction, or distribution is prohibited. For licensing inquiries or partnership requests, contact the developer.

**Developer**: **Kaan Çınar**  
**Project Status**: In development & Published On Google Play — Android priority, iOS planned.  

---

## Key Features

- **Game Modes**: Single player (3 maps × 4 stages = 12 stages) and local multiplayer (LAN).
- **Core Mechanic**: Color‑matching cube — the cube’s bottom face must match the tile you roll onto.
- **Procedural Levels**: DFS‑based generation with seed support for reproducible runs.
- **AI**: Enemies use A* pathfinding with dynamic replanning.
- **Networking**: UDP‑based LAN multiplayer with lightweight reliability for critical messages.
- **Engine and Rendering**: Unity, C#, URP (SRP), custom HLSL shaders; Vulkan 1.0+ and OpenGL ES 3.2+ backends.
- **Localization**: Native support for English, French, German, Spanish.
- **Performance Target**: Optimized for Mali‑G52 devices with a measured baseline of 60 FPS under normal conditions.

---

## Technical Details and Performance

**Procedural Generation**  
- **Algorithm**: DFS for platform layout and event placement.  
- **Reproducibility**: Seedable generation for deterministic runs used in QA and leaderboards.  
- **Tunable Parameters**: Enemy spawn rate, obstacle frequency, dynamic tile ratio, loot probability.

## Abstract Factory Pattern — Platform Creation
In Rolora, platform instantiation is implemented using the **Abstract Factory (creational) pattern**. Each map (Colorful, Ancient, Mystical) is backed by its own `PlatformFactory` abstraction and corresponding concrete factory classes. This factory hierarchy provides several key benefits:

- **Easy Extensibility**: New maps or platform types can be introduced simply by adding a new factory, without altering existing code.  
- **Map‑Specific Customization**: Each factory encapsulates the unique tile sets, dynamic behaviors, and event rules of its respective map, ensuring clean separation of creation logic.  
- **Testability**: Factories can be replaced with mock implementations, while seed‑based generation enables reproducible runs for QA, multiplayer synchronization, and leaderboard validation.

**AI and Pathfinding**  
- **Pathfinding**: A* on a grid with controlled replanning frequency.  
- **Optimizations**: Grouped path updates, LOD for distant agents, and simple state machines for common behaviors.

**Rendering and Shaders**  
- **Pipeline**: URP with mobile‑optimized HLSL shaders.  
- **Performance Strategies**: Batching, GPU instancing, reduced shader variants, optional post‑processing toggles for lower‑end devices.  
- **Profiling Tools**: Unity Profiler and device GPU profilers; record FPS, memory, draw calls, and thermal behavior per scene.

**Networking and Security**  
- **Protocol**: UDP with application‑level reliability for important events.  
- **Discovery**: LAN broadcast/mDNS for lobby discovery.  
- **Security Considerations**: Basic validation of client actions; consider server‑authoritative checks or relay services if online play is added.

---

### Assets Screenshots and Captions

- **Main Menu**  
  ![Main Menu](./docs/assets/MenuRenewed.png)  
  *Caption: Main menu with Play, Settings, Profile, and Store entries. The players will start to game in here after lauched app. But if the tutorial already had passed , they must would have been continued by clicking the button of continuous by progressing in the store. Otherwise the recorded game is reseted.**

- **Settings**  
  ![Settings](./docs/assets/Settings.png)  
  *Caption: Language,Graphics and audio toggles; post‑processing recommended for high‑end devices.*

## Multiplayer

**Local LAN Play**  
- **Discovery**: Lobby creation and join via LAN using UDP broadcast or mDNS. Devices must be on the same Wi‑Fi network.  
- **Match Setup**: Host configures stage, difficulty, and grid size (default 6×6). Host starts at (0,0); client starts at (5,5) or (6,6) depending on difficulty.  
- **Win Condition**: First player to reach the evacuation point wins and receives rewards (coins, shields, clues, diamonds).  
- **Scoring**: Rolling on nonmatching tiles deducts time/points; total points determine reward tiers.  
- **Networking Notes**: UDP chosen for low latency; critical messages use sequence numbers and lightweight ACKs to mitigate packet loss. Lobby discovery is LAN only; if connection is lost both players return to the main menu.

- **Multiplayer Lobby**  
  ![Multiplayer Lobby](./docs/assets/Multiplayer.png)  
  *Caption: Create or join a LAN lobby; configure stage and difficulty.*

- **Create Game**  
  ![Create Game](./docs/assets/CreateGame.png)  
  *Caption: Host settings for grid size, difficulty, and stage selection.*

- **Join Game**  
  ![Join Game](./docs/assets/JoinGame.png)  
  *Caption: Discover and join available local lobbies.*

- **Rewards**  
  ![Rewards](./docs/assets/RewardsMultiplayer.png)  
  *Caption: In multiplayer mode, rolling your cube onto a non‑matching tile deducts points from your total score. Final scores are calculated from these totals, which directly determine the rewards earned. Higher scores unlock better reward cards, giving players access to valuable items and bonuses.*

- **In Game**
  ![Multiplayer](./docs/assets/MultiplayerEvent.png)  
  *Caption: In multiplayer mode, players can view detailed statistics for each participant. A dedicated button above the map provides quick access to these stats, allowing both competitors to track performance, compare progress, and review results during or after a match.*

- **Map Selection**  
  ![Map Selection](./docs/assets/MapSelection.png)  
  ![Stage Selection](./docs/assets/StageSelection.png)  
- *Caption: Choose a map and stage to continue progress or start a new run.*

- **Profile Colorful**  
  ![Profile Colorful1](./docs/assets/Colorful-Profile-1.png)  			
  ![Profile Colorful2](./docs/assets/Colorful-Profile-2.png)  			
  ![Colorful_Flag_Selection](./docs/assets/Colorful_Flag_Selection.png)
  ![Profile Ancient1](./docs/assets/Ancient-Profile-1.png)
  ![Profile Ancient2](./docs/assets/Ancient-Profile-2.png)
  ![Ancient_Flag_Selection](./docs/assets/Ancient_Flag_Selection.png)
  ![Profile Mystical1](./docs/assets/Mystical-Profile-1.png)
  ![Profile Mystical2](./docs/assets/Mystical-Profile-2.png)
  ![Mystical_Flag_Selection](./docs/assets/Mystical_Flag_Selection.png)
  *Caption: Material,frame,flag preview with stat modifiers.*

- **Store**  
  ![Store](./docs/assets/Store.png)  
  *Caption: Purchase cosmetics and view owned items before starting a stage.*

- **Map Colorful**        
  ![Colorful-1](./docs/assets/Colorful-1.png)  
  ![Colorful-2](./docs/assets/Colorful-2.png) 
  ![Colorful-2](./docs/assets/Colorful-3.png)  
  *Caption: The Colorful map introduces four stages, each beginning with a guide that explains the objectives. Players can skip the guide using the forward button if they prefer. Across the stages, three dynamic weather conditions appear — sunny with floating dust particles, rainy, and snowy — adding variety and atmosphere to the gameplay.*

- **Map Ancient**        
  ![Ancient](./docs/assets/Ancient-1.png)  
  ![Ancient](./docs/assets/Ancient-2.png)  
  ![Ancient](./docs/assets/Ancient-3.png)  
  *Caption: The Ancient map unlocks after completing Colorful and surrounds players with a dangerous lava sea. It is longer and more complex than Colorful, requiring activation of rotary bridges and collection of keys to progress. Obstacles are more challenging, and emerald loot appears with a higher probability, rewarding careful exploration.*

- **Map Mystical**        
  ![Mystical](./docs/assets/Mystical-1.png)
  ![Mystical](./docs/assets/Mystical-2.png)
  ![Mystical](./docs/assets/Mystical-3.png)	 
  *Caption: The Mystical map is the final stage of the game, offering a magical atmosphere that feels lighter in complexity than Ancient but more demanding in endurance. Its enchanting visuals and ambience create surprises throughout, while the last stage introduces a formidable dwarf enemy that players must watch out for.*

- **Localization Support**
*Caption: Rolora offers full native localization in four languages —  
English ![UK Flag](https://flagcdn.com/w20/gb.png),  
French ![France Flag](https://flagcdn.com/w20/fr.png),  
German ![Germany Flag](https://flagcdn.com/w20/de.png),  
Spanish ![Spain Flag](https://flagcdn.com/w20/es.png).  
Additional languages are planned after launch, ensuring wider accessibility and a seamless experience for players worldwide.*

- **Performans & Optimization**
*Caption: The game has been optimized for devices using the Mali‑G52 GPU, achieving a minimum of 60 FPS under normal conditions. Exceptions may occur during extended play sessions or when thermal levels rise.*

---

### Developer Diaries - Rolora v2.0.3 (What's new v2.0.3)
- *Caption: 🎨 Dynamic Visuals: Tile styles now dynamically change based on your selected frame!*
- *Caption: ⚡ Performance Improvements: Enhanced shader and core optimizations for smoother gameplay.*
- *Caption: 📦 Visual Enhancements: Refined and updated 3D models throughout the game.*
- *Caption: 🛠️ Bug Fixes: Resolved minor bugs and improved overall system stability.*
---

### Developer Diaries - (Coming Soon)
- **NEW MAP - POISONED**
- ![Poisoned](./docs/assets/Poisoned-1.png)
- ![Poisoned](./docs/assets/Poisoned-2.png)
- *Caption: The environment that is surrounded with toxic sea and biohazard obstacles and enemies is being designed for players. So new adventures are loading with renewed.*
- *Caption: This new adventure is to challenge a hazardous environment and has a different atmosphere.*
- *Caption: The cube is able to roll where is fitted as either even or odd number with the surface of the tile and bottom faces of the cube, which is matched the bottom face of the cube that is rolled as either even or odd with the tile.*

### Enjoy the Game
---