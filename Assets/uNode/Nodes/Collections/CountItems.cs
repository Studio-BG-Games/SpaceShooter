using UnityEngine;
using System.Collections;
using System.Linq;

namespace MaxyGames.uNode.Nodes {
    [NodeMenu("Collections", "Count Items", typeof(int))]
	[AddComponentMenu("")]
	public class CountItems : ValueNode {
		[Hide, ValueIn, Filter(typeof(IEnumerable))]
		public MemberData target = new MemberData();

		public override System.Type ReturnType() {
			return typeof(int);
		}

		protected override object Value() {
			var val = target.Get<IEnumerable>();
			if(val is ICollection col) {
				return col.Count;
			} else {
				return val.Cast<object>().Count();
			}
		}

		public override string GenerateValueCode() {
			var type = target.type;
			if(type.IsCastableTo(typeof(ICollection))) {
				return CG.Access(target, "Count");
			}
			//Because the function is using Linq we need to make sure that System.Linq namespaces is registered.
			CG.RegisterUsingNamespace("System.Linq");
			return CG.GenericInvoke<object>(target, "Cast").CGInvoke("Count");
		}

		public override string GetNodeName() {
			return "Count Items";
		}

		public override string GetRichName() {
			return target.GetNicelyDisplayName(richName:true).Add(".Count");
		}

		public override void CheckError() {
			base.CheckError();
			uNodeUtility.CheckError(target, this, "target");
		}
	}
}