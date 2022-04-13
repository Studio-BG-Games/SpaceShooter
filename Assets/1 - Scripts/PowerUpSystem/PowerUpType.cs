using UnityEngine;

namespace PowerUpSystem
{
    [CreateAssetMenu(fileName = "Power up", menuName = "Game/Power up type", order = 51)]
    public class PowerUpType : ScriptableObject
    {
        [Multiline(15)]public string Description;
    }
}