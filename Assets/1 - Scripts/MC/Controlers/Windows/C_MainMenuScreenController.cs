using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Infrastructure;
using ModelCore;
using Models;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Screen = UnityEngine.Screen;

public class C_MainMenuScreenController : MonoBehaviour
{
    public Object IdWindowsEntity;
    public Button PlayButton;
    
    public TargetWindow Main;
    public TargetWindow Sign;

    public string GameSceneName = "Game";
    public string TutorialSceneName = "Tutor";

    public ScreenModel LoadScreen => C_FadeScreen.Instance.Load.Model;

    [Min(0.1f)] public float DelayLoadLevel = 4;

    void Start() => Wait(0.1f, Init);

    private void Init()
    {
        var entityWindows = EntityAgregator.Instance.Select(x => x.Select<LabelObjectGo>(x => x.IsAlias(IdWindowsEntity)));
        var windows = entityWindows.SelectAll<ScreenModel>(x => true);
        TargetWindows().ForEach(t =>
        {
            var model = windows.First(w => w.Id == t.Id);
            t.Controll.Init(model);
            t.Model = model;
        });

        PlayButton.onClick.AddListener(StartGame);

        Wait(1, () =>
        {
            LoadScreen.Status = false;
            Sign.Model.Status = true;
        });
    }

    public void FakeSign()
    {
        Sign.Model.Status = false;
        Main.Model.Status = true;
    }

    public void StartGame()
    {
        LoadScreen.Status = true;
        Wait(DelayLoadLevel ,()=>SceneLoader.Load(GameSceneName));
    }

    private TargetWindow[] TargetWindows() => new[] { Main, Sign};


    private void Wait(float delay, Action callback) => StartCoroutine(WaitCor(delay, callback));

    private IEnumerator WaitCor(float d, Action delay)
    {
        yield return new WaitForSeconds(d);
        delay.Invoke();
    }
}