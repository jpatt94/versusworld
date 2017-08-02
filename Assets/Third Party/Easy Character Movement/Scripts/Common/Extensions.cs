using UnityEngine;

namespace ECM.Common
{
    public static class Extensions
    {
        /// <summary>
        /// Discards vector Y component.
        /// </summary>

        public static Vector3 onlyXZ(this Vector3 vector3)
        {
            vector3.y = 0.0f;
            return vector3;
        }

        /// <summary>
        /// Transform a given vector to be realtive to target transform.
        /// Eg: Use to perform movement relative to camera's view direction.
        /// </summary>

        public static Vector3 relativeTo(this Vector3 vector3, Transform target)
        {
            var forward = target.forward.onlyXZ();
            return Quaternion.LookRotation(forward) * vector3;
        }
    }
}