module Extensions
open System
open System.Collections.Generic
open System.Xml.Linq
open System.Linq
open FarseerPhysics.Dynamics
open FarseerPhysics.Factories
open FarseerPhysics.Collision.Shapes
open FarseerPhysics.Common.Decomposition
open FarseerPhysics.Common

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

let ns = "{http://www.w3.org/2000/svg}"

type IEnumerable<'T> with 
    member this.Pairwise() = 
        let rest = this.Skip(1)
        (Seq.zip this rest)

type Body with
    member this.Contains(point: Vector2) =
        this.FixtureList.Any(fun fixture ->
            let t = this.GetTransform()
            fixture.Shape.TestPoint(ref t, ref point)
        )
        
type Vector2 with
    member this.To3() = new Vector3(this.X, this.Y, 0.0f)
    member this.Rotate(theta: float) = 
        let sin = float32(Math.Sin(theta))
        let cos = float32(Math.Cos(theta))
        new Vector2(
            this.X * sin + this.Y * cos,
            this.X * cos + this.Y * -sin
        )
    member this.Normal() = new Vector2(-this.Y, this.X)
    member this.Unit() = Vector2.Normalize(this)

type Vector3 with
    member this.To2() = new Vector2(this.X, this.Y)
    
type Matrix with
    member this.Transform(v: Vector2) = 
        let x = new Vector3(v.X, v.Y, 0.0f);
        let z = Vector3.Transform(x, this);
        new Vector2(z.X, z.Y);
        
type XElement with
    member this.Attr<'T>(name) : 'T = 
        
        match this.Attribute(XName.Get name) with
            | null -> Unchecked.defaultof<'T>
            | value -> Convert.ChangeType(value.Value, typeof<'T>, null) :?> 'T

    member this.Subs(?name: String, ?id: String, ?fill: String) = 
        this.Descendants().Where(fun (e: XElement) ->
            name |> (Option.fold (fun y x -> ns + x = e.Name.ToString()) true) && 
            id |> (Option.fold (fun y x -> x = e.Attr<String>("id")) true) && 
            fill |> (Option.fold (fun y x -> x = e.Attr<String>("fill")) true)
        )

    member this.Sub(?name: String, ?id: String, ?fill: String) = 
        this.Subs(?name=name, ?id=id, ?fill=fill).Single()