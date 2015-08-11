#I "../lib"
#r "../lib/Xwt.dll"
#r "../src/bin/Debug/FunSharp.Library.dll"

open System
open System.Collections.Generic
open Library

let GW = float GraphicsWindow.Width
let GH = float GraphicsWindow.Height
let Radius = 200.0
let MidX = GW/2.0
let MidY = GW/2.0

let initWindow () =
   GraphicsWindow.Show()
   GraphicsWindow.Title <- "Analog Clock"
   GraphicsWindow.BackgroundColor <- Colors.Black
   GraphicsWindow.BrushColor <- Colors.BurlyWood
   GraphicsWindow.DrawEllipse(MidX-Radius-15.,MidY-Radius-5.,Radius*2.+30.,Radius*2.+20.)
   GraphicsWindow.FillEllipse(MidX-Radius-15.,MidY-Radius-5.,Radius*2.+30.,Radius*2.+20.)
   for angle in 1.0..180.0 do
     let x = MidX+(Radius+15.)*Math.Cos(Math.GetRadians(angle))
     let y1 = MidY+Radius*Math.Sin(Math.GetRadians(angle))+15.
     let y2 = MidY+(Radius+15.)*Math.Sin(Math.GetRadians(-angle))+10.
     let blue = Math.GetRandomNumber(40)+30
     GraphicsWindow.PenWidth <- Math.GetRandomNumber(5) |> float
     let color = 
       GraphicsWindow.GetColorFromRGB(
         blue+100+Math.GetRandomNumber(10),
         blue+60+Math.GetRandomNumber(20),
         blue)
     GraphicsWindow.PenColor <- color
     Shapes.AddLine(x,y1,x,y2) |> ignore
   GraphicsWindow.BrushColor <- Colors.White   
   let ClockNum = Dictionary()
   for i in 1. .. 12. do
     let Radians = Math.GetRadians(-i * 30. + 90.)
     ClockNum.[i] <- Shapes.AddText(i.ToString())
     Shapes.Move(ClockNum.[i],MidX-4.+Radius*Math.Cos(Radians),MidY-4.-Radius*Math.Sin(Radians))   
   
let mutable HourHand = "<shape name>"
let mutable MinuteHand = "<shape name>"
let mutable SecondHand = "<shape name>"
let mutable Hour = 0.
let mutable Minute = 0.
let mutable Second = 0.
let setHands () = 
   if (Clock.Hour + Clock.Minute/60. + Clock.Second/3600. <> Hour) then
     Shapes.Remove(HourHand)
     Hour <- Clock.Hour + Clock.Minute/60. + Clock.Second/3600.
     GraphicsWindow.PenColor <- Colors.Black
     GraphicsWindow.PenWidth <- 3.
     HourHand <- 
       Shapes.AddLine(
         MidX,
         MidY,
         MidX+Radius/2.*Math.Cos(Math.GetRadians(Hour*30.-90.)),
         MidY+Radius/2.*Math.Sin(Math.GetRadians(Hour*30.-90.)))   
   if Clock.Minute <> Minute then
     Shapes.Remove(MinuteHand)
     Minute <- Clock.Minute + Clock.Second/60.
     GraphicsWindow.PenColor <- Colors.Blue
     GraphicsWindow.PenWidth <- 2.
     MinuteHand <- 
       Shapes.AddLine(
         MidX,
         MidY,
         MidX+Radius/1.2*Math.Cos(Math.GetRadians(Minute*6.-90.)),
         MidY+Radius/1.2*Math.Sin(Math.GetRadians(Minute*6.-90.)))   
   if Clock.Second <> Second then
     Shapes.Remove(SecondHand)
     Second <- Clock.Second
     GraphicsWindow.PenColor <- Colors.Red
     GraphicsWindow.PenWidth <- 1.
     SecondHand <- 
       Shapes.AddLine(
         MidX,
         MidY,
         MidX+Radius*Math.Cos(Math.GetRadians(Second*6.-90.)),
         MidY+Radius*Math.Sin(Math.GetRadians(Second*6.-90.)))
   
initWindow()
while true do
   setHands()
   //Sound.PlayClick()
   Program.Delay(1000)