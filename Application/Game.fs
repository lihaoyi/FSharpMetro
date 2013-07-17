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
    
type Paths(world: World) = 
    inherit Component()
    let path = new Dictionary<int, Vector2 * list<Body>>()
    override this.Update(g: GameTime) = 
        let touchCollection = TouchPanel.GetState();
        
        for loc in touchCollection do     
           this.HandleTouch(loc)

    member this.HandleTouch(loc: TouchLocation) = 
            
        match loc.State with
        | TouchLocationState.Pressed ->                
            path.[loc.Id] <- (Misc.TouchScreenToWorld.Transform(loc.Position), [])

        | TouchLocationState.Moved ->
            let last, p = path.[loc.Id]
            Debug.WriteLine(last)
            let pos = Misc.TouchScreenToWorld.Transform(loc.Position)

            
            let noCollisions = 
                [pos; last].Select(world.TestPoint)
                           .All(fun x -> x = null || x.CollisionGroup = -1s)
            let newBody = 
                if noCollisions then
                    let body = BodyFactory.CreateEdge(world, last, pos)
                    body.CollisionGroup <- -1s
                    [body]
                else
                    []

            path.[loc.Id] <- (pos, newBody @ p)
                
            ()
                
        | TouchLocationState.Released ->
            let last, bodies = path.[loc.Id]
            for body in world.BodyList do
                body.Awake <- true

            for body in bodies do
                    
                world.RemoveBody(body)
                    
            path.Remove(loc.Id)
            ()

    override this.Draw(g: GraphicsDevice, gameTime: GameTime) = 
        for last, body in path.Values do 
                for line in body do
                    let shape = line.FixtureList.[0].Shape :?> EdgeShape
                    g.Lines(Misc.WorldToScreen, [shape.Vertex1; shape.Vertex2], Color.Yellow, 0.1f)

type Level(world: World,
           statics: list<Body>,
           dynamics: list<Body>,
           player: Body) =
    inherit Component()
    let paths = new Paths(world)
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