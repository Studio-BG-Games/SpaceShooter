using ModelCore;
using UltEvents;
using UnityEngine;

namespace MC.Models
{
    public class C_OnOffInput : MonoBehaviour
    {
        public UltEvent On;
        public UltEvent Off;
        private OnOffInput _modelInput;

        private void Start()
        {
            var entity = EntityAgregator.Instance.Select(x => x.Select<OnOffInput>());
            if (!entity)
            {
                Debug.LogError($"Нет модели для включения отключения ввода пользователя - {nameof(OnOffInput)}", this);
                return;
            }
            _modelInput = entity.Select<OnOffInput>(x => true);
            _modelInput.Updated += Handler;
            Handler();
        }

        private void OnDestroy()
        {
            if(_modelInput)
                _modelInput.Updated -= Handler;
        }

        private void Handler()
        {
            if(_modelInput.IsOn) On.Invoke();
            else Off.Invoke();
        }
    }
}