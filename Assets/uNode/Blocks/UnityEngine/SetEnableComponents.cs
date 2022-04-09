using UnityEngine;
using MaxyGames.uNode;
using System.Collections.Generic;

namespace MaxyGames.Events {
	[BlockMenu("UnityEngine", "SetEnableComponents")]
	public class SetEnableComponents : Action {
		[Tooltip("Only support enable or disable class Inherits from Behaviour, Renderer, and Collider")]
		[ObjectType(typeof(Component))]
		public MemberData[] components;
		[ObjectType(typeof(bool))]
		public MemberData setValue;

		protected override void OnExecute() {
			foreach(var variable in components) {
				Component comp = variable.Get<Component>();
				if(comp != null) {
					Behaviour behavior = comp as Behaviour;
					if(behavior != null) {
						behavior.enabled = setValue.Get<bool>();
						continue;
					}
					Renderer rend = comp as Renderer;
					if(rend != null) {
						rend.enabled = setValue.Get<bool>();
						continue;
					}
					Collider col = comp as Collider;
					if(col != null) {
						col.enabled = setValue.Get<bool>();
						continue;
					}
				}
			}
		}

		public override string GenerateCode(Object obj) {
			string data = null;
			foreach(var var in components) {
				if(var.isAssigned) {
					data += CG.Set(
						var.CGValue().CGAccess("enabled"),
						setValue.CGValue()
					);
				}
			}
			return data;
		}
	}
}