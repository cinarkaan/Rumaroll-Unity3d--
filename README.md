<h1>Structures Of Scripts</h1>

<h2>MANAGERS</h2>

<p>The all scripts explained and conduct hierarchy of them on the following as briefly. (What are works ? , How are used ? , The logic of conduct...) e.g was explained.</p>

<h2>Platform Manager</h2>

<p>The platform that consists of colurful tiles and dynamics are initialized in here. According to the stage , it creates the platform and find unique solution each time(By DFS Algorithm). Beside this and the operations that includes all of them are excecuted by this script.This manager always works as firstly.</p>

<h2>Event Manager</h2>

<p>The events that consists of both diamonds and coins are initialized in here. E.g the after the collides with player as well as placed on the platform..That is second works after platform manager.</p>

<h2>Obstacles Manager</h2>

<p>All obstacles are initialized in here. According to the stage the certain obstacles are placed on the platform. After of that , the moves of obstacles are launched in here. This scripts are triggered as third order. </p>

<h2>Enemy Manager</h2>

<p>At the latest , this script runs. All enemies are initialized in here. The enemy are moved as autonomic by the both A* and special algorithm. The moving path are created in here , the mechanics of move are launhed in here. Finally the enemy is placed on the platform. </p>

<h2>Rolling Cube Controller</h2>

<p>The main control center of player cube . All operations , game mechanics that belongs of player cubes are executed in here. As well as if the player want to use shield. Or after the each move of player , the gps are updated in here then it sets to UIController</p>

<h2>Aspect Controller</h2>

<p>In here the player controls the aspect point view of the cube. When the player use the swipe from Controller. This script are updated the aspect point view toward to the player cube.</p>

<h2>UI Controller</h2>

<p>Player controls the user interfaces objects(Buttons , swipes ,clicks) e.g are controlled in here. Briefly the operations about the UI are placed in this script. The all UI operations either keyboard or screen(Mobile) is conducted by this scripts.</p>

<h2>Game Map Controller</h2>

<p>The game map to see where the player is placed are conducted in this script. The script are initialized after platform manager. It adjusts as stage.</p>

<h2>Server Manager</h2>

<p>The top structure in Multiplayer. First , this script are executed in multiplayer scene to init server settings that was adjusted by main menu side. It runs before Network Platform Manager script. It controls the connect and disconnect, kick of , sets authorized e.g that belongs the player cube. At the end of the game the struct is disposed.</p>

<h2>Network Platform Manager</h2>

<p>Just like the platform manager at the singleplayer side. The script conducts the operations belongs on the platform. It runs end of the server manager.</p>

<h2>Network UI Manager</h2>

<p>Just like the UIManager at the singleplayer side. The script conducts the operations belongs on UI. It runs end of the server manager.</p>

<h2>Network Cube Controller</h2>

<p>Just like the UIManager at the singleplayer side. The script conducts the operations belongs on Player(game mechanics , e.g). It runs end of the Network UI Manager , which is the control mechanics assigned as dynamic the player that connected.</p>

<h2>Room Broadcaster & Room Listener</h2>

<p>The player that has been connected at the common share point wifi might be founded each other through this script. The player(Host) who was created game share with Local IP. The another player(client) is might be explored it by using thought named as ''RoomListener". The signaficant things are firstly the wifi must be turned on and both of two player must be connected common share point wi-fi. The room name , min/max player and join button will be seems after refreshed.</p>

<h2>CUBE & UTIL</h2>

<h2>Cube Simulator And Cube Explosion</h2>

<p>The first script are recorded the face indicates of player cube. It's part of the cube controller class. After each roll operations the indicates are updated. The second script is conducted the rigidbody operations what will be happend end of the game over. For example if the cannon obstacles shoots the player. The rigidbody are triggered on the cube player that was scattered prefab and then the cube expodes by using pyhsic engine.</p>

<h2>Scene Loader</h2>
<p>It's conducted the async scene loading operations both multiplayer and singleplayer. Also this script responses from async scene loading. </p>

<h2>ColorfulTile</h2>

<p>It's part of the platform manager. It makes to change colors of tile at the runtime within sync way. It is launched end of the obstacle manager to be indicated dynamic tiles.</p>

<h2>MobileTrail</h2>

<p>It's part of the cube controller. It responses from ghost trail effect by using mesh component of player.At the each move of player it is triggered.</p>

<h2>OverlapBoxNonAllocPoller</h2>

<p>It's responsive from colliders at the game. This script are used physics engine on the game engine to be inspected the collisions each other with obstacles and player. This script is lighter than rigidbody colliders. With the high accuracy , it is inspected the collisions.</p>

<h2>Tutorial</h2>
<p>At the scene named as the "Tutorial" , this script is called to be showed instructions to the player. What is the purpose of game ? How is played ?  e.g</p>

<h2>FlagWaver</h2>
<p>It just the small flag wave effect through by using mesh renderer of gameobject./p>

<h2>UniqueRandomGenerator</h2>
<p>It is used to place dynamics on the multiplayer and events where are placed at the singleplayer scenes. They must not placed same position. Because of that this script execute this task./p>

<h2>Menu & Multiplayer</h2>
<p>It is only responsive the UI task on the main menu and multiplayer.(E.g Settings , Store , Multiplayer)/p>

<h2>Exceptionals</h2>

<p>To implement OOP principles into game , the exceptional classes has been created. At the both side singleplayer and multiplayer the exceptionals was used to avoid code repeat</p>













