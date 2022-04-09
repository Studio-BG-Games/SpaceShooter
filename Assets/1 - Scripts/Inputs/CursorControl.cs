using Sirenix.OdinInspector;
using UnityEngine;

namespace Services.Inputs
{
    public class CursorControl : MonoBehaviour
    {
        public void SetLock(bool value)
        {
            if(value) LockCursor(); 
            else FreeCursor();
        }

        [Button]
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        [Button]
        public void FreeCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}