namespace Library

open System
open System.Threading

[<Sealed>]
type Program private () =
   static member Delay(ms:int) = Thread.Sleep(ms)
   static member End() = 
      Xwt.Application.Exit()
      Environment.Exit(0)     