using MeowDSIO;
using MeowDSIO.DataFiles;
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
    }
    public class TextureFetchRequest
    {
        private static object _lock_Texture_IO = new object();

        public TextureFetchRequestType FetchType;
        public string FetchUri;
        public string TexName;
        private Texture2D CachedTexture;

        public void Flush()
        {
            CachedTexture?.Dispose();
            CachedTexture = null;
        }

        public TextureFetchRequest(TextureFetchRequestType type, string uri, string texName)
        {
            FetchType = type;
            FetchUri = uri;
            TexName = texName;
        }

        private byte[] FetchBytes()
        {
            lock (_lock_Texture_IO)
            {
                switch (FetchType)
                {
                    case TextureFetchRequestType.EntityBnd:
                        var texData = FLVEROptimized.ReadTextureDataFromBnd(FetchUri, 0);
                        return texData[TexName];
                    case TextureFetchRequestType.Tpf:
                        var tpf = DataFile.LoadFromFile<TPF>(FetchUri);
                        foreach (var t in tpf)
                        {
                            if (t.Name == TexName)
                            {
                                return t.DDSBytes;
                            }
                        }
                        return null;
                }
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

            var bytes = FetchBytes();
            if (bytes == null)
                return null;

            using (var tempStream = new MemoryStream(bytes))
            {
                //var dds = TeximpNet.Surface.LoadFromStream(tempStream);

                using (DSBinaryReader bin = new DSBinaryReader(TexName, tempStream))
                {
                    bin.Position += 4;
                    int headerSize = bin.ReadInt32();
                    int flags = bin.ReadInt32();
                    int height = bin.ReadInt32();
                    int width = bin.ReadInt32();

                    bin.Position += (4 * 2);

                    int mipmapCount = bin.ReadInt32();

                    bin.Position += (4 * 13);

                    string ddsType = bin.ReadStringAscii(4);

                    var surfaceFormat = GetSurfaceFormatFromString(ddsType);

                    bin.Position += (4 * 10);

                    Texture2D tex = new Texture2D(GFX.Device, width, height, true, surfaceFormat);

                    for (int i = 0; i < mipmapCount; i++)
                    {
                        int numTexels = GetNextMultipleOf4(width >> i) * GetNextMultipleOf4(height >> i);
                        if (surfaceFormat == SurfaceFormat.Dxt1)
                            numTexels /= 2;
                        byte[] thisMipMap = bin.ReadBytes(numTexels);
                        tex.SetData(i, 0, null, thisMipMap, 0, numTexels);
                        thisMipMap = null;
                    }

                    CachedTexture = tex;
                }
            }
            return CachedTexture;

        }

       
    }
}
