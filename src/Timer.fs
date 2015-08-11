namespace Library

[<Sealed>]
type Timer private () =
   static member Pause() = My.App.PauseTimer()
   static member Resume() = My.App.ResumeTimer()
   static member Tick with set (callback:Callback) = My.App.TimerTick <- callback
   static member Interval with set (ms:int) = My.App.TimerInterval <- ms