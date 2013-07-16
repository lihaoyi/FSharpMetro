module Loader
open Extensions
open System
open System.Diagnostics
open System.Collections.Generic
open System.Xml.Linq
open System.Linq
open Misc
open FarseerPhysics.Dynamics.Joints
open FarseerPhysics.Dynamics
open FarseerPhysics.Common.PolygonManipulation
open FarseerPhysics.Collision.Shapes
open FarseerPhysics.Factories
open FarseerPhysics.Common
open FarseerPhysics.Common.Decomposition
open Microsoft.Xna.Framework

let fixed = "#880000"
let movable = "#008800"
let fixedPin = "#FF0000"
let movablePin = "#00FF00"
let player = "#0000FF"

let Load (EditorToWorld: Matrix) (doc: XElement) = 
    let world = new World(new Vector2(0.0f, 9.81f))
        
    let pins (fill: String) = 
        doc.Subs(name="circle", fill=fill)
           .Select(fun (x: XElement) -> new Vector2(x.Attr<float32>("cx"), x.Attr<float32>("cy")))
           .Select(fun x -> EditorToWorld.Transform(x))

    let staticPins = pins fixedPin

    let dynamicPins = pins movablePin

    let playerLoc = doc.Sub(name="circle", fill=player)
                            |> (fun x -> new Vector3(x.Attr<float32>("cx"), x.Attr<float32>("cy"), x.Attr<float32>("r")))

    let newPlayerLoc = EditorToWorld.Transform(playerLoc.To2())

    let sidePoint = EditorToWorld.Transform(newPlayerLoc + new Vector2(playerLoc.X + playerLoc.Z, playerLoc.Y))

    let newPlayerR = (sidePoint - newPlayerLoc).Length()

    let player = BodyFactory.CreateCircle(world, newPlayerR, 1.0f, newPlayerLoc)
    player.BodyType <- BodyType.Dynamic;
    
    
    let handleElem (elem: XElement) = 
        Debug.WriteLine("Handling")
        Debug.WriteLine(elem.Name.LocalName)
        match elem.Name.LocalName with
        | "rect" ->
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
        | "polygon" ->
            let rawPoints = 
                elem.Attr<String>("points")
                    .Split(' ', '\n')
                    .Select(fun (x: String) -> x.Split(','))
                    .ToArray()
            Debug.WriteLine("RawPoints")        
            Debug.WriteLine(elem.Attr<String>("points"))
            let points = [|
                for x in rawPoints do 
                    match x with 
                    | [|a; b|] -> yield EditorToWorld.Transform(new Vector2(float32 a, float32 b))
                    | _ -> ()    
            |]
            
            Debug.WriteLine("XXX")
            let b = BodyFactory.CreateCompoundPolygon(
                world,
                BayazitDecomposer.ConvexPartition(new Vertices(points)), 
                1.0f
            )
            Debug.WriteLine(b.Position)
            Debug.WriteLine("YYY")
            b


    let statics = 
        doc.Subs(fill=fixed)
                 .Select(handleElem)
                 |> List.ofSeq
        
    let dynamics = [
        for elem in doc.Subs(fill=movable) ->
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
                 
    world, statics, dynamics, player

