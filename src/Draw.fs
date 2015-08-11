module internal Library.Draw

open Xwt
open Xwt.Drawing

let drawEllipse (ctx:Context) (x,y,w,h) =
   let kappa = 0.5522848
   let ox = (w/2.0) * kappa
   let oy = (h/2.0) * kappa
   let xe = x + w
   let ye = y + h
   let xm = x + w / 2.0
   let ym = y + h / 2.0
   ctx.MoveTo(x,ym)
   ctx.CurveTo(x, ym - oy, xm - ox, y, xm, y)
   ctx.CurveTo(xm + ox, y, xe, ym - oy, xe, ym)
   ctx.CurveTo(xe, ym + oy, xm + ox, ye, xm, ye)
   ctx.CurveTo(xm - ox, ye, x, ym + oy, x, ym)

let drawTriangle (ctx:Context) (Triangle(x1,y1,x2,y2,x3,y3)) =
   ctx.MoveTo(x1,y1)
   ctx.LineTo(x2,y2)
   ctx.LineTo(x3,y3)
   ctx.LineTo(x1,y1)

let penStroke (ctx:Context) (Pen(color,width)) =
   ctx.SetColor(toXwtColor color)
   ctx.SetLineWidth(width)
   ctx.Stroke()

let fill (ctx:Context) (color:Color) =
   ctx.SetColor(color)  
   ctx.Fill()

let toLayout text (Font(size,family,isBold,isItalic)) =
   let layout = new TextLayout(Text=text)
   layout.Font <- layout.Font.WithSize(size/2.0)      
   if isBold then layout.Font <- layout.Font.WithWeight(FontWeight.Bold)
   if isItalic then layout.Font <- layout.Font.WithStyle(FontStyle.Italic)
   if family <> "" then layout.Font <- layout.Font.WithFamily(family)
   layout
 
type DrawingInfo = { 
   Drawing:Drawing; 
   mutable Offset:Point; 
   mutable Opacity:float option
   mutable IsVisible:bool 
   mutable Rotation:float option
   mutable Scale:(float * float) option
   }

let drawImage (ctx:Context) (info:DrawingInfo) (image:Image) (x,y) =
   match info.Rotation with
   | Some angle ->           
      let w,h = image.Width, image.Height
      ctx.Save()        
      ctx.Translate(x+w/2.0,y+h/2.0)
      ctx.Rotate(angle)
      ctx.Translate(-w / 2.0, -h / 2.0)
      match info.Scale with
      | Some(sx,sy) -> ctx.Scale(sx,sy)
      | None -> ()    
      ctx.Rectangle(0.0,0.0,w,h)                
      let pattern = new ImagePattern(image)            
      ctx.Pattern <- pattern           
      ctx.Fill()
      ctx.Restore()
   | None ->
      ctx.Save()            
      match info.Scale with
      | Some(sx,sy) -> 
         ctx.Scale(sx,sy)
         ctx.DrawImage(image,x/sx,y/sy)
      | None ->
         ctx.DrawImage(image,x,y)
      ctx.Restore()

let draw (ctx:Context) (info:DrawingInfo) =
   let x,y = info.Offset.X, info.Offset.Y
   let withOpacity (color:Color) =
      match info.Opacity with
      | Some opacity -> color.WithAlpha(color.Alpha * opacity)
      | None -> color
   match info.Drawing with
   | DrawLine(Line(x1,y1,x2,y2),pen) ->
      ctx.MoveTo(x1,y1)
      ctx.LineTo(x2,y2)
      penStroke ctx pen
   | DrawRect(Rect(w,h),pen) ->
      ctx.Rectangle(x,y,w,h)
      penStroke ctx pen
   | DrawTriangle(triangle,pen) ->
      drawTriangle ctx triangle
      penStroke ctx pen
   | DrawEllipse(Ellipse(w,h),pen) ->
      drawEllipse ctx (x,y,w,h)
      penStroke ctx pen
   | DrawImage(image,x',y') ->
      if !image <> null then         
         drawImage ctx info !image (x+x',y+y')
   | DrawText(x,y,text,font,color) ->
      let layout = toLayout text font
      ctx.SetColor(toXwtColor color)
      ctx.DrawTextLayout(layout,x,y)
   | DrawBoundText(x,y,width,text,font,color) ->
      let layout = toLayout text font
      layout.Width <- width      
      ctx.SetColor(toXwtColor color)
      ctx.DrawTextLayout(layout,x,y)
   | FillRect(Rect(w,h),fillColor) ->
      ctx.Rectangle(x,y,w,h)
      fill ctx (toXwtColor fillColor)
   | FillTriangle(triangle,fillColor) ->
      drawTriangle ctx triangle
      fill ctx (toXwtColor fillColor)
   | FillEllipse(Ellipse(w,h),fillColor) ->
      drawEllipse ctx (x,y,w,h)
      fill ctx (toXwtColor fillColor)
   | DrawShape(_,LineShape(Line(x1,y1,x2,y2),pen)) ->
      ctx.MoveTo(x+x1,y+y1)
      ctx.LineTo(x+x2,y+y2)
      penStroke ctx pen
   | DrawShape(_,RectShape(Rect(w,h),pen,fillColor)) ->
      ctx.Save() 
      ctx.Translate(x,y)
      match info.Rotation with
      | Some angle -> ctx.Rotate(angle)
      | None -> ()            
      ctx.Rectangle(0.,0.,w,h)
      fill ctx (toXwtColor fillColor)
      ctx.Rectangle(0.,0.,w,h)
      penStroke ctx pen
      ctx.Restore()
   | DrawShape(_,TriangleShape(triangle,pen,fillColor)) ->
      drawTriangle ctx triangle
      fill ctx (withOpacity (toXwtColor fillColor))
      drawTriangle ctx triangle
      penStroke ctx pen
   | DrawShape(_,EllipseShape(Ellipse(w,h),pen,fillColor)) ->
      drawEllipse ctx (x,y,w,h)      
      fill ctx (withOpacity (toXwtColor fillColor))
      drawEllipse ctx (x,y,w,h)
      penStroke ctx pen
   | DrawShape(_,TextShape(textRef,font,color)) ->
      let layout = toLayout !textRef font      
      ctx.SetColor(withOpacity (toXwtColor color))
      ctx.DrawTextLayout(layout,x,y)
   | DrawShape(_,ImageShape(image)) ->
      if !image <> null then                 
         drawImage ctx info !image (x,y)
