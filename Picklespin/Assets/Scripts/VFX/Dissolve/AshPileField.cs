using UnityEngine;

public class AshPileField : MonoBehaviour
{
    private const int SlotBits = 5;
    private const int SlotMask = (1 << SlotBits) - 1;

    public const int Capacity = 1 << SlotBits;
    public const float HiddenAmount = 0.9f;

    private static readonly int progress = Shader.PropertyToID("_DissolveAmount");

    private static AshPileField instance;

    private readonly Transform[] piles = new Transform[Capacity];
    private readonly Renderer[] renderers = new Renderer[Capacity];
    private readonly Material[] materials = new Material[Capacity];
    private readonly int[] generations = new int[Capacity];
    private int next;

    public static void Prewarm(GameObject template)
    {
        if (instance || !template) return;

        instance = new GameObject(nameof(AshPileField)).AddComponent<AshPileField>();
        instance.Build(template);
    }

    public static int Place(Vector3 position, Quaternion rotation, float scale)
    {
        return instance ? instance.Claim(position, rotation, scale) : -1;
    }

    public static void SetDissolve(int handle, float amount)
    {
        if (!instance || handle < 0) return;

        int slot = handle & SlotMask;
        if (handle >> SlotBits != instance.generations[slot]) return;

        instance.materials[slot].SetFloat(progress, amount);
    }

    private void Build(GameObject template)
    {
        for (int i = 0; i < Capacity; i++)
        {
            GameObject pile = Instantiate(template, transform);
            pile.name = nameof(AshPileField) + i;

            Renderer pileRenderer = pile.GetComponentInChildren<Renderer>(true);
            pileRenderer.enabled = false;

            piles[i] = pile.transform;
            renderers[i] = pileRenderer;
            materials[i] = pileRenderer.material;
            materials[i].SetFloat(progress, HiddenAmount);
        }
    }

    private int Claim(Vector3 position, Quaternion rotation, float scale)
    {
        int slot = next;
        next++;
        if (next >= Capacity) next = 0;

        int generation = generations[slot] + 1;
        generations[slot] = generation;

        materials[slot].SetFloat(progress, HiddenAmount);
        piles[slot].SetPositionAndRotation(position, rotation);
        piles[slot].localScale = new Vector3(scale, scale, scale);
        renderers[slot].enabled = true;
        return (generation << SlotBits) | slot;
    }
}
