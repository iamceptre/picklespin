using UnityEngine;

public class AngelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] angels;
    private AngelMind[] angelMinds;
    private int _rand = 0;

    private AngelPointerHelper pointerHelper;

    private void Awake()
    {
         angels = GameObject.FindGameObjectsWithTag("Angel");
         angelMinds = new AngelMind[angels.Length];
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

        if (!CanSpawnAngel())
        {
            return; 
        }

        RandomizeAngelIndex();

        while (angelMinds[_rand].isActive)
        {
            RandomizeAngelIndex();
        }

        angelMinds[_rand].SetActive(true);
        pointerHelper.PointTo(angels[_rand].transform);
    }

    bool CanSpawnAngel()
    {
        bool allActive = true;

        for (int i = 0; i < angels.Length; i++)
        {
            if (!angelMinds[i].isActive)
            {
                allActive = false;
                break;
            }
        }
        
        if (allActive)
        {
            return false;
        }

        for (int i = 0; i < angels.Length; i++)
        {
            if (angelMinds[i].isActive && !angelMinds[i].healed)
            {
                return false;
            }
        }

        return true;
    }

    private void RandomizeAngelIndex()
    {
        _rand = Random.Range(0, angels.Length);
    }
}
