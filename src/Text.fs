namespace Library

module Text =
   let Append(a:string,b:obj) = a + b.ToString()
   let GetLength (text:string) = text.Length
   let GetSubText (text:string,startIndex,length) = text.Substring(startIndex,length)

