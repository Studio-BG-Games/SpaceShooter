using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.uNode {
	/// <summary>
	/// Used to save uNode Setting
	/// </summary>
	[AddComponentMenu("")]
	public class uNodeData : uNodeComponentSystem, INamespaceSystem {
		#region Classes
		[System.Serializable]
		public class GeneratorSettings {
			[Tooltip("The namespace of the generated script")]
			public string Namespace = "";
			[Tooltip("List of using namespace of the generated script")]
			public string[] usingNamespace = { "UnityEngine", "System.Collections.Generic" };
			[Tooltip("If true, the script will be able to debug from editor.\nRecommended using prefab for generate with debug mode.")]
			public bool debug = false;
			[Hide("debug", false)]
			[Tooltip("if true, the value node will be able to debug.\nSince this not always success for debug a script, if you have error when using debug try to disable this.")]
			public bool debugValueNode = true;
		}
		#endregion

		/// <summary>
		/// List of delegate in this uNode.
		/// </summary>
		[HideInInspector]
		public DelegateData[] delegates = new DelegateData[0];
		/// <summary>
		/// List of enum in this uNode.
		/// </summary>
		[HideInInspector]
		public EnumData[] enums = new EnumData[0];
		[HideInInspector]
		public InterfaceData[] interfaces = new InterfaceData[0];
		[Hide]
		public GeneratorSettings generatorSettings = new GeneratorSettings();

		/// <summary>
		/// Get / Set namespace of this uNode.
		/// </summary>
		public string Namespace {
			get {
				return generatorSettings.Namespace;
			}
			set {
				generatorSettings.Namespace = value;
			}
		}

		/// <summary>
		/// Get using namespaces
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetNamespaces() {
			if(!string.IsNullOrEmpty(generatorSettings.Namespace)) {
				List<string> namespaces = new List<string>(generatorSettings.usingNamespace);
				var strs = generatorSettings.Namespace.Split('.');
				string ns = string.Empty;
				for(int x = 0; x < strs.Length; x++) {
					if(x != 0) {
						ns += ".";
					}
					ns += strs[x];
					namespaces.Add(ns);
				}
				return namespaces;
			}
			return generatorSettings.usingNamespace;
		}
	}
}