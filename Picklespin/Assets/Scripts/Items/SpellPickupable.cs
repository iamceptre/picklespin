using UnityEngine;
using DG.Tweening;
using UnityEngine.Pool;
using FMODUnity;

public class SpellPickupable : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter mySound;
    [SerializeField] private int spellID;
    private int myOccupiedWaypointIndex;

    private UnlockedSpells unlockedSpells;
    private Ammo ammo;
    private SpellSpawner spellSpawnerScript;

    private Light myLight;
    private float myLightRange = 0;
    private Color myLightColor;

    private Renderer rend;

    private ParticleSystem particle;
    private ParticleSystem.EmissionModule emission;

    private Collider myCollider;

    private ObjectPool<SpellPickupable> _pool;

    public int spellClass; //0 - LO, 1 - HI, 2 - FINAl



    private void Awake()
    {
        myLight = gameObject.GetComponentInChildren<Light>();
        rend = GetComponent<Renderer>();
        particle = GetComponentInChildren<ParticleSystem>();
        myCollider = GetComponent<Collider>();

        if (particle != null)
        {
            emission = particle.emission;
        }
    }

    private void Start()
    {
        unlockedSpells = UnlockedSpells.instance;
        ammo = Ammo.instance;
        myLightRange = myLight.range;
        myLightColor = myLight.color;

        if (mySound != null)
        {
            mySound.Play();
        }
    }

    private void OnEnable()
    {
        myCollider.enabled = true;
        //rend.enabled = true;

        if (particle != null)
        {
            emission.enabled = true;
        }

        if (myLightRange != 0)
        {
            myLight.range = myLightRange;
            myLight.color = myLightColor;
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

            unlockedSpells.UnlockASpell(spellID);
            spellSpawnerScript.isSpawnPointTaken[myOccupiedWaypointIndex] = false;
            spellSpawnerScript.avaliableSpawnPointsCount++;
            spellSpawnerScript.ClampSpawnCount();
            FadeOut();
        }

    }

    private void PoolReleaser()
    {
        if (spellClass == 0)
        {
            spellSpawnerScript.spellsLoPool.Release(this);
            return;
        }

        /*

        if (spellClass == 1) {
            spellSpawnerScript.spellsHiPool.Release(this);
            return ;
        }
        */

        // spellSpawnerScript.spellsFinalPool.Release(this); //this does not even need to be a pool, could be a single object

    }



    private void FadeOut()
    {
        myCollider.enabled = false;
        rend.enabled = false;
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

    public void SetOccupiedWaypoint(int myWaypointIndex, SpellSpawner spawnerScript, int spellClassID)
    {
        if (spellSpawnerScript == null)
        {
            spellSpawnerScript = spawnerScript;
        }

        myOccupiedWaypointIndex = myWaypointIndex;
        spellSpawnerScript.isSpawnPointTaken[myOccupiedWaypointIndex] = true;
        spellClass = spellClassID;
    }


    public void SetPool(ObjectPool<SpellPickupable> pool)
    {
        _pool = pool;
    }
}
