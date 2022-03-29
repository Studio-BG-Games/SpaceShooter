using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Parents a first person character's weapon to the camera.
    /// </summary>
    public class FirstPersonCharacterWeaponParenter : MonoBehaviour
    {

        [SerializeField]
        protected Transform weaponsParent;

        /// <summary>
        /// Set the parent of the weapon parent.
        /// </summary>
        /// <param name="newParent">The new parent.</param>
        public void SetParent(Transform newParent)
        {
            weaponsParent.transform.SetParent(newParent);
            weaponsParent.transform.localPosition = Vector3.zero;
            weaponsParent.transform.localRotation = Quaternion.identity;
        }
    }
}
