using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ButtonPositionLerper : MonoBehaviour
{
    public Transform origin;
    public Transform destination;
    public float time = 1f;
    public float delay = 1f;
    public Ease ease = Ease.OutBounce;

    // Start is called before the first frame update
    void Awake()
    {
        transform.position = origin.position;
        transform.DOMove(destination.position, time).SetDelay(delay).SetEase(ease);
    }
}
