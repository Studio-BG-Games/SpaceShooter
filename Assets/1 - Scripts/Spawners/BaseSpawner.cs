using System;
using DIContainer;
using Services;
using UnityEngine;

namespace Spawners
{
    public abstract class BaseSpawner : MonoBehaviour
    {
        private FactoryUnit _factoryUnit;
        protected FactoryUnit factoryUnit => _factoryUnit??=DiBox.MainBox.ResolveSingle<FactoryUnit>();
        
        public abstract void Generate();
    }
}