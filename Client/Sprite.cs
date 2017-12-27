using System;
using System.Drawing;
using System.IO;

using OpenTK.Graphics.OpenGL;

namespace Client
{
    class Sprite
    {
        public byte[] Pixels { get; private set; } // пиксели спрайта
        public int Width { get; private set; } // ширина изображения
        public int Height { get; private set; } // высота изображения
        public int PixelUnpackBuffer { get; private set; } // PixelBufferObject ID
        public const float ImageHeight = 64f; // высота выводимого спрайта - каждый спрайт уменьшается до этих размеров

        public Sprite(string filename, bool isTexture)
        {
            Bitmap bm;
            if (File.Exists(filename))
            {
                if (filename.EndsWith("png") || filename.EndsWith("bmp")) // для PNG и BMP используем класс Image
                    bm = new Bitmap(Image.FromFile(filename));
                else if (filename.EndsWith("ico")) // для ICO используем класс Icon
                    bm = new Bitmap(new Icon(filename, new Size(64, 64)).ToBitmap());
                else throw new FormatException("Данный тип файла не поддерживается");
                // создаем превью спрайта с заданным в ImageHeight размером
                if (isTexture)
                    bm = new Bitmap(bm.GetThumbnailImage((int)(bm.Width / (bm.Height / ImageHeight)), 64, new Image.GetThumbnailImageAbort(new Func<bool>(() => { return false; })), IntPtr.Zero));
            }
            else throw new FileNotFoundException("Файл не найден");
            bm.RotateFlip(RotateFlipType.Rotate180FlipX); // развернуть изображение для нормального его вывода
            bm.MakeTransparent(Color.White); // белый цвет заменяем на полностью прозрачный
            Height = bm.Height;
            Width = bm.Width;
            Pixels = new byte[Height * Width * 4];
            // считываем изображение из файла
            for (int h = 0; h < Height; h++)
            {
                for (int w = 0; w < Width; w++)
                {
                    Color pixel = bm.GetPixel(w, h);
                    Pixels[(Width * h + w) * 4] = pixel.R;
                    Pixels[(Width * h + w) * 4 + 1] = pixel.G;
                    Pixels[(Width * h + w) * 4 + 2] = pixel.B;
                    Pixels[(Width * h + w) * 4 + 3] = pixel.A;
                }
            }
            if (isTexture)
            {
                // заполняем PixelBufferObject
                PixelUnpackBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.PixelUnpackBuffer, PixelUnpackBuffer);
                GL.BufferData(BufferTarget.PixelUnpackBuffer, Pixels.Length, Pixels, BufferUsageHint.StaticDraw);
            }
        }

        public void Draw(int x, int y)
        {
            // включаем blending
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.Enable(EnableCap.Blend);
            // задаем параметры распаковки спрайта
            GL.DrawBuffer(DrawBufferMode.Back);
            GL.WindowPos2(x, y);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.PixelStore(PixelStoreParameter.UnpackSwapBytes, 1);
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, Width);
            GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
            // рисуем спрайт
            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, PixelUnpackBuffer);
            GL.DrawPixels(Width, Height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            // выключаем blending
            GL.Disable(EnableCap.Blend);
        }
    }
}
