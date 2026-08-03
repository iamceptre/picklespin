using System.Collections.Generic;
using UnityEngine;

public sealed class PositionTurbulence : MonoBehaviour
{
    [SerializeField, Tooltip("how far the drift can pull the object off its path, in metres")]
    private float driftRadius = 0.25f;
    [SerializeField, Tooltip("how fast the noise walks - higher wanders more nervously")]
    private float driftSpeed = 2f;
    [SerializeField, Tooltip("how quickly the object eases toward a new drift target")]
    private float smoothing = 8f;

    private const int TickStride = 2;

    private static readonly List<PositionTurbulence> active = new(128);

    private Transform myTransform;
    private int activeIndex = -1;
    private float phaseX;
    private float phaseY;
    private float phaseZ;
    private float offsetX;
    private float offsetY;
    private float offsetZ;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Hook()
    {
        active.Clear();
        Application.onBeforeRender -= TickAll;
        Application.onBeforeRender += TickAll;
    }

    private void Awake()
    {
        myTransform = transform;
    }

    private void OnEnable()
    {
        offsetX = 0f;
        offsetY = 0f;
        offsetZ = 0f;
        phaseX = Random.Range(0f, LoopingNoise.Size);
        phaseY = Random.Range(0f, LoopingNoise.Size);
        phaseZ = Random.Range(0f, LoopingNoise.Size);

        activeIndex = active.Count;
        active.Add(this);
    }

    private void OnDisable()
    {
        if (activeIndex < 0) return;

        int last = active.Count - 1;
        active[activeIndex] = active[last];
        active[activeIndex].activeIndex = activeIndex;
        active.RemoveAt(last);
        activeIndex = -1;
    }

    private static void TickAll()
    {
        float deltaTime = Time.deltaTime * TickStride;
        if (deltaTime <= 0f) return;

        float time = Time.time;
        for (int i = Time.frameCount % TickStride; i < active.Count; i += TickStride)
        {
            active[i].Drift(time, deltaTime);
        }
    }

    private void Drift(float time, float deltaTime)
    {
        float walk = time * driftSpeed;
        float radius = driftRadius;

        float ease = smoothing * deltaTime;
        ease /= 1f + ease;

        float stepX = (LoopingNoise.Sample(walk + phaseX) * radius - offsetX) * ease;
        float stepY = (LoopingNoise.Sample(walk + phaseY) * radius - offsetY) * ease;
        float stepZ = (LoopingNoise.Sample(walk + phaseZ) * radius - offsetZ) * ease;

        offsetX += stepX;
        offsetY += stepY;
        offsetZ += stepZ;

        Vector3 position = myTransform.position;
        position.x += stepX;
        position.y += stepY;
        position.z += stepZ;
        myTransform.position = position;
    }
}
