using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Bezier :MonoBehaviour
{
    private static Bezier instance;
    public static Bezier Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }

    [SerializeField] int curveCount = 50;
    List<Vector2> vecList = new List<Vector2>();

    [SerializeField] int curvePointCount =2;
    [SerializeField] float power=3;
    [SerializeField] float moveSpeed = 1;

    public void MoveTo(Transform tr, Vector2 startPos, Vector2 endPos, Action reached)
    {
        StartCoroutine(ProcessMoveTo(tr, GetBezier(startPos, endPos), reached));
    }
    public void MoveTo(Transform[] trs, Vector2 startPos, Vector2 endPos, Action reached)
    {
        for(int i =0;i< trs.Length; i++)
        {
            StartCoroutine(ProcessMoveTo(trs[i], GetBezier(startPos, endPos), reached));
        }
    }

    IEnumerator ProcessMoveTo(Transform tr, Vector2[] points, Action reached)
    {
        tr.position = points[0];

        float speed = 0.01f;
        float _tempSpeed = moveSpeed + UnityEngine.Random.Range(-moveSpeed/2, moveSpeed);
        for (int i = 0; i < points.Length; i++)
        {
            while (true)
            {
                if (Vector2.SqrMagnitude(points[i] - (Vector2)tr.position) < 0.05f)
                    break;

                speed += Time.deltaTime * _tempSpeed;
                tr.position = Vector2.MoveTowards(tr.position, points[i], speed);
                yield return null;
            }
        }
        reached.Invoke();
    }


    
    public Vector2[] GetBezier(Vector2 startPos, Vector2 endPos)
    {
        vecList.Clear();

        vecList.Add(startPos);

        Vector2 centerPos = (startPos + endPos) / 2;
        for(int i =0;i< curvePointCount; i++)
        {
            vecList.Add(centerPos + UnityEngine.Random.insideUnitCircle * power);
        }

        vecList.Add(endPos);

        return CalculateCurvePoints(vecList, curveCount);
    }

    private  Vector2[] CalculateCurvePoints(List<Vector2> points, int count)
    {
        if (points == null || points.Count < 2) return null;

        Vector2[] curvePoints = new Vector2[count + 1];
        float unit = 1.0f / count;

        int n = points.Count - 1;
        int[] C = GetCombinationValues(n); // nCi
        float[] T = new float[n + 1];      // t^i
        float[] U = new float[n + 1];      // (1-t)^i

        // Iterate curvePoints : 0 ~ count(200)
        int k = 0; float t = 0f;
        for (; k < count + 1; k++, t += unit)
        {
            curvePoints[k] = Vector3.zero;

            T[0] = 1f;
            U[0] = 1f;
            T[1] = t;
            U[1] = 1f - t;

            // T[i] = t^i
            // U[i] = (1 - t)^i
            for (int i = 2; i <= n; i++)
            {
                T[i] = T[i - 1] * T[1];
                U[i] = U[i - 1] * U[1];
            }

            // Iterate Bezier Points : 0 ~ n(number of points - 1)
            for (int i = 0; i <= n; i++)
            {
                curvePoints[k] += C[i] * T[i] * U[n - i] * points[i];
            }
        }
        return curvePoints;
    }

     int[] GetCombinationValues(int n)
    {
        int[] arr = new int[n + 1];

        for (int r = 0; r <= n; r++)
        {
            arr[r] = Combination(n, r);
        }
        return arr;
    }

     int Factorial(int n)
    {
        if (n == 0 || n == 1) return 1;
        if (n == 2) return 2;

        int result = n;
        for (int i = n - 1; i > 1; i--)
        {
            result *= i;
        }
        return result;
    }

     int Permutation(int n, int r)
    {
        if (r == 0) return 1;
        if (r == 1) return n;

        int result = n;
        int end = n - r + 1;
        for (int i = n - 1; i >= end; i--)
        {
            result *= i;
        }
        return result;
    }

     int Combination(int n, int r)
    {
        if (n == r) return 1;
        if (r == 0) return 1;

        // C(n, r) == C(n, n - r)
        if (n - r < r)
            r = n - r;

        return Permutation(n, r) / Factorial(r);
    }
}
