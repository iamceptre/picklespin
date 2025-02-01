using System.Collections;
using UnityEngine;

public class FireHurts : MonoBehaviour
{
    private PlayerHP playerHP;
    private const int damage = 10;
    private Coroutine damageCoroutine;


    [SerializeField] private Collider fireTrigger;
    private Transform playerTransform;

    //ADD FIRE DAMAGE SOUNDS PLAYER WITH EVERY DAMAGE HIT


    private static readonly WaitForSeconds waitHalfSecond = new WaitForSeconds(0.5f);

    private void OnEnable()
    {
        if (!playerHP) playerHP = PlayerHP.Instance;
        if (!playerTransform) playerTransform = PublicPlayerTransform.Instance.PlayerTransform;

        StartHurting();
    }

    private void OnDisable()
    {
        StopHurting();
    }

    private void StartHurting()
    {
        if (damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(DamageOverTime());
        }
    }

    private void StopHurting()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator DamageOverTime()
    {
        while (true)
        {
            if (IsPlayerInFire())
            {
                playerHP.ModifyHP(-damage);
            }
            yield return waitHalfSecond;
        }
    }

    private bool IsPlayerInFire()
    {
        return fireTrigger.bounds.Contains(playerTransform.position);
    }
}
