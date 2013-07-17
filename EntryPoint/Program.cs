
using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace FarseerPhysics.SamplesFramework
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            var factory = new MonoGame.Framework.GameFrameworkViewSource<FarseerPhysicsGame>();
            Windows.ApplicationModel.Core.CoreApplication.Run(factory);
        }
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class FarseerPhysicsGame : Microsoft.Xna.Framework.Game
    {
        public FarseerPhysicsGame()
        {
            
            Window.Title = "Farseer Samples Framework";
            var graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            
            Content.RootDirectory = "Content";
            
            graphics.DeviceCreated += (x, y) => {
                graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            };
            
            
            Components.Add(new AppView(this));
        }
    }
    public class AppView : DrawableGameComponent
    {
        private Game.App app;
        public AppView(Microsoft.Xna.Framework.Game game)
            : base(game)
        {
            app = new Game.App(x => XElement.Load(x), game);
        }
        public override void Update(GameTime gameTime)
        {
            app.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            app.Draw(gameTime);
        }
    }
}  