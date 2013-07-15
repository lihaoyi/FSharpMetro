using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.SamplesFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Samples_XNA_Win8.Game
{
    static class Graphics
    {
        private static List<Vector2> GetVertices(Shape shape)
        {    
            var x =  (shape is CircleShape)
                ? Enumerable.Range(0, 64).Select(theta => (shape as CircleShape).Position + new Vector2(0, (shape as CircleShape).Radius).Rotate(theta * 3.14f/32)).ToList()
                : (shape as PolygonShape).Vertices;
            return x;
        }

        public static void Draw(this GraphicsDevice g, Matrix WorldToScreen, Body body, Color color, float width)
        {
            var vertices = GetVertices(body.FixtureList[0].Shape);
            
            for (var i = 0; i < vertices.Count; i++)
            {
                var j = (i + 1)%vertices.Count;
                var p1 = body.GetWorldPoint(vertices[i]);
                var p2 = body.GetWorldPoint(vertices[j]);
                var normal = (p2 - p1).Normal().Unit() * (width / 2f);
                var points = new[] {p1 + normal, p1 - normal, p2 + normal, p2 - normal};
                var outs = points.Select(x => new VertexPositionColor(WorldToScreen.Transform(x).To3(), color))
                                 .ToArray();
                g.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleStrip,
                    outs,
                    0,
                    outs.Length - 2,
                    VertexPositionColor.VertexDeclaration
                );
            }
        }

        public static void Fill(this GraphicsDevice g, Matrix WorldToScreen, Body body, Color color)
        {
            var vertices = GetVertices(body.FixtureList[0].Shape);

            var decomposed = BayazitDecomposer.ConvexPartition(new Vertices(vertices));
            foreach (var vs in decomposed)
            {
                var outs = new VertexPositionColor[vs.Count];
                for (int i = 0; i < outs.Length; i++)
                {
                    var srcIndex = i%2 == 0 ? i/2 : outs.Length - i/2 - 1;

                    var src = vs[srcIndex] + body.WorldCenter;

                    src = Matrix.CreateRotationZ(body.Rotation).Transform(src - body.WorldCenter) + body.WorldCenter;

                    src = WorldToScreen.Transform(src);

                    outs[i] = new VertexPositionColor(
                        src.To3(),
                        color
                        );
                }
                g.DrawUserPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleStrip,
                    outs,
                    0,
                    outs.Length - 2,
                    VertexPositionColor.VertexDeclaration
                );
            }
        }
    }
}
