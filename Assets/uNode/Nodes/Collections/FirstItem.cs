using UnityEngine;
using System.Collections;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Collections", "First Item")]
	[AddComponentMenu("")]
	public class FirstItem : ValueNode {
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
				return list[0];
			} else {
				return val.Cast<object>().First();
			}
		}

		public override string GenerateValueCode() {
			var type = target.type;
			if(type.IsCastableTo(typeof(IList))) {
				return CG.AccessElement(target, CG.Value(0));
			}
			//Because the function is using Linq we need to make sure that System.Linq namespaces is registered.
			CG.RegisterUsingNamespace("System.Linq");
			return CG.GenericInvoke<object>(target, "Cast").CGInvoke("First");
		}

		public override string GetNodeName() {
			return "First Item";
		}

		public override string GetRichName() {
			return target.GetNicelyDisplayName(richName:true).Add(".First");
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
		}
	}
}