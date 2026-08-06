using UnityEngine;

public class AngelSpawner : MonoBehaviour
{
    public static AngelSpawner instance;

    [SerializeField] private GameObject[] angels;
    private AngelMind[] angelMinds;
    private int[] summonableIndices;

    private AngelPointerHelper pointerHelper;

    private void Awake()
    {
         instance = this;
         angels = GameObject.FindGameObjectsWithTag("Angel");
         angelMinds = new AngelMind[angels.Length];
         summonableIndices = new int[angels.Length];
    }

    private void Start()
    {
        for (int i = 0; i < angels.Length; i++)
        {
            angelMinds[i] = angels[i].GetComponent<AngelMind>();
            angelMinds[i].SetActive(false);
        }

        pointerHelper = AngelPointerHelper.Instance;
        SpawnAngel();
    }

    public void SpawnAngel()
    {
        if (AnAngelIsStillWaiting())
        {
            return;
        }

        int summonableCount = CollectSummonable();

        if (summonableCount == 0)
        {
            return;
        }

        int chosen = summonableIndices[Random.Range(0, summonableCount)];

        angelMinds[chosen].SetActive(true);
        pointerHelper.PointTo(angels[chosen].transform);
    }

    public bool AnAngelIsStillWaiting()
    {
        for (int i = 0; i < angelMinds.Length; i++)
        {
            if (angelMinds[i].isActive && !angelMinds[i].healed && !angelMinds[i].IsDead)
            {
                return true;
            }
        }

        return false;
    }

    private int CollectSummonable()
    {
        int count = 0;

        for (int i = 0; i < angelMinds.Length; i++)
        {
            if (!angelMinds[i].isActive && !angelMinds[i].IsDead)
            {
                summonableIndices[count++] = i;
            }
        }

        return count;
    }
}
