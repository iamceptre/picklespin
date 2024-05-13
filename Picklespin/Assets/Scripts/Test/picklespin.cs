using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class picklespin : MonoBehaviour
{
    [SerializeField] private EventReference picklespinSound;
    [SerializeField] private Transform myTransform;


    private Vector3 rotateAmountVector = new Vector3(0, 0, 180);
    //x and y gets randomized




    private void Awake()
    {
        if (myTransform == null)
        {
            myTransform = GetComponent<Transform>();
        }
        RandomizeAngles();
    }

    private void RandomizeAngles()
    {
        rotateAmountVector = new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), rotateAmountVector.z);
    }

    private void OnEnable()
    {
        //myTransform.position = startingPosition; 
        myTransform.localEulerAngles = Vector3.zero;
        RandomizeAngles();
    }

    private void OnDisable()
    {
        myTransform.localEulerAngles = Vector3.zero;
        RandomizeAngles();
    }

    public void Spin()
    {
        RuntimeManager.PlayOneShot(picklespinSound);

        Vector3 rotationForDotween = new Vector3(rotateAmountVector.x + myTransform.localEulerAngles.x, rotateAmountVector.y + myTransform.localEulerAngles.y, rotateAmountVector.z + myTransform.localEulerAngles.z);
        myTransform.DOLocalRotate(rotationForDotween, 1).SetEase(Ease.OutExpo);
    }

}
