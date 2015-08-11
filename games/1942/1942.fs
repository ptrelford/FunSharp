#if INTERACTIVE
#r "./bin/debug/Xwt.dll"
#r "./bin/debug/FunSharp.dll"
#endif

open Library

type System.Collections.Generic.Dictionary<'TKey,'TValue> with
   member this.GetOrDefault(key:'TKey,value:'TValue) =
      match this.TryGetValue(key) with
      | true, value -> value
      | false, _ -> value


// 1942 like Game ID: KJW676
// Copyright (C) 2010, Laurent GIRAUD laurent.giraud1@free.fr
// License: MIT license http://www.opensource.org/licenses/mit-license.php
// most of Game artwork created by Ari Feldman ari@arifeldman.com"
// and others grabed from free web sprites

let ammoSync = obj()

//-------------------------
// or load from web url
//let Path= "http://imode.free.fr/images1942/"
let Path = ""
//-------------------------

// Game area controls
let gameWidth  = 640
let gameHeight = 480
let fps = 50
let bgs = 2 //backgroundspeed
let mutable Player_Lives = 10
let nbrisland = 5 //nomber of island images
let islandcount = 5 //nomber of island in the wall field

// Window title
let gameTitle = "1942, Score: "

GraphicsWindow.Hide()
GraphicsWindow.Title <- gameTitle + "0"
GraphicsWindow.CanResize <- false
GraphicsWindow.Width <- gameWidth
GraphicsWindow.Height <- gameHeight

// Global variables

let mutable player = "<shape name>"
let mutable paddleX = 0
let mutable paddleY = 0
let mutable fond = "<shape name>"

// addImage in the right order is needed to define the shapes depth
let island4 = ImageList.LoadImage(Path + "island4.png")
let island1= ImageList.LoadImage(Path + "island1.png")
let island2= ImageList.LoadImage(Path + "island2.png")
let island3= ImageList.LoadImage(Path + "island3.png")
let island5= ImageList.LoadImage(Path + "island5.png")
let player0 = ImageList.LoadImage(Path + "myplane1.png")
let background = ImageList.LoadImage(Path + "fond.png")
let bullet0 = ImageList.LoadImage(Path + "bullet.png")
let enemy= ImageList.LoadImage(Path + "enemy1.png")
let enemy2 = ImageList.LoadImage(Path + "enemy2.png")
let enemy_expl1=ImageList.LoadImage(Path + "explo1.png")
let enemy_expl2=ImageList.LoadImage(Path + "explo2.png")
let player_expl=ImageList.LoadImage(Path + "explo2.png")
let Enemy_bullet=ImageList.LoadImage(Path + "E_bullet.png")
let ``end`` = ImageList.LoadImage(Path + "End.png")

let enemy_Array = Dictionary() // Array that contain all the enemies
let enemy_TimeLine = Dictionary()
let enemy_line = Dictionary()
let enemy_PosX = Dictionary()
let enemy_PosY = Dictionary()
let enemy_PathNBR = Dictionary()

let mutable enemy_Nbr = 6
let mutable enemy_Count = 0
let enemy_speed = 4.0
//enemy_NextRemove = 1
let enemy_Life = 10
let player_size= 65
let enemy_size = 32
let enemy_ammo_size = 8
let player_bullet_size = 32

let island_Array = Dictionary()

let Player_Ammo = Dictionary()
let Player_AmmoAge = Dictionary()
Player_AmmoAge.[1] <- 0
let Player_AmmoMax = 50
let mutable Player_AmmoCount = 0
let Player_Ammospeed = float bgs + 4.0
let Player_AmmoLife = 100

let mutable ShootX = nan
let mutable ShootY = nan

let mutable incx = 0  // X-axis increment for background
let mutable incy = 0  // Y-axis increment for background
let mutable incbx = 0.0 // increment for bullets and all objects on X
let mutable incby = 0.0 // increment for bullets and all objects on Y

let Enemy_Ammo = Dictionary()
let Enemy_AmmoAge = Dictionary()
Enemy_AmmoAge.[1] <- 0
let Enemy_Ammo_Angle= Dictionary()
let Enemy_AmmoMax = 30
let mutable Enemy_AmmoCount = 0
let Enemy_Ammospeed = bgs + 4
let Enemy_AmmoLife = 50
let Enemy_Agresivity = 200
let mutable Enemy_ShootX = 0
let mutable Enemy_ShootY = 0

let mutable TimePlay = 0

let mutable pathNBR = 1
let mutable enemyPosX1 = 0
let mutable enemyPosY1 = 0
let mutable enemy_type = 0

let mutable score = 0

let islandPos = Dictionary<int,Dictionary<int,int>>()
let incislandx = Dictionary<int,int>()
let incislandy = Dictionary<int,int>()
let level1 = Dictionary<int,Dictionary<int,int>>()
let enemyPath = Dictionary<int, Dictionary<int, Dictionary<int,float>>>()
let posx = Dictionary<int,_>()
let posy = Dictionary<int,_>()

// Setup world
let rec Init() =
   GraphicsWindow.Hide()

   Mouse.HideCursor()

   // every enemy in tha array has 5 informations
   // TimeLine: each enemy has its own timeline
   // PathNBR: the precalculated path defineing the movement
   // the movement is decomposed in LineNbr differents lines
   // each line is define in the enemy_path table with rotation, deltaY and deltaY
   // PositonX, PositonY: position in space
   //
   //

   IslandsPosition()
   create_level1()

   fond <- Shapes.AddImage(background)
   island_Array.[1] <- Shapes.AddImage(island1)
   island_Array.[2] <- Shapes.AddImage(island2)
   island_Array.[3] <- Shapes.AddImage(island3)
   island_Array.[4] <- Shapes.AddImage(island4)
   island_Array.[5] <- Shapes.AddImage(island5)
   for i = 6 to islandcount do
      island_Array.[i] <- island_Array.[Math.Remainder(i,4)]

   player <- Shapes.AddImage(player0)
   GraphicsWindow.FontSize <- 20.0
   GraphicsWindow.BackgroundColor <- Colors.Gray
   GraphicsWindow.PenColor <- Colors.Yellow



// Main gane routine
and Play () =
   GraphicsWindow.Show()

   // Main loop
   let play = 1
   let pause = 0
   incx <- 0  // X-axis increment for background
   incy <- 0  // Y-axis increment for background
   incbx <- 0.0 // increment for bullets and all objects on X
   incby <- 0.0 // increment for bullets and all objects on Y
   let mutable squadron = 1
   // Island initial position 
   let mutable j=0

   for i=1 to islandcount do
      j <- Math.Remainder(i,6)
      posy.[i] <- islandPos.[j].[2]
      posx.[i] <- islandPos.[j].[3]
      incislandx.[i] <- 0
      incislandy.[i] <- 0   
   TimePlay <- 0
   Shapes.Move(player, gameWidth/2 , gameHeight - 80 )
   while (play = 1) do
      Program.Delay(1000/fps)
      TimePlay <- TimePlay + 1

      if (pause = 0) then
         if (TimePlay = level1.[squadron].[1]) then

            pathNBR <- level1.[squadron].[2]
            enemyPosX1 <- level1.[squadron].[3]  //X position
            enemyPosY1 <- level1.[squadron].[4]  //Y position
            enemy_Nbr <- level1.[squadron].[6]
            enemy_type <- level1.[squadron].[7]
            if (level1.[squadron].[5]=1) then
               create_enemies_left()
            else
               create_enemies_right()         
            squadron <- squadron + 1
                 
         if (TimePlay > 4000) then
            Shapes.Move(Shapes.AddImage(``end``), 230,200)
            Program.Delay(5000)
            Program.End()     
         lock ammoSync (fun () ->  
            moveall()
         )

         GraphicsWindow.MouseMove <- Callback(OnMouseMove)
         GraphicsWindow.MouseDown <- Callback(OnMouseDown)

         AgePlayer_Ammo()

