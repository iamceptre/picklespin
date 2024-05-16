using TMPro;
using UnityEngine;

public class DamageUI_Spawner : MonoBehaviour
{
    public static DamageUI_Spawner instance { get; private set; }

    [SerializeField] private GameObject damageUi;
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
        whereIshouldGo = whereIshouldGo + new Vector3(0, 3.5f) + (Random.insideUnitSphere); //get collision point in the future, in whereIshouldGo
        var spawnedDamageUi = Instantiate(damageUi, whereIshouldGo, Quaternion.identity);
        spawnedDamageUi.gameObject.GetComponent<DamageUI_V2>().Do(whereIshouldGo, howMuchDamageDealt, isCritical);
        spawnedDamageUi.transform.SetParent(transform);
    }


}
