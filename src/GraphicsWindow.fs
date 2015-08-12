namespace Library

open System

[<Sealed>]
type GraphicsWindow private () =   
   static let rnd = Random()
   static let mutable backgroundColor = Colors.White
   static let mutable width = 640
   static let mutable height = 480
   static let pen () = Pen(GraphicsWindow.PenColor,GraphicsWindow.PenWidth)
   static let brush () = GraphicsWindow.BrushColor
   static let font () = 
      Font.Font(GraphicsWindow.FontSize,GraphicsWindow.FontName,GraphicsWindow.FontBold, GraphicsWindow.FontItalic)
   static let draw drawing = addDrawing drawing      
   static let drawAt (x,y) drawing = addDrawingAt drawing (x,y)
   static member Title
      with set title =
         My.App.Invoke (fun () -> My.App.Window.Title <- title)
   static member BackgroundColor
      with get () = backgroundColor
      and set color = 
         backgroundColor <- color
         My.App.Invoke (fun () -> My.App.Canvas.BackgroundColor <- toXwtColor color)
   static member Width
      with get () = width
      and set newWidth =
         width <- newWidth
         My.App.Invoke (fun () -> My.App.SetWindowWidth(float newWidth))
   static member Height
      with get () = height
      and set newHeight =
         height <- newHeight
         My.App.Invoke (fun () -> My.App.SetWindowHeight(float newHeight))
   static member CanResize
      with get () = true
      and set (value:bool) = ()
   static member val PenColor = Colors.Black with get, set
   static member val PenWidth = 2.0 with get, set
   static member val BrushColor = Colors.Purple with get,set
   static member val FontSize = 12.0 with get,set
   static member val FontName = "" with get,set
   static member val FontBold = false with get,set
   static member val FontItalic = false with get,set
   static member Clear () =
      My.App.Invoke (fun () -> My.App.Canvas.ClearDrawings())
   static member DrawLine(x1,y1,x2,y2) =
      DrawLine(Line(x1,y1,x2,y2),pen()) |> draw
   static member DrawLine(x1:int,y1:int,x2:int,y2:int) =
      GraphicsWindow.DrawLine(float x1, float y1, float x2, float y2)
   static member DrawRectangle(x,y,width,height) =
      DrawRect(Rect(width,height),pen()) |> drawAt (x,y)
   static member DrawRectangle(x:int,y:int,width:int,height:int) =
      GraphicsWindow.DrawRectangle(float x, float y, float width, float height)
   static member DrawTriangle(x1,y1,x2,y2,x3,y3) =
      DrawTriangle(Triangle(x1,y1,x2,y2,x3,y3),pen()) |> draw
   static member DrawEllipse(x,y,width,height) =
      DrawEllipse(Ellipse(width,height),pen()) |> drawAt (x,y)
   static member DrawEllipse(x:int,y:int,width:int,height:int) =
      GraphicsWindow.DrawEllipse(float x, float y, float width, float height)
   static member DrawImage(imageName,x,y) =
      let imageRef =
         match ImageList.TryGetImage imageName with
         | Some image -> ref image
         | None ->
            if imageName.StartsWith("http:") || imageName.StartsWith("https:") 
            then
                let imageRef = ref null
                async {
                   let! image = Http.LoadImageAsync imageName
                   imageRef := image
                   My.App.Invoke(fun () -> My.App.Canvas.Invalidate())
                } |> Async.Start
                imageRef
            else
                ref (Xwt.Drawing.Image.FromResource(imageName))
      DrawImage(imageRef,x,y) |> draw
   static member DrawImage(imageName,x:int,y:int) =
      GraphicsWindow.DrawImage(imageName, float x, float y)
   static member DrawText(x,y,text) =
      DrawText(x,y,text,font(),brush()) |> draw
   static member DrawText(x:int,y:int,text) =
      GraphicsWindow.DrawText(float x,float y,text)
   static member DrawBoundText(x,y,width,text) =
      DrawBoundText(x,y,width,text,font(),brush()) |> draw
   static member FillRectangle(x,y,width,height) =
      FillRect(Rect(width,height),brush()) |> drawAt (x,y)
   static member FillRectangle(x:int,y:int,width:int,height:int) =
      GraphicsWindow.FillRectangle(float x,float y,float width,float height)
   static member FillTriangle(x1,y1,x2,y2,x3,y3) =
      FillTriangle(Triangle(x1,y1,x2,y2,x3,y3),brush()) |> draw
   static member FillEllipse(x,y,width,height) =
      FillEllipse(Ellipse(width,height),brush()) |> drawAt (x,y)
   static member FillEllipse(x:int,y:int,width:int,height:int) =
      FillEllipse(Ellipse(float width,float height),brush()) |> drawAt (float x,float y)
   static member LastKey with get() = My.App.LastKey
   static member KeyUp with set callback = My.App.KeyUp <- callback
   static member KeyDown with set callback = My.App.KeyDown <- callback 
   static member MouseX with get() = My.App.MouseX
   static member MouseY with get() = My.App.MouseY
   static member MouseDown with set callback = My.App.MouseDown <- callback
   static member MouseUp with set callback = My.App.MouseUp <- callback
   static member MouseMove with set callback = My.App.MouseMove <- callback
   static member GetColorFromRGB(r,g,b) = Color(255uy,byte r,byte g,byte b)
   static member GetRandomColor() : Color =
      let bytes = [|1uy..3uy|]
      rnd.NextBytes(bytes)
      Color(255uy,bytes.[0],bytes.[1],bytes.[2])
   static member Show() = My.App.Show()
   static member Hide() = My.App.Hide()
   static member ShowMessage(text:string,title) = 
      My.App.Invoke(fun () -> My.App.ShowMessage(text,title))