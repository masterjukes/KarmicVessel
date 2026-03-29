using ThunderRoad;
using UnityEngine;

namespace KarmicVessel.Other
{
    public static class ExtensionMethods
    {

        public enum Axis
        {
            X,
            Y,
            Z
        }

        public static float GetAxis(this Vector3 vector, Axis axis)
        {
            float result = 0;
            switch (axis)
            {
                case Axis.X:
                    result = vector.x;
                    break;
                case Axis.Y:
                    result = vector.y;
                    break;
                case Axis.Z:
                    result = vector.z;
                    break;
            }

            return result;
        }

        public static void ScaleToGlobalSize(this Item item, float size)
        {
            var localBounds = item.GetLocalBounds();
            var largestAxis = ((!(localBounds.size.x > localBounds.size.y)) ? Axis.Y : Axis.X);
            largestAxis = ((localBounds.size.GetAxis(largestAxis) > localBounds.size.z) ? largestAxis : Axis.Z);
            var orgLargestAxisSize = localBounds.size.GetAxis(largestAxis);
            var scaleRatio = Mathf.Clamp01(size / orgLargestAxisSize);
            var orgMass = item.physicBody.mass;
            var smallestAxis = ((!(localBounds.size.x < localBounds.size.y)) ? Axis.Y : Axis.X);
            smallestAxis = ((localBounds.size.GetAxis(smallestAxis) < localBounds.size.z) ? smallestAxis : Axis.Z);

            item.physicBody.mass = scaleRatio;
            item.transform.localScale = Vector3.one * scaleRatio;
            item.ResetCenterOfMass();
        }

        public static Vector3 GetUnitAxis(this Transform transform, Axis axis)
        {
            
            switch (axis) {
                case Axis.X:
                    return transform.right;
                case Axis.Y:
                    return transform.up;
                case Axis.Z:
                    return transform.forward;
                
            }
            return Vector3.zero;
        }

    }
}