using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Designed for one target object to enter the area at a time.
public class TriggerBoxAudioBlender : MonoBehaviour
{
    public enum TagOrGameObject { Tag, GameObject }

    // Public Fields
    [Header("Choose Object Identification Method")]
    public TagOrGameObject identifyObjectBy;
    public string checkForTag = "Player";
    public GameObject targetObject;

    [Header("Choose Response Type")]
    public bool multiAxis = false;
    [Range(1f, 99f)] public float halfWayMark = 50;

    [Header("Call Functions With Trigger")]
    public UnityEvent onEnterPointA;
    public UnityEvent onEnterPointB;
    public UnityEvent onStay;
    public UnityEvent onExitPointA;
    public UnityEvent onExitPointB;

    [Header("Debug Tools")]
    public bool showGizmos = true;
    public GUIStyle textSettings;

    // Private Fields
    private BoxCollider boxCollider;
    private bool trackingPosition;
    private float progressThroughTrigger;
    private Vector3 pointA, pointB, pointAB;
    private float progressAB;
    private Vector3 lastTargetPosition, lastTriggerBoxPosition, lastTriggerBoxScale;
    private Quaternion lastTriggerBoxRotation;

    public float ProgressThroughTrigger => progressThroughTrigger;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (boxCollider == null && TryGetComponent(out BoxCollider collider))
            boxCollider = collider;

        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 center = boxCollider.center;
        Vector3 size = boxCollider.size;

        DrawGizmoCube(Vector3.left * 0.5f * size.x + center, Color.green, size);
        DrawGizmoCube(Vector3.right * 0.5f * size.x + center, Color.red, size);

        if (multiAxis)
            DrawGizmoCube(new Vector3((halfWayMark / 100 - 0.5f) * size.x, 0, 0) + center, Color.yellow, size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);

        if (!trackingPosition) return;

        GUI.color = Color.white;
        Handles.Label(pointAB, $"Progress Through Trigger:\n{progressThroughTrigger:F2}%", textSettings);

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawLine(pointA, pointB);

        if (multiAxis)
        {
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector3(ProgressToScale(progressAB) - 0.5f * size.x, 0, -0.5f * size.z) + center),
                transform.TransformPoint(new Vector3(ProgressToScale(progressAB) - 0.5f * size.x, 0, 0.5f * size.z) + center)
            );
        }
    }

    private void DrawGizmoCube(Vector3 position, Color color, Vector3 size)
    {
        Gizmos.color = new Color(color.r, color.g, color.b, 0.4f);
        Gizmos.DrawCube(position, new Vector3(0.01f * size.x, size.y, size.z));
    }
