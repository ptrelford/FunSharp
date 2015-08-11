#I "../lib"
#r "../lib/Xwt.dll"
#r "../src/bin/Debug/FunSharp.Library.dll"

open Library

let angle = 30.0
let delta = 10.0

let rec drawTree distance =
   if distance > 0.0 then
      Turtle.Move(distance)
      Turtle.Turn(angle)
      drawTree(distance - delta)
      Turtle.Turn(-angle * 2.0)
      drawTree(distance - delta)
      Turtle.Turn(angle)
      Turtle.Move(-distance)

drawTree 60.0