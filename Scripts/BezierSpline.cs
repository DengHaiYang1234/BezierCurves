using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BezierControlPointMode
{
    Free,
    Aligned,
    Mirrored,
}

public enum CurveMode
{
    None = 1,
    二次曲线 = 2,
    三次曲线 = 3
}

public class BezierSpline : MonoBehaviour
{
    [SerializeField]
    public Vector3[] points;

    [SerializeField]
    private BezierControlPointMode[] pointModes;

    [SerializeField]
    private bool loop;

    [SerializeField]
    private bool uniformVelocity;//匀速贝塞尔曲线

    [SerializeField]
    public CurveMode curveMode = CurveMode.三次曲线;

    //控制点数量
    private int controlPointCount => (int)curveMode + 1;
    //起点到交点的差值
    private int intersectionCount => (int)curveMode;

    public bool UniformVelocity
    {
        get { return uniformVelocity; }
        set
        {
            uniformVelocity = value;
            if (value == true)
            {

            }
        }
    }

    public bool Loop
    {
        get { return loop; }
        set
        {
            loop = value;
            if (value == true)
            {
                pointModes[pointModes.Length - 1] = pointModes[0];
                SetControlPoint(0, points[0]);
            }
        }
    }

    /// <summary>
    /// 控制点总数
    /// </summary>
    /// <value></value>
    public int ControlPointCount
    {
        get { return points.Length; }
    }

