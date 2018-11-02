using DarkSoulsModelViewerDX.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public class WorldView
    {
        public Transform CameraTransform;
        public Transform CameraOrigin;
        public Transform CameraPositionDefault;

        public float OrbitCamDistance = 0;
        public bool IsOrbitCam = false;

        public Vector3 LightRotation = Vector3.Zero;
        public Vector3 LightDirectionVector => 
            Vector3.Transform(Vector3.Forward,
            Matrix.CreateRotationY(LightRotation.Y)
            * Matrix.CreateRotationZ(LightRotation.Z)
            * Matrix.CreateRotationX(LightRotation.X)
            );

        public Matrix MatrixWorld;
        public Matrix MatrixProjection;

        public float FieldOfView = 43;
        public float NearClipDistance = 0.1f;
        public float FarClipDistance = 10000;
        public float CameraTurnSpeedGamepad = 1.5f;
        public float CameraTurnSpeedMouse = 1.5f;
        public float CameraMoveSpeed = 10;

        public float GetDistanceSquaredFromCamera(Transform t)
        {
            return (t.Position - GetCameraPhysicalLocation().Position).LengthSquared();
        }

        public byte GetLOD(Transform modelTransform)
        {
            if (GFX.ForceLOD >= 0)
                return (byte)GFX.ForceLOD;
            else
            {
                var distSquared = GetDistanceSquaredFromCamera(modelTransform);
                if (distSquared >= (GFX.LOD2Distance * GFX.LOD2Distance))
                    return (byte)2;
                else if (distSquared >= (GFX.LOD1Distance * GFX.LOD1Distance))
                    return (byte)1;
                else
                    return (byte)0;
            }
        }

        public void ApplyViewToShader<T>(IGFXShader<T> shader)
            where T : Effect
        {
            shader.ApplyWorldView(MatrixWorld, CameraTransform.CameraViewMatrix, MatrixProjection);
        }

        public void ApplyViewToShader<T>(IGFXShader<T> shader, Transform modelTransform)
            where T : Effect
        {
            shader.ApplyWorldView(modelTransform.WorldMatrix * MatrixWorld, CameraTransform.CameraViewMatrix, MatrixProjection);
        }

        public bool IsInFrustum(BoundingBox objBounds, Transform objTransform)
        {
            if (!GFX.EnableFrustumCulling)
                return true;
            return new BoundingFrustum(CameraTransform.CameraViewMatrix * MatrixProjection)
                .Intersects(new BoundingBox(
                    Vector3.Transform(objBounds.Min, objTransform.WorldMatrix),
                    Vector3.Transform(objBounds.Max, objTransform.WorldMatrix)
                    ));
        }

        public Vector3 ROUGH_GetPointOnFloor(Vector3 pos, Vector3 dir, float stepDist)
        {
            Vector3 result = pos;
            Vector3 nDir = Vector3.Normalize(dir);
            while (result.Y > 0)
            {
                if (result.Y >= 1)
                    result += nDir * 1;
                else
                    result += nDir * stepDist;
            }
            result.Y = 0;
            return result;
        }

        public Transform GetSpawnPointFromScreenPos(Vector2 screenPos, float distance, bool faceBackwards, bool lockPitch, bool alignToFloor)
        {
            var result = new Transform();
            var point1 = GFX.Device.Viewport.Unproject(
                new Vector3(screenPos, 0),
                MatrixProjection, CameraTransform.CameraViewMatrix, MatrixWorld);

            var point2 = GFX.Device.Viewport.Unproject(
                new Vector3(screenPos, 0.5f),
                MatrixProjection, CameraTransform.CameraViewMatrix, MatrixWorld);



            var directionVector = Vector3.Normalize(point2 - point1);

            //If align to floor is requested, the camera is looking downward, and the camera is above the floor
            if (alignToFloor && directionVector.Y < 0 && point1.Y > 0)
            {
                result.Position = ROUGH_GetPointOnFloor(point1, directionVector, 0.05f);
            }
            else
            {
                result.Position = point1 + (directionVector * distance);
            }

            if (faceBackwards)
                directionVector = -directionVector;

            result.EulerRotation.Y = (float)Math.Atan2(directionVector.X, directionVector.Z);
            result.EulerRotation.X = lockPitch ? 0 : (float)Math.Asin(directionVector.Y);
            result.EulerRotation.Z = 0;

            return result;
        }

        public Transform GetSpawnPointInFrontOfCamera(float distance, bool faceBackwards, bool lockPitch, bool alignToFloor)
        {
            return GetSpawnPointFromScreenPos(new Vector2(GFX.Device.Viewport.Width * 0.5f, GFX.Device.Viewport.Height * 0.5f),
                distance, faceBackwards, lockPitch, alignToFloor);
        }

        public Transform GetSpawnPointFromMouseCursor(float distance, bool faceBackwards, bool lockPitch, bool alignToFloor)
        {
            var mouse = Mouse.GetState();
            return GetSpawnPointFromScreenPos(mouse.Position.ToVector2() - GFX.Device.Viewport.Bounds.Location.ToVector2(),
                distance, faceBackwards, lockPitch, alignToFloor);
        }

        public Transform GetCameraPhysicalLocation()
        {
            var result = new Transform();
            var point1 = GFX.Device.Viewport.Unproject(
                new Vector3(GFX.Device.Viewport.Width * 0.5f,
                GFX.Device.Viewport.Height * 0.5f, 0),
                MatrixProjection, CameraTransform.CameraViewMatrix, MatrixWorld);

            var point2 = GFX.Device.Viewport.Unproject(
                new Vector3(GFX.Device.Viewport.Width * 0.5f,
                GFX.Device.Viewport.Height * 0.5f, 0.5f),
                MatrixProjection, CameraTransform.CameraViewMatrix, MatrixWorld);

            result.Position = point1;

            var directionVector = Vector3.Normalize(point2 - point1);
            result.EulerRotation.Y = (float)Math.Atan2(directionVector.X, directionVector.Z);
            result.EulerRotation.X = (float)Math.Asin(directionVector.Y);
            result.EulerRotation.Z = 0;

            return result;
        }

        public void SetCameraLocation(Vector3 pos, Vector3 rot)
        {
            CameraTransform.Position = pos;
            CameraTransform.EulerRotation = rot;
        }

        private float GetDistanceFromCam(Vector3 location)
        {
            return (location - ScreenPointToWorld(Vector2.One / 2)).Length();
        }

        public Vector3 ScreenPointToWorld(Vector2 screenPoint, float depth = 0)
        {
            return GFX.Device.Viewport.Unproject(
                new Vector3(GFX.Device.Viewport.Width * screenPoint.X, 
                GFX.Device.Viewport.Height * screenPoint.Y, depth), 
                MatrixProjection, CameraTransform.CameraViewMatrix, MatrixWorld);
        }

        public void UpdateMatrices(GraphicsDevice d)
        {
            MatrixWorld = Matrix.CreateRotationY(MathHelper.Pi)
                * Matrix.CreateTranslation(0, 0, 0)
                * Matrix.CreateScale(-1, 1, 1)
                // * Matrix.Invert(CameraOrigin.ViewMatrix)
                ;

            MatrixProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfView),
                    (float)d.Viewport.Width / (float)d.Viewport.Height, NearClipDistance, FarClipDistance);
        }

        public void MoveCamera(float x, float y, float z, float speed)
        {
            CameraTransform.Position += Vector3.Transform(new Vector3(-x, -y, z),
                Matrix.CreateRotationX(-CameraTransform.EulerRotation.X)
                * Matrix.CreateRotationY(-CameraTransform.EulerRotation.Y)
                * Matrix.CreateRotationZ(-CameraTransform.EulerRotation.Z)
                ) * speed;
        }

        public void RotateCameraOrbit(float h, float v, float speed)
        {
            CameraTransform.EulerRotation.Y -= h * speed;
            CameraTransform.EulerRotation.X += v * speed;
            CameraTransform.EulerRotation.Z = 0;
        }

        public void MoveCamera_OrbitOriginVertical(float y, float speed)
        {
            CameraTransform.Position.Y -= y * speed;
            CameraOrigin.Position.Y -= y * speed;
        }

        public void PointCameraToLocation(Vector3 location)
        {
            var newLookDir = Vector3.Normalize(location - (CameraTransform.Position));
            CameraTransform.EulerRotation.Y = (float)Math.Atan2(newLookDir.X, newLookDir.Z);
            CameraTransform.EulerRotation.X = (float)Math.Asin(newLookDir.Y);
            CameraTransform.EulerRotation.Z = 0;
        }


        private Vector2 mousePos = Vector2.Zero;
        private Vector2 oldMouse = Vector2.Zero;
        private int oldWheel = 0;
        private bool currentMouseClick = false;
        private bool oldMouseClick = false;
        //軌道カムトグルキー押下
        bool oldOrbitCamToggleKeyPressed = false;
        //非常に悪いカメラピッチ制限    ファトキャット
        const float SHITTY_CAM_PITCH_LIMIT_FATCAT = 0.999f;
        //非常に悪いカメラピッチ制限リミッタ    ファトキャット
        const float SHITTY_CAM_PITCH_LIMIT_FATCAT_CLAMP = 0.999f;
        const float SHITTY_CAM_ZOOM_MIN_DIST = 0.2f;

        private float GetGamepadTriggerDeadzone(float t, float d)
        {
            if (t < d)
                return 0;
            else if (t >= 1)
                return 0;

            return (t - d) * (1.0f / (1.0f - d));
        }

        public void UpdateInput(Main game, GameTime gameTime)
        {
            var gamepad = GamePad.GetState(PlayerIndex.One);

            MouseState mouse = Mouse.GetState();
            mousePos = new Vector2((float)mouse.X, (float)mouse.Y);
            KeyboardState keyboard = Keyboard.GetState();
            int currentWheel = mouse.ScrollWheelValue;

            bool mouseInWindow = mousePos.X > 0 && mousePos.X < game.ClientBounds.Width && mousePos.Y > 0 && mousePos.Y < game.ClientBounds.Height;

            currentMouseClick = mouse.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && mouseInWindow;

            bool isSpeedupKeyPressed = keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl);
            bool isSlowdownKeyPressed = keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);
            bool isResetKeyPressed = false;// keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.R);
            bool isMoveLightKeyPressed = keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space);
            bool isOrbitCamToggleKeyPressed = false;// keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F);
            bool isPointCamAtObjectKeyPressed = false;// keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.T);
            

            if (gamepad.IsConnected)
            {
                if (gamepad.IsButtonDown(Buttons.LeftShoulder))
                    isSlowdownKeyPressed = true;
                if (gamepad.IsButtonDown(Buttons.RightShoulder))
                    isSpeedupKeyPressed = true;
                //if (gamepad.IsButtonDown(Buttons.LeftStick))
                //    isResetKeyPressed = true;
                if (gamepad.IsButtonDown(Buttons.LeftStick))
                    isMoveLightKeyPressed = true;
                //if (gamepad.IsButtonDown(Buttons.DPadDown))
                //    isOrbitCamToggleKeyPressed = true;
                //if (gamepad.IsButtonDown(Buttons.RightStick))
                //    isPointCamAtObjectKeyPressed = true;
            }

            

            if (isResetKeyPressed)
            {
                SetCameraLocation(CameraPositionDefault.Position, Vector3.Zero);
                CameraTransform.Position = CameraPositionDefault.Position;
                CameraOrigin.Position.Y = CameraPositionDefault.Position.Y;
                OrbitCamDistance = (CameraOrigin.Position - (CameraTransform.Position)).Length();
                CameraTransform.EulerRotation = Vector3.Zero;
                LightRotation = Vector3.Zero;
            }

            if (isOrbitCamToggleKeyPressed && !oldOrbitCamToggleKeyPressed)
            {
                if (!IsOrbitCam)
                {
                    CameraOrigin.Position.Y = CameraPositionDefault.Position.Y;
                    OrbitCamDistance = (CameraOrigin.Position - (CameraTransform.Position)).Length();
                }
                IsOrbitCam = !IsOrbitCam;
            }

            if (isPointCamAtObjectKeyPressed)
            {
                PointCameraToLocation(CameraPositionDefault.Position);
            }

            float moveMult = (float)gameTime.ElapsedGameTime.TotalSeconds * CameraMoveSpeed;

            if (isSpeedupKeyPressed)
            {
                moveMult *= 10f;
            }

            if (isSlowdownKeyPressed)
            {
                moveMult /= 100f;
            }

            var cameraDist = CameraOrigin.Position - CameraTransform.Position;

            if (gamepad.IsConnected)
            {
                var lt = GetGamepadTriggerDeadzone(gamepad.Triggers.Left, 0.1f);
                var rt = GetGamepadTriggerDeadzone(gamepad.Triggers.Right, 0.1f);


                if (IsOrbitCam && !isMoveLightKeyPressed)
                {
                    float camH = gamepad.ThumbSticks.Left.X * (float)1.5f
                        * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    float camV = gamepad.ThumbSticks.Left.Y * (float)1.5f
                        * (float)gameTime.ElapsedGameTime.TotalSeconds;




                    //DEBUG($"{(CameraTransform.Rotation.X / MathHelper.PiOver2)}");
                    if (CameraTransform.EulerRotation.X >= MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT)
                    {
                        //DEBUG("UPPER CAM LIMIT");
                        camV = Math.Min(camV, 0);
                    }
                    if (CameraTransform.EulerRotation.X <= -MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT)
                    {
                        //DEBUG("LOWER CAM LIMIT");
                        camV = Math.Max(camV, 0);
                    }

                    RotateCameraOrbit(camH, camV, MathHelper.PiOver2);

                    var zoom = gamepad.Triggers.Right - gamepad.Triggers.Left;

                    if (Math.Abs(cameraDist.Length()) <= SHITTY_CAM_ZOOM_MIN_DIST)
                    {
                        zoom = Math.Min(zoom, 0);
                    }


                    OrbitCamDistance -= zoom * moveMult;




                    //PointCameraToModel();
                    MoveCamera_OrbitOriginVertical(gamepad.ThumbSticks.Right.Y, moveMult);
                }
                else
                {
                    float camH = gamepad.ThumbSticks.Right.X * (float)1.5f * CameraTurnSpeedGamepad
                            * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    float camV = gamepad.ThumbSticks.Right.Y * (float)1.5f * CameraTurnSpeedGamepad
                        * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (isMoveLightKeyPressed)
                    {
                        LightRotation.Y += camH;
                        LightRotation.X -= camV;
                    }
                    else
                    {
                        MoveCamera(gamepad.ThumbSticks.Left.X, gamepad.Triggers.Right - gamepad.Triggers.Left, gamepad.ThumbSticks.Left.Y, moveMult);



                        CameraTransform.EulerRotation.Y += camH;
                        CameraTransform.EulerRotation.X -= camV;
                    }
                }


            }




            if (IsOrbitCam)
            {
                if (game.IsActive)
                {
                    float z = 0;
                    float y = 0;

                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W) && Math.Abs(cameraDist.Length()) > 0.1f)
                        z += 1;
                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                        z -= 1;
                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.E))
                        y += 1;
                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Q))
                        y -= 1;


                    if (Math.Abs(cameraDist.Length()) <= SHITTY_CAM_ZOOM_MIN_DIST)
                    {
                        z = Math.Min(z, 0);
                    }

                    OrbitCamDistance -= z * moveMult;

                    MoveCamera_OrbitOriginVertical(y, moveMult);
                }
            }
            else
            {
                if (game.IsActive)
                {
                    float x = 0;
                    float y = 0;
                    float z = 0;

                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                        x += 1;
                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
                        x -= 1;
                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.E))
                        y += 1;
                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Q))
                        y -= 1;
                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
                        z += 1;
                    if (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                        z -= 1;

                    MoveCamera(x, y, z, moveMult);
                }
            }


            //if (isToggleAllSubmeshKeyPressed && !prev_isToggleAllSubmeshKeyPressed)
            //{
            //    game.ModelListWindow.TOGGLE_ALL_SUBMESH();
            //}

            //if (isToggleAllDummyKeyPressed && !prev_isToggleAllDummyKeyPressed)
            //{
            //    game.ModelListWindow.TOGGLE_ALL_DUMMY();
            //}

            //if (isToggleAllBonesKeyPressed && !prev_isToggleAllBonesKeyPressed)
            //{
            //    game.ModelListWindow.TOGGLE_ALL_BONES();
            //}

            if (game.IsActive)
            {
                if (currentMouseClick)
                {
                    if (!oldMouseClick)
                    {
                        game.IsMouseVisible = false;
                        oldMouse = mousePos;
                        Mouse.SetPosition(game.ClientBounds.Width / 2, game.ClientBounds.Height / 2);
                        mousePos = new Vector2(game.ClientBounds.Width / 2, game.ClientBounds.Height / 2);
                        //oldMouseClick = true;
                        //return;
                    }
                    game.IsMouseVisible = false;
                    Vector2 mouseDelta = mousePos - new Vector2((float)(game.ClientBounds.Width / 2), (float)(game.ClientBounds.Height / 2));

                    float camH = mouseDelta.X * 0.025f * CameraTurnSpeedMouse * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    float camV = mouseDelta.Y * -0.025f * CameraTurnSpeedMouse * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (IsOrbitCam && !isMoveLightKeyPressed)
                    {
                        if (CameraTransform.EulerRotation.X >= MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT)
                        {
                            camV = Math.Min(camV, 0);
                        }
                        if (CameraTransform.EulerRotation.X <= -MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT)
                        {
                            camV = Math.Max(camV, 0);
                        }

                        RotateCameraOrbit(camH, camV, MathHelper.PiOver2);
                        //PointCameraToModel();
                    }
                    else if (isMoveLightKeyPressed)
                    {
                        LightRotation.Y += camH;
                        LightRotation.X -= camV;
                    }
                    else
                    {
                        CameraTransform.EulerRotation.Y += camH;
                        CameraTransform.EulerRotation.X -= camV;
                    }


                    //CameraTransform.Rotation.Z -= (float)Math.Cos(MathHelper.PiOver2 - CameraTransform.Rotation.Y) * camV;

                    //RotateCamera(mouseDelta.Y * -0.01f * (float)moveMult, 0, 0, moveMult);
                    //RotateCamera(0, mouseDelta.X * 0.01f * (float)moveMult, 0, moveMult);

                    Mouse.SetPosition(game.ClientBounds.Width / 2, game.ClientBounds.Height / 2);
                }
                else
                {
                    if (oldMouseClick)
                    {
                        Mouse.SetPosition((int)oldMouse.X, (int)oldMouse.Y);
                    }
                    game.IsMouseVisible = true;
                }
            }
            else
            {
                game.IsMouseVisible = true;
            }

            

            if (IsOrbitCam)
            {
                //DEBUG("Dist:" + ORBIT_CAM_DISTANCE);
                //DEBUG("AngX:" + CameraTransform.Rotation.X / MathHelper.Pi + " PI");
                //DEBUG("AngY:" + CameraTransform.Rotation.Y / MathHelper.Pi + " PI");
                //DEBUG("AngZ:" + CameraTransform.Rotation.Z / MathHelper.Pi + " PI");

                CameraTransform.EulerRotation.X = MathHelper.Clamp(CameraTransform.EulerRotation.X, -MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT_CLAMP, MathHelper.PiOver2 * SHITTY_CAM_PITCH_LIMIT_FATCAT_CLAMP);

                OrbitCamDistance = Math.Max(OrbitCamDistance, SHITTY_CAM_ZOOM_MIN_DIST);

                var distanceVectorAfterMove = Vector3.Transform(Vector3.Forward, CameraTransform.RotationMatrixXYZ * Matrix.CreateRotationY(MathHelper.Pi)) * new Vector3(1, 1, 1);
                CameraTransform.Position = (Vector3.Zero + (distanceVectorAfterMove * OrbitCamDistance));
            }
            else
            {
                CameraTransform.EulerRotation.X = MathHelper.Clamp(CameraTransform.EulerRotation.X, -MathHelper.PiOver2, MathHelper.PiOver2);
            }


            LightRotation.X = MathHelper.Clamp(LightRotation.X, -MathHelper.PiOver2, MathHelper.PiOver2);
            oldWheel = currentWheel;

            //prev_isToggleAllSubmeshKeyPressed = isToggleAllSubmeshKeyPressed;
            //prev_isToggleAllDummyKeyPressed = isToggleAllDummyKeyPressed;
            //prev_isToggleAllBonesKeyPressed = isToggleAllBonesKeyPressed;

            oldMouseClick = currentMouseClick;

            oldOrbitCamToggleKeyPressed = isOrbitCamToggleKeyPressed;

            
        }
    }
}
