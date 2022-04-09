using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;
using System.IO;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace MaxyGames.uNode.Editors {
	public static class RoslynUtility {
		#region Fields
		public static IList<Assembly> assemblies;
		#endregion

		#region Compiles
		public static CompileResult CompileScript(IEnumerable<string> scripts) {
			return CompileScript(Path.GetRandomFileName(), scripts);
		}

		public static CompileResult CompileScript(string assemblyName, IEnumerable<string> scripts) {
			if(scripts == null) {
				throw new ArgumentNullException(nameof(scripts));
			}
			var trees = GetSyntaxTrees(scripts);
			return DoCompileAndLoad(assemblyName, trees);
		}

		public static CompileResult CompileFiles(IEnumerable<string> files) {
			return CompileFiles(Path.GetRandomFileName(), files);
		}

		public static CompileResult CompileFiles(string assemblyName, IEnumerable<string> files) {
			if(files == null) {
				throw new ArgumentNullException(nameof(files));
			}
			var trees = GetSyntaxTreesFromFiles(files);
			return DoCompileAndLoad(assemblyName, trees, files.ToArray());
		}

		public static CompileResult CompileScriptAndSave(string assemblyName, IEnumerable<string> scripts, string savePath, bool loadAssembly) {
			if(scripts == null) {
				throw new ArgumentNullException(nameof(scripts));
			}
			var trees = GetSyntaxTrees(scripts);
			return DoCompileAndSave(assemblyName, trees, savePath, loadAssembly: loadAssembly);
		}

		public static CompileResult CompileFilesAndSave(string assemblyName, IEnumerable<string> files, string savePath, bool loadAssembly) {
			if(files == null) {
				throw new ArgumentNullException(nameof(files));
			}
			var trees = GetSyntaxTreesFromFiles(files);
			return DoCompileAndSave(assemblyName, trees, savePath, files.ToArray(), loadAssembly: loadAssembly);
		}
		#endregion

		#region Private Functions
		private static List<MetadataReference> GetMetadataReferences() {
			List<MetadataReference> references = new List<MetadataReference>();
			if(assemblies == null) {
				assemblies = EditorReflectionUtility.GetAssemblies();
			}
			foreach(var assembly in assemblies) {
				try {
					if(assembly != null && !string.IsNullOrEmpty(assembly.Location)) {
						//Skip AssetStoreTools assembly
						if(assembly.GetName().Name.StartsWith("AssetStoreTools", StringComparison.Ordinal))
							continue;
						references.Add(MetadataReference.CreateFromFile(assembly.Location));
					}
				}
				catch { continue; }
			}
			if(uNodePreference.preferenceData.generatorData.compilationMethod == CompilationMethod.Roslyn) {
				if(File.Exists(GenerationUtility.tempAssemblyPath)) {
					references.Add(MetadataReference.CreateFromFile(GenerationUtility.tempAssemblyPath));
				}
			}
			return references;
		}

		private static List<string> GetPreprocessorSymbols() {
			List<string> preprocessorSymbols = new List<string>();
			if(EditorBinding.IsInMainThread()) {
				foreach(var symbol in UnityEditor.EditorUserBuildSettings.activeScriptCompilationDefines) {
					if(symbol.StartsWith("UNITY_EDITOR", StringComparison.Ordinal))
						continue;
					preprocessorSymbols.Add(symbol);
				}
			} else {
				uNodeThreadUtility.QueueAndWait(() => {
					foreach(var symbol in UnityEditor.EditorUserBuildSettings.activeScriptCompilationDefines) {
						if(symbol.StartsWith("UNITY_EDITOR", StringComparison.Ordinal))
							continue;
						preprocessorSymbols.Add(symbol);
					}
				});
			}
			return preprocessorSymbols;
		}

		private static List<Microsoft.CodeAnalysis.SyntaxTree> GetSyntaxTrees(IEnumerable<string> scripts) {
			var result = new List<Microsoft.CodeAnalysis.SyntaxTree>();
			foreach(var script in scripts) {
				var tree = CSharpSyntaxTree.ParseText(script, new CSharpParseOptions(preprocessorSymbols: GetPreprocessorSymbols()));
				result.Add(tree);
			}
			return result;
		}

		private static List<Microsoft.CodeAnalysis.SyntaxTree> GetSyntaxTreesFromFiles(IEnumerable<string> paths) {
			var result = new List<Microsoft.CodeAnalysis.SyntaxTree>();
			foreach(var path in paths) {
				var script = System.IO.File.ReadAllText(path);
				var buffer = System.Text.Encoding.UTF8.GetBytes(script);
				var sourceText = SourceText.From(buffer, buffer.Length, System.Text.Encoding.UTF8, canBeEmbedded: true);
				var tree = CSharpSyntaxTree.ParseText(
					text: sourceText,
					options: new CSharpParseOptions(preprocessorSymbols: GetPreprocessorSymbols()),
					path: path);
				result.Add(tree);
			}
			return result;
		}

		private static CompileResult DoCompileAndLoad(
			string assemblyName,
			IEnumerable<Microsoft.CodeAnalysis.SyntaxTree> syntaxTrees,
			string[] scriptPaths = null) {
			CompileResult result = new CompileResult();
			try {
				List<EmbeddedText> embeddedTexts = null;
				if(scriptPaths != null) {
					List<Microsoft.CodeAnalysis.SyntaxTree> syntaxs = new List<Microsoft.CodeAnalysis.SyntaxTree>();
					embeddedTexts = new List<EmbeddedText>();
					int index = 0;
					foreach(var tree in syntaxTrees) {
						syntaxs.Add(CSharpSyntaxTree.Create(tree.GetRoot() as CSharpSyntaxNode, null, scriptPaths[index], System.Text.Encoding.UTF8));
						embeddedTexts.Add(EmbeddedText.FromSource(scriptPaths[index], tree.GetText()));
						index++;
					}
					syntaxTrees = syntaxs;
				}
				var compilation = CSharpCompilation.Create(
					assemblyName,
					syntaxTrees: syntaxTrees,
					references: GetMetadataReferences(),
					options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug));
				using(var assemblyStream = new MemoryStream())
				using(var symbolsStream = new MemoryStream()) {
					bool useDebug = false;
					EmitResult emitResult;
					if(embeddedTexts != null) {
						useDebug = true;
						var emitOptions = new EmitOptions(
							debugInformationFormat: DebugInformationFormat.PortablePdb,
							pdbFilePath: Path.ChangeExtension(assemblyName, "pdb"));
						emitResult = compilation.Emit(
							assemblyStream,
							symbolsStream,
							embeddedTexts: embeddedTexts,
							options: emitOptions);
					} else {
						emitResult = compilation.Emit(assemblyStream);
					}
					if(emitResult.Success) {
						assemblyStream.Seek(0, SeekOrigin.Begin);
						symbolsStream?.Seek(0, SeekOrigin.Begin);
						if(useDebug) {
							result.assembly = Assembly.Load(assemblyStream.ToArray(), symbolsStream.ToArray());
						} else {
							result.assembly = Assembly.Load(assemblyStream.ToArray());
						}
					} else {
						var failures = emitResult.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);
						List<CompileResult.CompileError> compileErrors = new List<CompileResult.CompileError>();
						foreach(var d in failures) {
							//Debug.LogError($"{d.Id} - {d.GetMessage()}");
							string errorMessage = d.GetMessage();
							int column = 0;
							int line = 0;
							string fileName = string.Empty;
							if(d.Location != null && d.Location.IsInSource && (d.Location.GetLineSpan().IsValid || d.Location.GetMappedLineSpan().IsValid)) {
								var span = d.Location.GetMappedLineSpan().IsValid ? d.Location.GetMappedLineSpan() : d.Location.GetLineSpan();
								line = span.Span.Start.Line + 1;
								column = span.Span.Start.Character + 1;
								fileName = d.Location.SourceTree?.FilePath;
								if(d.Location.IsInSource) {
									errorMessage += " | source script: " + d.Location.SourceTree.ToString().Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length);
								}
							}
							compileErrors.Add(new CompileResult.CompileError() {
								errorColumn = column,
								errorLine = line,
								fileName = fileName,
								errorNumber = d.Id,
								isWarning = d.Severity == DiagnosticSeverity.Warning,
								errorText = errorMessage
							});
						}
						result.errors = compileErrors;
					}
				}
			}
			catch(Exception ex) {
				Debug.LogError(ex);
			}
			return result;
		}

		private static CompileResult DoCompileAndSave(
			string assemblyName,
			IEnumerable<Microsoft.CodeAnalysis.SyntaxTree> syntaxTrees,
			string assemblyPath,
			string[] scriptPaths = null,
			bool loadAssembly = true) {
			CompileResult result = new CompileResult();
			try {
				List<EmbeddedText> embeddedTexts = null;
				if(scriptPaths != null) {
					List<Microsoft.CodeAnalysis.SyntaxTree> syntaxs = new List<Microsoft.CodeAnalysis.SyntaxTree>();
					embeddedTexts = new List<EmbeddedText>();
					int index = 0;
					foreach(var tree in syntaxTrees) {
						syntaxs.Add(CSharpSyntaxTree.Create(tree.GetRoot() as CSharpSyntaxNode, null, scriptPaths[index], System.Text.Encoding.UTF8));
						embeddedTexts.Add(EmbeddedText.FromSource(scriptPaths[index], tree.GetText()));
						index++;
					}
					syntaxTrees = syntaxs;
				}
				var compilation = CSharpCompilation.Create(
					assemblyName,
					syntaxTrees: syntaxTrees,
					references: GetMetadataReferences(),
					options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug));
				using(var assemblyStream = new MemoryStream())
				using(var symbolsStream = new MemoryStream()) {
					bool useDebug = false;
					EmitResult emitResult;
					if(embeddedTexts != null) {
						useDebug = true;
						var emitOptions = new EmitOptions(
							debugInformationFormat: DebugInformationFormat.PortablePdb,
							pdbFilePath: Path.ChangeExtension(assemblyName, "pdb"));
						emitResult = compilation.Emit(
							assemblyStream,
							symbolsStream,
							embeddedTexts: embeddedTexts,
							options: emitOptions);
					} else {
						emitResult = compilation.Emit(assemblyStream);
					}
					if(emitResult.Success) {
						assemblyStream.Seek(0, SeekOrigin.Begin);
						symbolsStream?.Seek(0, SeekOrigin.Begin);
						if(useDebug) {
							if(loadAssembly)
								result.assembly = Assembly.Load(assemblyStream.ToArray(), symbolsStream.ToArray());
							var pdbPath = Path.ChangeExtension(assemblyPath, "pdb");
							File.Open(assemblyPath, FileMode.OpenOrCreate).Close();
							File.Open(pdbPath, FileMode.OpenOrCreate).Close();
							File.WriteAllBytes(assemblyPath, assemblyStream.ToArray());
							File.WriteAllBytes(pdbPath, symbolsStream.ToArray());
						} else {
							if(loadAssembly)
								result.assembly = Assembly.Load(assemblyStream.ToArray());
							File.Open(assemblyPath, FileMode.OpenOrCreate).Close();
							File.WriteAllBytes(assemblyPath, assemblyStream.ToArray());
						}
					} else {
						var failures = emitResult.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error);
						List<CompileResult.CompileError> compileErrors = new List<CompileResult.CompileError>();
						foreach(var d in failures) {
							//Debug.LogError($"{d.Id} - {d.GetMessage()}");
							string errorMessage = d.GetMessage();
							int column = 0;
							int line = 0;
							string fileName = string.Empty;
							if(d.Location != null && d.Location.IsInSource && (d.Location.GetLineSpan().IsValid || d.Location.GetMappedLineSpan().IsValid)) {
								var span = d.Location.GetMappedLineSpan().IsValid ? d.Location.GetMappedLineSpan() : d.Location.GetLineSpan();
								line = span.Span.Start.Line + 1;
								column = span.Span.Start.Character + 1;
								fileName = d.Location.SourceTree?.FilePath;
								if(d.Location.IsInSource) {
									errorMessage += " | source script: " + d.Location.SourceTree.ToString().Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length);
								}
							}
							compileErrors.Add(new CompileResult.CompileError() {
								errorColumn = column,
								errorLine = line,
								fileName = fileName,
								errorNumber = d.Id,
								isWarning = d.Severity == DiagnosticSeverity.Warning,
								errorText = errorMessage
							});
						}
						result.errors = compileErrors;
					}
				}
			}
			catch(Exception ex) {
				Debug.LogError(ex);
			}
			return result;
		}
		#endregion

		public static CompilationUnitSyntax GetSyntaxTree(string script) {
			if(script == null) {
				throw new NullReferenceException("Can't parse, Scripts is null");
			}
			var tree = CSharpSyntaxTree.ParseText(script, new CSharpParseOptions(preprocessorSymbols: GetPreprocessorSymbols()));
			return (CompilationUnitSyntax)tree.GetRoot();
		}

		public static CompilationUnitSyntax GetSyntaxTree(string script, out SemanticModel model) {
			if(script == null) {
				throw new NullReferenceException("Can't parse, Scripts is null");
			}
			var tree = CSharpSyntaxTree.ParseText(script, new CSharpParseOptions(preprocessorSymbols: GetPreprocessorSymbols()));
			var compilation = CSharpCompilation.Create("CSharpParser", syntaxTrees: new[] { tree }, references: GetMetadataReferences());
			model = compilation.GetSemanticModel(tree, true);
			return (CompilationUnitSyntax)tree.GetRoot();
		}
	}
}