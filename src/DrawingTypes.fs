namespace Library

type Color = 
   struct
      val A:byte
      val R:byte
      val G:byte
      val B:byte
   end
   new (a,r,g,b) = { A=a; R=r; G=g; B=b }

[<AutoOpen>]
module internal ColorConverter =
   let toXwtColor (color:Color) = Xwt.Drawing.Color.FromBytes(color.R, color.G, color.B, color.A)

type internal Width = float
type internal Height = float
type internal Size = float
type internal Family = string
type internal IsBold = bool
type internal IsItalic = bool
type internal X = float
type internal Y = float

type internal Pen = Pen of Color * Width
type internal Font = Font of Size * Family * IsBold * IsItalic
type internal Line = Line of X * Y * X * Y
type internal Rect = Rect of Width * Height
type internal Triangle = Triangle of X * Y * X * Y * X * Y
type internal Ellipse = Ellipse of Width * Height

type internal Shape =
   | LineShape of Line * Pen   
   | RectShape of Rect * Pen * Color
   | TriangleShape of Triangle * Pen * Color
   | EllipseShape of Ellipse * Pen * Color
   | ImageShape of Xwt.Drawing.Image ref
   | TextShape of string ref * Font * Color

type internal Drawing =
   | DrawLine of Line * Pen
   | DrawRect of Rect * Pen
   | DrawTriangle of Triangle * Pen
   | DrawEllipse of Ellipse * Pen
   | DrawImage of Xwt.Drawing.Image ref * float * float
   | DrawText of float * float * string * Font * Color
   | DrawBoundText of float * float * float * string * Font * Color
   | FillRect of Rect * Color
   | FillTriangle of Triangle * Color
   | FillEllipse of Ellipse * Color
   | DrawShape of string * Shape

