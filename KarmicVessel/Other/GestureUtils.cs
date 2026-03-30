using UnityEngine;

namespace KarmicVessel.Other
{
    public static class GestureUtils
{
    public enum Direction
    {
        None,
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down
    }

    public static Direction GetCameraRelativeDirection(
        Transform obj,
        Vector3 previousPosition,
        Transform cam,
        float threshold = 0.01f)
    {
        Vector3 delta = obj.position - previousPosition;

        if (delta.magnitude < threshold)
            return Direction.None;

        Transform camTransform = cam.transform;

        float forward = Vector3.Dot(delta, camTransform.forward);
        float right   = Vector3.Dot(delta, camTransform.right);
        float up      = Vector3.Dot(delta, camTransform.up);

        float absF = Mathf.Abs(forward);
        float absR = Mathf.Abs(right);
        float absU = Mathf.Abs(up);

        if (absF > absR && absF > absU)
            return forward > 0 ? Direction.Forward : Direction.Backward;

        if (absR > absU)
            return right > 0 ? Direction.Right : Direction.Left;

        return up > 0 ? Direction.Up : Direction.Down;
    }

    public static Direction GetFacingDirectionRelativeToCamera(
        Transform obj,
        Transform cam)
    {
        Vector3 forward = obj.forward;
        Transform camTransform = cam.transform;

        float camForward = Vector3.Dot(forward, camTransform.forward);
        float camRight   = Vector3.Dot(forward, camTransform.right);
        float camUp      = Vector3.Dot(forward, camTransform.up);

        float absF = Mathf.Abs(camForward);
        float absR = Mathf.Abs(camRight);
        float absU = Mathf.Abs(camUp);

        if (absF > absR && absF > absU)
            return camForward > 0 ? Direction.Forward : Direction.Backward;

        if (absR > absU)
            return camRight > 0 ? Direction.Right : Direction.Left;

        return camUp > 0 ? Direction.Up : Direction.Down;
    }
}
}