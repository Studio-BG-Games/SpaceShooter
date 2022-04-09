#if ENABLE_INPUT_SYSTEM
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace MaxyGames.uNode.Nodes {
	[AddComponentMenu(""), EventMenu("Input System", "On Input System Event")]
	public class EventOnInputSystemButton : BaseGraphEvent, IExtendedOutput {
		[Serializable]
		public class Data {
			public string id;
			public string name;
			public string assetGuid;
		}
		public enum OutputType {
			Button,
			Float,
			Vector2
		}
		public enum UpdateEvent {
			Update,
			FixedUpdate,
		}

		public enum InputActionOption {
			OnPressed,
			OnHold,
			OnReleased,
		}

		public UpdateEvent updateEvent;
		public InputActionOption inputActionChangeType;
		public OutputType outputType;
		[HideInInspector]
		public Data data = new Data();

		[HideInInspector]
		[ValueIn("Target"), Filter(typeof(PlayerInput), typeof(Component), typeof(GameObject))]
		public MemberData target = MemberData.none;

		private InputAction m_Action;
		private bool m_WasRunning;

		public override void OnRuntimeInitialize() {
			var obj = target.Get() ?? owner;
			if(obj != null) {
				PlayerInput pi = null;
				if(obj is PlayerInput) {
					pi = obj as PlayerInput;
				} else if(obj is GameObject) {
					pi = (obj as GameObject).GetComponent<PlayerInput>();
				} else if(obj is Component) {
					pi = (obj as Component).GetComponent<PlayerInput>();
				}
				if(pi != null) {
					m_Action = pi.actions.FindAction(data.id, false);
					if(m_Action == null) {
						throw new Exception($"No action with id: '{data.id}' in {pi}.\nAction name: {data.name} ( For reference only )");
					}
				} else {
					throw new NullReferenceException("target PlayerInput is null");
				}
			}
			if(updateEvent == UpdateEvent.Update) {
				UEvent.Register(UEventID.Update, owner, OnUpdate);
			} else {
				UEvent.Register(UEventID.FixedUpdate, owner, OnUpdate);
			}
		}

		void OnUpdate() {
			if(m_Action == null)
				return;
			bool shouldtrigger;
			// "Started" is true while the button is held, triggered is true only one frame. hence what looks like a bug but isn't
			switch(inputActionChangeType) {
				case InputActionOption.OnPressed:
					shouldtrigger = m_Action.triggered; // started is true too long
					break;
				case InputActionOption.OnHold:
					shouldtrigger = m_Action.phase == InputActionPhase.Started; // triggered is only true one frame
					break;
				case InputActionOption.OnReleased:
					shouldtrigger = m_WasRunning && m_Action.phase != InputActionPhase.Started; // never equal to InputActionPhase.Cancelled when polling
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			m_WasRunning = m_Action.phase == InputActionPhase.Started;

			if(shouldtrigger)
				Trigger();
		}

		public override void OnGeneratorInitialize() {
			CG.RegisterUserObject(
				new[] {
					CG.RegisterPrivateVariable(nameof(m_Action), typeof(InputAction)),
					inputActionChangeType == InputActionOption.OnReleased ? CG.RegisterPrivateVariable(nameof(m_WasRunning), typeof(bool)) : "",
				}, 
				this
			);
		}

		public override void GenerateCode() {
			var contents = GenerateFlows();
			if(!string.IsNullOrEmpty(contents)) {
				var names = CG.GetUserObject<string[]>(this);
				var startMethod = CG.GetOrRegisterFunction("Start", typeof(void));
				if(target.isAssigned) {
					if(target.type == typeof(PlayerInput)) {
						startMethod.AddCodeForEvent(
							CG.WrapWithInformation(
								CG.Set(
									names[0],
									target.CGValue()
										.CGAccess(nameof(PlayerInput.actions))
										.CGInvoke(
											nameof(PlayerInput.actions.FindAction),
											data.id.CGValue(),
											true.CGValue()
										)
								)
								, this)
						);
					} else {
						startMethod.AddCodeForEvent(
							CG.WrapWithInformation(
								CG.Set(
									names[0],
									target.CGValue()
										.CGInvoke(
											nameof(Component.GetComponent),
											new[] { typeof(PlayerInput) })
										.CGAccess(nameof(PlayerInput.actions))
										.CGInvoke(
											nameof(PlayerInput.actions.FindAction),
											data.id.CGValue(),
											true.CGValue()
										)
								)
								, this)
						);
					}
				} else {
					startMethod.AddCodeForEvent(
						CG.WrapWithInformation(
							CG.Set(
								names[0],
								CG.This
									.CGInvoke(
										nameof(Component.GetComponent),
										new[] { typeof(PlayerInput) })
									.CGAccess(nameof(PlayerInput.actions))
									.CGInvoke(
										nameof(PlayerInput.actions.FindAction),
										data.id.CGValue(),
										true.CGValue()
									)
							)
							, this)
					);
				}
				var mData = CG.GetOrRegisterFunction(updateEvent.ToString(), typeof(void));
				mData.AddCodeForEvent(
					CG.WrapWithInformation(
						CG.If(
							CG.Compare(names[0], CG.Null, ComparisonType.NotEqual),
							CG.Flow(
								inputActionChangeType == InputActionOption.OnPressed ?
									CG.If(
										names[0].CGAccess(nameof(InputAction.triggered)),
										contents
									)
								: inputActionChangeType == InputActionOption.OnHold ?
									CG.If(
										names[0].CGAccess(nameof(InputAction.phase)).CGCompare(InputActionPhase.Started.CGValue()),
										contents
									)
								: inputActionChangeType == InputActionOption.OnReleased ?
									CG.Flow(
										CG.If(
											CG.And(
												names[1],
												names[0].CGAccess(nameof(InputAction.phase)).CGCompare(InputActionPhase.Started.CGValue(), ComparisonType.NotEqual)
											),
											contents
										),
										CG.Set(
											names[1],
											names[0].CGAccess(nameof(InputAction.phase)).CGCompare(InputActionPhase.Started.CGValue())
										)
									)
								: throw new InvalidOperationException()
							)
						)
						, this)
				);
			}
		}

		public override Type GetNodeIcon() {
			return typeof(PlayerInput);
		}

		public override string GetNodeName() {
			switch(outputType) {
				case OutputType.Button:
					return "On Input System Button";
				case OutputType.Float:
					return "On Input System Float";
				case OutputType.Vector2:
					return "On Input System Vector2";
				default:
					throw new InvalidOperationException();
			}
		}


		public int OutputCount {
			get {
				switch(outputType) {
					case OutputType.Float:
					case OutputType.Vector2:
						return 1;
					default:
						return 0;
				}
			}
		}

		public object GetOutputValue(string name) {
			switch(outputType) {
				case OutputType.Float:
					return m_Action.ReadValue<float>();
				case OutputType.Vector2:
					return m_Action.ReadValue<Vector2>();
				case OutputType.Button:
					return m_Action.ReadValue<bool>();
				default:
					throw new InvalidOperationException();
			}
		}

		public string GetOutputName(int index) {
			return "value";
		}

		public Type GetOutputType(string name) {
			switch(outputType) {
				case OutputType.Button:
					return typeof(bool);
				case OutputType.Float:
					return typeof(float);
				case OutputType.Vector2:
					return typeof(Vector2);
				default:
					return typeof(object);
			}
		}

		public string GenerateOutputCode(string name) {
			var names = CG.GetUserObject<string[]>(this);
			switch(outputType) {
				case OutputType.Button:
					return names[0].CGInvoke<bool>(nameof(InputAction.ReadValue));
				case OutputType.Float:
					return names[0].CGInvoke<float>(nameof(InputAction.ReadValue));
				case OutputType.Vector2:
					return names[0].CGInvoke<Vector2>(nameof(InputAction.ReadValue));
				default:
					throw new InvalidOperationException();
			}
		}
	}
}

#if UNITY_EDITOR
namespace MaxyGames.uNode.Editors {
	using UnityEditor;
	using MaxyGames.uNode.Nodes;
	using System.Linq;

	[CustomEditor(typeof(EventOnInputSystemButton), true)]
	public class EventOnInputSystemButtonEditor : EventNodeEditor {
		public override void OnInspectorGUI() {
			var node = target as EventOnInputSystemButton;
			DrawDefaultEditor();
			var rect = EditorGUI.PrefixLabel(uNodeGUIUtility.GetRect(), new GUIContent("Action"));
			var inputAsset = uNodeEditorUtility.LoadAssetByGuid<InputActionAsset>(node.data.assetGuid);
			{//For Update action name if the original is changed.
				if(inputAsset != null) {
					foreach(var actionMap in inputAsset.actionMaps) {
						foreach(var action in actionMap.actions) {
							if(action.id.ToString() == node.data.id) {
								node.data.name = action.name;
							}
						}
					}
				}
			}
			if(GUI.Button(rect, string.IsNullOrEmpty(node.data.name) ? "<None>" : node.data.name)) {
				var assets = uNodeEditorUtility.FindAllAssetsByType<InputActionAsset>();
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("<None>"), string.IsNullOrEmpty(node.data.id), () => {
					node.data.id = "";
					node.data.name = "";
				});
				foreach(var asset in assets) {
					foreach(var actionMap in asset.actionMaps) {
						for(int i=0;i<actionMap.actions.Count;i++) {
							var action = actionMap.actions[i];
							menu.AddItem(new GUIContent($"{asset.name}/{actionMap.name} > {action.name}"), node.data.id == action.id.ToString(), () => {
								node.data.id = action.id.ToString();
								node.data.name = action.name;
								node.data.assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
							});
						}
						//Debug.Log(actionMap);
						//Debug.Log(actionMap + string.Join(", ", actionMap.actions.Select(a => a.name)));
					}
				}
				menu.ShowAsContext();
			}
			if(inputAsset != null) {
				var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(inputAsset));
				foreach(var asset in assets) {
					if(asset is InputActionReference action && action.action?.id.ToString() == node.data.id) {
						EditorGUI.BeginDisabledGroup(true);
						EditorGUI.indentLevel++;
						EditorGUILayout.ObjectField("Action Asset", action, typeof(InputActionReference), false);
						EditorGUI.indentLevel--;
						EditorGUI.EndDisabledGroup();
						break;
					}
				}
			}
			DrawFooter(target as BaseEventNode);
		}
	}
}
#endif
#endif