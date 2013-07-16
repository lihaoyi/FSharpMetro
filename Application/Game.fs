﻿module Game
open System
open System.Diagnostics
open System.Collections.Generic

open FarseerPhysics.Dynamics.Joints
open FarseerPhysics.Dynamics
open FarseerPhysics.Common.PolygonManipulation
open FarseerPhysics.Collision.Shapes
open FarseerPhysics.Common
open Microsoft.Xna.Framework
open System.Xml.Linq
open Microsoft.Xna.Framework.Input.Touch
open Extensions
open Microsoft.Xna.Framework.Graphics
open Graphics
module Dragger = 
    let touchJoints = new Dictionary<int, List<FixedMouseJoint>>()
    let TouchEvent(world: World, loc: TouchLocation) = 
        match loc.State with
        | TouchLocationState.Moved when touchJoints.ContainsKey(loc.Id) ->
            for joint in touchJoints.[loc.Id] do
                joint.WorldAnchorB <- loc.Position   
            
        | TouchLocationState.Released ->
            for joint in touchJoints.[loc.Id] do
                world.RemoveJoint(joint)
            touchJoints.Remove(loc.Id)
            ()

        | _ ->         
            touchJoints.[loc.Id] <- new List<FixedMouseJoint>()
            for body in world.BodyList do
                if body.Contains(loc.Position) then
                    let joint = new FixedMouseJoint(body, loc.Position)
                    joint.MaxForce <- 1000.0f * body.Mass
                    touchJoints.[loc.Id].Add(joint)
                    world.AddJoint(joint)

            
type Level(
        world: World,
        statics: list<Body>,
        dynamics: list<Body>,
        player: Body
    ) =

    member this.World = world
        
    member this.Update(gameTime: GameTime) = 
        let touchCollection = TouchPanel.GetState();
            
        for loc in touchCollection do
            Dragger.TouchEvent(
                world,
                new TouchLocation(
                    loc.Id, 
                    loc.State, 
                    Misc.TouchScreenToWorld.Transform(loc.Position)
                )
            )
           
        world.Step(
            Math.Min(
                float32 gameTime.ElapsedGameTime.TotalSeconds, 
                float32 (1.0f / 30.0f)
            )
        )

    member this.Draw(g: GraphicsDevice, gameTime: GameTime) = 
        g.Clear(Color.DarkGreen);
        let effect = new BasicEffect(g)
        effect.VertexColorEnabled <- true
        for pass in effect.CurrentTechnique.Passes do
            pass.Apply();
            for body in world.BodyList do
                g.Draw(Misc.WorldToScreen, body, Color.Blue, 0.1f)
                g.Fill(Misc.WorldToScreen, body, Color.PowderBlue)
                


type App(loader: Func<String, XElement>, game: Game) as this = 
    inherit DrawableGameComponent(game)

    let level = 
        let data = loader.Invoke("out.svg")
        let world, statics, dynamics, player = Loader.Load Misc.EditorToWorld data
        game.ResetElapsedTime();
        new Level(world, statics, dynamics, player)

    member this.Level = level
    member this.Update(gameTime: GameTime) = 
        this.Level.Update(gameTime)

    member this.Draw(gameTime: GameTime) = 
        level.Draw(game.GraphicsDevice, gameTime) 