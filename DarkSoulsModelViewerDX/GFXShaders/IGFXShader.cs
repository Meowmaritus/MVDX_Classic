using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.GFXShaders
{
    public interface IGFXShader
    {
        void ApplyWorldView(Matrix world, Matrix view, Matrix projection);
    }
}
