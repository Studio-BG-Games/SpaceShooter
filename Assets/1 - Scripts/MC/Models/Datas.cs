using Jint.Parser.Ast;
using ModelCore;
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
        [Min(0)]public int Live;
        public uint CurrentLvl;
    }

    [System.Serializable]
    public class GameData
    {
        public C_ShipByPlayer Ship;
        public Entity DataPlayer;
    }
    
    [System.Serializable]
    public class SettingData
    {
        [Range(0, 1)] public float MasterSound;
        [Range(0, 1)] public float MusicSound;
        [Range(0, 1)] public float EffectSound;
    }
}