using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AiVision))]
public class AiVisionEditor : Editor
{
    private void OnSceneGUI()
    {
        AiVision aiVision = (AiVision)target;

        Handles.color = Color.white;
        Handles.DrawWireArc(aiVision.transform.position, Vector3.up, Vector3.forward, 360, aiVision.radius);

        Vector3 leftBoundary = DirectionFromAngle(aiVision.transform.eulerAngles.y, -aiVision.angle * 0.5f);
        Vector3 rightBoundary = DirectionFromAngle(aiVision.transform.eulerAngles.y, aiVision.angle * 0.5f);

        Handles.color = Color.yellow;
        Handles.DrawLine(aiVision.transform.position, aiVision.transform.position + leftBoundary * aiVision.radius);
        Handles.DrawLine(aiVision.transform.position, aiVision.transform.position + rightBoundary * aiVision.radius);

        if (aiVision.seeingPlayer)
        {
            Handles.color = Color.green;
            Handles.DrawLine(aiVision.transform.position, aiVision.playerRef.position);
        }
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        float radians = (eulerY + angleInDegrees) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));
    }
}
