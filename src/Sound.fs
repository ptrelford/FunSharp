module Library.Sound

open System.Media

type private IMarker = interface end

let private play name =
    let assembly = System.Reflection.Assembly.GetAssembly(typeof<IMarker>)
    let stream = assembly.GetManifestResourceStream(name+".wav")
    let player = new SoundPlayer(stream)
    player.Play()   

let PlayBellRing () =
    play "BellRing"

let PlayChime () =
    play "Chime"

let PlayClick () =
    play "Click"