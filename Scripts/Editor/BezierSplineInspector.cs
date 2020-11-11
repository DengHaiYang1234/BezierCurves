using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor
{
    //样条曲线
    private BezierSpline spline;
    //拖动对象变换
    private Transform handleTransform;
    //拖动对象旋转
    private Quaternion handleRotation;
    //
    private const int stepsPerCurve = 10;
    private float directionScale = 0.5f;
    private const float handleSize = 0.04f;
    private const float pickSize = 0.06f;
    private int selectedIndex = -1;


    public override void OnInspectorGUI()
    {
        spline = target as BezierSpline;

        //添加loop按钮
        EditorGUI.BeginChangeCheck();
        bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Toggle Loop");
            EditorUtility.SetDirty(spline);
            spline.Loop = loop;
        }


        if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount)
            DrawSelectedPointInspector();

        //添加曲线按钮    
        if (GUILayout.Button("Add Curve"))
        {
            Undo.RecordObject(spline, "Add Curve");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
    }

    private void DrawSelectedPointInspector()
    {
        GUILayout.Label("Selected Point");
        //位移坐标显示
        EditorGUI.BeginChangeCheck();
        Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Move Ponit");
            EditorUtility.SetDirty(spline);
            spline.SetControlPoint(selectedIndex, point);
        }

        //mode枚举
        EditorGUI.BeginChangeCheck();
        BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Change Point Mode");
            spline.SetControlPonitMode(selectedIndex, mode);
            EditorUtility.SetDirty(spline);
        }
    }

    /// <summary>
    /// 场景UI
    /// </summary>
    private void OnSceneGUI()
    {
        spline = target as BezierSpline;
        handleTransform = spline.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        //曲线起点
        Vector3 p0 = ShowPoint(0);

        for (int i = 1; i < spline.ControlPointCount; i += 3)
        {
            //曲线调整点1
            Vector3 p1 = ShowPoint(i);
            //曲线调整点2
            Vector3 p2 = ShowPoint(i + 1);
            //曲线终点
            Vector3 p3 = ShowPoint(i + 2);

            Handles.color = Color.gray;
            //控制点之间用直线表示
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            //贝塞尔曲线API接口
            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;

            //实际贝塞尔曲线显示算法
            // int lineStep = 50;//决定曲线平滑度
            // for (int bezierPonitCount = 0; bezierPonitCount < lineStep; bezierPonitCount++)
            // {
            //     Handles.color = Color.white;
            //     //曲线上的某个点
            //     Vector3 p = spline.GetPoint(bezierPonitCount / (float)lineStep);
            //     Handles.DrawLine(p0, p);
            //     //线段的终点作为下一个点的起点
            //     p0 = p;
            // }
        }
        ShowDirections();
    }

    /// <summary>
    /// 显示曲线某个点的方向
    /// </summary>
    private void ShowDirections()
    {
        Handles.color = Color.green;
        Vector3 point = spline.GetPoint(0f);
        Handles.DrawLine(point, point + spline.GetDirection(0f) * directionScale);
        //每段曲线显示10个点方向
        int steps = stepsPerCurve * spline.CurveCount;
        for (int i = 0; i < steps; i++)
        {
            point = spline.GetPoint(i / (float)steps);
            Handles.DrawLine(point, point + spline.GetDirection(i / (float)steps) * directionScale);
        }
    }

    private static Color[] modeColors =
    {
        Color.white,
        Color.yellow,
        Color.cyan
    };
    
    /// <summary>
    /// 展示曲线控制点
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
        //根据点获取世界空间显示的尺寸
        float size = HandleUtility.GetHandleSize(point);
        if (index == 0)
            size *= 2f;
        Handles.color = modeColors[(int)spline.GetControlPointMode(index)];
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap))
        {
            selectedIndex = index;
            Repaint();
        }

        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck();
            //获取当前控制点的坐标
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Ponit");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
            }
        }
        return point;
    }
}
