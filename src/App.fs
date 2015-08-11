namespace Library

open Xwt
open System
open System.Threading

type Callback = delegate of unit -> unit

type internal MyApp () =
   let mutable isHidden : bool = true
   let mutable mainWindow : Window = null
   let mutable mainCanvas : DrawingCanvas = null
   let mutable keyUp = Callback(ignore)
   let mutable keyDown = Callback(ignore)   
   let mutable mouseDown = Callback(ignore)
   let mutable mouseUp = Callback(ignore)
   let mutable mouseMove = Callback(ignore)
   let mutable timerTick = Callback(ignore)
   let mutable timerPaused = false
   let mutable lastKey = ""
   let mutable mouseX = 0.0
   let mutable mouseY = 0.0
   let mutable isLeftButtonDown = false
   let mutable isRightButtonDown = false
   let initCanvas () =
      mainCanvas <- new DrawingCanvas(BackgroundColor=toXwtColor Colors.White)
      mainCanvas.KeyPressed.Add(fun args -> 
         lastKey <- args.Key.ToString()
         if keyDown <> null then keyDown.Invoke()
      )
      mainCanvas.KeyReleased.Add(fun args ->
         lastKey <- args.Key.ToString()
         if keyUp <> null then keyUp.Invoke()
      )
      mainCanvas.ButtonPressed.Add(fun args ->
         mouseX <- args.X
         mouseY <- args.Y
         if args.Button = PointerButton.Left then isLeftButtonDown <-true
         if args.Button = PointerButton.Right then isRightButtonDown <-true
         if mouseDown <> null then mouseDown.Invoke()
      )
      mainCanvas.ButtonReleased.Add(fun args ->
         mouseX <- args.X
         mouseY <- args.Y
         if args.Button = PointerButton.Left then isLeftButtonDown <- false
         if mouseUp <> null then mouseUp.Invoke()
      )
      mainCanvas.MouseMoved.Add(fun args ->
         mouseX <- args.X
         mouseY <- args.Y
         if mouseMove <> null then mouseMove.Invoke()
      )
      mainWindow.Content <- mainCanvas
      mainCanvas.CanGetFocus <- true
      mainCanvas.SetFocus()
   let showWindow () = 
      if isHidden then mainWindow.Show(); isHidden <- false
   let hideWindow () = 
      if not isHidden then mainWindow.Hide(); isHidden <- true
   let mutable timerDisposable : IDisposable = null
   let setTimerInterval (ms:int) =
      if timerDisposable <> null then timerDisposable.Dispose()
      timerDisposable <-
         Application.TimeoutInvoke(ms, fun () -> 
            if not timerPaused then timerTick.Invoke()
            true
         )
   let runApp onInit =      
      Application.Initialize (ToolkitType.Gtk)      
      mainWindow <- new Window(Title="App", Padding = WidgetSpacing(), Width=640.0, Height=480.0)
      initCanvas ()
      showWindow ()         
      let onInit = unbox<unit->unit> onInit
      onInit ()        
      Application.Run()
   let startAppThread () =
      use isInitialized = new AutoResetEvent(false)
      let thread = Thread(ParameterizedThreadStart runApp)
      thread.SetApartmentState(ApartmentState.STA)
      thread.Start(fun () -> isInitialized.Set() |> ignore)
      isInitialized.WaitOne() |> ignore      
   do startAppThread()
   member app.Window = mainWindow
   member app.SetWindowWidth(width) =
      hideWindow()     
      mainWindow.Width <- width
      showWindow()
   member app.SetWindowHeight(height) =
      hideWindow()     
      mainWindow.Height <- height
      showWindow()
   member app.Canvas = mainCanvas
   member app.Invoke action = Application.Invoke action   
   member app.KeyUp with set callback = keyUp <- callback
   member app.KeyDown with set callback = keyDown <- callback
   member app.LastKey with get() = lastKey
   member app.MouseDown with set callback = mouseDown <- callback
   member app.MouseUp with set callback = mouseUp <- callback
   member app.MouseMove with set callback = mouseMove <- callback
   member app.MouseX with get() = mouseX
   member app.MouseY with get() = mouseY
   member app.IsLeftButtonDown with get() = isLeftButtonDown
   member app.IsRightButtonDown with get() = isRightButtonDown
   member app.Show() = app.Invoke (fun () -> showWindow())
   member app.Hide() = app.Invoke (fun () -> hideWindow())
   member app.TimerTick with set callback = timerTick <- callback
   member app.PauseTimer() = timerPaused <- true
   member app.ResumeTimer() = timerPaused <- false
   member app.TimerInterval with set ms = app.Invoke(fun () -> setTimerInterval ms)
   member app.ShowMessage(text:string,title) = MessageDialog.ShowMessage(mainWindow,text,title)

[<Sealed>]
type internal My private () = 
   static let mutable app = None
   static let sync = obj ()
   static let isFsi () =
      let args = System.Environment.GetCommandLineArgs()
      args.Length > 0 && System.IO.Path.GetFileName(args.[0]) = "fsi.exe"
   static let closeApp () =
      lock (sync) (fun () ->
         Application.Exit()
         if not (isFsi()) then
            Environment.Exit(0)
         app <- None       
      )
   static let getApp () =
      lock (sync) (fun () ->
         match app with
         | Some app -> app
         | None ->
            let newApp = MyApp()
            app <- Some (newApp)
            newApp.Window.CloseRequested.Add(fun e ->
               closeApp()
            )
            newApp
      )      
   static member App = getApp ()

[<AutoOpen>]
module internal AppDrawing =
   let addDrawing drawing =
      My.App.Invoke (fun () -> My.App.Canvas.AddDrawing(drawing))
   let addDrawingAt drawing (x,y) =
      My.App.Invoke (fun () -> My.App.Canvas.AddDrawingAt(drawing,Point(x,y)))

