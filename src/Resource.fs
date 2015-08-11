module internal Library.Resource

let private assembly = System.Reflection.Assembly.GetEntryAssembly()
let GetStream path =
    assembly.GetManifestResourceStream(path)
let LoadBytes path =
    use stream = GetStream(path)
    let length = int stream.Length
    let bytes = Array.zeroCreate length
    stream.Read(bytes, 0, length) |> ignore
    bytes

