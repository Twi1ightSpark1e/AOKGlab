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
                if (filename.EndsWith("png"))
                    bm = new Bitmap(Image.FromFile(filename));
                else if (filename.EndsWith("ico"))
                    bm = new Bitmap(Icon.ExtractAssociatedIcon(filename).ToBitmap());
                else throw new FormatException("Данный тип файла не поддерживается");
            }
            else throw new FileNotFoundException("Файл не найден");
            bm.RotateFlip(RotateFlipType.Rotate180FlipX);
            Height = bm.Height;
            Width = bm.Width;
            Pixels = new byte[Height * Width * 4];
            for (int h = 0; h < Height; h++)
            {
                for (int w = 0; w < Width; w++)
                {
                    Debug.WriteLine((Height * h + w) * 4);
                    Color pixel = bm.GetPixel(w, h);
                    Pixels[(Height * h + w) * 4] = pixel.R;
                    Pixels[(Height * h + w) * 4 + 1] = pixel.G;
                    Pixels[(Height * h + w) * 4 + 2] = pixel.B;
                    Pixels[(Height * h + w) * 4 + 3] = pixel.A;
                }
            }

            PixelUnpackBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, PixelUnpackBuffer);
            GL.BufferData(BufferTarget.PixelUnpackBuffer, Pixels.Length, Pixels, BufferUsageHint.StaticDraw);
        }

        public void Draw()
        {
            GL.DrawBuffer(DrawBufferMode.Back);
            GL.WindowPos2(MainForm.GLControlSize.Width - Width, MainForm.GLControlSize.Height - Height);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.PixelStore(PixelStoreParameter.UnpackSwapBytes, 0);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, Height);
            GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);

            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, PixelUnpackBuffer);
            GL.DrawPixels(Width, Height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }
    }
}
