using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace UniverGraphics
{
    class Teapot
    {
        private const int GL_TEAPOT_N_INPUT_PATCHES = 10;
        static int[][] patchdata_teapot = new int[GL_TEAPOT_N_INPUT_PATCHES][]
        {
            new int[] {  0,   1,   2,   3,   4,   5,   6,   7,   8,   9,  10,  11,  12,  13,  14,  15, }, /* rim    */
            new int[] { 12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23,  24,  25,  26,  27, }, /* body   */
            new int[] { 24,  25,  26,  27,  28,  29,  30,  31,  32,  33,  34,  35,  36,  37,  38,  39, },
            new int[] { 40,  41,  42,  40,  43,  44,  45,  46,  47,  47,  47,  47,  48,  49,  50,  51, }, /* lid    */
            new int[] { 48,  49,  50,  51,  52,  53,  54,  55,  56,  57,  58,  59,  60,  61,  62,  63, },
            new int[] { 64,  64,  64,  64,  65,  66,  67,  68,  69,  70,  71,  72,  39,  38,  37,  36, }, /* bottom */
            new int[] { 73,  74,  75,  76,  77,  78,  79,  80,  81,  82,  83,  84,  85,  86,  87,  88, }, /* handle */
            new int[] { 85,  86,  87,  88,  89,  90,  91,  92,  93,  94,  95,  96,  97,  98,  99, 100, },
            new int[] {101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, }, /* spout  */
            new int[] {113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128  }
        };

static float[][] cpdata_teapot = new float[][]
{
    new float[] { 1.40000f,  0.00000f,  2.40000f}, new float[] { 1.40000f, -0.78400f,  2.40000f},
    new float[] { 0.78400f, -1.40000f,  2.40000f}, new float[] { 0.00000f, -1.40000f,  2.40000f},
    new float[] { 1.33750f,  0.00000f,  2.53125f}, new float[] { 1.33750f, -0.74900f,  2.53125f},
    new float[] { 0.74900f, -1.33750f,  2.53125f}, new float[] { 0.00000f, -1.33750f,  2.53125f},
    new float[] { 1.43750f,  0.00000f,  2.53125f}, new float[] { 1.43750f, -0.80500f,  2.53125f},
    new float[] { 0.80500f, -1.43750f,  2.53125f}, new float[] { 0.00000f, -1.43750f,  2.53125f},
    new float[] { 1.50000f,  0.00000f,  2.40000f}, new float[] { 1.50000f, -0.84000f,  2.40000f},
    new float[] { 0.84000f, -1.50000f,  2.40000f}, new float[] { 0.00000f, -1.50000f,  2.40000f},
    new float[] { 1.75000f,  0.00000f,  1.87500f}, new float[] { 1.75000f, -0.98000f,  1.87500f},
    new float[] { 0.98000f, -1.75000f,  1.87500f}, new float[] { 0.00000f, -1.75000f,  1.87500f},
    new float[] { 2.00000f,  0.00000f,  1.35000f}, new float[] { 2.00000f, -1.12000f,  1.35000f},
    new float[] { 1.12000f, -2.00000f,  1.35000f}, new float[] { 0.00000f, -2.00000f,  1.35000f},
    new float[] { 2.00000f,  0.00000f,  0.90000f}, new float[] { 2.00000f, -1.12000f,  0.90000f},
    new float[] { 1.12000f, -2.00000f,  0.90000f}, new float[] { 0.00000f, -2.00000f,  0.90000f},
    new float[] { 2.00000f,  0.00000f,  0.45000f}, new float[] { 2.00000f, -1.12000f,  0.45000f},
    new float[] { 1.12000f, -2.00000f,  0.45000f}, new float[] { 0.00000f, -2.00000f,  0.45000f},
    new float[] { 1.50000f,  0.00000f,  0.22500f}, new float[] { 1.50000f, -0.84000f,  0.22500f},
    new float[] { 0.84000f, -1.50000f,  0.22500f}, new float[] { 0.00000f, -1.50000f,  0.22500f},
    new float[] { 1.50000f,  0.00000f,  0.15000f}, new float[] { 1.50000f, -0.84000f,  0.15000f},
    new float[] { 0.84000f, -1.50000f,  0.15000f}, new float[] { 0.00000f, -1.50000f,  0.15000f},
    new float[] { 0.00000f,  0.00000f,  3.15000f}, new float[] { 0.00000f, -0.00200f,  3.15000f},
    new float[] { 0.00200f,  0.00000f,  3.15000f}, new float[] { 0.80000f,  0.00000f,  3.15000f},
    new float[] { 0.80000f, -0.45000f,  3.15000f}, new float[] { 0.45000f, -0.80000f,  3.15000f},
    new float[] { 0.00000f, -0.80000f,  3.15000f}, new float[] { 0.00000f,  0.00000f,  2.85000f},
    new float[] { 0.20000f,  0.00000f,  2.70000f}, new float[] { 0.20000f, -0.11200f,  2.70000f},
    new float[] { 0.11200f, -0.20000f,  2.70000f}, new float[] { 0.00000f, -0.20000f,  2.70000f},
    new float[] { 0.40000f,  0.00000f,  2.55000f}, new float[] { 0.40000f, -0.22400f,  2.55000f},
    new float[] { 0.22400f, -0.40000f,  2.55000f}, new float[] { 0.00000f, -0.40000f,  2.55000f},
    new float[] { 1.30000f,  0.00000f,  2.55000f}, new float[] { 1.30000f, -0.72800f,  2.55000f},
    new float[] { 0.72800f, -1.30000f,  2.55000f}, new float[] { 0.00000f, -1.30000f,  2.55000f},
    new float[] { 1.30000f,  0.00000f,  2.40000f}, new float[] { 1.30000f, -0.72800f,  2.40000f},
    new float[] { 0.72800f, -1.30000f,  2.40000f}, new float[] { 0.00000f, -1.30000f,  2.40000f},
    new float[] { 0.00000f,  0.00000f,  0.00000f}, new float[] { 0.00000f, -1.42500f,  0.00000f},
    new float[] { 0.79800f, -1.42500f,  0.00000f}, new float[] { 1.42500f, -0.79800f,  0.00000f},
    new float[] { 1.42500f,  0.00000f,  0.00000f}, new float[] { 0.00000f, -1.50000f,  0.07500f},
    new float[] { 0.84000f, -1.50000f,  0.07500f}, new float[] { 1.50000f, -0.84000f,  0.07500f},
    new float[] { 1.50000f,  0.00000f,  0.07500f}, new float[] {-1.60000f,  0.00000f,  2.02500f},
    new float[] {-1.60000f, -0.30000f,  2.02500f}, new float[] {-1.50000f, -0.30000f,  2.25000f},
    new float[] {-1.50000f,  0.00000f,  2.25000f}, new float[] {-2.30000f,  0.00000f,  2.02500f},
    new float[] {-2.30000f, -0.30000f,  2.02500f}, new float[] {-2.50000f, -0.30000f,  2.25000f},
    new float[] {-2.50000f,  0.00000f,  2.25000f}, new float[] {-2.70000f,  0.00000f,  2.02500f},
    new float[] {-2.70000f, -0.30000f,  2.02500f}, new float[] {-3.00000f, -0.30000f,  2.25000f},
    new float[] {-3.00000f,  0.00000f,  2.25000f}, new float[] {-2.70000f,  0.00000f,  1.80000f},
    new float[] {-2.70000f, -0.30000f,  1.80000f}, new float[] {-3.00000f, -0.30000f,  1.80000f},
    new float[] {-3.00000f,  0.00000f,  1.80000f}, new float[] {-2.70000f,  0.00000f,  1.57500f},
    new float[] {-2.70000f, -0.30000f,  1.57500f}, new float[] {-3.00000f, -0.30000f,  1.35000f},
    new float[] {-3.00000f,  0.00000f,  1.35000f}, new float[] {-2.50000f,  0.00000f,  1.12500f},
    new float[] {-2.50000f, -0.30000f,  1.12500f}, new float[] {-2.65000f, -0.30000f,  0.93750f},
    new float[] {-2.65000f,  0.00000f,  0.93750f}, new float[] {-2.00000f,  0.00000f,  0.90000f},
    new float[] {-2.00000f, -0.30000f,  0.90000f}, new float[] {-1.90000f, -0.30000f,  0.60000f},
    new float[] {-1.90000f,  0.00000f,  0.60000f}, new float[] { 1.70000f,  0.00000f,  1.42500f},
    new float[] { 1.70000f, -0.66000f,  1.42500f}, new float[] { 1.70000f, -0.66000f,  0.60000f},
    new float[] { 1.70000f,  0.00000f,  0.60000f}, new float[] { 2.60000f,  0.00000f,  1.42500f},
    new float[] { 2.60000f, -0.66000f,  1.42500f}, new float[] { 3.10000f, -0.66000f,  0.82500f},
    new float[] { 3.10000f,  0.00000f,  0.82500f}, new float[] { 2.30000f,  0.00000f,  2.10000f},
    new float[] { 2.30000f, -0.25000f,  2.10000f}, new float[] { 2.40000f, -0.25000f,  2.02500f},
    new float[] { 2.40000f,  0.00000f,  2.02500f}, new float[] { 2.70000f,  0.00000f,  2.40000f},
    new float[] { 2.70000f, -0.25000f,  2.40000f}, new float[] { 3.30000f, -0.25000f,  2.40000f},
    new float[] { 3.30000f,  0.00000f,  2.40000f}, new float[] { 2.80000f,  0.00000f,  2.47500f},
    new float[] { 2.80000f, -0.25000f,  2.47500f}, new float[] { 3.52500f, -0.25000f,  2.49375f},
    new float[] { 3.52500f,  0.00000f,  2.49375f}, new float[] { 2.90000f,  0.00000f,  2.47500f},
    new float[] { 2.90000f, -0.15000f,  2.47500f}, new float[] { 3.45000f, -0.15000f,  2.51250f},
    new float[] { 3.45000f,  0.00000f,  2.51250f}, new float[] { 2.80000f,  0.00000f,  2.40000f},
    new float[] { 2.80000f, -0.15000f,  2.40000f}, new float[] { 3.20000f, -0.15000f,  2.40000f},
    new float[] { 3.20000f,  0.00000f,  2.40000f}
}; 

        private const int GL_WIRE_N_SUBDIV = 10;
        private static float[][] bernWire0 = new float[GL_WIRE_N_SUBDIV][]
            {
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0}
            };
        private static float[][] bernWire1 = new float[GL_WIRE_N_SUBDIV][]
            {
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0},
                new float[4] {0, 0, 0, 0}
            };
        private const int GL_TEAPOT_N_PATCHES = 6 * 4 + 4 * 2;
        private const int GL_WIRE_TEAPOT_N_VERT = GL_WIRE_N_SUBDIV * GL_WIRE_N_SUBDIV * GL_TEAPOT_N_PATCHES;
        private static ushort[] vertIdxsTeapotW = new ushort[GL_WIRE_TEAPOT_N_VERT * 2];
        private static float[] normsTeapotW = new float[GL_WIRE_TEAPOT_N_VERT * 3];
        private static float[] vertsTeapotW = new float[GL_WIRE_TEAPOT_N_VERT * 3];
        private static float lastScaleTeapotW = 0f;
        private static bool initedTeapotW = false;

        static void bernstein3(int i, float x, ref float r0, ref float r1)
        {
            float invx = 1f - x;
            float temp;
            switch (i)
            {
                case 0:
                    temp = invx * invx;
                    r0 = invx * temp;
                    r1 = -3 * temp;
                    break;
                case 1:
                    temp = invx * invx;
                    r0 = 3 * x * temp;
                    r1 = 3 * temp - 6 * x * invx;
                    break;
                case 2:
                    temp = x * x;
                    r0 = 3 * temp * invx;
                    r1 = 6 * x * invx - 3 * temp;
                    break;
                case 3:
                    temp = x * x;
                    r0 = x * temp;
                    r1 = 3 * temp;
                    break;
                default:
                    r0 = r1 = 0;
                    break;
            }
        }

        static void pregenBernstein(int nSubDivs, float[][] bern0, float[][] bern1)
        {
            for (int s = 0; s < nSubDivs; s++)
            {
                float x = s / (nSubDivs - 1f);
                for (int i = 0; i < bern0[s].Length; i++)
                {
                    bernstein3(i, x, ref bern0[s][i], ref bern1[s][i]);
                }
            }
        }

        static void rotOrReflect(int flag, int nVals, int nSubDivs, float[] vals)
        {
            if (flag == 4)
            {
                int i1 = nVals, i2 = nVals * 2, i3 = nVals * 3;
                for (int o = 0; o < nVals; o += 3)
                {
                    // 90deg rotation
                    vals[i1 + o + 0] =  vals[o + 2];
                    vals[i1 + o + 1] =  vals[o + 1];
                    vals[i1 + o + 2] = -vals[o + 0];
                    // 180deg rotation
                    vals[i1 + o + 0] = -vals[o + 0];
                    vals[i1 + o + 1] =  vals[o + 1];
                    vals[i1 + o + 2] = -vals[o + 2];
                    // 270deg rotation
                    vals[i1 + o + 0] = -vals[o + 2];
                    vals[i1 + o + 1] =  vals[o + 1];
                    vals[i1 + o + 2] = -vals[o + 0];
                }
            }
            else if (flag == 2)
            {
                for (int u = 0; u < nSubDivs; u++)
                {
                    int off = (nSubDivs - u - 1) * nSubDivs * 3;
                    int o = nVals + u * nSubDivs * 3;
                    for (int i = 0; i < nSubDivs*3; i+=3, o+=3)
                    {
                        vals[o + 0] =  vals[off + i + 0];
                        vals[o + 1] =  vals[off + i + 1];
                        vals[o + 2] = -vals[off + i + 2];
                    }
                }
            }
        }

        static int evalBezierWithNorm(float[][][] cp, int nSubDivs, float[][] bern0, float[][] bern1, int flag, int normalFix, float[] verts, float[] norms)
        {
            int nVerts = nSubDivs * nSubDivs;
            int nVertVals = nVerts * 3;

            for (int u = 0, o = 3; u < nSubDivs; u++)
            {
                for (int v = 0; v < nSubDivs; v++, o += 3)
                {
                    float[] tan1 = new float[3] { 0, 0, 0 };
                    float[] tan2 = new float[3] { 0, 0, 0 };
                    float len;
                    for (int i = 0; i <= 3; i++)
                    {
                        float[] vert_0 = new float[3] { 0, 0, 0 };
                        float[] vert_1 = new float[3] { 0, 0, 0 };
                        for (int j = 0; j <= 3; j++)
                        {
                            vert_0[0] += bern0[v][j] * cp[i][j][0];
                            vert_0[1] += bern0[v][j] * cp[i][j][1];
                            vert_0[2] += bern0[v][j] * cp[i][j][2];

                            vert_1[0] += bern1[v][j] * cp[i][j][0];
                            vert_1[1] += bern1[v][j] * cp[i][j][1];
                            vert_1[2] += bern1[v][j] * cp[i][j][2];
                        }
                        verts[o + 0] += bern0[u][i] * vert_0[0];
                        verts[o + 1] += bern0[u][i] * vert_0[1];
                        verts[o + 2] += bern0[u][i] * vert_0[2];

                        tan1[0] += bern0[u][i] * vert_1[0];
                        tan1[1] += bern0[u][i] * vert_1[1];
                        tan1[2] += bern0[u][i] * vert_1[2];
                        tan2[0] += bern1[u][i] * vert_0[0];
                        tan2[1] += bern1[u][i] * vert_0[1];
                        tan2[2] += bern1[u][i] * vert_0[2];
                    }
                    norms[o + 0] = tan1[1] * tan2[2] - tan1[2] * tan2[1];
                    norms[o + 1] = tan1[2] * tan2[0] - tan1[0] * tan2[2];
                    norms[o + 2] = tan1[0] * tan2[1] - tan1[1] * tan2[0];
                    len = (float)Math.Sqrt(norms[o + 0] * norms[o + 0] + norms[o + 1] * norms[o + 1] + norms[o + 2] * norms[o + 2]);
                    norms[o + 0] /= len;
                    norms[o + 1] /= len;
                    norms[o + 2] /= len;
                }
            }

            if (normalFix != 0)
            {
                for (int o = 0; o < nSubDivs*3; o += 3)
                {
                    norms[o + 0] = 0f;
                    norms[o + 1] = normalFix == 1 ? 1f : -1f;
                    norms[o + 2] = 0f;
                }
            }

            rotOrReflect(flag, nVertVals, nSubDivs, verts);
            rotOrReflect(flag, nVertVals, nSubDivs, norms);

            return nVertVals * flag;
        }

        static int evalBezier(float[][][] cp, int nSubDivs, float[][] bern0, int flag, float[] verts)
        {
            int nVerts = nSubDivs * nSubDivs;
            int nVertVals = nVerts * 3;

            for (int u = 0, o = 0; u < nSubDivs; u++)
            {
                for (int v = 0; v < nSubDivs; v++, o += 3)
                {
                    for (int i = 0; i <= 3; i++)
                    {
                        float[] vert0 = { 0, 0, 0 };
                        for (int j = 0; j <= 3; j++)
                        {
                            vert0[0] += bern0[v][j] * cp[i][j][0];
                            vert0[1] += bern0[v][j] * cp[i][j][1];
                            vert0[2] += bern0[v][j] * cp[i][j][2];
                        }
                        verts[o + 0] += bern0[u][i] * vert0[0];
                        verts[o + 1] += bern0[u][i] * vert0[1];
                        verts[o + 2] += bern0[u][i] * vert0[2];
                    }
                }
            }

            rotOrReflect(flag, nVertVals, nSubDivs, verts);

            return nVertVals * flag;
        }

        static void fghTeaset(float scale, /*bool useWireMode, */float[][] cpdata, int[][] patchdata,
            ushort[] vertIdxs, float[] verts, float[] norms, float[] texcs, ref float lastScale, ref bool inited,
            bool needNormalFix, bool rotFlip, float zOffset, int nVerts, int nInputPatches, int nPatches, int nTriangles)
        {
            float[][] bern0 = bernWire0;
            float[][] bern1 = bernWire1;
            float[][][] cp = new float[4][][];
            for (int i = 0; i < 4; i++)
                cp[i] = new float[4][];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    cp[i][j] = new float[3];
            int nSubDivs = GL_WIRE_N_SUBDIV;

            if (!inited || scale != lastScale)
            {
                for (int i = 0; i < nVerts*3; i++)
                {
                    verts[i] = 0;
                }

                if (!inited)
                    pregenBernstein(nSubDivs, bern0, bern1);

                for (int p = 0, o = 0; p < nInputPatches; p++)
                {
                    int flag = rotFlip ? (p < 6 ? 4 : 2) : 1;
                    int normalFix = needNormalFix ? (p == 3 ? 1 : (p == 5 ? 2 : 0)) : 0;
                    for (int i = 0; i < 16; i++)
                    {
                        cp[i / 4][i % 4][0] =  cpdata[patchdata[p][i]][0]            * scale / 2f;
                        cp[i / 4][i % 4][1] = (cpdata[patchdata[p][i]][2] - zOffset) * scale / 2f;
                        cp[i / 4][i % 4][2] = -cpdata[patchdata[p][i]][1]            * scale / 2f;
                    }

                    float[] vertsSinceO = new float[verts.Length - o];
                    float[] normsSinceO = new float[norms.Length - o];
                    for (int i = o, j = 0; i < verts.Length; i++, j++)
                    {
                        vertsSinceO[j] = verts[i];
                        normsSinceO[j] = norms[i];
                    }

                    if (!inited)
                        o += evalBezierWithNorm(cp, nSubDivs, bern0, bern1, flag, normalFix, vertsSinceO, normsSinceO);
                    else o += evalBezier(cp, nSubDivs, bern0, flag, vertsSinceO);
                }
            }
            lastScale = scale;

            if (!inited)
            {
                int o = 0;
                for (int p = 0; p < nPatches; p++)
                {
                    int idx = nSubDivs * nSubDivs * p;
                    for (int c = 0; c < nSubDivs; c++)
                        for (int r = 0; r < nSubDivs; r++, o++)
                            vertIdxs[o] = (ushort)(idx + r * nSubDivs + c);
                }

                for (int p = 0; p < nPatches; p++)
                {
                    int idx = nSubDivs * nSubDivs * p;
                    for (int r = 0; r < nSubDivs; r++)
                    {
                        int loc = r * nSubDivs;
                        for (int c = 0; c < nSubDivs; c++, o++)
                            vertIdxs[o] = (ushort)(idx + loc + c);
                    }
                }
                inited = true;
            }

            fghDrawGeometryWire(verts, norms, vertIdxs, nPatches * nSubDivs * 2, nSubDivs, (int)BeginMode.LineLoop, null, 0, 0);
        }

        static void fghDrawGeometryWire(float[] vertices, float[] normals,
            ushort[] vertIdxs, int numParts, int numVertPerPart, int vertexMode,
            ushort[] vertIdxs2, int numParts2, int numVertPerPart2)
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

            GL.VertexPointer(3, VertexPointerType.Float, 0, vertices);
            GL.NormalPointer(NormalPointerType.Float, 0, normals);

            if (vertIdxs != null)
                for (int i = 0; i < numParts; i++)
                    GL.DrawArrays((BeginMode)vertexMode, i * numVertPerPart, numVertPerPart);
            else
                for (int i = 0; i < numParts; i++)
                    GL.DrawElements((BeginMode)vertexMode, numVertPerPart, DrawElementsType.UnsignedInt, vertIdxs[i * numVertPerPart]);

            if (vertIdxs2 != null)
                for (int i = 0; i < numParts2; i++)
                    GL.DrawElements(BeginMode.LineLoop, numVertPerPart2, DrawElementsType.UnsignedInt, vertIdxs2[i * numVertPerPart2]);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
        }

        public static void GlWireTeapot(double size)
        {
            fghTeaset((float)size, cpdata_teapot, patchdata_teapot, vertIdxsTeapotW, vertsTeapotW, normsTeapotW, null, ref lastScaleTeapotW, ref initedTeapotW,
                true, true, 1.575f, GL_WIRE_TEAPOT_N_VERT, GL_TEAPOT_N_INPUT_PATCHES, GL_TEAPOT_N_PATCHES, 0);
        }
    }
}
