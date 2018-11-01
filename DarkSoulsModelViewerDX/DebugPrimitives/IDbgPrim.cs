using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX.DebugPrimitives
{
    public interface IDbgPrim : IDisposable
    {
        Transform Transform { get; set; }
        void Draw();
        void LabelDraw();
    }
}
