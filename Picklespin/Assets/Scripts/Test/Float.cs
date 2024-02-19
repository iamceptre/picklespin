using UnityEngine;
using System.Collections;
 

public class Float : MonoBehaviour {

    public float wysokosc = 0.5f;
    public float predkosc = 1f;
 
    // Position Storage Variables
    Vector3 posOffset = new Vector3 ();
    Vector3 tempPos = new Vector3 ();
 
    void Start () {

        posOffset = transform.position;
    }
     
    void Update () {
 
        // Float up/down with a Sin()
        tempPos = posOffset;
        tempPos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * predkosc) * wysokosc;
 
        transform.position = tempPos;
    }
}