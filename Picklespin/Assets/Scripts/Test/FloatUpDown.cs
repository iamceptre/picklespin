using UnityEngine;
 

public class FloatUpDown : MonoBehaviour {

    public float height = 0.5f;
    public float speed = 1f;
 
    // Position Storage Variables
    Vector3 posOffset = new Vector3 ();
    Vector3 tempPos = new Vector3 ();
 
    void Start () {

        posOffset = transform.localPosition;
    }
     
    void Update () {
 
        // Float up/down with a Sin()
        tempPos = posOffset;
        tempPos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * speed) * height;
 
        transform.localPosition = tempPos;
    }
}