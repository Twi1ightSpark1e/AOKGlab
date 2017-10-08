using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UniverGraphics
{
    struct ModelPoint
    {
        Vector3 coordinates, colors;

        public ModelPoint((float x, float y, float z) coordinates, (float r, float g, float b) colors)
        {
            this.coordinates = new Vector3(coordinates.x, coordinates.y, coordinates.z);
            this.colors = new Vector3(colors.r, colors.g, colors.b);
        }

        public static int Size()
        {
            return Vector3.SizeInBytes * 2;
        }

        public static IntPtr CoordinatesOffset()
        {
            return Marshal.OffsetOf<ModelPoint>("coordinates");
        }

        public static IntPtr ColorsOffset()
        {
            return Marshal.OffsetOf<ModelPoint>("colors");
        }
    }

    static class ShapeArrays
    {
        public readonly static ModelPoint[] FlatPoints = new ModelPoint[]
        {
            //upper panel
            new ModelPoint((-1, -1 , -1), (0.75f, 0.75f, 0.75f)),
            new ModelPoint((-1, -1, 1), (0.75f, 0.75f, 0.75f)),
            new ModelPoint((1, -1, -1), (0.75f, 0.75f, 0.75f)),
            new ModelPoint((1, -1, 1), (0.75f, 0.75f, 0.75f)),
            //lower panel
            //new ModelPoint((-1, -1 , -1), (1, 0, 1)),
            //new ModelPoint((-1, -1, 1), (1, 0, 1)),
            //new ModelPoint((1, -1, -1), (1, 0, 1)),
            //new ModelPoint((1, -1, 1), (1, 0, 1)),
        };

        public readonly static uint[] FlatIndices = new uint[]
        {
            0,  1,  2,  2,  1,  3,
            //6,  5,  4,  7,  5,  6
        };

        public readonly static ModelPoint[] CubePoints = new ModelPoint[]
        {
            //front
            new ModelPoint((-1, -1 , 1), (1, 1, 1)),
            new ModelPoint((1, -1, 1), (1, 1, 1)),
            new ModelPoint((1, 1, 1), (1, 1, 1)),
            new ModelPoint((-1, 1, 1), (1, 1, 1)),
            //right
            new ModelPoint((1, 1, 1), (1, 0, 0)),
            new ModelPoint((1, 1, -1), (1, 0, 0)),
            new ModelPoint((1, -1, -1), (1, 0, 0)),
            new ModelPoint((1, -1, 1), (1, 0, 0)),
            //back
            new ModelPoint((-1, -1, -1), (0, 1, 0)),
            new ModelPoint((1, -1, -1), (0, 1, 0)),
            new ModelPoint(( 1, 1, -1), (0, 1, 0)),
            new ModelPoint((-1, 1, -1), (0, 1, 0)),
            //left
            new ModelPoint((-1, -1, -1), (0, 0, 1)),
            new ModelPoint((-1, -1, 1), (0, 0, 1)),
            new ModelPoint((-1, 1, 1), (0, 0, 1)),
            new ModelPoint((-1, 1, -1), (0, 0, 1)),
            //upper
            new ModelPoint(( 1, 1, 1), (1, 1, 0)),
            new ModelPoint((-1, 1, 1), (1, 1, 0)),
            new ModelPoint((-1, 1, -1), (1, 1, 0)),
            new ModelPoint((1, 1, -1), (1, 1, 0)),
            //bottom
            new ModelPoint((-1, -1, -1), (1, 0, 1)),
            new ModelPoint((1, -1, -1), (1, 0, 1)),
            new ModelPoint(( 1, -1, 1), (1, 0, 1)),
            new ModelPoint((-1, -1, 1), (1, 0, 1)),
        };

        public readonly static uint[] CubeIndices = new uint[]
        {
            0,  1,  2,  0,  2,  3,   //front
            6,  5,  4,  7,  6,  4,   //right
            10, 9,  8,  11, 10, 8,   //back
            12, 13, 14, 12, 14, 15,  //left
            18, 17, 16, 19, 18, 16,  //upper
            20, 21, 22, 20, 22, 23   //bottom
        };

        public readonly static ModelPoint[] ParallelepipedPoints = new ModelPoint[]
        {
            //front
            new ModelPoint((-1, -1 , 1), (1, 1, 1)),
            new ModelPoint((1, -1, 1), (1, 1, 1)),
            new ModelPoint((1, 3, 1), (1, 1, 1)),
            new ModelPoint((-1, 3, 1), (1, 1, 1)),
            //right
            new ModelPoint((1, 3, 1), (1, 0, 0)),
            new ModelPoint((1, 3, -1), (1, 0, 0)),
            new ModelPoint((1, -1, -1), (1, 0, 0)),
            new ModelPoint((1, -1, 1), (1, 0, 0)),
            //back
            new ModelPoint((-1, -1, -1), (0, 1, 0)),
            new ModelPoint((1, -1, -1), (0, 1, 0)),
            new ModelPoint(( 1, 3, -1), (0, 1, 0)),
            new ModelPoint((-1, 3, -1), (0, 1, 0)),
            //left
            new ModelPoint((-1, -1, -1), (0, 0, 1)),
            new ModelPoint((-1, -1, 1), (0, 0, 1)),
            new ModelPoint((-1, 3, 1), (0, 0, 1)),
            new ModelPoint((-1, 3, -1), (0, 0, 1)),
            //upper
            new ModelPoint(( 1, 3, 1), (1, 1, 0)),
            new ModelPoint((-1, 3, 1), (1, 1, 0)),
            new ModelPoint((-1, 3, -1), (1, 1, 0)),
            new ModelPoint((1, 3, -1), (1, 1, 0)),
            //bottom
            new ModelPoint((-1, -1, -1), (1, 0, 1)),
            new ModelPoint((1, -1, -1), (1, 0, 1)),
            new ModelPoint(( 1, -1, 1), (1, 0, 1)),
            new ModelPoint((-1, -1, 1), (1, 0, 1)),
        };

        public readonly static uint[] ParallelepipedIndices = new uint[]
        {
            0,  1,  2,  0,  2,  3,   //front
            6,  5,  4,  7,  6,  4,   //right
            10, 9,  8,  11, 10, 8,   //back
            12, 13, 14, 12, 14, 15,  //left
            18, 17, 16, 19, 18, 16,  //upper
            20, 21, 22, 20, 22, 23   //bottom
        };
    }
}
