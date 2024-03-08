using Unity.Burst;
using Unity.Mathematics;

namespace PLUME
{
    public static class MathUtils
    {
        /// <summary>
        /// Returns the angle between two quaternions in radians.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Returns the angle in radians.</returns>
        [BurstCompile]
        public static float Angle(quaternion a, quaternion b)
        {
            var z = math.abs(math.dot(a, b));
            return 2 * math.acos(z);
        }

        [BurstCompile]
        public static quaternion Multiply(quaternion q, quaternion p)
        {
            var lhs = q.value;
            var rhs = p.value;
            var x = lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y;
            var y = lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z;
            var z = lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x;
            var w = lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z;
            return new quaternion(x, y, z, w);
        }
    }
}