using UnityEngine;
using DG.Tweening;
using System.Collections;

public class SpellPickupable : MonoBehaviour
{
    [SerializeField] private int spellID;
    private UnlockedSpells unlockedSpells;
    private Light myLight;
    private Renderer rend;
    private ParticleSystem particle;
    private ParticleSystem.EmissionModule emission;


    private void Awake()
    {
        unlockedSpells = GameObject.FindWithTag("Player").GetComponent<UnlockedSpells>();
        myLight = gameObject.GetComponentInChildren<Light>();
        rend = gameObject.GetComponent<Renderer>();
        particle = gameObject.GetComponentInChildren<ParticleSystem>();

        if (particle != null)
        {
            emission = particle.emission;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            unlockedSpells.UnlockASpell(spellID);
            FadeOut();
        }

    }

    private void FadeOut()
    {
        rend.enabled = false;
        if (particle != null)
        {
            emission.enabled = false;
        }
        LightRangeTweener();
    }


    private void LightRangeTweener()
    {
        DOTween.To(() => myLight.range, x => myLight.range = x, 45, 0.5f).OnComplete(FadeOutLight);
    }

    private void FadeOutLight()
    {
        myLight.DOColor(Color.black, 0.5f).OnComplete(KillMe);
    }

    private void KillMe()
    {
        Destroy(gameObject);
    }
}
