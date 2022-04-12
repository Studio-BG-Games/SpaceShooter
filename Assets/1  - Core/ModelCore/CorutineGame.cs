using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    public class CorutineGame : MonoBehaviour
    {
        public static CorutineGame Instance => _instance??=Create();
        private static CorutineGame _instance;

        private Dictionary<string, Coroutine> _waitActions = new Dictionary<string, Coroutine>();

        private static CorutineGame Create()
        {
            var objectCor = new GameObject(nameof(CorutineGame));
            DontDestroyOnLoad(objectCor);
            return objectCor.AddComponent<CorutineGame>();
        }

        public void StopWait(string id)
        {
            if(_waitActions.TryGetValue(id, out var r)) StopCoroutine(r);
        }

        public string WaitFrame(int count, Action callback)
        {
            var guid = Guid.NewGuid().ToString();
            _waitActions.Add(guid, StartCoroutine(WaitFrameCorutine(count, callback, guid)));
            return guid;
        }

        public string Wait(float time, Action callback)
        {
            var guid = Guid.NewGuid().ToString();
            _waitActions.Add(guid, StartCoroutine(WaitSecondCorutine(time, callback, guid)));
            return guid;
        }

        private IEnumerator WaitSecondCorutine(float time, Action callback, string myId)
        {
            if (time < 0) time = 0;
            yield return new WaitForSeconds(time);
            callback.Invoke();
            _waitActions.Remove(myId);
        }
        
        private IEnumerator WaitFrameCorutine(int count, Action callback, string myId)
        {
            if (count <= 0) count = 1;
            for (int i = 0; i < count; i++)
            {
                yield return null;
            }
            callback.Invoke();
            _waitActions.Remove(myId);
        }
    }
}