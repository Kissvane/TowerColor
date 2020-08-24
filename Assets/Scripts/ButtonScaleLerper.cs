using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ButtonScaleLerper : MonoBehaviour
{
    public Vector3 origin;
    public Vector3 destination;
    public float duration = 1f;
    public float delay = 1f;
    public Ease ease = Ease.InOutElastic;

    // Start is called before the first frame update
    void Awake()
    {
        transform.localScale = origin;
        transform.DOScale(destination, duration).SetDelay(delay).SetEase(Ease.InOutBounce);
    }
}
