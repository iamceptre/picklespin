using System.Collections;
using UnityEngine;

public class WhiteEnemyEyeCycler : MonoBehaviour
{
    private WaitForSeconds onTimeQuick = new WaitForSeconds(3);
    private WaitForSeconds onTimeLong = new WaitForSeconds(5);

    private WaitForSeconds offTimeQuick= new WaitForSeconds(3);
    private WaitForSeconds offTimeLong = new WaitForSeconds(7);

    [SerializeField] private WhiteEnemyEye eye;

    [SerializeField] private AiHealth aiHp;
    [SerializeField] private ShowSelectedTip tip;



    private IEnumerator Start()
    {


        while (enabled)
        {

            int random1 = Random01();
            int random2 = Random01();

            if (random1 == 0)
            {
                yield return offTimeLong;
            }
            else
            {
                yield return offTimeQuick;
            }

            eye.On();


            if (random2 == 0)
            {
                yield return onTimeLong;
            }
            else
            {
                yield return onTimeQuick;
            }


            eye.Off();
        }
    }


    public void ShowTipWhenMissedTheThing()
    {
        if (aiHp.hp == 100)
        {
            tip.Show();
        }
    }


    private int Random01()
    {
        int randomized = Random.Range(0, 2);
        //Debug.Log(randomized);
        return randomized;
    }

}
