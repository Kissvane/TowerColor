using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public HashSet<TowerPart> towerParts = new HashSet<TowerPart>();
    public HashSet<GameObject> towerPartsStillOnThatFloor = new HashSet<GameObject>();
    public Transform myTransform;
    public Collider myCollider;
    public bool empty = false;
    public bool isKinematic = false;
    public bool initialized = false;
    public int floorIndex = 0;

    public void AddToFloor(TowerPart[] parts)
    {
        foreach (TowerPart part in parts)
        {
            towerParts.Add(part);
            //towerPartsStillOnThatFloor.Add(part.gameObject);
        }
    }

    public void GoBlack()
    {
        isKinematic = true;
        foreach (TowerPart part in towerParts)
        {
            part.SetInBlackState();
        }
    }

    public void GoColored()
    {
        isKinematic = false;
        foreach (TowerPart part in towerParts)
        {
            if(part != null) part.SetInColoredState();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TowerPart"))
        {
            towerPartsStillOnThatFloor.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TowerPart"))
        {            
            towerPartsStillOnThatFloor.Remove(other.gameObject);
            if (towerPartsStillOnThatFloor.Count == 0 && !empty)
            {
                empty = true;
                MyEventSystem.instance.FireEvent("EmptyFloor");
            }
        }
    }

    public void TowerPartDestructed(TowerPart part)
    {
        towerPartsStillOnThatFloor.Remove(part.gameObject);
        if (towerPartsStillOnThatFloor.Count == 0 && !empty)
        {
            empty = true;
            MyEventSystem.instance.FireEvent("EmptyFloor");
        }
    }
}
