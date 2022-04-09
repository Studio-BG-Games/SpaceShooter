﻿using System;
using UnityEngine;
using System.Reflection;
using MaxyGames.uNode.Nodes;

namespace MaxyGames.uNode.Editors.PortConverter {
	class CastConverter : AutoConvertPort {
		public override bool CreateNode(System.Action<Node> action) {
			NodeEditorUtility.AddNewNode<Nodes.NodeValueConverter>(
				graph, parent,
				new Vector2(position.x - 250, position.y),
				(nod) => {
					nod.target = GetConnection();
					nod.type = rightType;
					action?.Invoke(nod);
				});
			return true;
			//return NodeEditorUtility.AddNewNode<ASNode>(
			//	graph, parent,
			//		new Vector2(position.x - 250, position.y),
			//		(nod) => {
			//			nod.compactDisplay = true;
			//			nod.type = new MemberData(rightType);
			//			nod.target = GetConnection();
			//		});
		}

		public override bool IsValid() {
			if(force) {
				return !(rightType is RuntimeGraphType || rightType is RuntimeGraphInterface);
			}
			return !(rightType is RuntimeGraphType || rightType is RuntimeGraphInterface) 
				&& 
				(rightType.IsCastableTo(leftType) || 
				rightType == typeof(string) || 
				(leftType == typeof(GameObject) || leftType.IsCastableTo(typeof(Component))) && rightType.IsCastableTo(typeof(Component)));
		}

		public override int order {
			get {
				return int.MaxValue;
			}
		}
	}

	class StringToPrimitiveConverter : AutoConvertPort {
		public override bool CreateNode(System.Action<Node> action) {
			NodeEditorUtility.AddNewNode<Nodes.NodeValueConverter>(
				graph, parent,
				new Vector2(position.x - 250, position.y),
				(nod) => {
					nod.target = GetConnection();
					nod.type = rightType;
					action?.Invoke(nod);
				});
			NodeEditorUtility.AddNewNode<MultipurposeNode>(
				graph, parent,
				new Vector2(position.x - 250, position.y),
				(nod) => {
					nod.target.target = new MemberData(rightType.GetMethod("Parse", new Type[] { typeof(string) }));
					nod.target.parameters = new MemberData[] {
						GetConnection()
					};
					action?.Invoke(nod);
				});
			return true;
		}

		public override bool IsValid() {
			if(leftType == typeof(string)) {
				if(rightType == typeof(float) ||
					rightType == typeof(int) ||
					rightType == typeof(double) ||
					rightType == typeof(decimal) ||
					rightType == typeof(short) ||
					rightType == typeof(ushort) ||
					rightType == typeof(uint) ||
					rightType == typeof(long) ||
					rightType == typeof(byte) ||
					rightType == typeof(sbyte)) {
					return true;
				}
			}
			return false;
		}

		public override int order {
			get {
				return 1000000;
			}
		}
	}

	class ElementToArray : AutoConvertPort {
		public override bool CreateNode(System.Action<Node> action) {
			NodeEditorUtility.AddNewNode<MakeArrayNode>(
				graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.elementType = new MemberData(rightType.GetElementType());
						nod.values[0] = GetConnection();
						action?.Invoke(nod);
					});
			return true;
		}

		public override bool IsValid() {
			return rightType.IsArray && leftType.IsCastableTo(rightType.GetElementType());
		}

