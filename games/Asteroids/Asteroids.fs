#if INTERACTIVE
#r "./bin/debug/Xwt.dll"
#r "./bin/debug/FunSharp.dll"
#endif

open Library

// Asteroids Game
// Copyright (C) 2009, Jason T. Jacques 
// License: MIT license http://www.opensource.org/licenses/mit-license.php

// Game area controls
let gameWidth  = 640.0
let gameHeight = 480.0
let backColor = Colors.Black

// Window title
let gameTitle = "Asteroids, Score: "

// Target frames per second
let fps = 25

// Key controls
let leftKey  = "Left"
let rightKey = "Right"
let forwardKey = "Up"
let backKey = "Down"
let fireKey = "Space"
let pauseKey = "P"

// Asteroid (rock) settings
let rockSpeed = 1.0
let rockColor = Colors.White
let rockMin = 20 // small size rock
let rockTypes = 3 // number of rock sizes (multiples of small rock size)
let mutable initRocks = 5

// Ammo settings
let ammoSpeed = 5.0
let ammoColor = Colors.White
let ammoLife = 60 // moves before auto destruct
let ammoMax = 10
let ammoSize = 5.0

// Player settings
let playerColor = Colors.White
let playerHeight = 30.0
let playerWidth = 20.0
let mutable player = ""
let mutable playerAngle = 0.0
let mutable playerSpeed = 0.0
let safeTime = 100 // time player has to get out of the way on level up

// Point multiplier
let pointsMultiply = 10

// Array name initialisation
let rock = ResizeArray()
let rockAngle = ResizeArray()
let rockSize = ResizeArray()
let ammo = ResizeArray()
let ammoAngle = ResizeArray<float>()
let ammoAge = ResizeArray()

let mutable rockCount = 0
let mutable ammoCount = 0

let path = "" // "http://smallbasic.com/drop/"
let bigRock = ImageList.LoadImage(path + "Asteroids_BigRock.png")
let medRock = ImageList.LoadImage(path + "Asteroids_MediumRock.png")
let smlRock = ImageList.LoadImage(path + "Asteroids_SmallRock.png")
let background = ImageList.LoadImage(path + "Asteroids_Sky.png")

let mutable pause = 0
let mutable play = 0
let mutable playerSafe = 0
let mutable score = 0

let mutable nextSize = 0
let mutable nextPosition = ""
let mutable px1 = 0.0
let mutable py1 = 0.0
let mutable px2 = 0.0
let mutable py2 = 0.0

// Setup world
let rec Init () =
   GraphicsWindow.Hide()
   GraphicsWindow.Title <- gameTitle + "0"
   //GraphicsWindow.CanResize <- "False"
   GraphicsWindow.Width <- int gameWidth
   GraphicsWindow.Height <-int gameHeight

   GraphicsWindow.BackgroundColor <- backColor
   GraphicsWindow.BrushColor <- backColor
   GraphicsWindow.DrawImage(background, 0, 0)

   LevelCheck()

   GraphicsWindow.PenColor <- playerColor
   player <- Shapes.AddImage(path + "Asteroids_Ship.png")
   // player = Shapes.AddTriangle(playerWidth/2, 0, 0, playerHeight, playerWidth, playerHeight)
   Shapes.Move(player, (gameWidth - playerWidth) / 2.0, (float gameHeight - playerHeight) / 2.0)
   playerAngle <- 0.0

// Main gane routine
and Play () =
   GraphicsWindow.Show()
   GraphicsWindow.KeyDown <- Callback(ChangeDirection)

   // Main loop
   play <- 1
   pause <- 0
   while (play = 1) do
     Program.Delay(1000/fps)
     if (pause = 0) then
       Move()
       CollisionCheck()
       AgeAmmo()
       LevelCheck()

// Read key event and act
and ChangeDirection () =   
   if (GraphicsWindow.LastKey = rightKey) then
     playerAngle <- (playerAngle + 10.0) % 360.0
   elif (GraphicsWindow.LastKey = leftKey) then
     playerAngle <- (playerAngle - 10.0) % 360.0
   elif (GraphicsWindow.LastKey = forwardKey) then
     playerSpeed <- playerSpeed + 1.0
   elif (GraphicsWindow.LastKey = backKey) then
     playerSpeed <- playerSpeed - 1.0
   elif (GraphicsWindow.LastKey = fireKey) then
     Fire()
   elif (GraphicsWindow.LastKey = pauseKey) then
     pause <- Math.Remainder(pause + 1, 2)  
   Shapes.Rotate(player, playerAngle)