and OnMouseMove () =
   paddleX <- int GraphicsWindow.MouseX - player_size / 2
   paddleY <- int GraphicsWindow.MouseY - player_size

   if (paddleX < 0) then
      paddleX <- 0   
   Shapes.Move(player, paddleX , paddleY )

and OnMouseDown () =
   ShootX <- GraphicsWindow.MouseX - 15.0 // in order to be from the neck of the plane
   ShootY <- GraphicsWindow.MouseY - 80.0
   Fire()

and moveall () =
   incbx <- 0.0
   incby <- 0.0
   GraphicsWindow.Title <- gameTitle + score.ToString() + " Lives:" + Player_Lives.ToString()

   if (paddleX > (gameWidth-62) ) then
      incx <- incx - bgs
      incbx <- incbx - float bgs
      for i=1 to islandcount do
         incislandx.[i] <- incislandx.[i] - bgs    
      if (incx = -32 ) then
         incx <- 0            
   if (paddleX < 64 ) then
      incx <- incx + bgs
      incbx <- incbx + float bgs
      for i=1 to islandcount do
         incislandx.[i] <- incislandx.[i] + bgs          
      if (incx = 32 ) then
         incx <- 0
   Shapes.Move(fond,incx - 32 ,incy - 32)

   for i=1 to islandcount do
      let islx = posx.[i]+incislandx.[i]
      let isly = posy.[i]+incislandy.[i]
      Shapes.Move(island_Array.[i],islx,isly)

   incy <- incy + bgs
   incby <- incby + float bgs
   for i=1 to islandcount do
      incislandy.[i] <- incislandy.[i] + bgs    

   if (incy = 32) then
      incy <- 0   

   for i=1 to islandcount do
      if ((posy.[i]+incislandy.[i]) > (gameHeight+15)) then // relaunch island if no more visible
         let R = int (Math.Round(float (Math.GetRandomNumber(nbrisland))))
         let AA = Math.Remainder(TimePlay,6)
         // give new coordinates
         posx.[i] <- islandPos.[AA].[2]
         posy.[i] <- islandPos.[AA].[3]
         Shapes.Move(island_Array.[R],posx.[i],posy.[i])
         incislandy.[i] <- 0
         incislandx.[i] <- 0         
   // Move playerammo
   for i = 1 to Player_AmmoCount do
      let shapeName = Player_Ammo.[i]
      let Player_Ammox = Shapes.GetLeft(shapeName) + incbx
      let Player_Ammoy = Shapes.GetTop(shapeName) - Player_Ammospeed
      Shapes.Move(shapeName, Player_Ammox, Player_Ammoy)
      let oldAge = Player_AmmoAge.GetOrDefault(i, 0)
      Player_AmmoAge.[i] <- oldAge + 1
   // Move Enemy ammo
   for iea = 1 to Enemy_AmmoCount do
      let dx = (Math.Sin((Enemy_Ammo_Angle.[iea] )) * float Enemy_Ammospeed)
      let dy = (Math.Cos((Enemy_Ammo_Angle.[iea] )) * float Enemy_Ammospeed)
      let Enemy_Ammox = Shapes.GetLeft(Enemy_Ammo.[iea]) + dx + incbx
      let Enemy_Ammoy = Shapes.GetTop(Enemy_Ammo.[iea]) + dy + incby * 0.1
      Shapes.Move(Enemy_Ammo.[iea], Enemy_Ammox, Enemy_Ammoy)
      let oldAge =
         match Enemy_AmmoAge.TryGetValue(iea) with
         | true, n -> n
         | false, _ -> 0
      Enemy_AmmoAge.[iea] <- oldAge + 1

   // move ennemies
   let mutable i=1
   while i <= enemy_Count do
      // move as TimeLine and Path say
      let eNBR = enemy_PathNBR.[i]
      let mutable uu = enemy_line.[i]
      let Time = enemy_TimeLine.[i]
      let etl = enemyPath.[eNBR].[1].GetOrDefault(uu, 0.0)  //enemy own timeLine
      if (Time=etl) then  //it's time to rotate enemy
         let rr = enemyPath.[eNBR].[2].GetOrDefault(uu+1, 0.0)            
         Shapes.Rotate(enemy_Array.[i],rr)
      if (Time > etl)  then
         uu <- uu+1
         enemy_line.[i] <- uu    // next line for enemy move      
      let xx1 = enemy_PosX.[i]
      let yy1 = enemy_PosY.[i]
      let xx = float xx1+enemyPath.[eNBR].[3].GetOrDefault(uu, 0.0)+incbx
      let yy = float yy1+enemyPath.[eNBR].[4].GetOrDefault(uu, 0.0)+incby*0.1
      // Randomly fire-enemy
      if (Math.GetRandomNumber(Enemy_Agresivity)=1) then
         Enemy_ShootX <- xx1 + 16
         Enemy_ShootY <- yy1 + 4
         if (yy1 > 0 && xx1 > 0 && yy1 < gameHeight && xx1 < gameWidth) then
            // this avoid enemy fire from outside the screen
            fire_Enemy()

      Shapes.Move(enemy_Array.[i],xx,yy)
      enemy_PosX.[i] <- int xx
      enemy_PosY.[i] <- int yy
      enemy_TimeLine.[i] <- Time + 1.0
      if ((float uu > enemyPath.[eNBR].[0].[0]) && (Time > etl)) then // if last timelife remove the enemy sprite 
         let next_enemy_remove = i
         remove_enemy(next_enemy_remove)      
      
      Collision_pbe()
      Collision_ep()

      i <- i + 1
      
and RemovePlayer_Ammo (player_Ammo_nextRemove) =
   Shapes.Remove(Player_Ammo.[player_Ammo_nextRemove])
   for iz = player_Ammo_nextRemove to Player_AmmoCount - 1 do
       Player_Ammo.[iz] <- Player_Ammo.[iz+1]
       Player_AmmoAge.[iz] <- Player_AmmoAge.[iz+1]
   Player_Ammo.Remove(Player_AmmoCount) |> ignore
   Player_AmmoAge.Remove(Player_AmmoCount) |> ignore
   Player_AmmoCount <- Player_AmmoCount - 1

