using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerConstructor : MonoBehaviour
{
    public GameObject model;
    public GameObject floorModel;
    public Transform TowerBase;
    public Transform myTransform;
    public int floorNumber = 24;
    public int objectNumberPerFloor = 12;
    float neededRadius = 0f;
    float angleStep = 0f;
    public float totalBuildTime = 3f;
    float physicActivationTime = 2f;
    float buildDelay = 0f;
    [HideInInspector]
    public List<Floor> floors;

    public void Intialization(float _totalBuildTime, float _physicActivationTime)
    {
        totalBuildTime = _totalBuildTime;
        physicActivationTime = _physicActivationTime;
    }

    public IEnumerator ConstructTower()
    {
        buildDelay = totalBuildTime / (float)(objectNumberPerFloor * floorNumber);
        neededRadius = ((model.transform.lossyScale.x / 2) * objectNumberPerFloor) / Mathf.PI;
        angleStep = 360f / (float)objectNumberPerFloor;
        float initialHeight = myTransform.position.y;

        for (int i = 0; i < floorNumber; i++)
        {
            yield return StartCoroutine(ConstructFloor());
            myTransform.position += Vector3.up * model.transform.lossyScale.y * 2;
            myTransform.eulerAngles += Vector3.up * angleStep / 2f;
        }

        myTransform.position = new Vector3(myTransform.position.x, initialHeight, myTransform.position.z);
    }

    public IEnumerator ActivateBlackMode()
    {
        float oneFloorActivationTime = physicActivationTime / (float)floors.Count;
        //go in black mode after the 8 last floors
        for (int i = floors.Count - 1; i >= 0; i--)
        {
            if (i < floors.Count - 8)
            {
                floors[i].GoBlack();
            }
            else
            {
                floors[i].GoColored();
            }
            //activate floor trigger to detect when a floor is empty
            yield return new WaitForSeconds(oneFloorActivationTime);
            floors[i].myCollider.enabled = true;
        }
        MyEventSystem.instance.FireEvent("TowerActivated");
    }

    public void UpdateTowerPhysics()
    {
        //find higher non empty floor 
        int higherNonEmptyFloorIndex = -1;
        for (int i = floors.Count - 1; i >= 0; i--)
        {
            if (floors[i].empty) continue;
            if (higherNonEmptyFloorIndex == -1)
            {
                higherNonEmptyFloorIndex = i;
                MyEventSystem.instance.Set("UpdatedTowerHeight", i * model.transform.lossyScale.y * 2);
            }
            //activate physics on the 7 floors under the higher non empty floor
            if (i+7 >= higherNonEmptyFloorIndex && floors[i].isKinematic)
            {
                floors[i].GoColored();
            }
        }
    }



    IEnumerator ConstructFloor()
    {
        GameObject floorObject = Instantiate(floorModel, Vector3.zero, Quaternion.identity, myTransform);
        floorObject.name = string.Concat("Floor",floors.Count);
        Floor floor = floorObject.GetComponent<Floor>();
        floor.floorIndex = floors.Count;
        floor.myTransform.localPosition = Vector3.zero;
        floors.Add(floor);
        floor.myTransform.SetParent(TowerBase);

        for (int i = 0; i < objectNumberPerFloor; i++)
        {
            GameObject part = Instantiate(model, Vector3.zero, Quaternion.identity, myTransform);
            part.name = string.Concat("TowerPart", floors.Count * objectNumberPerFloor + i);
            TowerPart towerPart = part.GetComponent<TowerPart>();
            towerPart.myTransform.localEulerAngles = Vector3.zero;
            towerPart.myTransform.localPosition = new Vector3(0f, model.transform.lossyScale.y, neededRadius);
            towerPart.myTransform.SetParent(floor.myTransform);
            //color the towerPart
            int randomSelectedColorIndex = MyEventSystem.instance.Get("GetRandomSelectedColorIndex");
            towerPart.SetInitialMaterial(MyEventSystem.instance.Get("selectedTowerPartMaterials")[randomSelectedColorIndex]);

            yield return new WaitForSeconds(buildDelay);
            myTransform.eulerAngles += Vector3.up * angleStep;
        }

        floor.AddToFloor(floor.myTransform.GetComponentsInChildren<TowerPart>());
    }
}