// Move all on screen items
and Move  () =
   // Move player
   let x = Math.Remainder(Shapes.GetLeft(player) + (Math.Cos(Math.GetRadians(playerAngle - 90.0)) * playerSpeed) + gameWidth, gameWidth)
   let y = Math.Remainder(Shapes.GetTop(player) + (Math.Sin(Math.GetRadians(playerAngle - 90.0)) * playerSpeed) + gameHeight, gameHeight)
   Shapes.Move(player, x, y)

   // Move rocks
   for i = 0 to rockCount-1 do
     let x = Math.Remainder(Shapes.GetLeft(rock.[i]) + (Math.Cos(Math.GetRadians(rockAngle.[i] - 90.0)) * rockSpeed) + gameWidth, gameWidth)
     let y = Math.Remainder(Shapes.GetTop(rock.[i]) + (Math.Sin(Math.GetRadians(rockAngle.[i] - 90.0)) * rockSpeed) + gameHeight, gameHeight)
     Shapes.Move(rock.[i], x, y)

   // Move ammo
   for i = 0 to ammoCount-1 do
     let x = Math.Remainder(Shapes.GetLeft(ammo.[i]) + (Math.Cos(Math.GetRadians(ammoAngle.[i] - 90.0)) * ammoSpeed) + gameWidth, gameWidth)
     let y = Math.Remainder(Shapes.GetTop(ammo.[i]) + (Math.Sin(Math.GetRadians(ammoAngle.[i] - 90.0)) * ammoSpeed) + gameHeight, gameHeight)
     Shapes.Move(ammo.[i], x, y)
     ammoAge.[i] <- ammoAge.[i] + 1

// Check for collisions between onscreen items
and CollisionCheck () =
   // Calculate player bounding box.
   px1 <- Shapes.GetLeft(player) - ( (Math.Abs(playerWidth * Math.Cos(Math.GetRadians(playerAngle)) + playerHeight * Math.Sin(Math.GetRadians(playerAngle))) - playerWidth) / 2.0)
   py1 <- Shapes.GetTop(player) - ( (Math.Abs(playerWidth * Math.Sin(Math.GetRadians(playerAngle)) + playerHeight * Math.Cos(Math.GetRadians(playerAngle))) - playerHeight) / 2.0)
   px2 <- px1 + Math.Abs(playerWidth * Math.Cos(Math.GetRadians(playerAngle)) + playerHeight * Math.Sin(Math.GetRadians(playerAngle)))
   py2 <- py1 + Math.Abs(playerWidth * Math.Sin(Math.GetRadians(playerAngle)) + playerHeight * Math.Cos(Math.GetRadians(playerAngle)))

   // Re-order co-oridinates if they are the wrong way arround
   if (px1 > px2) then
     let tmp = px1
     px1 <- px2
     px2 <- tmp  
   if (py1 > py2) then
     let tmp = py1
     py1 <- py2
     py2 <- tmp 

   let rocksToRemove = ResizeArray()
   let ammoToRemove = ResizeArray()
   // Check if each rock has hit something
   for i = 0 to rockCount-1 do
     let ax1 = Shapes.GetLeft(rock.[i])
     let ay1 = Shapes.GetTop(rock.[i])
     let ax2 = ax1 + float rockSize.[i]
     let ay2 = ay1 + float rockSize.[i]

     // Player collison
     if (playerSafe < 1) then
       if ( (ax1 < px1 && ax2 > px1) || (ax1 < px2 && ax2 > px2) ) then
         if ( (ay1 < py1 && ay2 > py1) || (ay1 < py2 && ay2 > py2) ) then
           EndGame()

     // Ammo collison
     for j in 0..ammoCount-1 do          
         let bx1 = Shapes.GetLeft(ammo.[j])
         let by1 = Shapes.GetTop(ammo.[j])
         let bx2 = bx1 + ammoSize
         let by2 = by1 + ammoSize

         if ( (ax1 < bx1 && ax2 > bx1) || (ax1 < bx2 && ax2 > bx2) ) then
            if ( (ay1 < by1 && ay2 > by1) || (ay1 < by2 && ay2 > by2) ) then           
               rocksToRemove.Add(i)
               ammoToRemove.Add(j)

   for i in rocksToRemove |> Seq.distinct |> List.ofSeq |> List.rev do
      RemoveRock i
   for j in ammoToRemove |> Seq.distinct |> List.ofSeq |> List.rev do
      RemoveAmmo j

   // Decrease the time player is safe
   if (playerSafe > 0) then
     playerSafe <- playerSafe - 1   

