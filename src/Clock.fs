namespace Library

open System

type Clock private () =
   static member Year = DateTime.Now.Year
   static member Month = DateTime.Now.Month
   static member Day = DateTime.Now.Day
   static member Hour = DateTime.Now.Hour
   static member Minute = DateTime.Now.Minute
   static member Second = DateTime.Now.Second
   static member ElapsedMilliseconds = Environment.TickCount