and RemoveEnemy_Ammo (enemy_Ammo_nextRemove) =
   Shapes.Remove(Enemy_Ammo.[enemy_Ammo_nextRemove])
   for irea = enemy_Ammo_nextRemove to Enemy_AmmoCount - 1 do
       Enemy_Ammo.[irea] <- Enemy_Ammo.[irea+1]      
       Enemy_AmmoAge.[irea] <-
         if (irea+1) < Enemy_AmmoAge.Count then Enemy_AmmoAge.[irea+1] else 0
       Enemy_Ammo_Angle.[irea] <- Enemy_Ammo_Angle.[irea+1]
   Enemy_Ammo.Remove(Enemy_AmmoCount) |> ignore
   Enemy_AmmoAge.Remove(Enemy_AmmoCount) |> ignore
   Enemy_Ammo_Angle.Remove(Enemy_AmmoCount) |> ignore
   Enemy_AmmoCount <- Enemy_AmmoCount - 1

and Fire () =
   lock ammoSync (fun () -> 
      // Remove additional player Ammo
      while (Player_AmmoCount > Player_AmmoMax ) do
         RemovePlayer_Ammo(1)

      // Add the player Ammo
      Player_AmmoCount <- Player_AmmoCount + 1
      Player_Ammo.[Player_AmmoCount] <- Shapes.AddImage(bullet0)
      Shapes.Move(Player_Ammo.[Player_AmmoCount], ShootX, ShootY)
   )

and fire_Enemy () =
   // Remove additional Enemy Ammo
   while (Enemy_AmmoCount > (Enemy_AmmoMax - 1)) do      
      RemoveEnemy_Ammo(1)

   // Add the Enemy Ammo
   Enemy_AmmoCount <- Enemy_AmmoCount + 1
   Enemy_Ammo.[Enemy_AmmoCount] <- Shapes.AddImage(Enemy_bullet)
   Enemy_Ammo_Angle.[Enemy_AmmoCount] <- Math.ArcTan(float(paddleX- Enemy_ShootX+player_size/2)/float(paddleY-Enemy_ShootY))
   let shape = Enemy_Ammo.[Enemy_AmmoCount]
   Shapes.Move(shape, Enemy_ShootX, Enemy_ShootY)

//Check playerammo age
and AgePlayer_Ammo () =
   while (Player_AmmoAge.Count > 0 && Player_AmmoAge.[1] > Player_AmmoLife) do
      RemovePlayer_Ammo(1)  

// Check enemy ammo age
and AgeEnemy_Ammo () =
   while (Enemy_AmmoAge.Count > 0 && Enemy_AmmoAge.[1] > Enemy_AmmoLife) do
      RemoveEnemy_Ammo(1)

and remove_enemy (next_enemy_remove) =
   Shapes.Remove(enemy_Array.[next_enemy_remove])
   // Remove all references from the arrays
   for ii = next_enemy_remove to enemy_Count - 1 do
      enemy_Array.[ii] <- enemy_Array.[ii+1]
      enemy_line.[ii] <- enemy_line.[ii+1]
      enemy_PosX.[ii] <- enemy_PosX.[ii+1]
      enemy_PosY.[ii] <- enemy_PosY.[ii+1]
      enemy_TimeLine.[ii] <- enemy_TimeLine.[ii+1]
      enemy_PathNBR.[ii] <- enemy_PathNBR.[ii+1]
   enemy_Array.Remove(enemy_Count) |> ignore
   enemy_line.Remove(enemy_Count) |> ignore
   enemy_PosX.Remove(enemy_Count) |> ignore
   enemy_PosY.Remove(enemy_Count) |> ignore
   enemy_TimeLine.Remove(enemy_Count) |> ignore
   enemy_PathNBR.Remove(enemy_Count) |> ignore
   enemy_Count <- enemy_Count - 1

and create_enemies_left () =

   let mutable TimeLine1 = 0.0
   for i=1 to enemy_Nbr do
      enemy_Count <- enemy_Count + 1
      enemy_PathNBR.[enemy_Count] <- pathNBR
      if (enemy_type = 2) then
         enemy_Array.[enemy_Count] <- Shapes.AddImage(enemy2)
      else
         enemy_Array.[enemy_Count] <- Shapes.AddImage(enemy)
      enemy_line.[enemy_Count] <- 1
      enemy_PosX.[enemy_Count] <- enemyPosX1
      enemy_PosY.[enemy_Count] <- enemyPosY1
      enemy_TimeLine.[enemy_Count] <- TimeLine1

      enemyPosX1 <- enemyPosX1 - 64  // distance between ennemies
      TimeLine1 <- TimeLine1 - 64.0 / float enemy_speed   
   for i=(enemy_Count-enemy_Nbr+1) to enemy_Count do
      let xxx = enemy_PosX.[i]
      let yyy = enemy_PosY.[i]
      Shapes.Move(enemy_Array.[i],xxx,yyy)

and create_enemies_right () =

   let mutable TimeLine1 = 0.0
   for i=1 to enemy_Nbr do
      enemy_Count <- enemy_Count + 1
      enemy_PathNBR.[enemy_Count] <- pathNBR
      enemy_Array.[enemy_Count] <- Shapes.AddImage(enemy)
      enemy_line.[enemy_Count] <- 1
      enemy_PosX.[enemy_Count] <- enemyPosX1
      enemy_PosY.[enemy_Count] <- enemyPosY1
      enemy_TimeLine.[enemy_Count] <- float TimeLine1

      enemyPosX1 <- enemyPosX1 + 64 // distance between ennemies
      TimeLine1 <- TimeLine1 - 64.0 / enemy_speed
         
   for i=(enemy_Count-enemy_Nbr+1) to enemy_Count do
      let xxx=enemy_PosX.[i]
      let yyy=enemy_PosY.[i]
      Shapes.Move(enemy_Array.[i],xxx,yyy)
   
and Collision_pbe () =  // for player-bullet and enemies

   let mutable i1 = 1
   while i1 <= Player_AmmoCount do
      // player bullet position
      let shapeName = Player_Ammo.[i1]
      let Player_Ammox = int (Shapes.GetLeft(shapeName))
      let Player_Ammoy = int (Shapes.GetTop(shapeName))
      let px1=Player_Ammox+player_bullet_size/3   // in order to have a more precise collison than the bullet image size
      let py1=Player_Ammoy+player_bullet_size/3
      let px2=px1+2*player_bullet_size/3
      let py2=py1+2*player_bullet_size/3

      let mutable ammoRemoved = false
      let mutable i2 = 1
      while i2 <= enemy_Count do
         // enemy position 
         let ax1=enemy_PosX.[i2]+enemy_size/4
         let ay1=enemy_PosY.[i2]+enemy_size/4

         let ax2=ax1+3*enemy_size/4
         let ay2=ay1+3*enemy_size/4

         if ( (ax1 < px1 && ax2 > px1) || (ax1 < px2 && ax2 > px2) ) then
            if ( (ay1 < py1 && ay2 > py1) || (ay1 < py2 && ay2 > py2) ) then
               // collision between enemy nbr i2 and player bullet i
               // remove bullet i and animate explosion and remove enemy i2
               if not ammoRemoved then 
                  RemovePlayer_Ammo(i1)
                  ammoRemoved <- true
               let next_enemy_remove = i2
               remove_enemy(next_enemy_remove)
               // begin animation for explosion at coordinate ax1, ay1
               let toto = Shapes.AddImage(enemy_expl1)
               Shapes.Move(toto,ax1,ay1)
               Program.Delay(30)
               Shapes.Remove(toto)
               let toto = Shapes.AddImage(enemy_expl2)
               Shapes.Move(toto,ax1,ay1)
               Program.Delay(30)
               Shapes.Remove(toto)
               score <- score + 100

         i2 <- i2 + 1

      
      i1 <- i1 + 1

