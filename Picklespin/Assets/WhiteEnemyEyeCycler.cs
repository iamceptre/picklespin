using System.Collections;
using UnityEngine;

public class WhiteEnemyEyeCycler : MonoBehaviour
{
    //make it random in the future, i mean create some set variables, and let coroutine random WaitForSeconds from those
    private WaitForSeconds onTime = new WaitForSeconds(4);
    private WaitForSeconds offTime = new WaitForSeconds(6);

    [SerializeField] private GameObject eye;
    [SerializeField] private Collider headshotHitbox;
    [SerializeField] private GameObject eyeShutDown;


    [SerializeField] private AiHealth aiHp;
    [SerializeField] private ShowSelectedTip tip;


    private IEnumerator Start()
    {
        headshotHitbox.enabled = false;

        while (enabled)
        {
            yield return offTime;
            eye.SetActive(true);
            headshotHitbox.enabled = true;
            yield return onTime;
            eye.SetActive(false);
            headshotHitbox.enabled = false;
            eyeShutDown.SetActive(true);
        }
    }


    public void ShowTipWhenMissedTheThing()
    {
        if (aiHp.hp == 100)
        {
            tip.Show();
        }
    }

}
