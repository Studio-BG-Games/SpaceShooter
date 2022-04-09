﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MaxyGames.uNode.Editors {
	public static class CSharpFormatter {
		public static string FormatCode(string source) {
			var root = RoslynUtility.GetSyntaxTree(source);
			return root.NormalizeWhitespace("\t", false).ToFullString();
		}
	}
}