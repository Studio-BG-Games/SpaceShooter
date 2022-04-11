using System;
using Jint.Parser.Ast;
using ModelCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Models
{
    public class Datas : MonoBehaviour
    {
        public SettingData Setting;
        public DataSave Save;
        public GameData Game;
    }

    [System.Serializable]
    public class DataSave
    {
        public ObjectValue<int> Live;
        public ObjectValue<int> CurrentLevel;
    }

    [System.Serializable]
    public class GameData
    {
        public Entity DataPlayer;
    }
    
    [System.Serializable]
    public class SettingData
    {
        public ObjectValue<float> MasterSound;
        public ObjectValue<float> MusicSound;
        public ObjectValue<float> EffectSound;
    }
    
    [System.Serializable]
    public class ObjectValue<T>
    {
        public event Action<T> Updated;
        [SerializeField]private T _value;

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                Updated?.Invoke(_value);
            }
        }
    }
}