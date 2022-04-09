using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	[AddComponentMenu("")]
	[GraphSystem("Class Component", order = -100, 
		inherithFrom = typeof(RuntimeBehaviour), 
		supportAttribute = false, 
		supportGeneric = false, 
		supportModifier = false, 
		supportConstructor = false, 
		allowAutoCompile = true, 
		allowCompileToScript = false)]
    public class uNodeClassComponent : BaseRuntimeComponentGraph {

	}
}
