using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	[AddComponentMenu("")]
	[GraphSystem("Class Asset", order = -100, 
		inherithFrom = typeof(RuntimeAsset), 
		supportAttribute = false, 
		supportGeneric = false, 
		supportModifier = false, 
		supportConstructor = false, 
		supportProperty = false, 
		allowAutoCompile = true, 
		allowCompileToScript = false)]
    public class uNodeClassAsset : BaseRuntimeAssetGraph {
		
	}
}
