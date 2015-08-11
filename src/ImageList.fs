namespace Library

open System
open System.IO
open System.Collections.Generic
open Xwt.Drawing

type ImageList private () =
   static let images = Dictionary<string,byte[]>()
   static member LoadImage(path:string) =
      let name = "ImageList" + Guid.NewGuid().ToString()
      let bytes =
         if path.StartsWith("http:") || path.StartsWith("https:") 
         then Http.LoadBytes path
         else Resource.LoadBytes path
      images.Add(name, bytes)
      name
   static member internal TryGetImage(name:string) =
      match images.TryGetValue(name) with
      | true, bytes ->         
         use memoryStream = new MemoryStream(bytes)
         let image = Image.FromStream(memoryStream)
         Some(image)
      | false, _ -> None



