using UnityEngine;
using DG.Tweening;
using UnityEngine.Pool;
using FMODUnity;

public class SpellPickable : MonoBehaviour
{
    [SerializeField, Tooltip("the loop that plays while the pickup lies in the level")]
    private StudioEventEmitter mySound;
    [SerializeField, Tooltip("the one-shot that plays when the player collects the pickup")]
    private StudioEventEmitter pickupSoundEmitter;
    [SerializeField] private SpellId spell;
    private int myOccupiedWaypointIndex;

    public SpellId Spell => spell;

    private const int PotionFlashCount = 3;

    private UnlockedSpells unlockedSpells;
    private SpellSpawner spellSpawnerScript;

    private Light myLight;
    private float myLightRange = 0;
    private Color myLightColor;

    private Renderer rend;

    private ParticleSystem particle;
    private ParticleSystem.EmissionModule emission;

    private Collider myCollider;

    private ObjectPool<SpellPickable> _pool;

    private HandBopAfterItemPickup handBop;

    private ScreenFlashTint screenFlashTint;

    private void Awake()
    {
        myLight = gameObject.GetComponentInChildren<Light>();
        rend = GetComponent<Renderer>();
        particle = GetComponentInChildren<ParticleSystem>();
        myCollider = GetComponent<Collider>();
        myLightRange = myLight.range;
        myLightColor = myLight.color;

        if (particle != null)
        {
            emission = particle.emission;
        }
    }

    private void Start()
    {
        handBop = HandBopAfterItemPickup.instance;
        unlockedSpells = UnlockedSpells.instance;
        screenFlashTint = ScreenFlashTint.instance;
    }

    private void OnEnable()
    {
        SpellAvailability.PickupPlaced(spell);
        myCollider.enabled = true;
        if (rend != null) rend.enabled = true;
        myLight.range = myLightRange;
        myLight.color = myLightColor;

        if (particle != null)
        {
            emission.enabled = true;
        }
    }

    private void OnDisable()
    {
        SpellAvailability.PickupRemoved(spell);
        if (mySound != null)
        {
            mySound.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player")) //SPELL PICKUP
        {

            if (mySound != null)
            {
                mySound.Stop();
            }

            if (pickupSoundEmitter != null)
            {
                pickupSoundEmitter.Play();
            }

            handBop.Do();
            unlockedSpells.UnlockASpell(spell);
            screenFlashTint.Flash(PotionFlashCount + (int)spell, 2f);
            FreeUpSpawnPoint();
            FadeOut();
        }

    }

    private void FreeUpSpawnPoint()
    {
        if (spellSpawnerScript) spellSpawnerScript.ReleasePoint(myOccupiedWaypointIndex);
    }

    private void PoolReleaser()
    {
        if (_pool != null) _pool.Release(this);
        else Destroy(gameObject);
    }

    private void FadeOut()
    {
        myCollider.enabled = false;
        if (rend != null) rend.enabled = false;
        if (particle != null)
        {
            emission.enabled = false;
        }
        LightRangeTweener();
    }

    private void LightRangeTweener()
    {
        DOTween.To(() => myLight.range, x => myLight.range = x, 45, 0.3f).OnComplete(FadeOutLight);
    }

    private void FadeOutLight()
    {
        myLight.DOColor(Color.black, 0.3f).OnComplete(PoolReleaser);
    }

    public void PlaceAt(Vector3 position, int point, SpellSpawner spawnerScript)
    {
        spellSpawnerScript = spawnerScript;
        myOccupiedWaypointIndex = point;
        transform.position = position;

        if (mySound != null)
        {
            mySound.Play();
        }
    }

    public void SetPool(ObjectPool<SpellPickable> pool)
    {
        _pool = pool;
    }
}
