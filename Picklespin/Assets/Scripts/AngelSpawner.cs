using UnityEngine;

public class AngelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] angels;
    private AngelMind[] angelMinds;
    private int _rand = 0;

    private Helper_Arrow helperArrow;

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

        helperArrow = Helper_Arrow.Instance;
        SpawnAngel();
    }


    public void SpawnAngel()
    {
        //Debug.Log("angel spawn function called");

        if (!CanSpawnAngel())
        {
            //Debug.Log("theres an angel already waiting to be healed, returning out of the function");
            return; 
        }

        RandomizeAngelIndex();

        while (angelMinds[_rand].isActive)
        {
            //Debug.Log("while loop ran, re-randomizing the number");
            RandomizeAngelIndex();
        }

        angelMinds[_rand].SetActive(true);
        helperArrow.ShowArrow(angels[_rand].transform);
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
            //Debug.Log("all angels active");
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
        //Debug.Log("randomized number = " + _rand);
    }
}
