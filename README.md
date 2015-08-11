# FunSharp
Fun cross-platform graphics library, based on [Small Basic](http://smallbasic.com/)'s library, made specifically for F# and C#.

## Building

* Appveyor: [![Build status](https://ci.appveyor.com/api/projects/status/vfuq7shh3piim4d3/branch/master?svg=true)](https://ci.appveyor.com/project/ptrelford/fshtml)
* Travis: [![Build Status](https://travis-ci.org/ptrelford/FsHtml.png?branch=master)](https://travis-ci.org/ptrelford/FsHtml/)

## Example

```F#
open Library

GraphicsWindow.PenColor <- Colors.Purple
Turtle.X <- 150.0
Turtle.Y <- 150.0
for i in 0..5..200 do
   Turtle.Move(float i)
   Turtle.Turn(90.0)
```
