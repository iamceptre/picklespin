using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class ReturnSpellToPoolAfterExplosion : MonoBehaviour
{

    [SerializeField] private Bullet whatToReturn;
    private ObjectPool<Bullet> _pool;
    private ParticleSystem ps;
    private ParticleSystem.MainModule main;
    private float particleDuration;

    private WaitForSeconds particleDurationTime;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        main = ps.main;
        particleDuration = main.duration;
        particleDurationTime = new WaitForSeconds(particleDuration);
    }


    public IEnumerator WaitAndReturn()
    {
        yield return particleDurationTime;
        _pool.Release(whatToReturn);
    }

    public void SetPool(ObjectPool<Bullet> pool)
    {
        _pool = pool;
    }
}
