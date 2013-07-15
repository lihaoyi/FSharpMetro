namespace MyFSharp
open System
open System.Diagnostics
open System.Collections.Generic
open FarseerPhysics.Dynamics.Joints
open FarseerPhysics.Dynamics
open Microsoft.Xna.Framework
open System.Xml.Linq
open Microsoft.Xna.Framework.Input.Touch
open Extensions

type Level(
        world: World,
        statics: list<Body>,
        dynamics: list<Body>,
        player: Body
    ) =
    

    let touchJoints = new Dictionary<int, List<FixedMouseJoint>>()

    member this.World = world
    
    member this.TouchEvent(loc: TouchLocation) = 
        match loc.State with
        | TouchLocationState.Moved when touchJoints.ContainsKey(loc.Id) ->
            Debug.WriteLine("MOVE")
            for joint in touchJoints.[loc.Id] do
                joint.WorldAnchorB <- loc.Position   
            
            ()
              
        | TouchLocationState.Released ->
            Debug.WriteLine("RELEASED")
            for joint in touchJoints.[loc.Id] do
                world.RemoveJoint(joint)

            touchJoints.Remove(loc.Id)
            
            ()
        | _ ->         
            Debug.WriteLine("INIT")
            touchJoints.[loc.Id] <- new List<FixedMouseJoint>()
            for body in world.BodyList do
                if body.Contains(loc.Position) then
                    Debug.WriteLine("A")
                    let joint = new FixedMouseJoint(body, loc.Position)
                    Debug.WriteLine("B")
                    joint.MaxForce <- 1000.0f * body.Mass
                    Debug.WriteLine("C")
                    touchJoints.[loc.Id].Add(joint)
                    Debug.WriteLine("D")
                    world.AddJoint(joint)
                    Debug.WriteLine("E")
        
    member this.Update(gameTime: GameTime) = 
        world.Step(
            Math.Min(
                float32 gameTime.ElapsedGameTime.TotalSeconds, 
                float32 (1.0f / 30.0f)
            )
        );

type App(data: XElement, matrix: Matrix, game: Game) = 

    let level = 
        let world, statics, dynamics, player = FLoad.Load matrix data
        new Level(world, statics, dynamics, player)

    member this.Level = level
    member this.Update(gameTime: GameTime) = 
        this.Level.Update(gameTime)