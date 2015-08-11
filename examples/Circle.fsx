#r "../lib/Xwt.dll"
#r "../src/bin/debug/FunSharp.Library.dll"

open Library

let colors = [Colors.Blue; Colors.Yellow; Colors.Purple; Colors.Salmon]
GraphicsWindow.BackgroundColor <- Colors.Black
for i = 1 to 1200 do
   GraphicsWindow.BrushColor <- colors.[Math.GetRandomNumber(colors.Length)-1]
   GraphicsWindow.FillEllipse(Math.GetRandomNumber(800), Math.GetRandomNumber(600), 30, 30)
