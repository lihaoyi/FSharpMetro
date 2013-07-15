
using Microsoft.Xna.Framework;
using Samples_XNA_Win8.Game;

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
        private GraphicsDeviceManager _graphics;

        public FarseerPhysicsGame()
        {
            Window.Title = "Farseer Samples Framework";
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferMultiSampling = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;

            Content.RootDirectory = "Content";

            //new-up components and add to App.Components
            App = new AppView(this);
            Components.Add(App);

        }

        public AppView App { get; set; }

    }
}  