and Collision_ep () =   // for enemies and player 
   let px1 = int (Shapes.GetLeft(player))
   let py1 = int (Shapes.GetTop(player))
   let px2 = px1 + player_size
   let py2 = py1 + player_size

   //Shapes.Move(Shapes.AddRectangle(px2-px1 ,py2-py1), px1, py1)
   let mutable i2 = 1
   while i2 <= enemy_Count do
      // enemy position 
      let ax1=enemy_PosX.[i2]
      let ay1=enemy_PosY.[i2]

      let ax2=ax1+enemy_size
      let ay2=ay1+enemy_size
      //Shapes.Move(Shapes.AddRectangle(ax2-ax1 ,ay2-ay1), ax1, ay1) 

      if ( (px1 < ax1 && px2 > ax1) || (px1 < ax2 && px2 > ax2) ) then
         if ( (py1 < ay1 && py2 > ay1) || (py1 < ay2 && py2 > ay2) ) then
            // collision between enemy nbr i2 and player 
            // animate explosion and decrease lives            
            remove_enemy(i2)
            // begin animation for explosion at coordinate ax1, ay1
            let toto = Shapes.AddImage(enemy_expl1)
            Shapes.Move(toto,ax1,ay1)
            Program.Delay(30)
            Shapes.Remove(toto)
            let toto = Shapes.AddImage(player_expl)
            Shapes.Move(toto,ax1,ay1)
            Program.Delay(300)
            Shapes.Remove(toto)
            Player_Lives <- Player_Lives - 1
            if (Player_Lives = 0) then
               Program.End()

      i2 <- i2 + 1

   let px1 = paddleX
   let py1 = paddleY
   let px2 = px1 + player_size
   let py2 = py1 + player_size
   // Shapes.Move(Shapes.AddRectangle(px2-px1 ,py2-py1), px1, py1)
   
   let mutable i3 = 1
   while i3 <= Enemy_AmmoCount do
      // enemy position 

      let ax1=int (Shapes.GetLeft(Enemy_Ammo.[i3]))
      let ay1=int (Shapes.GetTop(Enemy_Ammo.[i3]))

      let ax2=ax1+enemy_ammo_size
      let ay2=ay1+enemy_ammo_size
      //Shapes.Move(Shapes.AddRectangle(ax2-ax1 ,ay2-ay1), ax1, ay1) 

      if ( (px1 < ax1 && px2 > ax1) || (px1 < ax2 && px2 > ax2) ) then
         if ( (py1 < ay1 && py2 > ay1) || (py1 < ay2 && py2 > ay2) ) then
            // collision between enemy ammo nbr i3 and player 
            // animate explosion and decrease lives
            RemoveEnemy_Ammo(i3)

            // begin animation for explosion at coordinate ax1, ay1
            let toto = Shapes.AddImage(enemy_expl1)
            Shapes.Move(toto,paddleX+ player_size/2,paddleY+ player_size/2)
            Program.Delay(30)
            Shapes.Remove(toto)
            let toto = Shapes.AddImage(player_expl)
            Shapes.Move(toto,paddleX+ player_size/2,paddleY+ player_size/2)
            Program.Delay(300)
            Shapes.Remove(toto)
            Player_Lives <- Player_Lives - 1
            if (Player_Lives = 0) then
               Program.End()

      i3 <- i3 + 1

and IslandsPosition () =
   // island positions, avoid randomGeneration and islands overlap
   islandPos.[0] <- Dictionary()
   islandPos.[0].[1] <- 1
   islandPos.[0].[2] <- 0
   islandPos.[0].[3] <- -150
   islandPos.[1] <- Dictionary()
   islandPos.[1].[1] <- 1
   islandPos.[1].[2] <- - int (Math.Round(float gameWidth/2.0))
   islandPos.[1].[3] <- -150
   islandPos.[2] <- Dictionary()
   islandPos.[2].[1] <- 2
   islandPos.[2].[2] <- -2 * int (Math.Round(float gameWidth/3.0))
   islandPos.[2].[3] <- -150
   islandPos.[3] <- Dictionary()
   islandPos.[3].[1] <- 1
   islandPos.[3].[2] <- 2 * int (Math.Round(float gameWidth/3.0))
   islandPos.[3].[3] <- -150
   islandPos.[4] <- Dictionary()
   islandPos.[4].[1] <- 2
   islandPos.[4].[2] <- gameWidth
   islandPos.[4].[3] <- -150
   islandPos.[5] <- Dictionary()
   islandPos.[5].[1] <- 3
   islandPos.[5].[2] <- int (Math.Round(float gameWidth/3.0))
   islandPos.[5].[3] <- -150
   islandPos.[6] <- Dictionary()
   islandPos.[6].[1] <- 3
   islandPos.[6].[2] <- -gameWidth
   islandPos.[6].[3] <- -150

