using System;
using Models;
using UnityEngine;

namespace MC.Controlers
{
    public class C_GameLevel : MonoBehaviour
    {
        public C_GeneratorLevel GeneratorLevel;
        public C_PlayerShipUI UiController;
        
        private void Start()
        {
            GeneratorLevel.GenerateLevel(OnLevelGenerate);
        }

        private void OnLevelGenerate()
        {
            C_FadeScreen.Instance.Load.Model.Status = false;
            UiController.Init(GeneratorLevel.SpawnPlayer());
        }
    }
}