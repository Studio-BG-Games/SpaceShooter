using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Designates a group member for group behaviour purposes (e.g. for formations).
    /// </summary>
	public class GroupMember : MonoBehaviour
	{

        protected int groupMemberIndex = 0;
        public int GroupMemberIndex { get { return groupMemberIndex; } }
			
	}
}
