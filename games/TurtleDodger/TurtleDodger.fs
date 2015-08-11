#if INTERACTIVE
#r "./bin/debug/Xwt.dll"
#r "./bin/debug/FunSharp.dll"
#endif

open Library

// Turtle Dodger 0.5b
// Copyright (c) 2014 Nonki Takahashi.
//
// License:
// The MIT License (MIT)
// http://opensource.org/licenses/mit-license.php
//
// History:
// 0.5b 2014-04-17 Changed to detect collision. (QZN342-3)
// 0.4a 2014-04-17 Added opening. (QZN342-2)
// 0.3a 2014-04-02 Avoided to hold while Turtle moving. (QZN342-1)
// 0.2a 2014-04-02 Changed for Silverlight. (QZN342-0)
// 0.1a 2014-04-02 Created. (QZN342)

let title = "Turtle Dodger 0.5b"
GraphicsWindow.Title <- title
// Debug variables
let debug = false
let mutable cross1 = "<shape name>"
let mutable cross2 = "<shape name>"
let mutable pos = "<shape name>"
// Global variables
let gw = 598
let gh = 428
let mutable scrolling = false
let mutable moving = false
let mutable collisionDetected = false
let mutable passed = 0
let mutable lastKey = ""
let mutable lastems = 0
let color = dict [1,Colors.Orange; 2, Colors.Cyan; 3, Colors.Lime]
let size = dict [1,20; 2,16; 3,12]
let mutable score = "<shape name>"
let mutable iMin = 0
let mutable iMax = 0
/// Falling object
type Object = { mutable X:int; mutable Y:int; Kind:int; ShapeName:string }
/// Falling objects
let objects = ResizeArray<Object>()

let rec Closing () =
   Timer.Pause()
   Turtle.Turn(720)
   GraphicsWindow.BrushColor <- Colors.White
   GraphicsWindow.FontName <- "Trebuchet MS"
   GraphicsWindow.FontSize <- 40.0
   let x = (gw - 217) / 2
   let y = 100
   GraphicsWindow.DrawText(x, y, "GAME OVER")
   Program.Delay(3000)
and Opening () =
   let url = "" // "http://www.nonkit.com/smallbasic.files/"
   let bigTurtle = Shapes.AddImage(url + "Turtle.png")
   Shapes.Move(bigTurtle, 180, 140)
   GraphicsWindow.BrushColor <- Colors.White
   GraphicsWindow.FontName <- "Trebuchet MS"
   GraphicsWindow.FontSize <- 50.0
   let x = (gw - 443) / 2
   let y = 40
   GraphicsWindow.DrawText(x, y, title)
   Program.Delay(3000)
   GraphicsWindow.Clear()
and Ready () =
   GraphicsWindow.FontSize <- 40.0
   let ready = Shapes.AddText("Ready?")
   let x = (gw - 130) / 2
   let y = 100
   Shapes.Move(ready, x, y)
   for opacity in 100 .. -10 .. 0 do
     Shapes.SetOpacity(ready, opacity)
     Program.Delay(200)
   Shapes.Remove(ready)
and Game () =
   Turtle.Speed <- 7
   Turtle.PenUp()
   let x = gw / 2
   let y = gh - 40
   GraphicsWindow.BrushColor <- Colors.White
   GraphicsWindow.FontSize <- 18.0
   score <- Shapes.AddText("0")
   Shapes.Move(score, 20, 20)
   if debug then
     GraphicsWindow.BrushColor <- Colors.White
     GraphicsWindow.FontSize <- 12.0
     pos <- Shapes.AddText("(" + x.ToString() + "," + y.ToString() + ")")
     GraphicsWindow.PenWidth <- 1.0
     cross1 <- Shapes.AddLine(0, -8, 0, 8)
     cross2 <- Shapes.AddLine(-8, 0, 8, 0)
     Shapes.Move(cross1, x, y)
     Shapes.Move(cross2, x, y)
     Shapes.Move(pos, gw - 100, 20)   
   Turtle.MoveTo(x, y)
   Turtle.Angle <- 0.0   
   moving <- false
   scrolling <- false
   Ready()
   GraphicsWindow.KeyDown <- Callback(OnKeyDown)
   let tick = false
   Timer.Interval <- 1000 / 24
   Timer.Tick <- Callback(OnTick)
   lastems <- Clock.ElapsedMilliseconds
   iMin <- 0
   while not collisionDetected do
     if moving then
       if lastKey = "Left" then
         Turtle.TurnLeft()
         Turtle.Move(30)
         Turtle.TurnRight()
       elif lastKey = "Right" then
         Turtle.TurnRight()
         Turtle.Move(30)
         Turtle.TurnLeft()      
       moving <- false
     else
       Program.Delay(100)      
and Init () =
   GraphicsWindow.BackgroundColor <- Colors.DodgerBlue
   GraphicsWindow.Width <- gw
   GraphicsWindow.Height <- gh   
   passed <- 0
   collisionDetected <- false
and OnTick () =
   if not scrolling then
     scrolling <- true
     let ems = Clock.ElapsedMilliseconds
     if ems - lastems > 500 then
       AddObject()
       lastems <- ems    
     ScrollObject()
     scrolling <- false
   if debug then
     let x = Math.Floor(Turtle.X)
     let y = Math.Floor(Turtle.Y)
     Shapes.SetText(pos, "(" + x.ToString() + "," + y.ToString() + ")")
     Shapes.Move(cross1, x, y)
     Shapes.Move(cross2, x, y)
and ScrollObject () =
   for i = iMin to iMax-1 do
     let x = objects.[i].X
     let y = objects.[i].Y + 5
     let tx = Math.Floor(Turtle.X) |> int
     let ty = Math.Floor(Turtle.Y) |> int
     let d = Math.SquareRoot(float (Math.Power(tx - x, 2) + Math.Power(ty - y, 2))) |> int
     if d < (size.[objects.[i].Kind] + 16) / 2 then
       collisionDetected <- true   
     if y > gh then
       passed <- passed + 1
       Shapes.SetText(score, string passed)
       Shapes.Remove(objects.[i].ShapeName)
       iMin <- i + 1
     else
       Shapes.Move(objects.[i].ShapeName, x, y)
       objects.[i].X <- x
       objects.[i].Y <- y   
and AddObject () =   
   iMax <- iMax + 1
   GraphicsWindow.PenWidth <- 1.0
   let kind = Math.GetRandomNumber(3)
   GraphicsWindow.BrushColor <- color.[kind]
   let sz = size.[kind]
   let shapeName = Shapes.AddRectangle(sz, sz)
   let x = Math.GetRandomNumber(gw - 20) + 10
   let y = -20
   objects.Add({ X = x; Y=y; Kind=kind; ShapeName=shapeName})
   Shapes.Move(shapeName, x, y)
   Shapes.Rotate(shapeName, Math.GetRandomNumber(360))
and OnKeyDown () =
   if not moving then
     moving <- true
     lastKey <- GraphicsWindow.LastKey   

Init()
Opening()
Game()
Closing()