namespace Library

open System

type Clock private () =
   static member Hour = DateTime.Now.Hour |> float
   static member Minute = DateTime.Now.Minute |> float
   static member Second = DateTime.Now.Second |> float
   static member ElapsedMilliseconds = Environment.TickCount