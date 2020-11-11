using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SplineWalkerMode
{
    Once,
    Loop,
    PingPong
}

public class SplineWalker : MonoBehaviour
{
    public SplineWalkerMode mode;

    public BezierSpline spline;

    public float duration = 1f;

    private float progress = 0;

    private bool gongForward = true;

    public bool lookForward;

    void Update()
    {
        if (gongForward)
        {
            progress += Time.deltaTime / duration;
            if (progress > 1f)
            {
                if (mode == SplineWalkerMode.Once)
                {
                    progress = 1f;
                }
                else if (mode == SplineWalkerMode.Loop)
                {
                    progress -= 1f;
                }
                else
                {
                    progress = 2f - progress;
                    gongForward = false;
                }
            }
        }
        else
        {
            progress -= Time.deltaTime / duration;
            if (progress < 0f)
            {
                progress = -progress;
                gongForward = true;
            }
        }
        
        Vector3 position = spline.GetPoint(progress);
        transform.localPosition = position;
        if (lookForward)
            transform.LookAt(position + spline.GetDirection(progress));
    }
}
