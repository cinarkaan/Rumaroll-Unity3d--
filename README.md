# RUMAROLL

<h1><p>!!!!THE GAME LARGESTLY HAS BEEN UPDATED. THAT'S WHY THE IMAGES ABOUT THE GAME WHERE IS PLACED ON THE FOLLOWING IS OLD VERSION. FOR PC YOU CAN DOWNLOAD FROM THE FOLLOWING LINK THAT IS PLACED BOTTOM OF THE PAGE!!!!</p></h1>

<h1>Purpose</h1>

<p>The cube that is colorful is to reach out evacuation point by rolling on the tile that matches with it's bottom face colors. At the each new stage means more extended platform and more difficult obstacles</p>

<h1>How To Play</h1>

<h3>Android</h3>
<p>As you can see , The buttons are placed on the screen of both left and right. You are able to manage by pressing move or another things about ui in here.To turn another point view , you are slightly slide your finger either from left to right or from right to left. This makes to be able to seen another point view towards to cube. </p>

<h3>PC</h3>
<p>To rolling the cube that you see in the bottom of the screen , you are to press from keyboard 'W','A','S','D'. To get clue , you must press keyboard 'C' as well as activated shield 'R'. If you press 'ESC' pause menu will be opened. If you want to see where you are , you can open map by pressing 'M'. To change aspect view , you can use either left or right arrow from keyboard.</p>

<h2>MAIN MENU</h2>

 <img src="./SS/Main.png">
 
<p> The players will start to game in here after lauched app. But if the tutorial passed , they must be started from continuous by passing in the store. Otherwise the recorded game is reseted.</p>

<p> The main menu is the ui that is conducted the origin stream of game. The players might make various processes.</p>

<h2>Settings</h2>

<img src="./SS/Settings.png">

<p> The players might to prefer to turn off or initialize some settings about the game (e.g sound , vfx ..)</p>

<h2>Continue & Store</h2>

 <img src="./SS/Store.png">

<p> The players are able to purchase items or see count of items and loots that belongs just before start to game , if they would like</p>

<h2>Multiplayer</h2>

<img src="./SS/Loby.jpg">

<img src="./SS/Multiplayer.jpg">

<img src="./SS/Rewards.png">

<p> There are 2 different options. First one is the create game lobby that is settled rooms by players as well as are able to select the platform options (Stage,Difficulty, E.g). If you selected the either normal or hard difficulty , you will encounter either the obstacles or enemy depends on your option. Second one is the lobby that joins players. In a 10 seconds you will be see a room , of course if it exists. Signaficant note : The wifi of device must had been turned on by the players before either create room or join room. The player who won is to be own the rewards end of the stage. The player who reach out to evacuation point wins the game , the first player(host) starts at the (6,6) referance point and the second player(client) starts at the (18,18) referance point. In here two player are connected to each other by using UDP socket protocol. Both two player must have been connected  same wi-fi network. Otherwise the game will be terminated and ended up on main-menu both two player.  </p>

<h2>Tutorial - Stage 4 (4 x 4 Grid Platform)</h2>

<img src="./SS/Tutorial.png">

<p> In this scene , player what have to do which is the will be told them by guide  when it start game. Players can collect the coins that is to be spent on the store to purchase items. It is easy stage</p>

<h2>Day - From Stage 5 To Stage 12 (5 x 5 - 12 x 12) </h2>

<img src="./SS/Day.jpg">

<img src="./SS/Day-2.png">

<img src="./SS/Clue.png">

<img src="./SS/Shield.png">

<p> In this scene , player must be pay attendioned to obstacles and enemy that was placed either solution or unsolution ways. Anymore they can collect diamonds to purchase rare item on the store. Also they can use it on this scene. In addition these they can take a review map that is placed right bottom on the screen. The platform that is consist of colorful tiles is created with DFS data structures which is means each colorful tile where is placed in the platform has unique position of places when it restart. We might be take a look with point view just like mix of the open-word and platform games. At the each init of scene , the algorithm will run and generate another unique solution and platform. In addition these , at the last stage of day , you are going to encounter with enemy that has autonomous move features. In here , this system similarity with AI , it runs A* algorithm to find new path on the platform from start to arrive point to make arrive enemy. Also the obstacles that is placed at the previous stage are placed with particular data structures. The execute order is : First one is platform manager , Second one is event manager ,  Third one is Obstacle Manager , Latest is Enemy Manager. Thus the platform had been builded complately.</p>

<a href="https://github.com/cinarkaan/Rumaroll-Unity3d--/blob/master/Scripts/Exceptional/ExceptionalPlatform.cs">To Review The Code For To See Platform Manager - DFS Algorithm</a>

<a href="https://github.com/cinarkaan/Rumaroll-Unity3d--/blob/master/Scripts/SinglePlayer/Managers/EnemyManager.cs">To Review The Code For To See Enemy Manager - A* Algorithm</a>

<a href="https://github.com/cinarkaan/Rumaroll-Unity3d--/tree/master">To Review Full Code</a>

<a href="https://drive.google.com/drive/folders/1C1rGR2iXgM2iGUwZ8F179_H0qFYcZPaI?usp=drive_link">Get Access To Downloading Exe File</a>

