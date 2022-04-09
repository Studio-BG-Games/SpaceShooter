using System;
using UnityEngine.UIElements;

namespace MaxyGames.uNode.Editors {
	[System.Serializable]
	public class PortData {
		/// <summary>
		/// The ID of the port, this is used to identify the port and connections are created by looking this ID.
		/// </summary>
		public string portID;
		/// <summary>
		/// The owner of the port.
		/// </summary>
		public UNodeView owner;
		/// <summary>
		/// The owner of port data.
		/// </summary>
		public PortView port;
		/// <summary>
		/// Optionally filter for value ports.
		/// </summary>
		public FilterAttribute filter { set; private get; }
		/// <summary>
		/// Optionally data for retrieve information.
		/// </summary>
		public object userData;
		/// <summary>
		/// True if the port is flow port.
		/// </summary>
		public bool isFlow;

		/// <summary>
		/// The port name
		/// </summary>
		/// <value></value>
		public Func<string> getPortName { private get; set; }
		public Func<string> getPortTooltip { private get; set; }
		/// <summary>
		/// The port type. Required for value port but not when filter is assigned.
		/// </summary>
		public Func<Type> getPortType { private get; set; }
		/// <summary>
		/// The port value. Required for input value and output flow ports.
		/// </summary>
		public Func<MemberData> getPortValue { private get; set; }
		/// <summary>
		/// Required for output value and input flow ports.
		/// </summary>
		public Func<MemberData> getConnection { internal get; set; }
		/// <summary>
		/// Required for input value and output flow ports.
		/// </summary>
		public Action<MemberData> onValueChanged { get; set; }

		#region Constructors
		public static PortData CreateForInputValue(string id, Func<string> name, Func<Type> type, Func<MemberData> value, Action<MemberData> onValueChange) {
			return new PortData() {
				portID = id,
				getPortName = name,
				getPortValue = value,
				onValueChanged = onValueChange,
				getPortType = type,
			};
		}

		public static PortData CreateForInputValue(string id, Func<string> name, FilterAttribute filter, Func<MemberData> value, Action<MemberData> onValueChange) {
			return new PortData() {
				portID = id,
				getPortName = name,
				getPortValue = value,
				onValueChanged = onValueChange,
				filter = filter,
			};
		}
		#endregion

		public MemberControl InstantiateControl(bool autoLayout = false) {
			ControlConfig config = new ControlConfig() {
				owner = owner,
				value = GetPortValue(),
				type = GetPortType(),
				filter = GetFilter(),
				onValueChanged = (val) => ChangeValue(val as MemberData),
			};
			return new MemberControl(config, autoLayout);
		}

		private FilterAttribute cachedFilter;
		/// <summary>
		/// Get the filter of this port or create new if none.
		/// </summary>
		/// <returns></returns>
		public FilterAttribute GetFilter() {
			if(filter == null) {
				if(cachedFilter == null) {
					Type t = getPortType?.Invoke();
					if(t != null) {
						cachedFilter = new FilterAttribute(t);
					} else {
						cachedFilter = new FilterAttribute(typeof(object));
					}
				}
				return cachedFilter;
			}
			return filter;
		}

		/// <summary>
		/// Change the port value
		/// </summary>
		/// <param name="value"></param>
		public void ChangeValue(MemberData value) {
			var portValue = GetPortValue();
			if(portValue != null && portValue.IsTargetingPortOrNode && !(owner.targetNode is Nodes.NodeReroute)) {
				var tNode = portValue.GetTargetNode();
				if(tNode is Nodes.NodeValueConverter) {
					NodeEditorUtility.RemoveNode(owner.owner.editorData, tNode);
				}
			}
			onValueChanged?.Invoke(value);
			owner?.OnValueChanged();
		}

		public void ChangeValueWithoutNotify(MemberData value) {
			onValueChanged?.Invoke(value);
			owner?.OnValueChanged();
		}

		/// <summary>
		/// Get the port name
		/// </summary>
		/// <returns></returns>
		public string GetPortName() {
			return getPortName?.Invoke();
		}

		/// <summary>
		/// Get the port tooltip
		/// </summary>
		/// <returns></returns>
		public string GetPortTooltip() {
			return getPortTooltip?.Invoke();
		}

		/// <summary>
		/// Get the port type
		/// </summary>
		/// <returns></returns>
		public Type GetPortType() {
			return getPortType?.Invoke() ?? GetFilter().GetActualType();
		}

		/// <summary>
		/// Get the port value
		/// </summary>
		/// <returns></returns>
		public MemberData GetPortValue() {
			return getPortValue?.Invoke();
		}

		/// <summary>
		/// Get the connection for the port
		/// </summary>
		/// <returns></returns>
		public MemberData GetConnection() {
			return getConnection?.Invoke();
		}
	}
}