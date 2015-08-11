#I "../lib"
#r "../lib/Xwt.dll"
#r "../src/bin/Debug/FunSharp.Library.dll"

open Library

let ball = Shapes.AddRectangle(200.0, 100.0)

let OnMouseDown () =
  let x = GraphicsWindow.MouseX
  let y = GraphicsWindow.MouseY
  Shapes.Move(ball, x, y)

GraphicsWindow.MouseDown <- Callback(OnMouseDown)
