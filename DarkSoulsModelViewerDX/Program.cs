using System;

namespace DarkSoulsModelViewerDX
{

    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new MODEL_VIEWER_MAIN())
                game.Run(Microsoft.Xna.Framework.GameRunBehavior.Synchronous);
        }
    }

}
