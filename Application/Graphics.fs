module Graphics

open System.Linq
open FarseerPhysics.Dynamics
open FarseerPhysics.Factories
open FarseerPhysics.Collision.Shapes
open FarseerPhysics.Common.Decomposition
open FarseerPhysics.Common
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open Extensions

let getVertices(shape: Shape): seq<Vector2> = 
    let mul: float32 = 3.14f/32.0f
    match shape with
    | :? CircleShape as s ->
        [ 
            for theta in [0.0f..63.0f] -> 
            s.Position + (new Vector2(0.0f, s.Radius)).Rotate(float (theta * mul))
        ]  :> seq<Vector2>
                
    | :? PolygonShape as s -> 
        s.Vertices :> seq<Vector2>

type GraphicsDevice with
    member this.DrawFill(WorldToScreen: Matrix, body: Body, color: Color, width: float32) = 
        this.Draw(WorldToScreen, body, color, width)
        let d = 225
        this.Fill(WorldToScreen, body, new Color(int color.R + d, int color.G + d, int color.B + d))
    member this.Lines(WorldToScreen: Matrix, vertices: seq<Vector2>, color: Color, width: float32) = 
        let pairs = (Seq.append vertices [vertices.First()]).Select(fun (x: Vector2) -> x).Pairwise()

        for p1, p2 in pairs do
            
            let normal = (p2 - p1).Normal().Unit() * (width / 2.0f)
            let points = [| p1 + normal; p1 - normal; p2 + normal; p2 - normal |]
            let outs = [| for x in points -> new VertexPositionColor(WorldToScreen.Transform(x).To3(), color) |]
                                 
            this.DrawUserPrimitives<VertexPositionColor>(
                PrimitiveType.TriangleStrip,
                outs,
                0,
                outs.Length - 2,
                VertexPositionColor.VertexDeclaration
            );
    member this.Draw(WorldToScreen: Matrix, body: Body, color: Color, width: float32) = 
        for fixture in body.FixtureList do
            
            let vertices = getVertices(fixture.Shape)
                            |> Seq.map(fun x -> body.GetWorldPoint(x))

            this.Lines(WorldToScreen, vertices, color, width)
            
        
    member this.Fill(WorldToScreen: Matrix, body: Body, color: Color) = 
        for fixture in body.FixtureList do
            
            let decomposed = BayazitDecomposer.ConvexPartition(new Vertices(getVertices(fixture.Shape).ToArray()))
            for vs in decomposed do
                
                let outs = Array.zeroCreate<VertexPositionColor>(vs.Count)
                for i = 0 to outs.Length-1 do 
                
                    let srcIndex = if i%2 = 0 then i/2 else outs.Length - i/2 - 1

                    let src = vs.[srcIndex] + body.WorldCenter;
                            
                    let worldSrc = Matrix.CreateRotationZ(body.Rotation).Transform(src - body.WorldCenter) + body.WorldCenter;

                    let screenSrc = WorldToScreen.Transform(worldSrc);

                    outs.[i] <- new VertexPositionColor(
                        screenSrc.To3(),
                        color
                    )
                    ()

                
                this.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleStrip,
                    Array.rev outs,
                    0,
                    outs.Length - 2,
                    VertexPositionColor.VertexDeclaration
                );
         