		public override int order {
			get {
				return -1;
			}
		}
	}

	// class StringConverter : AutoConvertPort {
	// 	public override Node CreateNode() {
	// 		Node node = leftNode;
	// 		NodeEditorUtility.AddNewNode<MultipurposeNode>(
	// 			graph, parent,
	// 				new Vector2(position.x - 250, position.y),
	// 				(nod) => {
	// 					nod.target.target = new MemberData(typeof(object).GetMethod("ToString", Type.EmptyTypes));
	// 					nod.target.target.instance = GetLeftConnection();
	// 					node = nod;
	// 				});
	// 		return node;
	// 	}

	// 	public override bool IsValid() {
	// 		return rightType == typeof(string);
	// 	}
	// }

	// class GameObjectConverter : AutoConvertPort {
	// 	public override Node CreateNode() {
	// 		Node node = leftNode;
	// 		if(rightType is RuntimeType) {
	// 			return node;
	// 		}
	// 		if(rightType.IsCastableTo(typeof(Component))) {
	// 			NodeEditorUtility.AddNewNode<MultipurposeNode>(
	// 				graph, parent,
	// 				new Vector2(position.x - 250, position.y),
	// 				(nod) => {
	// 					nod.target.target = new MemberData(
	// 						typeof(GameObject).GetMethod("GetComponent", Type.EmptyTypes).MakeGenericMethod(rightType)
	// 					);
	// 					nod.target.target.instance = GetLeftConnection();
	// 					node = nod;
	// 				});
	// 		}
	// 		return node;
	// 	}

	// 	public override bool IsValid() {
	// 		return leftType == typeof(GameObject);
	// 	}
	// }

	class ComponentConverter : AutoConvertPort {
		public override bool CreateNode(System.Action<Node> action) {
			if(leftType == typeof(Transform)) {
				if(rightType == typeof(Vector3)) {
					NodeEditorUtility.AddNewNode<MultipurposeNode>(
						graph, parent,
						new Vector2(position.x - 250, position.y),
						(nod) => {
							nod.target.target = new MemberData(typeof(Transform).GetProperty("position"));
							nod.target.target.instance = GetConnection();
							action?.Invoke(nod);
						});
					return true;
				} else if(rightType == typeof(Quaternion)) {
					NodeEditorUtility.AddNewNode<MultipurposeNode>(
						graph, parent,
						new Vector2(position.x - 250, position.y),
						(nod) => {
							nod.target.target = new MemberData(typeof(Transform).GetProperty("rotation"));
							nod.target.target.instance = GetConnection();
							action?.Invoke(nod);
						});
					return true;
				}
			}
			if(rightType == typeof(GameObject)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(typeof(Component).GetProperty("gameObject"));
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType == typeof(Transform)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
						graph, parent,
						new Vector2(position.x - 250, position.y),
						(nod) => {
							nod.target.target = new MemberData(typeof(Component).GetProperty("transform"));
							nod.target.target.instance = GetConnection();
							action?.Invoke(nod);
						});
				return true;
			} else if(rightType.IsCastableTo(typeof(Component))) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(Component).GetMethod("GetComponent", Type.EmptyTypes).MakeGenericMethod(rightType)
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			}
			return false;
		}

		public override bool IsValid() {
			if(leftType == typeof(Transform)) {
				if(rightType == typeof(Vector3)) {
					return true;
				} else if(rightType == typeof(Quaternion)) {
					return true;
				}
			}
			return false;
			// return leftType.IsCastableTo(typeof(Component));
		}
	}

	class QuaternionConverter : AutoConvertPort {
		public override bool CreateNode(System.Action<Node> action) {
			if(rightType.IsCastableTo(typeof(Vector3))) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(typeof(Quaternion).GetProperty("eulerAngles"));
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			}
			return false;
		}

		public override bool IsValid() {
			return leftType == typeof(Quaternion);
		}
	}

	class Vector3Converter : AutoConvertPort {
		public override bool CreateNode(System.Action<Node> action) {
			if(rightType == typeof(Quaternion)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(Quaternion).GetMethod("Euler", new Type[] { typeof(Vector3) })
						);
						nod.target.parameters = new MemberData[] {
							GetConnection()
						};
						action?.Invoke(nod);
					});
				return true;
			}
			return false;
		}

		public override bool IsValid() {
			return leftType == typeof(Vector3) && rightType == typeof(Quaternion);
		}
	}

	class RaycastHitConverter : AutoConvertPort {
		public override bool CreateNode(System.Action<Node> action) {
			#region RaycastHit
			if(rightType == typeof(Collider)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(RaycastHit).GetProperty("collider")
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType == typeof(Transform)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(RaycastHit).GetProperty("transform")
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType == typeof(Rigidbody)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(RaycastHit).GetProperty("rigidbody")
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType == typeof(GameObject)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							new MemberInfo[] {
								typeof(RaycastHit),
								typeof(RaycastHit).GetProperty("collider"),
								typeof(Collider).GetProperty("gameObject"),
							}
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType.IsCastableTo(typeof(Component))) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							new MemberInfo[] {
								typeof(RaycastHit),
								typeof(RaycastHit).GetProperty("collider"),
								typeof(Collider).GetMethod("GetComponent", Type.EmptyTypes).MakeGenericMethod(rightType),
							}
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType.IsCastableTo(typeof(float))) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(RaycastHit).GetProperty("distance")
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType == typeof(Vector3)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(RaycastHit).GetProperty("point")
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			}
			#endregion
			return false;
		}

		public override bool IsValid() {
			return leftType == typeof(RaycastHit) && (
				rightType == typeof(Collider) ||
				rightType == typeof(Transform) ||
				rightType == typeof(Rigidbody) ||
				rightType == typeof(GameObject) ||
				rightType == typeof(Vector3) ||
				rightType == typeof(Rigidbody) ||
				rightType.IsCastableTo(typeof(float)) ||
				rightType.IsCastableTo(typeof(Component))
			);
		}
	}

	class RaycastHit2DConverter : AutoConvertPort {
		public override bool CreateNode(System.Action<Node> action) {
			#region RaycastHit2D
			if(rightType == typeof(Collider2D)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(RaycastHit2D).GetProperty("collider")
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType == typeof(Transform)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(RaycastHit2D).GetProperty("transform")
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType == typeof(Rigidbody2D)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(RaycastHit2D).GetProperty("rigidbody")
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType == typeof(GameObject)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							new MemberInfo[] {
								typeof(RaycastHit2D),
								typeof(RaycastHit2D).GetProperty("collider"),
								typeof(Collider).GetProperty("gameObject"),
							}
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType.IsCastableTo(typeof(Component))) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							new MemberInfo[] {
								typeof(RaycastHit2D),
								typeof(RaycastHit2D).GetProperty("collider"),
								typeof(Collider).GetMethod("GetComponent", Type.EmptyTypes).MakeGenericMethod(rightType),
							}
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType == typeof(float)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(RaycastHit2D).GetProperty("distance")
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			} else if(rightType == typeof(Vector3)) {
				NodeEditorUtility.AddNewNode<MultipurposeNode>(
					graph, parent,
					new Vector2(position.x - 250, position.y),
					(nod) => {
						nod.target.target = new MemberData(
							typeof(RaycastHit2D).GetProperty("point")
						);
						nod.target.target.instance = GetConnection();
						action?.Invoke(nod);
					});
				return true;
			}
			#endregion
			return false;
		}

		public override bool IsValid() {
			return leftType == typeof(RaycastHit2D) && (
				rightType == typeof(Collider2D) ||
				rightType == typeof(Transform) ||
				rightType == typeof(Rigidbody2D) ||
				rightType == typeof(GameObject) ||
				rightType == typeof(Vector3) ||
				rightType.IsCastableTo(typeof(float)) ||
				rightType.IsCastableTo(typeof(Component))
			);
		}
	}
}