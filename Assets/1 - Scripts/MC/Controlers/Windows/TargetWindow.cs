using Models;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class TargetWindow
{
    public string Id;
    public ScreenControll Controll;
    [ReadOnly] public ScreenModel Model;
}