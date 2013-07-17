module Extensions
open System
open System.Diagnostics
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
        Seq.zip this rest
type World with 
    
    member this.TestRay(p1: Vector2, p2: Vector2) = 
        
        if p1 = p2 then
            not(this.TestPointAll(p1).Any(fun f -> f.CollisionGroup <> -1s))
        else
            let res = ref true    

            // Doesn't Work???
            (*this.RayCast(
                fun (fixture: Fixture) (point: Vector2) (normal: Vector2) (fraction: float32) ->
                    Debug.WriteLine(fixture.Shape.GetType())
                    if fixture.CollisionGroup <> -1s then
                        res.Value <- false
                        
                    1.0f
                , 
                p1, 
                p2
            )*)
            
            let vertexRes = 
                [p1; p2].Select(this.TestPoint)
                        .All(fun x -> x = null || x.CollisionGroup = -1s)

            
            vertexRes
            

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
    member this.Dot(other: Vector2) = 
        this.X * other.X + this.Y * other.Y
    member this.Project(other: Vector2) = 
        this.Dot(other) * other

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