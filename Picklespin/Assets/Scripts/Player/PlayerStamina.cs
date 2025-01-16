//MOVE REST OF THE STAMINA CODE FROM PLAYER MOVEMENT HERE
using UnityEngine;
using FMODUnity;

public class PlayerStamina : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private bool canPlayExhaustedSound = true;
    [SerializeField] private EventReference exhaustedSound;

    void Start()
    {
        playerMovement = PlayerMovement.Instance;
    }

    void Update()
    {
        if (playerMovement.stamina == 0 && canPlayExhaustedSound)
        {
            canPlayExhaustedSound = false;
            ExhaustedPlayer(); 
        }

        if (playerMovement.stamina > 20 && !canPlayExhaustedSound)
        {
            canPlayExhaustedSound = true;
        }
        
    }


    private void ExhaustedPlayer()
    {
        //Play Red Light of exhausted bar
        RuntimeManager.PlayOneShot(exhaustedSound);
    }
}
