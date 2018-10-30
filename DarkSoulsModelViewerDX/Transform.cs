using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public struct Transform
    {
        public static readonly Transform Zero
            = new Transform(Vector3.Zero, Vector3.Zero);

        public Transform(Vector3 pos, Vector3 rot)
        {
            Position = pos;
            Rotation = rot;
        }

        public Transform(float x, float y, float z, float rx, float ry, float rz)
            : this(new Vector3(x, y, z), new Vector3(rx, ry, rz))
        {

        }

        public Vector3 Position;
        public Vector3 Rotation;

        public Matrix TranslationMatrix => Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);
        public Matrix RotationMatrix => Matrix.CreateRotationY(Rotation.Y)
            * Matrix.CreateRotationZ(Rotation.Z)
            * Matrix.CreateRotationX(Rotation.X);

        public Matrix RotationMatrixXYZ => Matrix.CreateRotationX(Rotation.X)
            * Matrix.CreateRotationY(Rotation.Y)
            * Matrix.CreateRotationZ(Rotation.Z);

        public Matrix ViewMatrix => TranslationMatrix * RotationMatrix;
        public Matrix ViewMatrix_Draw => TranslationMatrix * RotationMatrix;
    }
}
