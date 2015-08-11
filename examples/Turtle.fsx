#r "../lib/Xwt.dll"
#r "../src/bin/debug/FunSharp.Library.dll"

open Library

GraphicsWindow.PenColor <- Colors.Purple
Turtle.X <- 150.0
Turtle.Y <- 150.0
for i in 0..5..200 do
   Turtle.Move(float i)
   Turtle.Turn(90.0)
