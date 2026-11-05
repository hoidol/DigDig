
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public  abstract class DrawSO: ScriptableObject
{
    public DrawInfo[] drawInfos;
}
[System.Serializable]
public class DrawInfo
{    
    public GradeType grade;
    public float chance;
}