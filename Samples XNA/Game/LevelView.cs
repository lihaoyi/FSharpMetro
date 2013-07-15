using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics.SamplesFramework;
using Microsoft.Xna.Framework.Input.Touch;
using Windows.ApplicationModel;
using Windows.Storage;
using MyFSharp;
namespace Samples_XNA_Win8.Game
{
    public class LevelView
    {
        private Level level;

        public LevelView(Level level)
        {
            this.level = level;
        }

        public void Draw(GraphicsDevice g, GameTime gameTime)
        {
            var touchCollection = TouchPanel.GetState();
            
            foreach (var loc in touchCollection)
            {
                Debug.WriteLine(loc.Id + "\t" + Lib.TouchScreenToWorld.Transform(loc.Position));
                level.TouchEvent(
                    new TouchLocation(loc.Id, loc.State, Lib.TouchScreenToWorld.Transform(loc.Position))
                );
            }

            g.Clear(Color.DarkGreen);
            var effect = new BasicEffect(g);
            
            effect.VertexColorEnabled = true;
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                foreach(var body in level.World.BodyList)
                {
                    g.Fill(Lib.WorldToScreen, body, Color.PowderBlue);
                    g.Draw(Lib.WorldToScreen, body, Color.Blue, 0.05f);
                }
            }
        }
    }
}