using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ACParticle.Render
{
    public class TextureCache
    {
        public static Dictionary<uint, Texture2D> Textures;

        static TextureCache()
        {
            Textures = new Dictionary<uint, Texture2D>();
        }

        public static Texture2D LoadTexture(Surface surface, uint textureID)
        {
            var isClipMap = surface.Type.HasFlag(SurfaceType.Base1ClipMap);

            var texture = DatManager.PortalDat.ReadFromDat<RenderSurface>(textureID);
            if (texture.SourceData == null)
                texture = DatManager.HighResDat.ReadFromDat<RenderSurface>(textureID);

            var surfaceFormat = SurfaceFormat.Color;
            switch (texture.Format)
            {
                case SurfacePixelFormat.PFID_DXT1:
                    surfaceFormat = SurfaceFormat.Dxt1;
                    break;
                case SurfacePixelFormat.PFID_DXT3:
                    surfaceFormat = SurfaceFormat.Dxt3;
                    break;
                case SurfacePixelFormat.PFID_DXT5:
                    //if (!isClipMap)
                    surfaceFormat = SurfaceFormat.Dxt5;
                    break;
                case SurfacePixelFormat.PFID_CUSTOM_LSCAPE_ALPHA:
                case SurfacePixelFormat.PFID_A8:
                case SurfacePixelFormat.PFID_P8:    // indexed color
                    surfaceFormat = SurfaceFormat.Alpha8;
                    break;
            }

            var width = texture.Width;
            var height = texture.Height;

            var data = texture.SourceData;
            if (surfaceFormat == SurfaceFormat.Color)
            {
                switch (texture.Format)
                {
                    case SurfacePixelFormat.PFID_R8G8B8:
                    case SurfacePixelFormat.PFID_CUSTOM_LSCAPE_R8G8B8:
                        data = AddAlpha(data);
                        break;
                    case SurfacePixelFormat.PFID_INDEX16:
                        data = IndexToColor(surface, texture);
                        break;

                    case SurfacePixelFormat.PFID_CUSTOM_RAW_JPEG:
                    case SurfacePixelFormat.PFID_R5G6B5:
                    case SurfacePixelFormat.PFID_A4R4G4B4:
                        //case SurfacePixelFormat.PFID_DXT5:
                        var bitmap = texture.GetBitmap();
                        var _tex = GetTexture2DFromBitmap(ParticleViewer.Instance.GraphicsDevice, bitmap);

                        //if (isClipMap)
                        //AdjustClip(_tex);

                        return _tex;

                    case SurfacePixelFormat.PFID_A8R8G8B8:
                        ConvertToABGR(data);
                        break;
                }
            }

            var tex = new Texture2D(ParticleViewer.Instance.GraphicsDevice, texture.Width, texture.Height, false, surfaceFormat);
            tex.SetData(data);

            return tex;
        }

        public static void AdjustClip(Texture2D texture)
        {
            var colors = new Microsoft.Xna.Framework.Color[texture.Width * texture.Height];
            texture.GetData(colors);
            for (var i = 0; i < colors.Length; i++)
            {
                if (colors[i].A < 8)
                    colors[i] = new Microsoft.Xna.Framework.Color(0, 0, 0, 0);
            }
            texture.SetData(colors);
        }

        public static Texture2D GetTexture(Surface surface, uint textureID)
        {
            if (Textures.TryGetValue(textureID, out var cached))
                return cached;

            var texture = LoadTexture(surface, textureID);
            Textures.Add(textureID, texture);
            return texture;
        }

        public static SurfaceFormat GetSurfaceFormat(SurfacePixelFormat spf)
        {
            switch (spf)
            {
                case SurfacePixelFormat.PFID_INDEX16:
                    return SurfaceFormat.Color;

                case SurfacePixelFormat.PFID_A8:
                case SurfacePixelFormat.PFID_L8:
                    return SurfaceFormat.Alpha8;

                case SurfacePixelFormat.PFID_DXT1:
                    return SurfaceFormat.Dxt1;

                case SurfacePixelFormat.PFID_DXT3:
                    return SurfaceFormat.Dxt3;

                case SurfacePixelFormat.PFID_DXT5:
                    return SurfaceFormat.Dxt5;

                default:
                    return SurfaceFormat.Color;
            }
        }

        public static Texture2D GetTexture2DFromBitmap(GraphicsDevice device, Bitmap bitmap)
        {
            Texture2D tex = new Texture2D(device, bitmap.Width, bitmap.Height, false, SurfaceFormat.Color);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            int bufferSize = data.Height * data.Stride;

            //create data buffer 
            byte[] bytes = new byte[bufferSize];

            // copy bitmap data into buffer
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

            // copy our buffer to the texture
            tex.SetData(bytes);

            // unlock the bitmap data
            bitmap.UnlockBits(data);

            return tex;
        }

        public static byte[] AddAlpha(byte[] rgb)
        {
            var rgba = new byte[rgb.Length + rgb.Length / 3];

            var idx = 0;
            for (var i = 0; i < rgb.Length; i += 3)
            {
                rgba[idx++] = rgb[i + 2];
                rgba[idx++] = rgb[i + 1];
                rgba[idx++] = rgb[i];
                rgba[idx++] = 255;
            }
            return rgba;
        }

        public static void ConvertToABGR(byte[] argb)
        {
            for (var i = 0; i < argb.Length; i += 4)
            {
                var tmp = argb[i];
                argb[i] = argb[i + 2];
                argb[i + 2] = tmp;
            }
        }

        public static byte[] IndexToColor(Surface surface, RenderSurface texture)
        {
            var colors = GetColors(texture);
            var palette = DatManager.PortalDat.ReadFromDat<Palette>((uint)texture.DefaultPaletteId);
            bool isClipMap = surface.Type.HasFlag(SurfaceType.Base1ClipMap);

            // Apply any custom palette colors, if any, to our loaded palette (note, this may be all of them!)
            if (texture.CustomPaletteColors.Count > 0)
                foreach (var entry in texture.CustomPaletteColors)
                    if (entry.Key <= palette.Colors.Count)
                        palette.Colors[entry.Key] = entry.Value;

            var output = new byte[texture.Width * texture.Height * 4];

            for (int i = 0; i < texture.Height; i++)
                for (int j = 0; j < texture.Width; j++)
                {
                    int idx = (i * texture.Width) + j;
                    var color = colors[idx];
                    var paletteColor = palette.Colors[color];

                    byte a = Convert.ToByte((paletteColor & 0xFF000000) >> 24);
                    byte r = Convert.ToByte((paletteColor & 0xFF0000) >> 16);
                    byte g = Convert.ToByte((paletteColor & 0xFF00) >> 8);
                    byte b = Convert.ToByte(paletteColor & 0xFF);

                    if (isClipMap && color < 8)
                        r = g = b = a = 0;

                    output[idx * 4] = r;
                    output[idx * 4 + 1] = g;
                    output[idx * 4 + 2] = b;
                    output[idx * 4 + 3] = a;
                }

            return output;
        }

        public static List<int> GetColors(RenderSurface texture)
        {
            var colors = new List<int>();
            using (BinaryReader reader = new BinaryReader(new MemoryStream(texture.SourceData)))
            {
                for (uint y = 0; y < texture.Height; y++)
                    for (uint x = 0; x < texture.Width; x++)
                        colors.Add(reader.ReadInt16());
            }
            return colors;
        }

        public static Texture2D Get(uint gfxObjID)
        {
            Console.WriteLine($"TextureCache.Get({gfxObjID:X8})");

            var gfxObj = DatManager.PortalDat.ReadFromDat<GfxObj>(gfxObjID);

            var surfaceID = gfxObj.Surfaces[0];

            var surface = DatManager.PortalDat.ReadFromDat<Surface>(surfaceID);

            if (surface.ColorValue != 0)
            {
                // swatch
                var swatch = new Texture2D(ParticleViewer.Instance.GraphicsDevice, 1, 1);
                var a = surface.ColorValue >> 24;
                var r = (surface.ColorValue >> 16) & 0xFF;
                var g = (surface.ColorValue >> 8) & 0xFF;
                var b = surface.ColorValue & 0xFF;
                swatch.SetData(new Microsoft.Xna.Framework.Color[] { new Microsoft.Xna.Framework.Color(r, g, b, a) });
                return swatch;
            }

            var surfaceTexture = DatManager.PortalDat.ReadFromDat<SurfaceTexture>(surface.OrigTextureId);

            return LoadTexture(surface, surfaceTexture.Textures[0]);
        }
    }
}
