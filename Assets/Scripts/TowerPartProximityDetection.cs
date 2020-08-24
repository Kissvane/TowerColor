using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPartProximityDetection : MonoBehaviour
{
    public TowerPart towerPart;

    //update touching parts
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TowerPart"))
        {
            towerPart.touchingParts.Add(other.GetComponent<TowerPart>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TowerPart"))
        {
            towerPart.touchingParts.Remove(other.GetComponent<TowerPart>());
        }
    }
}
