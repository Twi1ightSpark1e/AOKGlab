using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

namespace Client
{
    class Texture
    {
        private static List<(string filename, Texture result)> textures = new List<(string filename, Texture result)>();
        
        private Sprite sprite;
        public byte[] Pixels => sprite.Pixels;
        public int Width => sprite.Width;
        public int Height => sprite.Height;
        public int TextureID { get; private set; }
        public static FilterMode FilterMode { get; set; }

        public Texture(string filename)
        {
            sprite = new Sprite(filename, false);
            TextureID = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, new int[] { 1 });
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, Pixels);
        }

        public void Apply()
        {
            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            float anisotropyLevelExt = GL.GetFloat((GetPName)All.TextureMaxAnisotropyExt);
            switch (FilterMode)
            {
                case FilterMode.Nearest:
                    GL.TexParameterI(TextureTarget.Texture2D, (TextureParameterName)anisotropyLevelExt, new int[] { 0 });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.Nearest });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
                    break;
                case FilterMode.Bilinear:
                    GL.TexParameterI(TextureTarget.Texture2D, (TextureParameterName)anisotropyLevelExt, new int[] { 0 });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.Linear });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Linear });
                    break;
                case FilterMode.BilinearMipmap:
                    GL.TexParameterI(TextureTarget.Texture2D, (TextureParameterName)anisotropyLevelExt, new int[] { 0 });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.LinearMipmapNearest });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Linear});
                    break;
                case FilterMode.Trilinear:
                    GL.TexParameterI(TextureTarget.Texture2D, (TextureParameterName)anisotropyLevelExt, new int[] { 0 });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.LinearMipmapLinear });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Linear });
                    break;
                case FilterMode.Anisotropic2:
                    GL.TexParameterI(TextureTarget.Texture2D, (TextureParameterName)anisotropyLevelExt, new int[] { 2 });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.LinearMipmapLinear });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
                    break;
                case FilterMode.Anisotropic4:
                    GL.TexParameterI(TextureTarget.Texture2D, (TextureParameterName)anisotropyLevelExt, new int[] { 4 });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.LinearMipmapLinear });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
                    break;
                case FilterMode.Anisotropic8:
                    GL.TexParameterI(TextureTarget.Texture2D, (TextureParameterName)anisotropyLevelExt, new int[] { 8 });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.LinearMipmapLinear });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
                    break;
                case FilterMode.Anisotropic16:
                    GL.TexParameterI(TextureTarget.Texture2D, (TextureParameterName)anisotropyLevelExt, new int[] { 16 });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.LinearMipmapLinear });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
                    break;
                case FilterMode.Anisotropic32:
                    GL.TexParameterI(TextureTarget.Texture2D, (TextureParameterName)anisotropyLevelExt, new int[] { 32 });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.LinearMipmapLinear });
                    GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
                    break;
            }
        }

        public static void Disable()
        {
            GL.Disable(EnableCap.Texture2D);
        }

        public static Texture GetTextureByName(string name)
        {
            var search = textures.Where((texture) => texture.filename == name);
            if (search.Count() == 0)
            {
                var texture = new Texture(name);
                textures.Add((name, texture));
                return texture;
            }
            else return search.First().result;
        }
    }
}
