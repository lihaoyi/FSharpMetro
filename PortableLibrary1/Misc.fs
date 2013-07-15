#light
namespace MyFSharp
open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Linq
open System.Xml.Linq
open FarseerPhysics.Dynamics
open FarseerPhysics.Factories
open Microsoft.Xna.Framework

module Extensions = 
    let ns = "{http://www.w3.org/2000/svg}"

    type IEnumerable<'T> with 
        member this.Pairwise() = 
            let rest = this.Skip(1)
            (Seq.zip this rest)

    type Body with
        member this.Contains(point: Vector2) =
            let shape = this.FixtureList.[0].Shape;
            let t = this.GetTransform();
            shape.TestPoint(ref t, ref point);
        
    type Vector2 with
        member this.To3() = new Vector3(this.X, this.Y, 0.0f)

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
            this.Elements().Where(fun (e: XElement) ->
                name |> (Option.fold (fun y x -> ns + x = e.Name.ToString()) true) && 
                id |> (Option.fold (fun y x -> x = e.Attr<String>("id")) true) && 
                fill |> (Option.fold (fun y x -> x = e.Attr<String>("fill")) true)
            )

        member this.Sub(?name: String, ?id: String, ?fill: String) = 
            this.Subs(?name=name, ?id=id, ?fill=fill).Single()

module FLoad = 
    open Extensions
    open System.Collections.Generic
    let Load (EditorToWorld: Matrix) (doc: XElement) = 
        let world = new World(new Vector2(0.0f, 9.81f))
        
        let dynamic = doc.Sub(id="Dynamic")

        let collision = doc.Sub(id="Collision")

        let pins (fill: String) = 
            doc.Sub(id="Pins")
               .Subs(name="circle", fill=fill)
               .Select(fun (x: XElement) -> new Vector2(x.Attr<float32>("cx"), x.Attr<float32>("cy")))
               .Select(fun x -> EditorToWorld.Transform(x))

        let staticPins = pins "#FF0000"

        let dynamicPins = pins "#0000FF"

        let playerLoc = dynamic.Sub(name="circle", fill="#0000FF")
                                |> (fun x -> new Vector3(x.Attr<float32>("cx"), x.Attr<float32>("cy"), x.Attr<float32>("r")))

        let newPlayerLoc = EditorToWorld.Transform(playerLoc.To2())

        let sidePoint = EditorToWorld.Transform(newPlayerLoc + new Vector2(playerLoc.X + playerLoc.Z, playerLoc.Y))

        let newPlayerR = (sidePoint - newPlayerLoc).Length()

        let player = BodyFactory.CreateCircle(world, newPlayerR, 1.0f, newPlayerLoc)
        player.BodyType <- BodyType.Dynamic;
    
    
        let handleElem (elem: XElement) = 
    
            
            let p1x = new Vector2(elem.Attr<float32>("x"), elem.Attr<float32>("y"));
            let p2x = p1x + new Vector2(elem.Attr<float32>("width"), elem.Attr<float32>("height"));
            let p1 = EditorToWorld.Transform(p1x);
            let p2 = EditorToWorld.Transform(p2x);
            let transform = elem.Attr<string>("transform");

            let rotation = 
                match transform with
                | null -> 0.0f
                | transform -> 
                    let nums = transform.Substring(7, transform.Length - 7 - 1).Split(' ');
                    let a = Single.Parse(nums.[0])
                    let b = Single.Parse(nums.[1])
                    let res = Math.Atan2((float a), (float b))
                    -(float32 res)
                
            let rect = BodyFactory.CreateRectangle(world, (p2 - p1).X, (p2 - p1).Y, 1.0f)
            rect.Rotation <- rotation
            rect.Position  <- (p2 + p1) / 2.0f
            rect
                

        let statics = [
            for elem in collision.Subs(name="rect", fill="#808080") ->
                handleElem elem
        ]

        let dynamics = [
            for color in ["#00FF00"; "#FF0000"] do
            for elem in collision.Subs(name="rect", fill=color) ->
                let body = handleElem elem
                body.BodyType <- BodyType.Dynamic
                body
        ]
        
        for p in staticPins do
            let hitBodies = dynamics |> Seq.filter(fun body -> body.Contains(p))
            for body in hitBodies do
                JointFactory.CreateFixedRevoluteJoint(world, body, body.GetLocalPoint(p), p);
                ()
            
        for p in dynamicPins do
            let hitBodies = dynamics |> Seq.filter(fun body -> body.Contains(p))
            for a, b in hitBodies.Pairwise() do
                JointFactory.CreateRevoluteJoint(world, a, b, b.GetLocalPoint(p))
                ()
         
        Debug.WriteLine("BODYLIST ZERO")
        Debug.WriteLine(world.BodyList.Count)
        world, statics, dynamics, player