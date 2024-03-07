using UnityEngine;

public class HitboxHandler : MonoBehaviour
{
    private Death death;
    private Win win;

    private void Start()
    {
        GameObject gameMind = GameObject.FindGameObjectWithTag("GameMind");
        death = gameMind.GetComponent<Death>();
        win = gameMind.GetComponent<Win>();
    }


    private void OnTriggerEnter(Collider other)  // change it to events
    {
        if (other.gameObject.CompareTag("EvilEntity"))//Death from evil entity collision
        {
            death.PlayerDeath();
        }

        if (other.gameObject.CompareTag("WinGate"))
        {
            win.PlayerWin();
        }
    }



}
