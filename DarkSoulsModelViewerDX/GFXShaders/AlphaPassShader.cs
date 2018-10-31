using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.GFXShaders
{
    public class AlphaPassShader : AlphaTestEffect, IGFXShader
    {
        public AlphaPassShader(GraphicsDevice device) : base(device)
        {
        }

        public void ApplyWorldView(Matrix world, Matrix view, Matrix projection)
        {
            World = world;
            View = view;
            Projection = projection;
        }
    }
}
