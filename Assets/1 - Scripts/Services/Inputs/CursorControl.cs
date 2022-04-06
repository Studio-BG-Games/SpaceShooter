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

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void FreeCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}