using UnityEngine;
using System.Collections;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Collections", "Last Item")]
	[AddComponentMenu("")]
	public class LastItem : ValueNode {
		[Hide, ValueIn, Filter(typeof(IEnumerable))]
		public MemberData target = new MemberData();

		public override System.Type ReturnType() {
			if(target.isAssigned) {
				return target.type.ElementType();
			}
			return typeof(object);
		}

		protected override object Value() {
			var val = target.Get<IEnumerable>();
			if(val is IList list) {
				return list[list.Count -1];
			} else {
				return val.Cast<object>().Last();
			}
		}

		public override string GenerateValueCode() {
			var type = target.type;
			if(type.IsCastableTo(typeof(IList))) {
				return CG.AccessElement(target, CG.Value(target).CGAccess(nameof(IList.Count)).CGSubtract(CG.Value(1)));
			}
			//Because the function is using Linq we need to make sure that System.Linq namespaces is registered.
			CG.RegisterUsingNamespace("System.Linq");
			return CG.GenericInvoke<object>(target, "Cast").CGInvoke("Last");
		}

		public override string GetNodeName() {
			return "Last Item";
		}

		public override string GetRichName() {
			return target.GetNicelyDisplayName(richName:true).Add(".Last");
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
		}
	}
}