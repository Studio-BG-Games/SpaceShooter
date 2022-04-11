using System;
using Dreamteck.Forever;
using MC.Models;
using ModelCore;
using Models;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MC.Controlers
{
    [RequireComponent(typeof(C_GeneratorLevel))]
    public class C_GeneratorLevel : MonoBehaviour
    {
        public LevelGenerator LevelGenerator;
        private Action _callback;
        public Entity PlayerData { get; private set; }

        public void GenerateLevel(Action callback)
        {
            _callback = callback;
            LevelGenerator.onReady += HanlerCallback;
            LevelGenerator.StartGeneration();
        }

        private void HanlerCallback()
        {
            _callback?.Invoke();
            LevelGenerator.onReady -= HanlerCallback;
        }

        public Entity SpawnPlayer()
        {
            var data = EntityAgregator.Instance.Select(x => x.Select<Datas>() != null).Select<Datas>();
            PlayerData = Instantiate(data.Game.DataPlayer, EntityScene.Instance.transform);
            Instantiate(PlayerData.Select<PlayerShipPrefab>().ShipPrefab, LevelGenerator.instance.transform).Init(PlayerData);
            EntityAgregator.Instance.Select(x => x.Has<OnOffInput>()).Select<OnOffInput>().IsOn = true;
            return PlayerData;
        }

        [Button]private void OnValidate()
        {
            LevelGenerator = GetComponent<LevelGenerator>();
            LevelGenerator.buildOnAwake = false;
        }
    }
}