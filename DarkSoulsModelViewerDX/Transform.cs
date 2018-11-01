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
            EulerRotation = rot;
        }

        public Transform(float x, float y, float z, float rx, float ry, float rz)
            : this(new Vector3(x, y, z), new Vector3(rx, ry, rz))
        {

        }

        public Vector3 Position;
        public Vector3 EulerRotation;

        public Matrix TranslationMatrix => Matrix.CreateTranslation(Position.X, Position.Y, Position.Z);
        public Matrix RotationMatrix => Matrix.CreateRotationY(EulerRotation.Y)
            * Matrix.CreateRotationZ(EulerRotation.Z)
            * Matrix.CreateRotationX(EulerRotation.X);

        public Matrix RotationMatrixXYZ => Matrix.CreateRotationX(EulerRotation.X)
            * Matrix.CreateRotationY(EulerRotation.Y)
            * Matrix.CreateRotationZ(EulerRotation.Z);

        public Matrix CameraViewMatrix => TranslationMatrix * RotationMatrix;
        public Matrix WorldMatrix => RotationMatrix * TranslationMatrix;

        public override string ToString()
        {
            return $"Pos: {Position.ToString()} Rot (deg): {EulerRotation.Rad2Deg().ToString()}";
        }
    }
}
