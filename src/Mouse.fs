namespace Library

[<Sealed>]
type Mouse private () =
   static member IsLeftButtonDown = My.App.IsLeftButtonDown
   static member IsRightButtonDown = My.App.IsRightButtonDown
   static member X = My.App.MouseX
   static member Y = My.App.MouseY
   static member HideCursor () =
      My.App.Invoke (fun () -> My.App.Canvas.Cursor <- Xwt.CursorType.Invisible)
   static member ShowCursor () =
      My.App.Invoke (fun () -> My.App.Canvas.Cursor <- Xwt.CursorType.Arrow)