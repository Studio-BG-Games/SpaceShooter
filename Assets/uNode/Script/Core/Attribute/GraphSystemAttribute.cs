using System;

namespace MaxyGames.uNode {
	[System.AttributeUsage(AttributeTargets.Class)]
	public class GraphSystemAttribute : Attribute {
		/// <summary>
		/// The menu of the graph
		/// </summary>
		public string menu;
		/// <summary>
		/// The menu order of the graph, default is 0
		/// </summary>
		public int order;

		/// <summary>
		/// Support modifier modification
		/// </summary>
		public bool supportModifier = true;
		/// <summary>
		/// Support attribute modification
		/// </summary>
		public bool supportAttribute = true;
		/// <summary>
		/// Support generic modification, default is false
		/// </summary>
		public bool supportGeneric = false;
		/// <summary>
		/// Allow the graph to be created in the scene, default is false
		/// </summary>
		public bool allowCreateInScene = false;

		/// <summary>
		/// Support constructor modification
		/// </summary>
		public bool supportConstructor = true;
		/// <summary>
		/// Support property modification
		/// </summary>
		public bool supportProperty = true;
		/// <summary>
		/// Support function modification
		/// </summary>
		public bool supportFunction = true;
		/// <summary>
		/// Support variable modification
		/// </summary>
		public bool supportVariable = true;

		/// <summary>
		/// Allow the graph to be compiled to script by using uNode Editor
		/// </summary>
		public bool allowCompileToScript = true;
		/// <summary>
		/// Allow the graph to be compiled by Full Script Compilation, default is false
		/// </summary>
		public bool allowAutoCompile = false;
		/// <summary>
		/// Allow the uNode editor to preview the generated script
		/// </summary>
		public bool allowPreviewScript = true;
		/// <summary>
		/// The generation mode of graph.
		/// -Default is using global settings
		/// -Performance: is forcing to generate pure script, this will have native performance since it have strongly type reference
		/// But it may give errors when other graph is not compiled into script
		/// -Compatibility: is to ensure the script compatible with all graph even when other graph is not compiled into script
		/// </summary>
		public GenerationKind generationKind = GenerationKind.Default;

		/// <summary>
		/// The inherith type when generating script.
		/// If null, will use GetInherithType from the graph itself.
		/// </summary>
		public Type inherithFrom;

		/// <summary>
		/// The type of the graph calss, this will filled automaticly
		/// </summary>
		public Type type;

        public GraphSystemAttribute(string menu, int order = 0) {
			this.menu = menu;
			this.order = order;
		}
	}
}