and define_paths () =
   for i = 0 to 6 do
      enemyPath.[i] <- Dictionary()
      for j = 0 to 4 do 
         enemyPath.[i].[j] <- Dictionary()

   enemyPath.[0].[0].[0] <- 3.   // nbr of strait lines of path
   enemyPath.[0].[1].[1] <- 30.  // from 0 to this in timeline
   enemyPath.[0].[1].[2] <- 100. // from this to next in timeline
   enemyPath.[0].[1].[3] <- 400. // sprite goes up

   enemyPath.[0].[2].[1] <- 0.   // first line rotation = 0
   enemyPath.[0].[2].[2] <- 45.  // second line rotation = 45
   enemyPath.[0].[2].[3] <- 45.

   enemyPath.[0].[3].[1] <- enemy_speed // first line x movment
   enemyPath.[0].[3].[2] <- enemy_speed // second line x movment
   enemyPath.[0].[3].[3] <- enemy_speed // third line ...

   enemyPath.[0].[4].[1] <- 0. // first line y movment
   enemyPath.[0].[4].[2] <- enemy_speed
   enemyPath.[0].[4].[3] <- enemy_speed
   // ---------- second pat
   enemyPath.[1].[0].[0] <- 4.
   enemyPath.[1].[1].[1] <- 1.
   enemyPath.[1].[1].[2] <- 1.
   enemyPath.[1].[1].[3] <- 100.
   enemyPath.[1].[1].[4] <- 400.

   enemyPath.[1].[2].[1] <- 180.
   enemyPath.[1].[2].[2] <- 135.
   enemyPath.[1].[2].[3] <- 135.
   enemyPath.[1].[2].[4] <- 90.

   enemyPath.[1].[3].[1] <- -enemy_speed
   enemyPath.[1].[3].[2] <- -enemy_speed
   enemyPath.[1].[3].[3] <- -enemy_speed
   enemyPath.[1].[3].[4] <- 0.

   enemyPath.[1].[4].[1] <- 0.
   enemyPath.[1].[4].[2] <- 0.
   enemyPath.[1].[4].[3] <- enemy_speed
   enemyPath.[1].[4].[4] <- enemy_speed
   // round r=5
   enemyPath.[2].[0].[0] <- 21.
   enemyPath.[2].[1].[1] <- 50.
   enemyPath.[2].[1].[2] <- 55.
   enemyPath.[2].[1].[3] <- 60.
   enemyPath.[2].[1].[4] <- 65.
   enemyPath.[2].[1].[5] <- 70.
   enemyPath.[2].[1].[6] <- 75.
   enemyPath.[2].[1].[7] <- 80.
   enemyPath.[2].[1].[8] <- 85.
   enemyPath.[2].[1].[9] <- 90.
   enemyPath.[2].[1].[10] <- 95.
   enemyPath.[2].[1].[11] <- 100.
   enemyPath.[2].[1].[12] <- 105.
   enemyPath.[2].[1].[13] <- 110.
   enemyPath.[2].[1].[14] <- 115.
   enemyPath.[2].[1].[15] <- 120.
   enemyPath.[2].[1].[16] <- 125.
   enemyPath.[2].[1].[17] <- 130.
   enemyPath.[2].[1].[18] <- 135.
   enemyPath.[2].[1].[19] <- 140.
   enemyPath.[2].[1].[20] <- 145.
   enemyPath.[2].[1].[21] <-350.
   enemyPath.[2].[2].[1] <- 0.
   enemyPath.[2].[2].[2] <- 18.
   enemyPath.[2].[2].[3] <- 36.
   enemyPath.[2].[2].[4] <- 54.
   enemyPath.[2].[2].[5] <- 72.
   enemyPath.[2].[2].[6] <- 90.
   enemyPath.[2].[2].[7] <- 108.
   enemyPath.[2].[2].[8] <- 126.
   enemyPath.[2].[2].[9] <- 144.
   enemyPath.[2].[2].[10] <- 162.
   enemyPath.[2].[2].[11] <- 180.
   enemyPath.[2].[2].[12] <- 198.
   enemyPath.[2].[2].[13] <- 216.
   enemyPath.[2].[2].[14] <- 234.
   enemyPath.[2].[2].[15] <- 252.
   enemyPath.[2].[2].[16] <- -90.
   enemyPath.[2].[2].[17] <- -72.
   enemyPath.[2].[2].[18] <- -54.
   enemyPath.[2].[2].[19] <- -36.
   enemyPath.[2].[2].[20] <- -18.
   enemyPath.[2].[2].[21] <- 0.
   enemyPath.[2].[3].[1] <- enemy_speed
   enemyPath.[2].[3].[2] <- 0.95*enemy_speed
   enemyPath.[2].[3].[3] <- 0.81*enemy_speed
   enemyPath.[2].[3].[4] <- 0.59*enemy_speed
   enemyPath.[2].[3].[5] <- 0.31*enemy_speed
   enemyPath.[2].[3].[6] <- 0.
   enemyPath.[2].[3].[7] <- -0.31*enemy_speed
   enemyPath.[2].[3].[8] <- -0.59*enemy_speed
   enemyPath.[2].[3].[9] <- -0.81*enemy_speed
   enemyPath.[2].[3].[10] <- -0.95*enemy_speed
   enemyPath.[2].[3].[11] <- -enemy_speed
   enemyPath.[2].[3].[12] <- -0.95*enemy_speed
   enemyPath.[2].[3].[13] <- -0.81*enemy_speed
   enemyPath.[2].[3].[14] <- -0.59*enemy_speed
   enemyPath.[2].[3].[15] <- -0.31*enemy_speed
   enemyPath.[2].[3].[16] <- 0.
   enemyPath.[2].[3].[17] <- 0.31*enemy_speed
   enemyPath.[2].[3].[18] <- 0.59*enemy_speed
   enemyPath.[2].[3].[19] <- 0.81*enemy_speed
   enemyPath.[2].[3].[20] <- 0.95*enemy_speed
   enemyPath.[2].[3].[21] <- enemy_speed
   enemyPath.[2].[4].[1] <- 0.
   enemyPath.[2].[4].[2] <- 0.31*enemy_speed
   enemyPath.[2].[4].[3] <- 0.59*enemy_speed
   enemyPath.[2].[4].[4] <- 0.81*enemy_speed
   enemyPath.[2].[4].[5] <- 0.95*enemy_speed
   enemyPath.[2].[4].[6] <- enemy_speed
   enemyPath.[2].[4].[7] <- 0.95*enemy_speed
   enemyPath.[2].[4].[8] <- 0.81*enemy_speed
   enemyPath.[2].[4].[9] <- 0.59*enemy_speed
   enemyPath.[2].[4].[10] <- 0.31*enemy_speed
   enemyPath.[2].[4].[11] <- 0.
   enemyPath.[2].[4].[12] <- -0.31*enemy_speed
   enemyPath.[2].[4].[13] <- -0.59*enemy_speed
   enemyPath.[2].[4].[14] <- -0.81*enemy_speed
   enemyPath.[2].[4].[15] <- -0.95*enemy_speed
   enemyPath.[2].[4].[16] <- -enemy_speed
   enemyPath.[2].[4].[17] <- -0.95*enemy_speed
   enemyPath.[2].[4].[18] <- -0.81*enemy_speed
   enemyPath.[2].[4].[19] <- -0.59*enemy_speed
   enemyPath.[2].[4].[20] <- -0.31*enemy_speed
   enemyPath.[2].[4].[21] <- 0.
   // round r=20
   enemyPath.[3].[0].[0] <- 21.
   enemyPath.[3].[1].[1] <- 120.
   enemyPath.[3].[1].[2] <- 140.
   enemyPath.[3].[1].[3] <- 160.
   enemyPath.[3].[1].[4] <- 180.
   enemyPath.[3].[1].[5] <- 200.
   enemyPath.[3].[1].[6] <- 220.
   enemyPath.[3].[1].[7] <- 240.
   enemyPath.[3].[1].[8] <- 260.
   enemyPath.[3].[1].[9] <- 280.
   enemyPath.[3].[1].[10] <- 300.
   enemyPath.[3].[1].[11] <- 320.
   enemyPath.[3].[1].[12] <- 340.
   enemyPath.[3].[1].[13] <- 360.
   enemyPath.[3].[1].[14] <- 380.
   enemyPath.[3].[1].[15] <- 400.
   enemyPath.[3].[1].[16] <- 420.
   enemyPath.[3].[1].[17] <- 440.
   enemyPath.[3].[1].[18] <- 460.
   enemyPath.[3].[1].[19] <- 480.
   enemyPath.[3].[1].[20] <- 500.
   enemyPath.[3].[1].[21] <- 600.
   enemyPath.[3].[2].[1] <- 0.
   enemyPath.[3].[2].[2] <- 18.
   enemyPath.[3].[2].[3] <- 36.
   enemyPath.[3].[2].[4] <- 54.
   enemyPath.[3].[2].[5] <- 72.
   enemyPath.[3].[2].[6] <- 90.
   enemyPath.[3].[2].[7] <- 108.
   enemyPath.[3].[2].[8] <- 126.
   enemyPath.[3].[2].[9] <- 144.
   enemyPath.[3].[2].[10] <- 162.
   enemyPath.[3].[2].[11] <- 180.
   enemyPath.[3].[2].[12] <- 198.
   enemyPath.[3].[2].[13] <- 216.
   enemyPath.[3].[2].[14] <- 234.
   enemyPath.[3].[2].[15] <- 252.
   enemyPath.[3].[2].[16] <- -90.
   enemyPath.[3].[2].[17] <- -72.
   enemyPath.[3].[2].[18] <- -54.
   enemyPath.[3].[2].[19] <- -36.
   enemyPath.[3].[2].[20] <- -18.
   enemyPath.[3].[2].[21] <- 0.
   enemyPath.[3].[3].[1] <- enemy_speed
   enemyPath.[3].[3].[2] <- 0.95*enemy_speed
   enemyPath.[3].[3].[3] <- 0.81*enemy_speed
   enemyPath.[3].[3].[4] <- 0.59*enemy_speed
   enemyPath.[3].[3].[5] <- 0.31*enemy_speed
   enemyPath.[3].[3].[6] <- 0.
   enemyPath.[3].[3].[7] <- -0.31*enemy_speed
   enemyPath.[3].[3].[8] <- -0.59*enemy_speed
   enemyPath.[3].[3].[9] <- -0.81*enemy_speed
   enemyPath.[3].[3].[10] <- -0.95*enemy_speed
   enemyPath.[3].[3].[11] <- -enemy_speed
   enemyPath.[3].[3].[12] <- -0.95*enemy_speed
   enemyPath.[3].[3].[13] <- -0.81*enemy_speed
   enemyPath.[3].[3].[14] <- -0.59*enemy_speed
   enemyPath.[3].[3].[15] <- -0.31*enemy_speed
   enemyPath.[3].[3].[16] <- 0.
   enemyPath.[3].[3].[17] <- 0.31*enemy_speed
   enemyPath.[3].[3].[18] <- 0.59*enemy_speed
   enemyPath.[3].[3].[19] <- 0.81*enemy_speed
   enemyPath.[3].[3].[20] <- 0.95*enemy_speed
   enemyPath.[3].[3].[21] <- enemy_speed
   enemyPath.[3].[4].[1] <- 0.
   enemyPath.[3].[4].[2] <- 0.31*enemy_speed
   enemyPath.[3].[4].[3] <- 0.59*enemy_speed
   enemyPath.[3].[4].[4] <- 0.81*enemy_speed
   enemyPath.[3].[4].[5] <- 0.95*enemy_speed
   enemyPath.[3].[4].[6] <- enemy_speed
   enemyPath.[3].[4].[7] <- 0.95*enemy_speed
   enemyPath.[3].[4].[8] <- 0.81*enemy_speed
   enemyPath.[3].[4].[9] <- 0.59*enemy_speed
   enemyPath.[3].[4].[10] <- 0.31*enemy_speed
   enemyPath.[3].[4].[11] <- 0.
   enemyPath.[3].[4].[12] <- -0.31*enemy_speed
   enemyPath.[3].[4].[13] <- -0.59*enemy_speed
   enemyPath.[3].[4].[14] <- -0.81*enemy_speed
   enemyPath.[3].[4].[15] <- -0.95*enemy_speed
   enemyPath.[3].[4].[16] <- -enemy_speed
   enemyPath.[3].[4].[17] <- -0.95*enemy_speed
   enemyPath.[3].[4].[18] <- -0.81*enemy_speed
   enemyPath.[3].[4].[19] <- -0.59*enemy_speed
   enemyPath.[3].[4].[20] <- -0.31*enemy_speed
   enemyPath.[3].[4].[21] <- 0.

   // round r=10
   enemyPath.[4].[0].[0] <- 21.
   enemyPath.[4].[1].[1] <- 110.
   enemyPath.[4].[1].[2] <- 120.
   enemyPath.[4].[1].[3] <- 130.
   enemyPath.[4].[1].[4] <- 140.
   enemyPath.[4].[1].[5] <- 150.
   enemyPath.[4].[1].[6] <- 160.
   enemyPath.[4].[1].[7] <- 170.
   enemyPath.[4].[1].[8] <- 180.
   enemyPath.[4].[1].[9] <- 190.
   enemyPath.[4].[1].[10] <- 200.
   enemyPath.[4].[1].[11] <- 210.
   enemyPath.[4].[1].[12] <- 220.
   enemyPath.[4].[1].[13] <- 230.
   enemyPath.[4].[1].[14] <- 240.
   enemyPath.[4].[1].[15] <- 250.
   enemyPath.[4].[1].[16] <- 260.
   enemyPath.[4].[1].[17] <- 270.
   enemyPath.[4].[1].[18] <- 280.
   enemyPath.[4].[1].[19] <- 290.
   enemyPath.[4].[1].[20] <- 300.
   enemyPath.[4].[1].[21] <- 500.
   enemyPath.[4].[2].[1] <- 0.
   enemyPath.[4].[2].[2] <- 18.
   enemyPath.[4].[2].[3] <- 36.
   enemyPath.[4].[2].[4] <- 54.
   enemyPath.[4].[2].[5] <- 72.
   enemyPath.[4].[2].[6] <- 90.
   enemyPath.[4].[2].[7] <- 108.
   enemyPath.[4].[2].[8] <- 126.
   enemyPath.[4].[2].[9] <- 144.
   enemyPath.[4].[2].[10] <- 162.
   enemyPath.[4].[2].[11] <- 180.
   enemyPath.[4].[2].[12] <- 198.
   enemyPath.[4].[2].[13] <- 216.
   enemyPath.[4].[2].[14] <- 234.
   enemyPath.[4].[2].[15] <- 252.
   enemyPath.[4].[2].[16] <- -90.
   enemyPath.[4].[2].[17] <- -72.
   enemyPath.[4].[2].[18] <- -54.
   enemyPath.[4].[2].[19] <- -36.
   enemyPath.[4].[2].[20] <- -18.
   enemyPath.[4].[2].[21] <- 0.
   enemyPath.[4].[3].[1] <- enemy_speed
   enemyPath.[4].[3].[2] <- 0.95*enemy_speed
   enemyPath.[4].[3].[3] <- 0.81*enemy_speed
   enemyPath.[4].[3].[4] <- 0.59*enemy_speed
   enemyPath.[4].[3].[5] <- 0.31*enemy_speed
   enemyPath.[4].[3].[6] <- 0.
   enemyPath.[4].[3].[7] <- -0.31*enemy_speed
   enemyPath.[4].[3].[8] <- -0.59*enemy_speed
   enemyPath.[4].[3].[9] <- -0.81*enemy_speed
   enemyPath.[4].[3].[10] <- -0.95*enemy_speed
   enemyPath.[4].[3].[11] <- -enemy_speed
   enemyPath.[4].[3].[12] <- -0.95*enemy_speed
   enemyPath.[4].[3].[13] <- -0.81*enemy_speed
   enemyPath.[4].[3].[14] <- -0.59*enemy_speed
   enemyPath.[4].[3].[15] <- -0.31*enemy_speed
   enemyPath.[4].[3].[16] <- 0.
   enemyPath.[4].[3].[17] <- 0.31*enemy_speed
   enemyPath.[4].[3].[18] <- 0.59*enemy_speed
   enemyPath.[4].[3].[19] <- 0.81*enemy_speed
   enemyPath.[4].[3].[20] <- 0.95*enemy_speed
   enemyPath.[4].[3].[21] <- enemy_speed
   enemyPath.[4].[4].[1] <- 0.
   enemyPath.[4].[4].[2] <- 0.31*enemy_speed
   enemyPath.[4].[4].[3] <- 0.59*enemy_speed
   enemyPath.[4].[4].[4] <- 0.81*enemy_speed
   enemyPath.[4].[4].[5] <- 0.95*enemy_speed
   enemyPath.[4].[4].[6] <- enemy_speed
   enemyPath.[4].[4].[7] <- 0.95*enemy_speed
   enemyPath.[4].[4].[8] <- 0.81*enemy_speed
   enemyPath.[4].[4].[9] <- 0.59*enemy_speed
   enemyPath.[4].[4].[10] <- 0.31*enemy_speed
   enemyPath.[4].[4].[11] <- 0.
   enemyPath.[4].[4].[12] <- -0.31*enemy_speed
   enemyPath.[4].[4].[13] <- -0.59*enemy_speed
   enemyPath.[4].[4].[14] <- -0.81*enemy_speed
   enemyPath.[4].[4].[15] <- -0.95*enemy_speed
   enemyPath.[4].[4].[16] <- -enemy_speed
   enemyPath.[4].[4].[17] <- -0.95*enemy_speed
   enemyPath.[4].[4].[18] <- -0.81*enemy_speed
   enemyPath.[4].[4].[19] <- -0.59*enemy_speed
   enemyPath.[4].[4].[20] <- -0.31*enemy_speed
   enemyPath.[4].[4].[21] <- 0.

   // round r=15 + exit down
   enemyPath.[5].[0].[0] <- 27.
   enemyPath.[5].[1].[1] <- 120.
   enemyPath.[5].[1].[2] <- 125.
   enemyPath.[5].[1].[3] <- 150.
   enemyPath.[5].[1].[4] <- 165.
   enemyPath.[5].[1].[5] <- 180.
   enemyPath.[5].[1].[6] <- 195.
   enemyPath.[5].[1].[7] <- 210.
   enemyPath.[5].[1].[8] <- 225.
   enemyPath.[5].[1].[9] <- 240.
   enemyPath.[5].[1].[10] <- 255.
   enemyPath.[5].[1].[11] <- 270.
   enemyPath.[5].[1].[12] <- 285.
   enemyPath.[5].[1].[13] <- 300.
   enemyPath.[5].[1].[14] <- 315.
   enemyPath.[5].[1].[15] <- 330.
   enemyPath.[5].[1].[16] <- 345.
   enemyPath.[5].[1].[17] <- 360.
   enemyPath.[5].[1].[18] <- 375.
   enemyPath.[5].[1].[19] <- 390.
   enemyPath.[5].[1].[20] <- 405.
   enemyPath.[5].[1].[21] <- 600.
   enemyPath.[5].[1].[22] <- 615.
   enemyPath.[5].[1].[23] <- 630.
   enemyPath.[5].[1].[24] <- 645.
   enemyPath.[5].[1].[25] <- 660.
   enemyPath.[5].[1].[26] <- 675.
   enemyPath.[5].[1].[27] <- 800.

   enemyPath.[5].[2].[1] <- 0.
   enemyPath.[5].[2].[2] <- 18.
   enemyPath.[5].[2].[3] <- 36.
   enemyPath.[5].[2].[4] <- 54.
   enemyPath.[5].[2].[5] <- 72.
   enemyPath.[5].[2].[6] <- 90.
   enemyPath.[5].[2].[7] <- 108.
   enemyPath.[5].[2].[8] <- 126.
   enemyPath.[5].[2].[9] <- 144.
   enemyPath.[5].[2].[10] <- 162.
   enemyPath.[5].[2].[11] <- 180.
   enemyPath.[5].[2].[12] <- 198.
   enemyPath.[5].[2].[13] <- 216.
   enemyPath.[5].[2].[14] <- 234.
   enemyPath.[5].[2].[15] <- 252.
   enemyPath.[5].[2].[16] <- -90.
   enemyPath.[5].[2].[17] <- -72.
   enemyPath.[5].[2].[18] <- -54.
   enemyPath.[5].[2].[19] <- -36.
   enemyPath.[5].[2].[20] <- -18.
   enemyPath.[5].[2].[21] <- 0.

   enemyPath.[5].[2].[22] <- 18.
   enemyPath.[5].[2].[23] <- 36.
   enemyPath.[5].[2].[24] <- 54.
   enemyPath.[5].[2].[25] <- 72.
   enemyPath.[5].[2].[26] <- 90.
   enemyPath.[5].[2].[27] <- 90.


   enemyPath.[5].[3].[1] <- enemy_speed
   enemyPath.[5].[3].[2] <- 0.95*enemy_speed
   enemyPath.[5].[3].[3] <- 0.81*enemy_speed
   enemyPath.[5].[3].[4] <- 0.59*enemy_speed
   enemyPath.[5].[3].[5] <- 0.31*enemy_speed
   enemyPath.[5].[3].[6] <- 0.
   enemyPath.[5].[3].[7] <- -0.31*enemy_speed
   enemyPath.[5].[3].[8] <- -0.59*enemy_speed
   enemyPath.[5].[3].[9] <- -0.81*enemy_speed
   enemyPath.[5].[3].[10] <- -0.95*enemy_speed
   enemyPath.[5].[3].[11] <- -enemy_speed
   enemyPath.[5].[3].[12] <- -0.95*enemy_speed
   enemyPath.[5].[3].[13] <- -0.81*enemy_speed
   enemyPath.[5].[3].[14] <- -0.59*enemy_speed
   enemyPath.[5].[3].[15] <- -0.31*enemy_speed
   enemyPath.[5].[3].[16] <- 0.
   enemyPath.[5].[3].[17] <- 0.31*enemy_speed
   enemyPath.[5].[3].[18] <- 0.59*enemy_speed
   enemyPath.[5].[3].[19] <- 0.81*enemy_speed
   enemyPath.[5].[3].[20] <- 0.95*enemy_speed
   enemyPath.[5].[3].[21] <- enemy_speed
   enemyPath.[5].[3].[22] <- 0.95*enemy_speed
   enemyPath.[5].[3].[23] <- 0.81*enemy_speed
   enemyPath.[5].[3].[24] <- 0.59*enemy_speed
   enemyPath.[5].[3].[25] <- 0.31*enemy_speed
   enemyPath.[5].[3].[26] <- 0.
   enemyPath.[5].[3].[27] <- 0.

   enemyPath.[5].[4].[1] <- 0.
   enemyPath.[5].[4].[2] <- 0.31*enemy_speed
   enemyPath.[5].[4].[3] <- 0.59*enemy_speed
   enemyPath.[5].[4].[4] <- 0.81*enemy_speed
   enemyPath.[5].[4].[5] <- 0.95*enemy_speed
   enemyPath.[5].[4].[6] <- enemy_speed
   enemyPath.[5].[4].[7] <- 0.95*enemy_speed
   enemyPath.[5].[4].[8] <- 0.81*enemy_speed
   enemyPath.[5].[4].[9] <- 0.59*enemy_speed
   enemyPath.[5].[4].[10] <- 0.31*enemy_speed
   enemyPath.[5].[4].[11] <- 0.
   enemyPath.[5].[4].[12] <- -0.31*enemy_speed
   enemyPath.[5].[4].[13] <- -0.59*enemy_speed
   enemyPath.[5].[4].[14] <- -0.81*enemy_speed
   enemyPath.[5].[4].[15] <- -0.95*enemy_speed
   enemyPath.[5].[4].[16] <- -enemy_speed
   enemyPath.[5].[4].[17] <- -0.95*enemy_speed
   enemyPath.[5].[4].[18] <- -0.81*enemy_speed
   enemyPath.[5].[4].[19] <- -0.59*enemy_speed
   enemyPath.[5].[4].[20] <- -0.31*enemy_speed
   enemyPath.[5].[4].[21] <- 0.
   enemyPath.[5].[4].[22] <- 0.31*enemy_speed
   enemyPath.[5].[4].[23] <- 0.59*enemy_speed
   enemyPath.[5].[4].[24] <- 0.81*enemy_speed
   enemyPath.[5].[4].[25] <- 0.95*enemy_speed
   enemyPath.[5].[4].[26] <- enemy_speed
   enemyPath.[5].[4].[27] <- enemy_speed

   enemyPath.[6].[0].[0] <- 3.
   enemyPath.[6].[1].[1] <- 1.
   enemyPath.[6].[1].[2] <- 80.
   enemyPath.[6].[1].[3] <- 200.
   enemyPath.[6].[2].[1] <- 0.
   enemyPath.[6].[2].[2] <- 90.
   enemyPath.[6].[2].[3] <- -90.
   enemyPath.[6].[3].[1] <- 0.
   enemyPath.[6].[3].[2] <- 0.
   enemyPath.[6].[3].[3] <- 0.
   enemyPath.[6].[4].[1] <- 0.
   enemyPath.[6].[4].[2] <- enemy_speed
   enemyPath.[6].[4].[3] <- -enemy_speed


