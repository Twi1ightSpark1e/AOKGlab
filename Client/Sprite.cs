using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Client
{
    class Sprite
    {
        public byte[] Pixels { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int PixelUnpackBuffer { get; private set; }

        public Sprite(string filename)
        {
            Bitmap bm;
            if (File.Exists(filename))
            {
                if (filename.EndsWith("png") || filename.EndsWith("bmp"))
                    bm = new Bitmap(Image.FromFile(filename));
                else if (filename.EndsWith("ico"))
                    bm = new Bitmap(new Icon(filename, new Size(64, 64)).ToBitmap());
                else throw new FormatException("Данный тип файла не поддерживается");
            }
            else throw new FileNotFoundException("Файл не найден");
            bm.RotateFlip(RotateFlipType.Rotate180FlipX);
            bm.MakeTransparent(Color.White);
            Height = bm.Height;
            Width = bm.Width;
            Pixels = new byte[Height * Width * 4];
            for (int h = 0; h < Height; h++)
            {
                for (int w = 0; w < Width; w++)
                {
                    Debug.WriteLine((Height * w + h) * 4);
                    Color pixel = bm.GetPixel(w, h);
                    Pixels[(Width * h + w) * 4] = pixel.R;
                    Pixels[(Width * h + w) * 4 + 1] = pixel.G;
                    Pixels[(Width * h + w) * 4 + 2] = pixel.B;
                    Pixels[(Width * h + w) * 4 + 3] = pixel.A;
                }
            }

            PixelUnpackBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, PixelUnpackBuffer);
            GL.BufferData(BufferTarget.PixelUnpackBuffer, Pixels.Length, Pixels, BufferUsageHint.StaticDraw);
        }

        public void Draw(int x, int y)
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.Blend);

            GL.DrawBuffer(DrawBufferMode.Back);
            GL.WindowPos2(x, y);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.PixelStore(PixelStoreParameter.UnpackSwapBytes, 1);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, Width);
            GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);

            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, PixelUnpackBuffer);
            GL.DrawPixels(Width, Height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            GL.Disable(EnableCap.Blend);
        }
    }
}
