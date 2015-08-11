namespace Library

open Xwt
open Xwt.Drawing
open Draw

[<AllowNullLiteral>]
type internal DrawingCanvas () =
   inherit Canvas ()
   let turtleImage = Xwt.Drawing.Image.FromResource("turtle.png")
   let drawings = ResizeArray<DrawingInfo>()
   let turtle =
      let w,h = turtleImage.Width, turtleImage.Height
      {Drawing=DrawImage(ref turtleImage,-w/2.,-h/2.); Offset=Point(); Opacity=None; IsVisible=false; Rotation=None; Scale=None}
   let onShape shapeName f =
      drawings
      |> Seq.tryPick (function
         | { Drawing=DrawShape(name,_) } as info when name = shapeName -> Some info 
         | _ -> None
      )
      |> Option.iter f   
   member canvas.Turtle = turtle
   member canvas.ClearDrawings() =
      drawings.Clear()
      canvas.QueueDraw()
   member canvas.AddDrawing(drawing) =
      { Drawing=drawing; Offset=Point(); Opacity=None; IsVisible=true; Rotation=None; Scale=None }
      |> drawings.Add
      canvas.QueueDraw()
   member canvas.AddDrawingAt(drawing, offset:Point) =
      { Drawing=drawing; Offset=offset; Opacity=None; IsVisible=true; Rotation=None; Scale=None }
      |> drawings.Add
      canvas.QueueDraw()
   member canvas.MoveShape(shape, offset:Point) =
      onShape shape (fun info -> info.Offset <- offset; canvas.QueueDraw())
   member canvas.SetShapeOpacity(shape, opacity) =
      onShape shape (fun info -> info.Opacity <- Some opacity; canvas.QueueDraw())
   member canvas.SetShapeVisibility(shape, isVisible) =
      onShape shape (fun info -> info.IsVisible <- isVisible; canvas.QueueDraw())
   member canvas.SetShapeRotation(shape, angle) =
      onShape shape (fun info -> info.Rotation <- Some(angle); canvas.QueueDraw())
   member canvas.SetShapeScale(shape, scaleX, scaleY) =
      onShape shape (fun info -> info.Scale <- Some(scaleX,scaleY); canvas.QueueDraw())
   member canvas.RemoveShape(shape) =
      drawings |> Seq.tryFindIndex (function 
         | { DrawingInfo.Drawing=DrawShape(shapeName,_) } -> shapeName = shape
         | _ -> false 
      )
      |> function Some index -> drawings.RemoveAt(index) | None -> ()
   member canvas.Invalidate() =
      canvas.QueueDraw()
   override this.OnDraw(ctx, rect) =
      base.OnDraw(ctx, rect)      
      for drawing in drawings do 
         if drawing.IsVisible then draw ctx drawing
      if turtle.IsVisible then draw ctx turtle