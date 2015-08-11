module Library.Math

let private rand = System.Random()
let GetRadians (deg:float) = deg * System.Math.PI / 180.
let GetRandomNumber(n) = rand.Next(n) + 1
let inline Remainder(x,y) = x % y
let Cos(angle) = cos angle
let Sin(angle) = sin angle
let ArcTan(angle) = atan angle
let inline Abs(n) = abs n
let inline Floor(n) = floor n |> int
let inline Round(n) = round n
let inline SquareRoot(d) = sqrt d
let inline Power(x,n) = pown x n

