#I "../lib"
#r "../lib/Xwt.dll"
#r "../src/bin/Debug/FunSharp.Library.dll"

open Library

GraphicsWindow.BackgroundColor <- Colors.Black
for i = 1 to 1200 do
   GraphicsWindow.BrushColor <- GraphicsWindow.GetRandomColor()
   GraphicsWindow.FillEllipse(Math.GetRandomNumber(800), Math.GetRandomNumber(600), 30, 30)
