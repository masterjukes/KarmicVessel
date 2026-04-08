using UnityEngine;

namespace KarmicVessel.Other
{
    public class dbg : MonoBehaviour
    {


        public static void Log(string message)
        {
            if(ModOptions.AllowLogs)
                Debug.Log(message);
        }


        public static LineRenderer DrawLine(Vector3 org, Vector3 end, float width, Color color, string name = "Line", float despawnTime = 10f)
        {
            if (!ModOptions.ShowDebugRays) 
                return null;
            
            var lineObj = new GameObject(name);
            var newLine = lineObj.AddComponent<LineRenderer>();
            
            newLine.material = new Material(Shader.Find("Sprites/Default"));
            if (newLine.material == null)
                newLine.material = new Material(Shader.Find("Standard"));

            newLine.startColor = color;
            newLine.endColor = color;
            newLine.startWidth = width;
            newLine.endWidth = width;
            newLine.positionCount = 2;
            newLine.SetPosition(0, org);
            newLine.SetPosition(1, end);
            Destroy(lineObj, despawnTime);
            return newLine;
        }
        
    }
}