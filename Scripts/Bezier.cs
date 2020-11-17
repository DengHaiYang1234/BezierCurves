using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    /// <summary>
    /// 贝塞尔曲线公式
    /// </summary>
    /// <param name="points"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector3 BezierCurveFunc(Vector3[] points, float t)
    {
        int count = points.Length;
        int n = count - 1;
        Vector3 result = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            float combinationNum = CombinationFunc(n, i);
            result.x += (float)(combinationNum * points[i].x * Mathf.Pow((1 - t), n - i) * Mathf.Pow(t, i));
            result.y += (float)(combinationNum * points[i].y * Mathf.Pow((1 - t), n - i) * Mathf.Pow(t, i));
            result.z += (float)(combinationNum * points[i].z * Mathf.Pow((1 - t), n - i) * Mathf.Pow(t, i));
        }
        return result;
    }

    /// <summary>
    /// 组合数公式
    /// </summary>
    /// <param name="n">  </param>
    /// <param name="i">  </param>
    /// <returns></returns>
    private static float CombinationFunc(int n, int m)
    {
        // if (m == 0 || m == 1)
        //     return 1;

        // int n_Factorial = 1;
        // int nm = n - m;
        // int nm_Factorial = 1;

        // for (int k = n; k < 1; k++)
        // {
        //     n_Factorial += n_Factorial * k;
        //     for (int j = k - 1; j >= 1; j--)
        //     {
                
        //     }
        // }

        // for (int k = 2; k <= nm; k++)
        // {
        //     nm_Factorial += nm_Factorial * k;
        // }

        // return n_Factorial / nm_Factorial;

        float[] result = new float[n + 1];
        for (int i = 1; i <= n; i++)
        {
            result[i] = 1;
            for (int j = i - 1; j >= 1; j--)
                result[j] += result[j - 1];
            result[0] = 1;
        }
        return result[m];
    }

    /// <summary>
    /// 获取三次贝塞尔曲线上某点的坐标
    /// </summary>
    /// <param name="p0"> 曲线起点 </param>
    /// <param name="p1"> 曲线终点 </param>
    /// <param name="p2"> 曲线调整点1 </param>
    /// <param name="p3"> 曲线调整点2 </param>
    /// <param name="t"> 曲线插值t </param>
    /// <returns></returns>
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * oneMinusT * p0 +
            3f * oneMinusT * oneMinusT * t * p1 +
            3f * oneMinusT * t * t * p2 +
            t * t * t * p3;
    }

    /// <summary>
    /// 获取二次贝塞尔曲线上某点的坐标
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0 + 2 * t * oneMinusT * p1 + t * t * p2;
    }

    /// <summary>
    /// 获取曲线某点的切线
    /// </summary>
    /// <param name="p0"> 曲线起点 </param>
    /// <param name="p1"> 曲线终点 </param>
    /// <param name="p2"> 曲线调整点1 </param>
    /// <param name="p3"> 曲线调整点2 </param>
    /// <param name="t"> 曲线插值t </param>
    /// <returns></returns>
    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
            3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }
}
