using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfGuessObj : MonoBehaviour
{
    public Vector3 resetPos;
    public Material red;
    public Material green;

    private void Start()
    {
        resetPos = transform.position;
    }
    public void PlaceObject(float X)
    {
        transform.position = new Vector3(transform.position.x + X, transform.position.y, transform.position.z);
    }

    public void ResetRound()
    {
        transform.position = resetPos;
    } 
    public void SetColour(bool Exploit)
    {
        if (Exploit)
        {
            this.gameObject.GetComponent<Renderer>().material = green;
            
        }
        else
        {
            this.gameObject.GetComponent<Renderer>().material = red;
           
        }
    }

}
