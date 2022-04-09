using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace MaxyGames.uNode.Editors {
	public class ConversionEdgeView : EdgeView {
		public Nodes.NodeValueConverter node;

		public ConversionEdgeView(Nodes.NodeValueConverter node, PortView input, PortView output) : base(input, output) {
			this.node = node;
			if(iMGUIContainer == null) {
				iMGUIContainer = new IMGUIContainer(OnGUI);
				iMGUIContainer.style.flexGrow = 1;
				iMGUIContainer.style.flexShrink = 0;
				iMGUIContainer.pickingMode = PickingMode.Ignore;
				edgeControl.Add(iMGUIContainer);
			}
		}

		protected override void OnGUI() {
			DebugGUI(false);
			if(Event.current.type == EventType.Repaint) {
				if(uNodeEditor.editorErrors != null && uNodeEditor.editorErrors.ContainsKey(node)) {
					var errors = uNodeEditor.editorErrors[node];
					if(errors != null && errors.Count > 0) {
						//System.Text.StringBuilder sb = new System.Text.StringBuilder();
						//for(int i = 0; i < errors.Count; i++) {
						//	if(i != 0) {
						//		sb.AppendLine();
						//		sb.AppendLine();
						//	}
						//	sb.Append("-" + uNodeEditorUtility.RemoveHTMLTag(errors[i].message));
						//}
						Vector2 v2 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[1]);
						Vector2 v3 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[2]);
						Vector2 vec = (v2 + v3) / 2;
						GUI.DrawTexture(new Rect(vec.x - 8, vec.y - 8, 16, 16), uNodeEditorUtility.GetTypeIcon(typeof(TypeIcons.MissingIcon)));
						return;
					}
				}
				{//Icon
					Vector2 v2 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[1]);
					Vector2 v3 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[2]);
					Vector2 vec = (v2 + v3) / 2;
					GUI.DrawTexture(new Rect(vec.x - 9, vec.y - 9, 18, 17), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(1, 1, 1, 0.5f), 1, 4);
					GUI.DrawTexture(new Rect(vec.x - 8, vec.y - 8, 16, 16), uNodeEditorUtility.GetTypeIcon(node.GetNodeIcon()));
					//GUI.DrawTexture(new Rect(vec.x - 17, vec.y - 10, 34, 18), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(1, 1, 1, 0.3f), 1, 4);
					//GUI.DrawTexture(new Rect(vec.x - 16, vec.y - 8, 16, 16), uNodeEditorUtility.GetTypeIcon(node.GetNodeIcon()));
					//GUI.DrawTexture(new Rect(vec.x, vec.y - 8, 16, 16), uNodeEditorUtility.GetTypeIcon(node.ReturnType()));
				}
			}
		}
	}

	public class EdgeView : Edge {
		public bool isProxy;
		protected IMGUIContainer iMGUIContainer;

		public bool isHidding;

		//private UGraphView graphView;

		public PortView Output => output as PortView;
		public PortView Input => input as PortView;

		public bool isFlow => Output?.isFlow == true || Input?.isFlow == true;

		public EdgeView() {
			ReloadView();
		}

		public EdgeView(PortView input, PortView output) {
			this.input = input;
			this.output = output;
			var port = input;
			if(port == null) {
				port = output;
			}
			if(port != null) {
				if(port.isFlow) {
					AddToClassList("flow");
				} else {
					AddToClassList("value");
				}
			}
			if(input != null && output != null) {
				RegisterCallback<MouseDownEvent>((e) => {
					if(isValid) {
						if(e.button == 0 && e.clickCount == 2) {
							var owner = input?.owner?.owner ?? output?.owner?.owner;
							if(owner != null) {
								var graph = owner.graph;
								var mPos = this.ChangeCoordinatesTo(owner.contentViewContainer, e.localMousePosition);
								Undo.SetCurrentGroupName("Create Reroute");
								NodeEditorUtility.AddNewNode<Nodes.NodeReroute>(graph.editorData, null, null, new Vector2(mPos.x, mPos.y), (node) => {
									if(port.isFlow) {
										node.kind = Nodes.NodeReroute.RerouteKind.Flow;
										node.onFinished = input.GetConnection();
										output.ChangeValue(MemberData.FlowInput(node));
									} else {
										node.kind = Nodes.NodeReroute.RerouteKind.Value;
										if(this is ConversionEdgeView) {
											node.target = MemberData.ValueOutput((this as ConversionEdgeView).node);
										} else {
											node.target = output.GetConnection();
										}
										input.portData.ChangeValueWithoutNotify(MemberData.ValueOutput(node));
									}
								});
								graph.Refresh();
								e.StopImmediatePropagation();
							}
						}
					}
				});
			}
			//if(input != null) {
			//	graphView = input.owner?.owner;
			//} else if(output != null) {
			//	graphView = output.owner?.owner;
			//}
			ReloadView();
		}

		public void ReloadView() {
			if(input == null || output == null) return;
			if(input.direction == Direction.Input && Input.isValue) {
				PortView port = input as PortView;
				MemberData member = port.portData.GetPortValue();
				isProxy = member != null && member.IsProxy();
			} else if(output.direction == Direction.Output && Output.isFlow) {
				PortView port = output as PortView;
				MemberData member = port.portData.GetPortValue();
				isProxy = member != null && member.IsProxy();
			}
			if(isProxy) {
				edgeControl.visible = false;
				edgeControl.SetEnabled(false);
			}
			#region Debug
			if(Application.isPlaying && GraphDebug.useDebug) {
				//if(graphView != null) {
				//	graphView.RegisterIMGUI(this, DebugGUI);
				//}
				//iMGUIContainer = graphView.IMGUIContainer;
				if(iMGUIContainer == null) {
					iMGUIContainer = new IMGUIContainer(OnGUI);
					iMGUIContainer.style.flexGrow = 1;
					iMGUIContainer.style.flexShrink = 0;
					iMGUIContainer.pickingMode = PickingMode.Ignore;
					edgeControl.Add(iMGUIContainer);
				}
			} else if(iMGUIContainer != null) {
				iMGUIContainer.RemoveFromHierarchy();
				iMGUIContainer = null;
			}
			#endregion
		}

		public void UpdateEndPoints() {
			if(input != null) {
				edgeControl.to = this.WorldToLocal(input.GetGlobalCenter());
				edgeControl.from = this.WorldToLocal(output.GetGlobalCenter());
			}
		}

		protected virtual void OnGUI() {
			DebugGUI(true);
		}

		protected void DebugGUI(bool showLabel) {
			if(isProxy && !visible)
				return;
			if(Application.isPlaying && GraphDebug.useDebug) {
				PortView port = input as PortView ?? output as PortView;
				if(port != null && edgeControl.controlPoints != null && edgeControl.controlPoints.Length == 4) {
					GraphDebug.DebugData debugData = port.owner.owner.graph.GetDebugInfo();
					if(debugData != null) {
						if(port.isFlow) {
							PortView portView = output as PortView;
							if(portView.GetPortID() == UGraphView.SelfPortID) {

							} else {
								MemberData member = portView.portData.GetPortValue();
								if(member != null) {
									var debugValue = debugData.GetDebugValue(member);
									if(debugValue.isValid) {
										var times = (Time.unscaledTime - debugValue.time) / 2;
										if(times >= 0) {
											if(Mathf.Abs(edgeControl.controlPoints[0].x - edgeControl.controlPoints[3].x) <= 4) {
												Vector2 v1 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[0]);
												Vector2 v4 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[3]);
												DrawDebug(v1, v4, edgeControl.inputColor, edgeControl.outputColor, times, true);
											} else {
												Vector2 v1 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[0]);
												Vector2 v2 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[1]);
												Vector2 v3 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[2]);
												Vector2 v4 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[3]);
												DrawDebug(new Vector2[] { v1, v2, v3, v4 }, edgeControl.inputColor, edgeControl.outputColor, times, true);
											}
										}
									}
								}
							}
						} else {
							PortView portView = input as PortView;
							MemberData member = portView.portData.GetPortValue();
							if(member != null) {
								var debugValue = debugData.GetDebugValue(member);
								if(debugValue.isValid) {
									var times = (Time.unscaledTime - debugValue.time) / 2;
									if(Mathf.Abs(edgeControl.controlPoints[0].y - edgeControl.controlPoints[3].y) <= 4) {
										Vector2 v1 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[0]);
										Vector2 v4 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[3]);
										if(debugValue.isSet) {
											DrawDebug(v4, v1, edgeControl.outputColor, edgeControl.inputColor, times, true);
										} else {
											DrawDebug(v1, v4, edgeControl.inputColor, edgeControl.outputColor, times, true);
										}
										if(showLabel) {//Debug label
											GUIContent debugContent;
											if(debugValue.value != null) {
												debugContent = new GUIContent(
													uNodeUtility.GetDebugName(debugValue.value),
													uNodeEditorUtility.GetTypeIcon(debugValue.value.GetType())
												);
											} else {
												debugContent = new GUIContent("null");
											}
											Vector2 vec = (v1 + v4) / 2;
											Vector2 size = EditorStyles.helpBox.CalcSize(new GUIContent(debugContent.text));
											size.x += 25;
											GUI.Box(
												new Rect(vec.x - (size.x / 2), vec.y - 10, size.x - 10, 20),
												debugContent,
												EditorStyles.helpBox);
										}
									} else {
										Vector2 v1 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[0]);
										Vector2 v2 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[1]);
										Vector2 v3 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[2]);
										Vector2 v4 = this.ChangeCoordinatesTo(iMGUIContainer, edgeControl.controlPoints[3]);
										if(debugValue.isSet) {
											DrawDebug(new Vector2[] { v4, v3, v2, v1 }, edgeControl.outputColor, edgeControl.inputColor, times, true);
										} else {
											DrawDebug(new Vector2[] { v1, v2, v3, v4 }, edgeControl.inputColor, edgeControl.outputColor, times, true);
										}
										if(showLabel) {//Debug label
											GUIContent debugContent;
											if(debugValue.value != null) {
												debugContent = new GUIContent(
													uNodeUtility.GetDebugName(debugValue.value),
													uNodeEditorUtility.GetTypeIcon(debugValue.value.GetType())
												);
											} else {
												debugContent = new GUIContent("null");
											}
											Vector2 vec = (v2 + v3) / 2;
											Vector2 size = EditorStyles.helpBox.CalcSize(new GUIContent(debugContent.text));
											size.x += 25;
											GUI.Box(
												new Rect(vec.x - (size.x / 2), vec.y - 10, size.x - 10, 20),
												debugContent,
												EditorStyles.helpBox);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		private static void DrawDebug(Vector2[] vectors, Color inColor, Color outColor, float time, bool isFlow) {
			float timer = Mathf.Lerp(1, 0, time * 2f);//The debug timer speed.
			float distance = 0;
			for(int i = 0; i + 1 < vectors.Length; i++) {
				distance += Vector2.Distance(vectors[i], vectors[i + 1]);
			}
			float size = 15 * timer;
			float pointDist = 0;
			int currentSegment = 0;
			if(isFlow) {
				for(float i = -1; i < 1; i += 50f / distance) {
					float t = i + GraphDebug.debugLinesTimer * (50f / distance);
					if(!(t < 0f || t > 1)) {
						if(currentSegment + 1 >= vectors.Length) break;
						float seqmentDistance = Vector2.Distance(vectors[currentSegment], vectors[currentSegment + 1]);
						while(Mathf.Lerp(0, distance, t) > pointDist + seqmentDistance && currentSegment + 2 < vectors.Length) {
							pointDist += seqmentDistance;
							currentSegment++;
							seqmentDistance = Vector2.Distance(vectors[currentSegment], vectors[currentSegment + 1]);
						}
						var vec = Vector2.Lerp(
							vectors[currentSegment],
							vectors[currentSegment + 1],
							(Mathf.Lerp(0, distance, t) - pointDist) / seqmentDistance);
						GUI.color = new Color(
							Mathf.Lerp(outColor.r, inColor.r, t),
							Mathf.Lerp(outColor.g, inColor.g, t),
							Mathf.Lerp(outColor.b, inColor.b, t), 1);
						GUI.DrawTexture(new Rect(vec.x - size / 2, vec.y - size / 2, size, size), uNodeUtility.DebugPoint);
					}
				}
			} else {
				for(float i = -1; i < 1; i += 50f / distance) {
					float t = i + GraphDebug.debugLinesTimer * (50f / distance);
					if(!(t < 0f || t > 1)) {
						if(currentSegment + 1 >= vectors.Length) break;
						float seqmentDistance = Vector2.Distance(vectors[currentSegment], vectors[currentSegment + 1]);
						while(Mathf.Lerp(0, distance, t) > pointDist + seqmentDistance && currentSegment + 2 < vectors.Length) {
							pointDist += seqmentDistance;
							currentSegment++;
							seqmentDistance = Vector2.Distance(vectors[currentSegment], vectors[currentSegment + 1]);
						}
						var vec = Vector2.Lerp(
							vectors[currentSegment + 1],
							vectors[currentSegment],
							(Mathf.Lerp(0, distance, t) - pointDist) / seqmentDistance);
						GUI.color = new Color(
							Mathf.Lerp(inColor.r, outColor.r, t),
							Mathf.Lerp(inColor.g, outColor.g, t),
							Mathf.Lerp(inColor.b, outColor.b, t), 1);
						GUI.DrawTexture(new Rect(vec.x - size / 2, vec.y - size / 2, size, size), uNodeUtility.DebugPoint);
					}
				}
			}
			GUI.color = Color.white;
		}

		private static void DrawDebug(Vector2 start, Vector2 end, Color inColor, Color outColor, float time, bool isFlow) {
			float timer = Mathf.Lerp(1, 0, time * 2f);//The debug timer speed.
			float dist = Vector2.Distance(start, end);
			float size = 15 * timer;
			if(isFlow) {
				for(float i = -1; i < 1; i += 50f / dist) {
					float t = i + GraphDebug.debugLinesTimer * (50f / dist);
					if(!(t < 0f || t > 1)) {
						GUI.color = new Color(
							Mathf.Lerp(outColor.r, inColor.r, t),
							Mathf.Lerp(outColor.g, inColor.g, t),
							Mathf.Lerp(outColor.b, inColor.b, t), 1);
						Vector2 vec = Vector2.Lerp(start, end, t);
						GUI.DrawTexture(new Rect(vec.x - size / 2, vec.y - size / 2, size, size), uNodeUtility.DebugPoint);
					}
				}
			} else {
				for(float i = -1; i < 1; i += 50f / dist) {
					float t = i + GraphDebug.debugLinesTimer * (50f / dist);
					if(!(t < 0f || t > 1)) {
						GUI.color = new Color(
							Mathf.Lerp(inColor.r, outColor.r, t),
							Mathf.Lerp(inColor.g, outColor.g, t),
							Mathf.Lerp(inColor.b, outColor.b, t), 1);
						Vector2 vec = Vector2.Lerp(end, start, t);
						GUI.DrawTexture(new Rect(vec.x - size / 2, vec.y - size / 2, size, size), uNodeUtility.DebugPoint);
					}
				}
			}
			GUI.color = Color.white;
		}

		/// <summary>
		/// Get the sender port.
		/// Value edge will return Input port.
		/// Flow edge will return Output port.
		/// </summary>
		/// <returns></returns>
		public PortView GetSenderPort() {
			if(isFlow) {
				if(input.direction == Direction.Input) {
					return output as PortView;
				} else {
					return input as PortView;
				}
			} else {
				if(input.direction == Direction.Input) {
					return input as PortView;
				} else {
					return output as PortView;
				}
			}
		}

		/// <summary>
		/// Get the receiver port.
		/// Value edge will return Output port.
		/// Flow edge will return Input port.
		/// </summary>
		/// <returns></returns>
		public PortView GetReceiverPort() {
			if(isFlow) {
				if(input.direction == Direction.Input) {
					return input as PortView;
				} else {
					return output as PortView;
				}
			} else {
				if(input.direction == Direction.Input) {
					return output as PortView;
				} else {
					return input as PortView;
				}
			}
		}

		/// <summary>
		/// Is the edge is valid ( not is ghost and visible )
		/// </summary>
		public bool isValid => (parent != null || isHidding) && !isGhostEdge && this.IsVisible();
	
		#region Overrides
		public override bool Overlaps(Rect rectangle)
        {
            if (isProxy) return false;
			return base.Overlaps(rectangle);
        }

		public override bool ContainsPoint(Vector2 localPoint) {
			if(isProxy) return false;
			return base.ContainsPoint(localPoint);
		}
		#endregion
	}
}