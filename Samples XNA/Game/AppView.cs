using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Samples_XNA_Win8.Game;
using MyFSharp;
namespace FarseerPhysics.SamplesFramework
{
    /// <summary>
    /// The screen manager is a component which manages one or more GameScreen
    /// instances. It maintains a stack of screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes input to the
    /// topmost active screen.
    /// </summary>
    public class AppView : DrawableGameComponent
    {
        private App app;
        private LevelView main;
        

        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public AppView(Microsoft.Xna.Framework.Game game): base(game)
        {
            this.app = new App(XElement.Load("Game/out.svg"), Lib.EditorToWorld, game);
            this.main = new LevelView(app.Level);
            Game.ResetElapsedTime();
        }

        protected override void LoadContent()
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            base.LoadContent();
        }
        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Tell each of the screens to unload their content.
        }

        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            app.Update(gameTime);
        }

        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            
            main.Draw(GraphicsDevice, gameTime);
        }
    }
}