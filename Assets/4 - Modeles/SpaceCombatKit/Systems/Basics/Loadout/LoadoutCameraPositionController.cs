using UnityEngine;
using System.Collections;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the position and rotation of the camera during loadout.
    /// </summary>
	public class LoadoutCameraPositionController : MonoBehaviour 
	{
	
		[SerializeField]
        protected Camera m_Camera;

		[SerializeField]
		protected float moduleMountViewDistance = 4;

		[SerializeField]
		protected float moveSpeed = 4;

		[SerializeField]
		protected Transform vehicleViewAnchor;
	
		[Header("Dynamic Zoom")]

		[SerializeField]
        protected Vector2 targetCenterViewportPosition = new Vector2(0.5f, 0.6f);
		
		[SerializeField]
        protected float maxViewportVehicleDiameter = 0.33f;
	
		[SerializeField]
        protected bool considerMeshPositions = true;

		public LoadoutManager loadoutManager;
		public LoadoutDisplayManager displayManager;




        private void Update()
        {
            if (loadoutManager.MenuState == LoadoutManager.LoadoutMenuState.ModuleSelection)
            {
				if (loadoutManager.SelectedVehicleIndex != -1 && loadoutManager.SelectedModuleMountIndex != -1)
                {
					ModuleMount moduleMount = loadoutManager.DisplayVehicles[loadoutManager.SelectedVehicleIndex].ModuleMounts[loadoutManager.SelectedModuleMountIndex];
					Vehicle vehicle = loadoutManager.DisplayVehicles[loadoutManager.SelectedVehicleIndex];

					Vector3 targetPosition = moduleMount.transform.position + (moduleMount.transform.position - vehicle.transform.position).normalized;
					targetPosition = moduleMount.transform.position + (targetPosition - moduleMount.transform.position).normalized * moduleMountViewDistance;
					Vector3 lookPosition = moduleMount.transform.position;

					transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
					transform.LookAt(lookPosition);
				}
            }
            else
            {
				if (loadoutManager.SelectedVehicleIndex != -1)
                {
					transform.position = Vector3.Lerp(transform.position, vehicleViewAnchor.position, moveSpeed * Time.deltaTime);
					//transform.LookAt(loadoutManager.DisplayVehicles[loadoutManager.SelectedVehicleIndex].transform.position);
					transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vehicleViewAnchor.forward), moveSpeed * Time.deltaTime);
				}
            }
        }

        protected void DynamicZoomPosition(Vehicle vehicle)
        {
			float diameter = 1;

			// Go through all the meshes on the vehicle to find the one sticking out the furthest, to determine the bounding sphere diameter
			MeshFilter[] meshFilters = vehicle.transform.GetComponentsInChildren<MeshFilter>();
			foreach (MeshFilter meshFilter in meshFilters)
			{

				Mesh mesh = meshFilter.mesh;
				if (mesh == null)
					continue;

				float tempDiameter = Mathf.Max(new float[]{mesh.bounds.size.x * meshFilter.transform.lossyScale.x,
															mesh.bounds.size.y * meshFilter.transform.lossyScale.y,
															mesh.bounds.size.z * meshFilter.transform.lossyScale.z});

				// Take into account if the mesh is offset from the vehicle (increasing the bounding sphere size)
				if (considerMeshPositions)
				{
					Vector3 meshCenterWorldPosition = meshFilter.transform.TransformPoint(mesh.bounds.center);
					Vector3 worldOffset = vehicle.transform.position - meshCenterWorldPosition;
					Vector3 localOffset = vehicle.transform.InverseTransformPoint(worldOffset);

					tempDiameter += localOffset.magnitude * 2;
				}

				diameter = Mathf.Max(diameter, tempDiameter);

			}

			// Get the smaller dimension of the screen for determining the angle used to calculate the distance
			// the camera has to be to achieve the max viewport size set in the inspector
			bool useHorizontalAngle = m_Camera.aspect < 1;
			float halfAngle;
			if (useHorizontalAngle)
			{
				float tmp = 0.5f / Mathf.Tan((m_Camera.fieldOfView / 2) * Mathf.Deg2Rad);
				halfAngle = Mathf.Atan((0.5f * m_Camera.aspect) / tmp) * Mathf.Rad2Deg;
			}
			else
			{
				halfAngle = m_Camera.fieldOfView / 2;
			}

			// Calculate the distance of the camera to the target vehicle to achieve the viewport size
			float distance = ((diameter / 2) / maxViewportVehicleDiameter) / Mathf.Tan(halfAngle * Mathf.Deg2Rad);
			transform.position = vehicle.transform.position - transform.forward * distance;

			// Position the camera such that the target vehicle appears centered at the viewport coordinates
			// set in the inspector
			Vector2 viewportHalfDimensions = Vector2.zero;
			viewportHalfDimensions.x = distance * Mathf.Tan(((m_Camera.fieldOfView * m_Camera.aspect) / 2) * Mathf.Deg2Rad);
			viewportHalfDimensions.y = distance * Mathf.Tan((m_Camera.fieldOfView / 2) * Mathf.Deg2Rad);

			transform.position += -transform.right * ((targetCenterViewportPosition.x - 0.5f) * (viewportHalfDimensions.x * 2));

			transform.position += -transform.up * ((targetCenterViewportPosition.y - 0.5f) * (viewportHalfDimensions.y * 2));

		}
	}
}