#endif

    private void Start()
    {
        if (TryGetComponent(out BoxCollider collider))
        {
            boxCollider = collider;
            if (!boxCollider.isTrigger) boxCollider.isTrigger = true;
        }
        else
        {
            Debug.LogError($"'Trigger Box Audio Blender' requires a 'Box Collider' component on {gameObject.name}.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ObjectCheck(other)) return;

        trackingPosition = true;
        CalculateTriggerProgress(other);

        if (progressThroughTrigger <= halfWayMark)
            onEnterPointA.Invoke();
        else
            onEnterPointB.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!ObjectCheck(other) || !trackingPosition) return;

        CalculateTriggerProgress(other);
        onStay.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!ObjectCheck(other)) return;

        trackingPosition = false;
        CalculateTriggerProgress(other);

        if (progressThroughTrigger <= halfWayMark)
            onExitPointA.Invoke();
        else
            onExitPointB.Invoke();
    }

    private bool ObjectCheck(Collider other)
    {
        if (identifyObjectBy == TagOrGameObject.Tag)
        {
            if (string.IsNullOrEmpty(checkForTag))
            {
                Debug.LogWarning($"The 'Check For Tag' field is empty on {gameObject.name}. This trigger will not function correctly.");
                return false;
            }

            return other.CompareTag(checkForTag);
        }

        if (targetObject == null)
        {
            Debug.LogWarning($"The 'Target Object' field is empty on {gameObject.name}. This trigger will not function correctly.");
            return false;
        }

        return other.gameObject == targetObject;
    }

    private void CalculateTriggerProgress(Collider other)
    {
        Vector3 targetGlobalPosition = other.transform.position;
        Vector3 targetLocalPosition = transform.InverseTransformPoint(targetGlobalPosition);

        if (HasTransformChanged(targetGlobalPosition))
        {
            pointA = new Vector3(-0.5f, 0, 0);
            pointB = new Vector3(0.5f, 0, 0);
            pointAB = GetIntersectionPoint(pointA, pointB, 1, targetLocalPosition);

            float lineAToAB = Vector3.Distance(pointA, pointAB);
            progressAB = Mathf.Clamp(lineAToAB * 100f, 0f, 100f);

            if (multiAxis)
            {
                CalculateMultiAxisProgress(targetLocalPosition);
            }
            else
            {
                progressThroughTrigger = progressAB;
            }

            pointA = transform.TransformPoint(pointA);
            pointB = transform.TransformPoint(pointB);
            pointAB = transform.TransformPoint(pointAB);
        }

        lastTargetPosition = targetGlobalPosition;
        lastTriggerBoxPosition = transform.position;
        lastTriggerBoxRotation = transform.rotation;
        lastTriggerBoxScale = transform.localScale;
    }

    private bool HasTransformChanged(Vector3 targetGlobalPosition)
    {
        return targetGlobalPosition != lastTargetPosition ||
               transform.position != lastTriggerBoxPosition ||
               transform.rotation != lastTriggerBoxRotation ||
               transform.localScale != lastTriggerBoxScale;
    }

    private void CalculateMultiAxisProgress(Vector3 targetLocalPosition)
    {
        Vector3 pointC = new Vector3(pointAB.x, 0, 0.5f);
        Vector3 pointD = new Vector3(pointAB.x, 0, -0.5f);
        Vector3 pointCD = GetIntersectionPoint(pointC, pointD, 1, targetLocalPosition);

        float distanceFromABToCD = Vector3.Distance(pointAB, pointCD);
        float distanceFromCDToEdge = 0.5f - distanceFromABToCD;

        if (progressAB <= halfWayMark)
        {
            float progressToHalfwayMark = PercentOf(progressAB, halfWayMark);
            float distanceToHypotenuse = PercentOf(progressToHalfwayMark, 0.5f);
            float distanceFromCToHypotenuse = 0.5f - distanceToHypotenuse;
            float progressCH = PercentOf(distanceFromCDToEdge, distanceFromCToHypotenuse);

            progressThroughTrigger = PercentOf(progressCH, progressAB);
        }
        else
        {
            float progressFromHalfwayMark = PercentOf(100 - progressAB, 100 - halfWayMark);
            float distanceToHypotenuse = PercentOf(progressFromHalfwayMark, 0.5f);
            float distanceFromCToHypotenuse = 0.5f - distanceToHypotenuse;
            float progressCH = 100 - PercentOf(distanceFromCDToEdge, distanceFromCToHypotenuse);
            float progressLeft = 100 - progressAB;

            progressThroughTrigger = progressAB + PercentOf(progressCH, progressLeft);
        }

        progressThroughTrigger = Mathf.Clamp(progressThroughTrigger, 0f, 100f);
    }

    private Vector3 GetIntersectionPoint(Vector3 firstPoint, Vector3 secondPoint, float distance, Vector3 target)
    {
        float t = Vector3.Dot(target - secondPoint, firstPoint - secondPoint) / (distance * distance);
        t = Mathf.Clamp(t, 0f, 1f);

        return secondPoint + t * (firstPoint - secondPoint);
    }

    private float ProgressToScale(float number)
    {
        return number / 100f;
    }

    private float PercentOf(float x, float y)
    {
        return (x / y) * 100f;
    }

}