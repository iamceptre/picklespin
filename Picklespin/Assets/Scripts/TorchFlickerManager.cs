using UnityEngine;
using System.Collections.Generic;

public class TorchFlickerManager : MonoBehaviour
{
    public static TorchFlickerManager instance { get; private set; }

    public float flickerSpeed = 1f;
    public float flickerAmplitude = 0.1f;
    public float minLightIntensity = 0.2f;
    public float intensityFlickerSpeed = 1f;

    public bool skipFrames;
    public int framesBetweenUpdates = 1;
    public float cullDistance;
    public Transform cullingCenter;

    private int frameCounter;
    private float cullDistanceSquared;
    private Vector3 cullingCenterPosition;

    private readonly List<TorchFlicker> activeTorches = new List<TorchFlicker>();

    private float[] precomputedSin;
    private float[] precomputedCos;
    private const int precomputeSteps = 360; // Fixed resolution for precomputed values

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        PrecomputeSinCos(precomputeSteps);

        if (cullingCenter == null && Camera.main != null)
        {
            cullingCenter = Camera.main.transform;
        }

        cullDistanceSquared = cullDistance * cullDistance;
    }

    private void Update()
    {
        if (skipFrames && (++frameCounter < framesBetweenUpdates))
        {
            return;
        }
        frameCounter = 0;

        if (cullingCenter != null)
        {
            cullingCenterPosition = cullingCenter.position;
        }

        float deltaTime = Time.deltaTime;

        for (int i = 0; i < activeTorches.Count; i++)
        {
            TorchFlicker torch = activeTorches[i];
            if (cullDistance > 0f && cullingCenter != null)
            {
                float distanceSquared = (torch.cachedTransform.position - cullingCenterPosition).sqrMagnitude;
                bool isCulled = distanceSquared > cullDistanceSquared;

                if (torch.isCulled != isCulled)
                {
                    torch.isCulled = isCulled;
                    if (!isCulled)
                    {
                        torch.ResetFlicker();
                    }
                }

                if (isCulled) continue;
            }

            torch.FlickerUpdate(deltaTime, flickerSpeed, flickerAmplitude, minLightIntensity, intensityFlickerSpeed);
        }
    }

    public void RegisterTorch(TorchFlicker torch)
    {
        if (torch != null && !activeTorches.Contains(torch))
        {
            activeTorches.Add(torch);
            torch.Initialize(precomputeSteps, precomputedSin, precomputedCos);
        }
    }

    public void UnregisterTorch(TorchFlicker torch)
    {
        if (torch != null)
        {
            activeTorches.Remove(torch);
        }
    }

    private void PrecomputeSinCos(int steps)
    {
        float interval = Mathf.PI * 2 / steps;
        precomputedSin = new float[steps];
        precomputedCos = new float[steps];
        for (int i = 0; i < steps; i++)
        {
            float angle = i * interval;
            precomputedSin[i] = Mathf.Sin(angle);
            precomputedCos[i] = Mathf.Cos(angle);
        }
    }
}
