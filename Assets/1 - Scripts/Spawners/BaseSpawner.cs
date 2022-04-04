using System;
using UnityEngine;

namespace Spawners
{
    public abstract class BaseSpawner : MonoBehaviour
    {
        public abstract void Generate();
    }
}