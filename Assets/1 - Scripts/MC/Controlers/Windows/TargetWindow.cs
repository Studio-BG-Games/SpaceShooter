using Models;
using UnityEngine;

[System.Serializable]
public class TargetWindow
{
    public string Id;
    public ScreenControll Controll;
    [HideInInspector] public ScreenModel Model;
}