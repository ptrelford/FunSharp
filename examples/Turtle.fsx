#I "../lib"
#r "../lib/Xwt.dll"
#r "../src/bin/Debug/FunSharp.Library.dll"

open Library

GraphicsWindow.PenColor <- Colors.Purple
Turtle.X <- 150.
Turtle.Y <- 150.
for i in 0..5..200 do
   Turtle.Move(i)
   Turtle.Turn(90)
