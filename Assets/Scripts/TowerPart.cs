using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class TowerPart : MonoBehaviour, IPointerClickHandler
{
    public HashSet<TowerPart> touchingParts = new HashSet<TowerPart>();
    public Renderer myRenderer;
    public Transform myTransform;
    public Color myColor;
    public Color blackColor;
    public List<TowerPart> recursiveResult;
    public Rigidbody myRigidbody;
    //public GameObject particle;
    public ParticleSystem particle;
    //explosion settings
    public float explosionForce = 1f;
    public float explosionRadius = 1f;
    public float upwardModifier = 1f;

    public bool RecursivelyGetSameColorTouchingParts(ref HashSet<TowerPart> currentGroup, ref HashSet<TowerPart> toProcessNextTurn, ref HashSet<TowerPart> processed)
    {
        bool finished = true;
        List<TowerPart> isTouchedByTheGroup = new List<TowerPart>();

        if (toProcessNextTurn.Count == 0) return true;

        //define the towerpart touching the unprocessed towerpart in group
        foreach (TowerPart currentpart in toProcessNextTurn)
        {
            //this part is not activated we ignore it
            if (currentpart.myRigidbody.isKinematic) continue;

            foreach (TowerPart touched in currentpart.touchingParts)
            {
                //if this part is already destroyed, already processed, will be processed this turn or is not activated we ignore it
                if (touched == null || processed.Contains(touched) || toProcessNextTurn.Contains(touched) || currentGroup.Contains(touched) || touched.myRigidbody.isKinematic) continue;
                isTouchedByTheGroup.Add(touched);
            }
        }
        //delete duplciate
        isTouchedByTheGroup.Distinct();

        //flag the towerParts as processed
        foreach (TowerPart currentPart in toProcessNextTurn)
        {
            if (!processed.Contains(currentPart))
            {
                processed.Add(currentPart);
            }
        }

        //clear the to process list
        toProcessNextTurn.Clear();

        //add same color towerpart in the group 
        foreach (TowerPart currentPart in isTouchedByTheGroup)
        {
            if (currentPart.myColor == myColor && !processed.Contains(currentPart) && !currentGroup.Contains(currentPart))
            {
                currentGroup.Add(currentPart);
                //towerpart added to the group will be the next processed
                toProcessNextTurn.Add(currentPart);
                finished = false;
            }
        }

        return finished;
    }

    public void GroupDestruction()
    {
        HashSet<TowerPart> alreadyProcessed = new HashSet<TowerPart>();
        HashSet<TowerPart> toProcess = new HashSet<TowerPart>();
        HashSet<TowerPart> recursiveResult = new HashSet<TowerPart>();
        HashSet<TowerPart> forceAffectedParts = new HashSet<TowerPart>();
        recursiveResult.Clear();
        recursiveResult.Add(this);
        toProcess.Add(this);

        //get all the same color touching towerpart
        bool funcResult = false;
        while (!funcResult)
        {
            funcResult = RecursivelyGetSameColorTouchingParts(ref recursiveResult, ref toProcess, ref alreadyProcessed);
        }

        MyEventSystem.instance.FireEvent("ProgressiveDestruction", new GenericDictionary().Set("GroupToDestroy",recursiveResult));
        
    }

    

    public void SolitaryDestruction()
    {
        //particles effect
        particle.transform.SetParent(null);
        particle.gameObject.SetActive(true);
        Vector3 explosionOrigin = myTransform.position;
        //add a small explosion force to touching parts
        foreach (TowerPart towerPart in touchingParts)
        {
            if (towerPart != null)
            {
                towerPart.myRigidbody.AddExplosionForce(explosionForce, explosionOrigin, explosionRadius, upwardModifier, ForceMode.VelocityChange);
            }
        }
        //score update
        MyEventSystem.instance.FireEvent("ScoreUpdate", new GenericDictionary().Set("DestructedTowerPart", this));
        //destroy the object
        Destroy(gameObject);
    }

    public void SetInitialMaterial(Material mat)
    {
        myColor = mat.color;
        myRenderer.material = mat;
        ParticleSystem.MainModule main = particle.main;
        main.startColor = new ParticleSystem.MinMaxGradient(mat.color);
    }

    public void SetInColoredState()
    {
        myRenderer.material.color = myColor;
        myRigidbody.isKinematic = false;
        myRigidbody.velocity = Vector3.zero;
        myRigidbody.angularVelocity = Vector3.zero;
    }

    public void SetInBlackState()
    {
        myRenderer.material.color = blackColor;
        myRigidbody.isKinematic = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Sea"))
        {
            SolitaryDestruction();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MyEventSystem.instance.FireEvent("TowerPartClicked", new GenericDictionary().Set("destination", eventData.pointerCurrentRaycast.worldPosition));
    }
}
