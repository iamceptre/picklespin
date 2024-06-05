using UnityEngine;

public class DamageUI_Spawner : MonoBehaviour
{
    public static DamageUI_Spawner instance { get; private set; }

    [SerializeField] private DamageUI_V2 damageUi;
    [SerializeField] private RectTransform worldCanvas;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }


    public void Spawn (Vector3 whereIshouldGo, int howMuchDamageDealt, bool isCritical)
    {
        whereIshouldGo = whereIshouldGo + Random.insideUnitSphere;
        DamageUI_V2 spawnedDamageUi = Instantiate(damageUi, whereIshouldGo, Quaternion.identity);
        spawnedDamageUi.Do(whereIshouldGo, howMuchDamageDealt, isCritical);
        spawnedDamageUi.transform.SetParent(transform);
    }


}
