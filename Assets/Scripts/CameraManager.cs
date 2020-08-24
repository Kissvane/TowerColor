using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CameraManager : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform cameraRotator;

    public float initialCameraHeight = 0f;
    public float initialZoomLevel = -10f;
    public float topOffset = 1;

    //in game camera manipulation parameters
    public float rotationSpeed = 1f;
    public float rotationTweenDuration = 0.5f;
    public float moveSpeedDuration = 0.5f;

    //intro and win sequence parameters
    public float winSequenceDuration = 2f;
    public Ease upAndDownMoveEase = Ease.InOutSine;
    public Ease introductionRotationEase = Ease.InOutSine;
    public Ease winSequenceUnzoomEase = Ease.InOutSine;

    public EventSystem eventSystem;
    bool detectInput = false;
    Vector3 lastMousePosition = Vector3.zero;
    bool mouseIsDown = false;
    float mouseXDistanceFromStartDrag = 0f;
    float currentCameraRotation = 0f;
    Vector3 startDragPosition = Vector3.zero;
    float startCameraRotation = 0f;
    float targetCameraRotation = 0f;
    float rotationVelocity = 0f;
    bool isDragging = false;


    private void Awake()
    {
        MyEventSystem.instance.RegisterToEvent("GameStart",this, GameStart);
        MyEventSystem.instance.RegisterToEvent("TowerActivated", this, TowerActivated);
        MyEventSystem.instance.RegisterToEvent("Win", this, Win);
        MyEventSystem.instance.Register("UpdatedTowerHeight", this, UpdateCameraHeight);
        cameraRotator.position = new Vector3(cameraRotator.position.x, initialCameraHeight,cameraRotator.position.y);
        cameraTransform.localPosition = new Vector3(cameraTransform.position.x, cameraTransform.position.y, initialZoomLevel);
    }

    #region event reaction
    void GameStart(string name, GenericDictionary args)
    {
        float introductionDuration = args.Get("ConstructionTime")+0.25f+ args.Get("PhysicalActivationTime");
        SetHeightLevel(Mathf.Max(MyEventSystem.instance.Get("TowerHeight")-topOffset,initialCameraHeight), introductionDuration);
        cameraRotator.DORotate(new Vector3(0f, cameraRotator.eulerAngles.y + 360f, 0f), introductionDuration, RotateMode.FastBeyond360).SetEase(introductionRotationEase);
    }

    void UpdateCameraHeight(dynamic height)
    {
        //do not go down lower than initial camera height
        SetHeightLevel(Mathf.Max(height-topOffset,initialCameraHeight), moveSpeedDuration);
    }

    void TowerActivated(string name, GenericDictionary args)
    {
        detectInput = true;
    }

    void Win(string name, GenericDictionary args)
    {
        detectInput = false;
        SetZoomLevel(-15f,winSequenceDuration/2f);
        cameraRotator.DOLocalRotate(cameraRotator.localEulerAngles+Vector3.up*180,winSequenceDuration*3f).SetLoops(-1,LoopType.Incremental).SetEase(Ease.Linear);
    }
    #endregion

    #region reusable functionalities

    public void SetHeightLevel(float wantedHeight, float duration)
    {
        //stop the tween if one is running
        DOTween.Kill("HeightMove");
        //start a new one
        cameraRotator.DOMoveY(wantedHeight, duration).SetEase(upAndDownMoveEase).SetId("HeightMove");
    }

    public void SetZoomLevel(float zoomLevel, float duration)
    {
        cameraTransform.DOLocalMoveZ(zoomLevel, duration).SetEase(winSequenceUnzoomEase);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (detectInput)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseIsDown = true;
                startDragPosition = Input.mousePosition;
                startCameraRotation = cameraRotator.eulerAngles.y;
                currentCameraRotation = startCameraRotation;
                targetCameraRotation = startCameraRotation;
            }
            else if (Input.GetMouseButton(0) && mouseIsDown && !isDragging)
            {
                float distance = Input.mousePosition.x - startDragPosition.x;
                if (Mathf.Abs(distance) >= eventSystem.pixelDragThreshold)
                {
                    isDragging = true;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                mouseIsDown = false;
                isDragging = false;
            }

            if (isDragging && mouseIsDown)
            {
                mouseXDistanceFromStartDrag = startDragPosition.x - Input.mousePosition.x;
                targetCameraRotation = startCameraRotation - mouseXDistanceFromStartDrag * rotationSpeed;
            }

            currentCameraRotation = Mathf.SmoothDamp(currentCameraRotation, targetCameraRotation, ref rotationVelocity, rotationTweenDuration);
            cameraRotator.eulerAngles = new Vector3(0, currentCameraRotation, 0);
        }
    }
    #endregion
}