and create_level1 () =  // this define the behavior of the différent squadron along the time play for level 1
   level1.[1] <- Dictionary()
   level1.[1].[1] <- 20    // when timeplay=level1[1][1]
   level1.[1].[2] <- 2    // lauch enemy with Path level1[1][2]
   level1.[1].[3] <- -10  // at x coordinate level1[1][3]
   level1.[1].[4] <- 0   // at y coordinate level1[1][4]
   level1.[1].[5] <- 1   // 1 for create enemy at the left side; 2 for the right side of screen
   level1.[1].[6] <- 4   // level[1][6] is the number of enemies
   level1.[1].[7] <- 1   // type of enemy
   level1.[2] <- Dictionary()
   level1.[2].[1] <- 80
   level1.[2].[2] <- 6
   level1.[2].[3] <- gameWidth/2
   level1.[2].[4] <- -500
   level1.[2].[5] <- 1
   level1.[2].[6] <- 3
   level1.[2].[7] <- 1
   level1.[3] <- Dictionary()
   level1.[3].[1] <- 150
   level1.[3].[2] <- 0
   level1.[3].[3] <- -10
   level1.[3].[4] <- 0
   level1.[3].[5] <- 1
   level1.[3].[6] <- 6
   level1.[3].[7] <- 2
   level1.[4] <- Dictionary()
   level1.[4].[1] <- 280
   level1.[4].[2] <- 4
   level1.[4].[3] <- -10
   level1.[4].[4] <- 0
   level1.[4].[5] <- 1
   level1.[4].[6] <- 3
   level1.[4].[7] <- 2
   level1.[5] <- Dictionary()
   level1.[5].[1] <- 410
   level1.[5].[2] <- 6
   level1.[5].[3] <- gameWidth/3
   level1.[5].[4] <- -50
   level1.[5].[5] <- 1
   level1.[5].[6] <- 3
   level1.[5].[7] <- 1
   level1.[6] <- Dictionary()
   level1.[6].[1] <- 430
   level1.[6].[2] <- 6
   level1.[6].[3] <- 2*gameWidth/3
   level1.[6].[4] <- -50
   level1.[6].[5] <- 2
   level1.[6].[6] <- 3
   level1.[6].[7] <- 1

   level1.[5].[1] <- 500
   level1.[5].[2] <- 6
   level1.[5].[3] <- gameWidth/3
   level1.[5].[4] <- -50
   level1.[5].[5] <- 1
   level1.[5].[6] <- 6
   level1.[5].[7] <- 2

   level1.[6].[1] <- 590
   level1.[6].[2] <- 5
   level1.[6].[3] <- 100
   level1.[6].[4] <- -80
   level1.[6].[5] <- 1
   level1.[6].[6] <- 3
   level1.[6].[7] <- 3
  
   level1.[7] <- Dictionary()
   level1.[7].[1] <- 690
   level1.[7].[2] <- 6
   level1.[7].[3] <- gameWidth/3
   level1.[7].[4] <- -50
   level1.[7].[5] <- 2
   level1.[7].[6] <- 3
   level1.[7].[7] <- 2
   for i= 1 to 10 do
      level1.[7+i] <- Dictionary()
      level1.[7+i].[1] <- 700+50*i
      level1.[7+i].[2] <- 6
      level1.[7+i].[3] <- Math.GetRandomNumber(gameWidth)
      level1.[7+i].[4] <- -50 + i
      level1.[7+i].[5] <- Math.GetRandomNumber(2)
      level1.[7+i].[6] <- Math.GetRandomNumber(3)
      level1.[7+i].[7] <- Math.GetRandomNumber(2)
   level1.[18] <- Dictionary()
   level1.[18].[1] <- 1300
   level1.[18].[2] <- 1
   level1.[18].[3] <- gameWidth
   level1.[18].[4] <- -10
   level1.[18].[5] <- 2
   level1.[18].[6] <- 6
   level1.[18].[7] <- 2

   for i=1 to 10 do
      level1.[18+i] <- Dictionary()
      level1.[18+i].[1] <- 1330+50*i
      level1.[18+i].[2] <- 4+Math.GetRandomNumber(2)
      level1.[18+i].[3] <- Math.GetRandomNumber(50)
      level1.[18+i].[4] <- i
      level1.[18+i].[5] <- 1
      level1.[18+i].[6] <- Math.GetRandomNumber(3)
      level1.[18+i].[7] <- Math.GetRandomNumber(2)

   for i= 1 to 10 do
      level1.[28+i] <- Dictionary()
      level1.[28+i].[1] <- 1900+50*i
      level1.[28+i].[2] <- 4+Math.GetRandomNumber(2)
      level1.[28+i].[3] <- -Math.GetRandomNumber(50)
      level1.[28+i].[4] <- i
      level1.[28+i].[5] <- 1
      level1.[28+i].[6] <- Math.GetRandomNumber(3)
      level1.[28+i].[7] <- Math.GetRandomNumber(2)

   for i= 1 to 10 do
     level1.[38+i] <- Dictionary()
     level1.[38+i].[1] <- 2450+100*i
     level1.[38+i].[2] <- 6
     level1.[38+i].[3] <- Math.GetRandomNumber(gameWidth)
     level1.[38+i].[4] <- -50 + i
     level1.[38+i].[5] <- Math.GetRandomNumber(2)
     level1.[38+i].[6] <- Math.GetRandomNumber(5)
     level1.[38+i].[7] <- Math.GetRandomNumber(2)

//Presentation
// todo

// Start game
Init()
define_paths()
//create_enemies1()
Play()
printfn "GAME OVER"