using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BallManager : MonoBehaviour
{
    public Transform ball;
    public Transform ballSpawn;
    public RectTransform ballSpawnAnchor;
    public float parabolicHeight = 0.5f;
    public float throwDuration = 0.5f;
    public float respawnDelay = 0.1f;
    public Ease throwEase = Ease.InFlash;
    public List<Transform> outOfScreenZones;

    Renderer myRenderer;
    Collider myCollider;
    Rigidbody myRigidbody;
    public bool canThrowBall = false;
    bool isRespawning = false;

    private void Awake()
    {
        myRenderer = GetComponent<Renderer>();
        myCollider = GetComponent<Collider>();
        myRigidbody = GetComponent<Rigidbody>();
        MyEventSystem.instance.RegisterToEvent("TowerPartClicked", this,TowerPartClicked);
        MyEventSystem.instance.RegisterToEvent("TowerActivated", this, TowerActivated);
        MyEventSystem.instance.RegisterToEvent("Win", this, StopBallRespawnWhenWin);
        MyEventSystem.instance.Register("BallsNumberleft",this, StopBallRespawnWhenNoMoreBalls);
        ActivateBall(false);
        MyEventSystem.instance.Set("ballRenderer",myRenderer);
        PositionBallSpawn();
    }

    //responsive positioning of ballrespawn transform based on an UI element
    void PositionBallSpawn()
    {
        ballSpawn.position = Camera.main.ScreenToWorldPoint(new Vector3(ballSpawnAnchor.position.x,ballSpawnAnchor.position.y,4f));
    }

    #region Events reactions
    void TowerPartClicked(string name, GenericDictionary args)
    {
        if(canThrowBall) ThrowBall(args.Get("destination"));
    }

    void TowerActivated(string name, GenericDictionary args)
    {
        
        StartCoroutine(BallRespawnCo());
    }

    void StopBallRespawnWhenWin(string name, GenericDictionary args)
    {
        StopCoroutine("BallRespawnCo");
        ActivateBall(false);
    }

    void StopBallRespawnWhenNoMoreBalls(dynamic ballsNumberleft)
    {
        if (ballsNumberleft < 0)
        {
            StopCoroutine("BallRespawnCo");
            ActivateBall(false);
        }

    }

    #endregion

    void ActivateBall(bool activate)
    {
        myRenderer.enabled = activate;
        myCollider.enabled = activate;
    }

    IEnumerator BallRespawnCo()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);
        //reposition ball
        ball.SetParent(ballSpawn);
        ball.localPosition = Vector3.zero;
        //remove inertia
        myRigidbody.velocity = Vector3.zero;
        myRigidbody.angularVelocity = Vector3.zero;
        //change ball color
        int randomSelectedColorIndex = MyEventSystem.instance.Get("GetRandomSelectedColorIndex");
        myRenderer.material = MyEventSystem.instance.Get("selectedTowerPartMaterials")[randomSelectedColorIndex];
        ActivateBall(true);
        canThrowBall = true;
        isRespawning = false;
    }

    public void ThrowBall(Vector3 destination)
    {
        canThrowBall = false;
        ball.SetParent(null);
        Vector3[] path = {ball.position, (ball.position+destination)/2+Vector3.up*parabolicHeight ,destination};
        //using path
        ball.DOPath(path, throwDuration).SetEase(throwEase).SetId("Throwing"); ;
    }

    void BallRespawn()
    {
        //respawn management
        StartCoroutine("BallRespawnCo");
        ActivateBall(false);
        MyEventSystem.instance.FireEvent("ReduceBallNumber");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isRespawning)
        {
            if (collision.gameObject.CompareTag("TowerPart"))
            {
                TowerPart towerPart = collision.gameObject.GetComponent<TowerPart>();
                //if it's a valid towerpart destroy the group and respawn ball
                if (towerPart.myColor == myRenderer.material.color && !towerPart.myRigidbody.isKinematic)
                {
                    towerPart.GroupDestruction();
                    //respawn management
                    BallRespawn();
                }
                else
                {
                    myCollider.enabled = false;
                    //choose a random zone outside of the screen
                    float distancewithZ0 = Vector3.Distance(outOfScreenZones[0].position, ball.position);
                    float distanceWithZ1 = Vector3.Distance(outOfScreenZones[1].position, ball.position);
                    Transform outOfScreenRandomSelectedZone = outOfScreenZones[0];
                    //we are at equal distance from the two zone, select one randomly
                    if (Mathf.Abs(distancewithZ0-distanceWithZ1) <= 0.05f)
                    {
                        outOfScreenRandomSelectedZone = outOfScreenZones[Random.Range(0, outOfScreenZones.Count)];
                    }
                    else if (distancewithZ0 > distanceWithZ1)
                    {
                        outOfScreenRandomSelectedZone = outOfScreenZones[1];
                    }

                    Vector3 destination = new Vector3(outOfScreenRandomSelectedZone.position.x, ball.position.y + Random.Range(-2f, 2f), outOfScreenRandomSelectedZone.position.z);

                    DOTween.Kill("Throwing");
                    DOTween.Kill("Rebound");
                    ball.DOMove(destination, throwDuration).SetEase(throwEase).OnComplete(BallRespawn).SetId("Rebound");
                }
            }
            
        }
    }
}
