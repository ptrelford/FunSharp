namespace Library

open Xwt
open System
open System.Collections.Generic

type internal ShapeInfo = { Shape:Shape; mutable Offset:Point; mutable Opacity:float }

[<Sealed>]
type Shapes private () =
   static let pen () = Pen(GraphicsWindow.PenColor,GraphicsWindow.PenWidth)
   static let brush () = GraphicsWindow.BrushColor
   static let font () = 
      Font(GraphicsWindow.FontSize,GraphicsWindow.FontName,GraphicsWindow.FontBold, GraphicsWindow.FontItalic)
   static let shapes = Dictionary<string,ShapeInfo>()
   static let addShape name shape =
      let info = { Shape=shape; Offset=Point(); Opacity=1.0 }
      shapes.Add(name,info)      
      addDrawing (DrawShape(name,shape))
   static let onShape shapeName action =
      match shapes.TryGetValue(shapeName) with
      | true, info -> action info
      | false, _ -> ()
   static let genName name = name + Guid.NewGuid().ToString()
   static member Remove(shapeName) =
      My.App.Invoke (fun () -> My.App.Canvas.RemoveShape(shapeName))
   static member AddLine(x1,y1,x2,y2) =
      let name = genName "Line"
      LineShape(Line(x1,y1,x2,y2),pen()) |> addShape name
      name
   static member AddLine(x1:int,y1:int,x2:int,y2:int) =
      Shapes.AddLine(float x1, float y1, float x2, float y2)
   static member AddRectangle(width,height) =
      let name = genName "Rectangle"
      RectShape(Rect(width,height),pen(),brush()) |> addShape name
      name
   static member AddRectangle(width:int,height:int) =
      Shapes.AddRectangle(float width, float height)
   static member AddTriangle(x1,y1,x2,y2,x3,y3) =
      let name = genName "Triangle"
      TriangleShape(Triangle(x1,y1,x2,y2,x3,y3),pen(),brush()) |> addShape name
      name
   static member AddEllipse(width,height) =
      let name = genName "Ellipse"
      EllipseShape(Ellipse(width,height),pen(),brush()) |> addShape name
      name
   static member AddEllipse(width:int,height:int) =
      Shapes.AddEllipse(float width,float height)
   static member AddImage(imageName) =
      let name = genName "Image"
      match ImageList.TryGetImage(imageName) with
      | Some image -> ImageShape(ref image) |> addShape name
      | None ->
         let imageRef = 
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
               let imageRef = ref null                 
               My.App.Invoke(fun () ->
                  use stream = Resource.GetStream(imageName)
                  imageRef := Xwt.Drawing.Image.FromStream(stream)
               )
               imageRef
         ImageShape(imageRef) |> addShape name
      name
   static member AddText(text) =
      let name = genName "Text"
      TextShape(ref text, font(), brush()) |> addShape name
      name
   static member HideShape(shapeName) =      
      My.App.Invoke (fun () -> My.App.Canvas.SetShapeVisibility(shapeName,false))      
   static member ShowShape(shapeName) =
      My.App.Invoke (fun () -> My.App.Canvas.SetShapeVisibility(shapeName,true))      
   static member Move(shapeName,x,y) =
      onShape shapeName (fun info ->
         info.Offset <- Point(x,y)
         My.App.Invoke (fun () -> My.App.Canvas.MoveShape(shapeName,info.Offset))
      )
   static member Move(shapeName,x:int,y:int) =
      Shapes.Move(shapeName, float x, float y)
   static member GetLeft(shapeName) =      
      match shapes.TryGetValue(shapeName) with
      | true, info -> info.Offset.X
      | false, _ -> 0.0
   static member GetTop(shapeName) =
      match shapes.TryGetValue(shapeName) with
      | true, info -> info.Offset.Y
      | false, _ -> 0.0
   static member SetOpacity(shapeName, opacity) =
      onShape shapeName (fun info ->
         info.Opacity <- opacity
         My.App.Invoke (fun () -> My.App.Canvas.SetShapeOpacity(shapeName,opacity))
      )
   static member SetOpacity(shapeName, opacity:int) =
      Shapes.SetOpacity(shapeName, float opacity)
   static member GetOpacity(shapeName) =
      match shapes.TryGetValue(shapeName) with
      | true, info -> info.Opacity
      | false, _ -> 1.0
   static member Rotate(shapeName, angle) =
      match shapes.TryGetValue(shapeName) with
      | true, info ->
         My.App.Invoke (fun () -> My.App.Canvas.SetShapeRotation(shapeName,angle))
      | false, _ -> ()
   static member Rotate(shapeName, angle:int) =
      Shapes.Rotate(shapeName, float angle)
   static member Zoom(shapeName, scaleX, scaleY) =
      match shapes.TryGetValue(shapeName) with
      | true, info ->
         My.App.Invoke (fun () -> My.App.Canvas.SetShapeScale(shapeName,scaleX,scaleY))
      | false, _ -> ()
   static member SetText(shapeName, text) =
      onShape shapeName (fun info ->
         match info.Shape with
         | TextShape(textRef, font, color) ->
            My.App.Invoke (fun () -> textRef := text; My.App.Canvas.Invalidate())
         | _ -> invalidOp "Expecting text shape"
      )       
   static member Animate(shapeName,x:float,y:float,ms:int) =
      Shapes.Move(shapeName, x, y)
   static member Animate(shapeName,x:int,y:int,ms:int) =
      Shapes.Animate(shapeName, float x, float y, ms)

