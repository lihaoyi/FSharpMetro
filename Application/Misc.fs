module Misc
open System
open System.Collections.Generic
open System.Diagnostics
open System.IO

open FarseerPhysics.Common

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics



// Normal world is 16x9
let EditorToWorld = Matrix.Multiply(
    Matrix.CreateTranslation(-800.0f, -450.0f, 0.0f),
    Matrix.CreateScale(0.01f)
)

// Screen is 1 x 1
let WorldToScreen = Matrix.Multiply(
    Matrix.CreateScale(1.0f / 8.0f, -1.0f / 4.5f, 1.0f),
    Matrix.Identity
)

// Screen is 1280x80
let TouchScreenToWorld = Matrix.Multiply(

    Matrix.CreateTranslation(-640.0f, -400.0f, 0.0f),
    Matrix.CreateScale(1.0f / 80.0f, 1.0f / 80.0f, 1.0f)
)
