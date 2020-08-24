using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public List<TowerPart> towerParts;
    public TowerConstructor towerConstructor;
    //end explosion settings
    public float explosionForce = 1f;
    public float explosionRadius = 1f;
    public float upwardModifier = 1f;
    public float finalDestructionDuration = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        int level = 1;
        if (PlayerPrefs.HasKey("Level"))
        {
            level = PlayerPrefs.GetInt("Level");
        }
        towerConstructor.floorNumber += level;

        MyEventSystem.instance.Set("InitialTowerPartsNumber", towerConstructor.objectNumberPerFloor * towerConstructor.floorNumber);
        MyEventSystem.instance.Set("TowerHeight", towerConstructor.floorNumber * towerConstructor.model.transform.lossyScale.y * 2);
        MyEventSystem.instance.RegisterToEvent("GameStart", this, GameStart);
        MyEventSystem.instance.RegisterToEvent("Win", this, Win);
        MyEventSystem.instance.RegisterToEvent("EmptyFloor", this, EmptyFloor);
        MyEventSystem.instance.RegisterToEvent("ScoreUpdate", this, ScoreUpdate);
    }

    #region eventReactions

    void GameStart(string name, GenericDictionary args)
    {
        towerConstructor.Intialization(args.Get("ConstructionTime"), args.Get("PhysicalActivationTime"));
        StartCoroutine(ConstructTowerThenActivatePhysic());
    }

    
    void Win(string name, GenericDictionary args)
    {
        StartCoroutine(EjectRemainingTowerParts());
    }

    void EmptyFloor(string name, GenericDictionary args)
    {
        towerConstructor.UpdateTowerPhysics();
    }

    void ScoreUpdate(string name, GenericDictionary args)
    {
        TowerPart part = args.Get("DestructedTowerPart");
        foreach(Floor floor in towerConstructor.floors)
        {
            floor.TowerPartDestructed(part);
        }
    }
    #endregion

    IEnumerator ConstructTowerThenActivatePhysic()
    {
        yield return StartCoroutine(towerConstructor.ConstructTower());
        yield return new WaitForSeconds(0.25f);
        yield return StartCoroutine(towerConstructor.ActivateBlackMode());
        foreach(Floor floor in towerConstructor.floors)
        {
            towerParts.AddRange(floor.towerParts);
        }
    }

    [ContextMenu("TEST")]
    public void TEST()
    {
        StartCoroutine(EjectRemainingTowerParts());
    }

    public IEnumerator EjectRemainingTowerParts()
    {
        yield return new WaitForSeconds(1f);

        List<TowerPart> nonNullTowerPart = new List<TowerPart>();
        foreach (TowerPart towerPart in towerParts)
        {
            if (towerPart != null)
            {
                towerPart.myRigidbody.AddExplosionForce(explosionForce,towerConstructor.myTransform.position, explosionRadius, upwardModifier, ForceMode.VelocityChange);
                nonNullTowerPart.Add(towerPart);
            }
        }

        yield return new WaitForSeconds(0.5f);

        float stepDestructionDuration = finalDestructionDuration / (float)nonNullTowerPart.Count;

        foreach (TowerPart towerPart in nonNullTowerPart)
        {
            if (towerPart != null)
            {
                towerPart.SolitaryDestruction();
                yield return new WaitForSeconds(stepDestructionDuration);
            }
        }
    }
}
