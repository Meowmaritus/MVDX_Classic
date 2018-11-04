//using MeowDSIO;
//using MeowDSIO.DataFiles;
using SoulsFormats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsModelViewerDX
{
    public enum TextureFetchRequestType
    {
        EntityBnd,
        Tpf,
        TexBnd,
    }
    public class TextureFetchRequest : IDisposable
    {
        public TPF TPFReference { get; private set; }
        public string TexName;
        private Texture2D CachedTexture;
        private bool IsDX10;

        public TextureFetchRequest(TPF tpf, string texName)
        {
            TPFReference = tpf;
            TexName = texName;
        }

        private byte[] FetchBytes()
        {
            if (TPFReference.Platform == TPF.TPFPlatform.PS4)
            {
                TPFReference.ConvertPS4ToPC();
            }
            else if (TPFReference.Platform == TPF.TPFPlatform.Xbone)
            {
                // Because there are actually xbone textures in the PC version for some dumb reason
                return null;
            }

            if (TPFReference == null)
                return null;

            var matchedTextures = TPFReference.Textures.Where(x => x.Name == TexName).ToList();

            if (matchedTextures.Count > 0)
            {
                var tex = matchedTextures.First();
                var texBytes = tex.Bytes;
                
                //foreach (var match in matchedTextures)
                //{
                //    match.Bytes = null;
                //    match.Header = null;
                //    TPFReference.Textures.Remove(match);
                //}

                //if (TPFReference.Textures.Count == 0)
                //{
                //    TPFReference.Textures = null;
                //    TPFReference = null;
                //}

                return texBytes;
            }
            else
            {
                return null;
            }
        }

        private static SurfaceFormat GetSurfaceFormatFromString(string str)
        {
            switch (str)
            {
                case "DXT1":
                    return SurfaceFormat.Dxt1;
                case "DXT3":
                    return SurfaceFormat.Dxt3;
                case "DXT5":
                    return SurfaceFormat.Dxt5;
                case "ATI1":
                    return SurfaceFormat.Dxt1; // Monogame workaround :fatcat
                case "ATI2":
                    return SurfaceFormat.Dxt3;
                default:
                    throw new Exception($"Unknown DDS Type: {str}");
            }
        }

        private static int GetNextMultipleOf4(int x)
        {
            if (x % 4 != 0)
                x += 4 - (x % 4);
            else if (x == 0)
                return 4;
            return x;
        }

        public Texture2D Fetch()
        {
            if (CachedTexture != null)
                return CachedTexture;

            var textureBytes = FetchBytes();
            if (textureBytes == null)
                return null;

            DDS header = new DDS(textureBytes);
            int height = header.dwHeight;
            int width = header.dwWidth;

            int mipmapCount = header.dwMipMapCount;
            using (var br = new BinaryReaderEx(false, textureBytes))
            {
                br.Skip((int)header.dataOffset);

                SurfaceFormat surfaceFormat;
                if (header.ddspf.dwFourCC == "DX10")
                {
                    // See if there are DX9 textures
                    int fmt = (int)header.header10.dxgiFormat;
                    if (fmt == 71)
                        surfaceFormat = SurfaceFormat.Dxt1;
                    else if (fmt == 72)
                        surfaceFormat = SurfaceFormat.Dxt1;
                    else if (fmt == 73)
                        surfaceFormat = SurfaceFormat.Dxt3;
                    else if (fmt == 74)
                        surfaceFormat = SurfaceFormat.Dxt3;
                    else if (fmt == 76)
                        surfaceFormat = SurfaceFormat.Dxt5;
                    else if (fmt == 77)
                        surfaceFormat = SurfaceFormat.Dxt5;
                    else
                    {
                        // No DX10 texture support in monogame yet
                        IsDX10 = true;
                        CachedTexture = Main.DEFAULT_TEXTURE_MISSING;
                        return CachedTexture;
                    }
                }
                else
                {
                    surfaceFormat = GetSurfaceFormatFromString(header.ddspf.dwFourCC);
                }
                // Adjust width and height because from has some DXTC textures that have dimensions not a multiple of 4 :shrug:
                Texture2D tex = new Texture2D(GFX.Device, GetNextMultipleOf4(width), GetNextMultipleOf4(height), true, surfaceFormat);

                for (int i = 0; i < mipmapCount; i++)
                {
                    int numTexels = GetNextMultipleOf4(width >> i) * GetNextMultipleOf4(height >> i);
                    if (surfaceFormat == SurfaceFormat.Dxt1 || surfaceFormat == SurfaceFormat.Dxt1SRgb)
                        numTexels /= 2;
                    byte[] thisMipMap = br.ReadBytes(numTexels);
                    tex.SetData(i, 0, null, thisMipMap, 0, numTexels);
                    thisMipMap = null;
                }

                CachedTexture = tex;


                return CachedTexture;
            }
            

        }

        public void Dispose()
        {
            TPFReference = null;

            CachedTexture?.Dispose();
            CachedTexture = null;
        }
    }
}
