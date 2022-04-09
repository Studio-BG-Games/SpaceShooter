using UltEvents;
using UnityEngine;

namespace Models
{
    public class ScreenControll : MonoBehaviour
    {
        private ScreenModel _screenModel;
        public UltEvent Open;
        public UltEvent Close;

        public void Init(ScreenModel screenModel)
        {
            if(_screenModel!=null) _screenModel.StatusChanged -= Handler;
            _screenModel = screenModel;
            _screenModel.StatusChanged += Handler;
        }

        private void Handler(bool obj)
        {
            if(obj) Open.Invoke();
            else Close.Invoke();
        }
    }
}