using Unity.Mathematics;

// ReSharper disable MemberCanBePrivate.Global

namespace Butterfly.Utility
{
    public static class Noise
    {
        public static float Mod(float x, float y)
        {
            return x - y * math.floor(x / y);
        }

        public static float2 Mod(float2 x, float2 y)
        {
            return x - y * math.floor(x / y);
        }

        public static float3 Mod(float3 x, float3 y)
        {
            return x - y * math.floor(x / y);
        }

        public static float4 Mod(float4 x, float4 y)
        {
            return x - y * math.floor(x / y);
        }

        public static float2 Fade(float2 t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        public static float3 Fade(float3 t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        public static float Mod289(float x)
        {
            return x - math.floor(x / 289) * 289;
        }

        public static float2 Mod289(float2 x)
        {
            return x - math.floor(x / 289) * 289;
        }

        public static float3 Mod289(float3 x)
        {
            return x - math.floor(x / 289) * 289;
        }

        public static float4 Mod289(float4 x)
        {
            return x - math.floor(x / 289) * 289;
        }

        public static float3 Permute(float3 x)
        {
            return Mod289((x * 34 + 1) * x);
        }

        public static float4 Permute(float4 x)
        {
            return Mod289((x * 34 + 1) * x);
        }

        public static float4 TaylorInvSqrt(float4 r)
        {
            return 1.79284291400159f - 0.85373472095314f * r;
        }

        private static readonly float2 C = new float2(1 / 6.0f, 1 / 3.0f);
        private static readonly float4 D = new float4(0, 0.5f, 1, 2);

        public static float4 SimplexNoiseGrad(float3 v)
        {
            // First corner
            var i = math.floor(v + new float3(math.dot(v, C.yyy)));
            var x0 = v - i + new float3(math.dot(i, C.xxx));

            // Other corners
            var g = math.step(x0.yzx, x0.xyz);
            var l = 1 - g;
            var i1 = math.min(g.xyz, l.zxy);
            var i2 = math.max(g.xyz, l.zxy);

            //   x0 = x0 - 0.0 + 0.0 * C.xxx;
            //   x1 = x0 - i1  + 1.0 * C.xxx;
            //   x2 = x0 - i2  + 2.0 * C.xxx;
            //   x3 = x0 - 1.0 + 3.0 * C.xxx;
            var x1 = x0 - i1 + C.xxx;
            var x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
            var x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

            // Permutations
            i = Mod289(i);

            var p = Permute(i.z + new float4(0, i1.z, i2.z, 1));
            p = Permute(p + i.y + new float4(0, i1.y, i2.y, 1));
            p = Permute(p + i.x + new float4(0, i1.x, i2.x, 1));

            // Gradients: 7x7 points over a square, mapped onto an octahedron.
            // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)

            var n_ = 0.142857142857f; // 1.0/7.0

            var ns = n_ * D.wyz - D.xzx;

            var j = p - 49.0f * math.floor(p * ns.z * ns.z); //  mod(p,7*7)

            var x_ = math.floor(j * ns.z);
            var y_ = math.floor(j - 7.0f * x_); // mod(j,N)

            var x = x_ * ns.x + ns.yyyy;
            var y = y_ * ns.x + ns.yyyy;
            var h = 1.0f - math.abs(x) - math.abs(y);

            var b0 = new float4(x.xy, y.xy);
            var b1 = new float4(x.zw, y.zw);

            //var s0 = new float4(lessThan(b0,0.0))*2.0 - 1.0;
            //var s1 = new float4(lessThan(b1,0.0))*2.0 - 1.0;
            var s0 = math.floor(b0) * 2.0f + 1.0f;
            var s1 = math.floor(b1) * 2.0f + 1.0f;
            var sh = -math.step(h, new float4(0.0));

            var a0 = b0.xzyw + s0.xzyw * sh.xxyy;
            var a1 = b1.xzyw + s1.xzyw * sh.zzww;

            var p0 = new float3(a0.xy, h.x);
            var p1 = new float3(a0.zw, h.y);
            var p2 = new float3(a1.xy, h.z);
            var p3 = new float3(a1.zw, h.w);

            //Normalise gradients
            var norm = TaylorInvSqrt(new float4(math.dot(p0, p0), math.dot(p1, p1), math.dot(p2, p2), math.dot(p3, p3)));
            p0 *= norm.x;
            p1 *= norm.y;
            p2 *= norm.z;
            p3 *= norm.w;

            // Mix final noise value
            var m = math.max(0.5f - new float4(math.dot(x0, x0), math.dot(x1, x1), math.dot(x2, x2), math.dot(x3, x3)), 0.0f);
            var m2 = m * m;
            var m4 = m2 * m2;
            var pdotx = new float4(math.dot(p0, x0), math.dot(p1, x1), math.dot(p2, x2), math.dot(p3, x3));

            // Determine noise gradient
            var temp = m2 * m * pdotx;
            var gradient = -8.0f * (temp.x * x0 + temp.y * x1 + temp.z * x2 + temp.w * x3);
            gradient += m4.x * p0 + m4.y * p1 + m4.z * p2 + m4.w * p3;
            gradient *= 105.0f;

            return 105.0f * math.dot(m4, pdotx);
        }

        public static float SimplexNoise(float3 v)
        {
            return SimplexNoiseGrad(v).w;
        }
    }
}