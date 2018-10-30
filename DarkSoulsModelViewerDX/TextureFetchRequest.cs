using MeowDSIO;
using MeowDSIO.DataFiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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
        public TextureFetchRequestType FetchType;
        public string FetchUri;
        public string TexName;
        private Texture2D CachedTexture;

        public TextureFetchRequest(TextureFetchRequestType type, string uri, string texName)
        {
            FetchType = type;
            FetchUri = uri;
            TexName = texName;
        }

        private byte[] FetchBytes()
        {
            switch (FetchType)
            {
                case TextureFetchRequestType.EntityBnd:
                    var entityBnd = DataFile.LoadFromFile<EntityBND>(FetchUri);
                    foreach (var m in entityBnd.Models)
                    {
                        foreach (var t in m.Textures)
                        {
                            if (MiscUtil.GetFileNameWithoutDirectoryOrExtension(t.Key) == 
                                MiscUtil.GetFileNameWithoutDirectoryOrExtension(TexName))
                            {
                                return t.Value;
                            }
                        }
                    }
                    return null;
                case TextureFetchRequestType.Tpf:
                    var tpf = DataFile.LoadFromFile<TPF>(FetchUri);
                    foreach (var t in tpf)
                    {
                        if (MiscUtil.GetFileNameWithoutDirectoryOrExtension(t.Name) ==
                                MiscUtil.GetFileNameWithoutDirectoryOrExtension(TexName))
                        {
                            return t.DDSBytes;
                        }
                    }
                    return null;
            }
            return null;
        }

        public Texture2D Fetch()
        {
            if (CachedTexture != null)
                return CachedTexture;

            var bytes = FetchBytes();
            if (bytes == null)
                return null;

            var dds = Pfim.Dds.Create(bytes, new Pfim.PfimConfig());

            CachedTexture = new Texture2D(GFX.Device, dds.Width, dds.Height, false, SurfaceFormat.Color);
            Color[] data = new Color[dds.Width * dds.Height];

            if (dds.Format == Pfim.ImageFormat.Rgb24)
            {
                for (int i = 0; i < dds.Width; i++)
                {
                    for (int j = 0; j < dds.Height; j++)
                    {
                        byte r = dds.Data[(((dds.Width * j) + i) * dds.BytesPerPixel) + 2];
                        byte g = dds.Data[(((dds.Width * j) + i) * dds.BytesPerPixel) + 1];
                        byte b = dds.Data[(((dds.Width * j) + i) * dds.BytesPerPixel) + 0];
                        data[((dds.Width * j) + i)] = new Color(r, g, b, (byte)255);
                    }
                }
            }
            else
            {
                //TODO
                return null;
            }

            CachedTexture.SetData<Color>(data);

            bytes = null;
            dds = null;
            data = null;

            GC.Collect();

            return CachedTexture;

        }
    }
}
