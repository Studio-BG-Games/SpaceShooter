using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ModelCore
{
    public class Timer : MonoBehaviour
    {
        private static  Timer _instance;
        private Dictionary<string, Coroutine> _cors = new Dictionary<string, Coroutine>();

        public static Timer Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                _instance = new GameObject("Cor singlet").AddComponent<Timer>();
                DontDestroyOnLoad(_instance);
                return _instance;
            }
        }

        private void OnDisable() => enabled = true;

        public void StopWait(string id)
        {
            _cors.TryGetValue(id, out var r);
            if(r!=null)
                StopCoroutine(r);
            _cors.Remove(id);
        }
        
        public string Wait(float time, Action callback)
        {
            var id = Guid.NewGuid().ToString();
            _cors.Add(id, StartCoroutine(WaitCor(time, callback, id)));
            return id;
        }

        private IEnumerator WaitCor(float time, Action callbcak, string id)
        {
            yield return new WaitForSeconds(time);
            callbcak?.Invoke();
            _cors.Remove(id);
        }
    }
}