// Add a new rock to the world
and AddRock () =
   // Check if the next rock size/position has been specified   
   let size,x,y =
      if (nextSize <> 0) then
         let size = rockMin * nextSize
         let x = Shapes.GetLeft(nextPosition)
         let y = Shapes.GetTop(nextPosition)
         nextSize <- 0
         size,x,y
      else
         // Choose a random size and position
         let size = rockMin * Math.GetRandomNumber(rockTypes)
         let x = Math.GetRandomNumber(int gameWidth - size)
         let y = Math.GetRandomNumber(int gameHeight - size)
         size,float x,float y
   // Draw the rock
   GraphicsWindow.PenColor <- rockColor
   let image =
      if size = 60 then bigRock
      elif size = 40 then medRock
      else smlRock
   rock.Add(Shapes.AddImage(image))
   //Shapes.Zoom(rock.[rockCount],1.0,1.0)
   Shapes.Move(rock.[rockCount], x, y)
   rockAngle.Add(float (Math.GetRandomNumber(360)))
   rockSize.Add(size)
   rockCount <- rockCount + 1

// Remove a rock from the world and update score
and RemoveRock nextRemove =
   let mutable removeSize = rockSize.[nextRemove] / rockMin

   // If not a mini rock
   if (removeSize > 1) then
     // ... add new rocks until we have made up for it being broken apart...
     while (removeSize > 0) do
       nextSize <- Math.GetRandomNumber(removeSize - 1)
       nextPosition <- rock.[nextRemove]
       removeSize <- removeSize - nextSize
       AddRock ()
     // And give a point for a 'hit'
     score <- score + 1
   else
     // We've destroyed it - give some extra points and 
     score <- score + 5   

   // Show updated score
   GraphicsWindow.Title <- gameTitle + (score * pointsMultiply).ToString()

   // Remove all references from the arrays
   Shapes.Remove(rock.[nextRemove])
      
   rock.RemoveAt(nextRemove)
   rockAngle.RemoveAt(nextRemove)
   rockSize.RemoveAt(nextRemove)
   rockCount <- rockCount - 1

// Check if the player has completed the level, if so, level up
and LevelCheck () =
   if (rockCount < 1) then
     nextSize <- 0
     for i = 1 to initRocks do
       AddRock()     
     initRocks <- initRocks + 1
     // Give players some time to move out of the way
     playerSafe <- safeTime   

// Add ammo to game
and Fire () =
   // Remove additional ammo
   while (ammoCount > (ammoMax - 1)) do     
     RemoveAmmo 0
   // Add the ammo
   GraphicsWindow.PenColor <- ammoColor   
   ammo.Add(Shapes.AddEllipse(ammoSize, ammoSize))
   Shapes.Move(ammo.[ammoCount], (px1 + px2 - ammoSize) / 2.0, (py1 + py2 - ammoSize) / 2.0)
   ammoAngle.Add(playerAngle)
   ammoAge.Add(0)
   ammoCount <- ammoCount + 1

// Check ammo age
and AgeAmmo () =
   while ammoAge.Count > 0 && (ammoAge.[0] > ammoLife) do     
      RemoveAmmo 0

// Remove top Ammo
and RemoveAmmo nextRemove =
   Shapes.Remove(ammo.[nextRemove])
   ammo.RemoveAt(nextRemove)
   ammoAngle.RemoveAt(nextRemove)
   ammoAge.RemoveAt(nextRemove)
   ammoCount <- ammoCount - 1
   
// Display simple end game message box
and EndGame () =
   play <- 0
   Shapes.Remove(player)
   GraphicsWindow.ShowMessage("You scored " + (score * pointsMultiply).ToString() + " points. Thanks for Playing.", "Game Over!")

// Start game
Init()
Play()
