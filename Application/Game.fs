module Game
open System
open System.Diagnostics
open System.Collections.Generic
open System.Xml.Linq
open System.Linq

open FarseerPhysics.Dynamics.Joints
open FarseerPhysics.Dynamics
open FarseerPhysics.Common.PolygonManipulation
open FarseerPhysics.Collision.Shapes
open FarseerPhysics.Common
open FarseerPhysics.Factories

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input.Touch
open Microsoft.Xna.Framework.Graphics

open Extensions
open Graphics

[<AbstractClass>]
type Component() = 
    abstract member Draw: GraphicsDevice * GameTime -> Unit
    abstract member Update: GameTime -> Unit
    
type Paths(world: World, player: Body) = 
    inherit Component()

    let path = new Dictionary<int, ref<Vector2> * Body>()

    override this.Update(g: GameTime) = 
        let touchCollection = TouchPanel.GetState();
            
        for loc in touchCollection do     
           this.HandleTouch(loc)
        
        let mutable force = new Vector2()
        for p, body in path.Values do
            let mutable pathForce = new Vector2()
            for fixture in body.FixtureList do
                let shape = fixture.Shape :?> EdgeShape
                if player.Contains(body.GetWorldPoint shape.Vertex1) || player.Contains(body.GetWorldPoint shape.Vertex2) then
                    let forward = (shape.Vertex2 - shape.Vertex1).Unit() * 10.0f
                    let perp = forward.Normal().Unit()
                    let suck = ((body.GetWorldPoint shape.Vertex1) - player.Position).Project(perp) * 10.0f
                    let drag = player.LinearVelocity.Project(perp) * -10.0f
                    pathForce <- forward + suck + drag
                    
            force <- force + pathForce
            player.ApplyForce(force)
        
                
        
    member this.HandleTouch(loc: TouchLocation) = 
            
        match loc.State with
        | TouchLocationState.Pressed ->                
            let pos = Misc.TouchScreenToWorld.Transform(loc.Position)
            path.[loc.Id] <- (
                ref pos, 
                BodyFactory.CreateBody(world, pos)
            )

        | TouchLocationState.Moved ->
            let last, body = path.[loc.Id]
            let pos = Misc.TouchScreenToWorld.Transform(loc.Position)
            
            if last.Value <> pos then
                let noCollisions = world.TestRay(pos, last.Value)
                
            
                if noCollisions then
                    let fixture = FixtureFactory.AttachEdge(last.Value - body.Position, pos - body.Position, body)
                
                    fixture.CollisionGroup <- -1s
                
                    ()

                last.Value <- pos
            ()
                
        | TouchLocationState.Released ->
            let last, body = path.[loc.Id]
            for body in world.BodyList do
                body.Awake <- true
        
            world.RemoveBody(body)
                    
            path.Remove(loc.Id)
            ()

    override this.Draw(g: GraphicsDevice, gameTime: GameTime) = 
        for last, body in path.Values do 
            for line in body.FixtureList do
                let shape = line.Shape :?> EdgeShape
                let points = [shape.Vertex1; shape.Vertex2].Select(fun p -> body.GetWorldPoint(ref p))
                g.Lines(Misc.WorldToScreen, points, Color.Yellow, 0.1f)

type Level(world: World,
           statics: list<Body>,
           dynamics: list<Body>,
           player: Body) =
    inherit Component()
    let paths = new Paths(world, player)

    override this.Update(gameTime: GameTime) = 
        paths.Update(gameTime)
        world.Step(
            Math.Min(
                float32 gameTime.ElapsedGameTime.TotalSeconds, 
                float32 (1.0f / 30.0f)
            )
        )
    
    override this.Draw(g: GraphicsDevice, gameTime: GameTime) = 
        g.Clear(Color.DarkGreen);
        let effect = new BasicEffect(g)
        effect.VertexColorEnabled <- true
        for pass in effect.CurrentTechnique.Passes do
            pass.Apply();
            for body in statics do 
                g.DrawFill(Misc.WorldToScreen, body, Color.DarkGray, 0.1f)
                
            for body in dynamics do 
                g.DrawFill(Misc.WorldToScreen, body, Color.Blue, 0.1f)    
                
            g.DrawFill(Misc.WorldToScreen, player, Color.Red, 0.1f)    

            paths.Draw(g, gameTime)

type App(loader: Func<String, XElement>, game: Game) = 
    inherit DrawableGameComponent(game)

    let level = 
        let data = loader.Invoke("Level/Test.svg")
        let world, statics, dynamics, player = Loader.Load Misc.EditorToWorld data
        game.ResetElapsedTime();
        new Level(world, statics, dynamics, player)

    member this.Level = level

    member this.Update(gameTime: GameTime) = 
        this.Level.Update(gameTime)

    override this.Draw(gameTime: GameTime) = 
        level.Draw(game.GraphicsDevice, gameTime) 