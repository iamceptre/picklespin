using UnityEngine;

public class DamageUI_Spawner : MonoBehaviour
{
    public static DamageUI_Spawner instance { get; private set; }

    [SerializeField] private DamageUI_V2 damageUi;
    [SerializeField] private RectTransform worldCanvas;

    private Vector3 offset = new Vector3(0,4,0);

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
        whereIshouldGo += Random.insideUnitSphere + offset;
        DamageUI_V2 spawnedDamageUi = Instantiate(damageUi, whereIshouldGo, Quaternion.identity);
        spawnedDamageUi.Do(whereIshouldGo, howMuchDamageDealt, isCritical);
        spawnedDamageUi.transform.SetParent(transform);
    }


}
