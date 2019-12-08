using System;

using OpenTK;

namespace Client
{
    // для принятия карты проходимости с сервера
    // именно из таких структур состоит поле
    public struct MapUnit
    {
        public int x, z;
        public byte value;
    }

    // направления движения камеры
    [Flags]
    enum Directions
    {
        None = 0x1,
        Left = 0x2,
        Right = 0x4,
        Up = 0x8,
        Down = 0x10,
        Forward = 0x20,
        Backward = 0x40
    }

    // перечисление, задающее направление движения
    // положительные значения задают движение по положительному направлению оси X или Z
    // отрицательные - по отрицательному направлению оси X или Z
    enum MoveDirection
    {
        None = 0,
        Left = -2,
        Up = -1,
        Down = 1,
        Right = 2
    }

    // текущий режим освещения
    enum LightMode
    {
        All, OnlyAmbient, OnlyDiffuse, OnlySpecular
    }

    // фигуры
    enum ShapeMode
    {
        Decal = -3, Bomb, Player, Empty, LightBarrier, HeavyBarrier, Wall
    }

    // режим вывода
    enum OutputMode
    {
        Triangles, Lines
    }

    enum FilterMode
    {
        Nearest,
        Bilinear,
        BilinearMipmap,
        Trilinear,
        Anisotropic2,
        Anisotropic4,
        Anisotropic8,
        Anisotropic16,
        Anisotropic32
    }

    enum SoundType
    {
        Ambient,
        Death,
        Explosion,
        Shift
    }

    struct ParticlePoint
    {
        public Vector4 color;
        public Vector3 coord;
        public Vector2 texCoord;
    }
}