    /// <summary>
    /// 获取控制点坐标
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }

    /// <summary>
    /// 设置控制点坐标
    /// </summary>
    /// <param name="index"></param>
    /// <param name="point"></param>
    public void SetControlPoint(int index, Vector3 point)
    {
        if (index % intersectionCount == 0)
        {
            //获取当前控制点变换增量
            Vector3 delta = point - points[index];

            if (loop)//保持环状
            {
                //保证所选择的曲线控制点的前后增量一致
                if (index == 0)
                {
                    points[1] += delta;
                    points[points.Length - 2] += delta;
                    points[points.Length - 1] = point;
                }
                else if (index == points.Length - 1)
                {
                    points[0] = point;
                    points[1] += delta;
                    points[index - 1] += delta;
                }
                else
                {
                    points[index - 1] += delta;
                    points[index + 1] += delta;
                }
            }
            else
            {
                //适配以交点对称的两个点或一个点的增量
                if (index > 0)
                {
                    points[index - 1] += delta;
                }
                if (index + 1 < points.Length)
                {
                    points[index + 1] += delta;
                }
            }

            points[index] = point;
            EnforceMode(index);
        }
    }

    public void Reset()
    {
        if (curveMode == CurveMode.三次曲线)
        {
            points = new Vector3[]
            {
                new Vector3(1f,0f,0f),
                new Vector3(2f,0f,0f),
                new Vector3(3f,0f,0f),
                new Vector3(4f,0f,0f)
            };
        }
        else if (curveMode == CurveMode.二次曲线)
        {
            points = new Vector3[]
            {
                new Vector3(1f,0f,0f),
                new Vector3(2f,0f,0f),
                new Vector3(3f,0f,0f),
            };
        }



        pointModes = new BezierControlPointMode[]
        {
            BezierControlPointMode.Free,
            BezierControlPointMode.Free
        };
    }

    public CurveMode GetCurveMode(int index)
    {
        return (CurveMode)(index + 1);
    }

    public void SetCurveMode(CurveMode mode)
    {
        curveMode = mode;
    }

    /// <summary>
    /// 获取当前控制点的控制模式
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public BezierControlPointMode GetControlPointMode(int index)
    {
        return pointModes[(index + 1) / intersectionCount];
    }

    /// <summary>
    /// 设置当前点的控制模式
    /// </summary>
    /// <param name="index"></param>
    /// <param name="mode"></param>
    public void SetControlPonitMode(int index, BezierControlPointMode mode)
    {
        int modeIndex = (index + 1) / intersectionCount;
        pointModes[modeIndex] = mode;
        if (loop)
        {
            //收尾相连
            if (modeIndex == 0)
            {
                pointModes[pointModes.Length - 1] = mode;
            }
            else if (modeIndex == pointModes.Length - 1)
            {
                pointModes[0] = mode;
            }
        }
        EnforceMode(index);
    }

    /// <summary>
    /// 强制修改模式
    /// </summary>
    /// <param name="index"></param>
    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / intersectionCount;//确定曲线索引
        BezierControlPointMode mode = pointModes[modeIndex];

        //曲线端点两个点和曲线终点两个点舍弃
        if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == pointModes.Length - 1))
            return;

        //获取曲线相交的中点
        int middleIndex = modeIndex * intersectionCount;
        int fixedIndex, enforcedIndex;

        //修正右侧点
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            //修正左侧点
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = points.Length - 2;
            }
        }

        //将与中心对称的另一个点修正与选择的点
        Vector3 middle = points[middleIndex];
        //以中心点对称
        Vector3 enforcedTangent = middle - points[fixedIndex];
        if (mode == BezierControlPointMode.Aligned)
        {
            //以中心点排齐
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
        }
        //修正
        points[enforcedIndex] = middle + enforcedTangent;
    }

    /// <summary>
    /// 曲线总数
    /// </summary>
    /// <value></value>
    public int CurveCount
    {
        get { return (points.Length - 1) / intersectionCount; }
    }

    /// <summary>
    /// 根据插值t获取曲线上对应点坐标
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector3 GetPoint(float t)
    {
        var v = GetCurvePointInofWithT(t);
        int i = (int)v.x;
        t = v.y;

        Vector3 p = Vector3.zero;
        if (curveMode == CurveMode.三次曲线)
        {
            p = Bezier.BezierCurveFunc(new Vector3[] { points[i], points[i + 1], points[i + 2], points[i + 3] }, t);
        }
        else if (curveMode == CurveMode.二次曲线)
        {
            p = Bezier.BezierCurveFunc(new Vector3[] { points[i], points[i + 1], points[i + 2] }, t);
        }

        return transform.TransformPoint(p);
    }

    /// <summary>
    /// 根据插值t获取曲线上对应点方向
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector3 GetVelocity(float t)
    {
        var v = GetCurvePointInofWithT(t);
        int i = (int)v.x;
        t = v.y;

        Vector3 p = Vector3.zero;
        if (curveMode == CurveMode.三次曲线)
        {
            p = Bezier.BezierCurveFunc(new Vector3[] { points[i], points[i + 1], points[i + 2], points[i + 3] }, t) - transform.position;
        }
        else if (curveMode == CurveMode.二次曲线)
        {
            p = Bezier.BezierCurveFunc(new Vector3[] { points[i], points[i + 1], points[i + 2] }, t) - transform.position;
        }

        //速度矢量
        return transform.TransformPoint(p);
    }

    /// <summary>
    /// 根据插值t获取曲线上对应点方向的归一化坐标
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    private Vector2 GetCurvePointInofWithT(float t)
    {
        float i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - controlPointCount;//取最后一段曲线
        }
        else
        {
            //找到当前所属曲线对应的t
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= intersectionCount;
        }

        return new Vector2(i, t);
    }

    public float L(float A, float B, float C, float t)
    {
        if (A < 0.00001f) return 0;

        float temp1 = Mathf.Sqrt(C + t * (B + A * t));
        float temp2 = (2.0f * A * t * temp1 + B * (temp1 - Mathf.Sqrt(C)));
        float temp3 = Mathf.Log(Mathf.Abs(B + 2.0f * Mathf.Sqrt(A) * Mathf.Sqrt(C) + 0.0001f));
        float temp4 = Mathf.Log(Mathf.Abs(B + 2.0f * A * t + 2.0f * Mathf.Sqrt(A) * temp1 + 0.0001f));
        float temp5 = 2.0f * Mathf.Sqrt(A) * temp2;
        float temp6 = (B * B - 4.0f * A * C) * (temp3 - temp4);
        return (float)((temp5 + temp6) / (8.0f * Mathf.Pow(A, 1.5f)));
    }

    public float InvertL(float A, float B, float C, float t, float l)
    {
        float t1 = t;
        float t2 = 0.0f;
        int counter = 0;

        while (true)
        {
            float s = this.S(A, B, C, t1);

            if (s < 0.0001f || float.IsNaN(s) || float.IsNaN(t1))
                break;

            float cl = this.L(A, B, C, t1);
            t2 = t1 - (cl - l) / s;

            if (Mathf.Abs(t1 - t2) < 0.0001f || counter > 50)
            {
                break;
            }

            counter++;
            t1 = t2;
        }

        return t2;
    }

    public float S(float A, float B, float C, float t)
    {
        return Mathf.Sqrt(A * t * t + B * t + C);
    }



    /// <summary>
    /// 添加一段曲线(起点，终点，2个控制点)
    /// </summary>
    public void AddCurve()
    {
        //曲线控制点
        Vector3 point = points[points.Length - 1];

        if (curveMode == CurveMode.二次曲线)
        {
            Array.Resize(ref points, points.Length + 2);
            point.x += 1f;
            points[points.Length - 2] = point;
            point.x += 1f;
            points[points.Length - 1] = point;
        }
        else if (curveMode == CurveMode.三次曲线)
        {
            Array.Resize(ref points, points.Length + 3);
            point.x += 1f;
            points[points.Length - 3] = point;
            point.x += 1f;
            points[points.Length - 2] = point;
            point.x += 1f;
            points[points.Length - 1] = point;
        }

        //曲线调整模式
        Array.Resize(ref pointModes, pointModes.Length + 1);
        pointModes[pointModes.Length - 1] = pointModes[pointModes.Length - 2];
        EnforceMode(points.Length - controlPointCount);

        //曲线呈环状
        if (loop)
        {
            points[points.Length - 1] = points[0];
            pointModes[pointModes.Length - 1] = pointModes[0];
            EnforceMode(0);
        }
    }
}
