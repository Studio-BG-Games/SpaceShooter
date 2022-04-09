using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using NodeView = UnityEditor.Experimental.GraphView.Node;
using Object = UnityEngine.Object;

namespace MaxyGames.uNode.Editors {
	public partial class UGraphView {
		private class AsyncLoadingData {
			public System.Diagnostics.Stopwatch watch;
			public int maxMilis = 5;
			public Action<string, string, float> displayProgressCallback;
			public bool isStopped { get; private set; }

			public void DisplayProgress(string title, string info, float progress) {
				displayProgressCallback?.Invoke(title, info, progress);
			}

			public bool needWait => isStopped || watch.ElapsedMilliseconds > maxMilis;

			public void Stop() {
				isStopped = true;
			}
		}

		public void Initialize(UIElementGraph graph) {
			this.graph = graph;

			ToggleMinimap(UIElementUtility.Theme.enableMinimap);
			ToogleGrid(uNodePreference.GetPreference().showGrid);
			UpdatePosition();
			MarkRepaint(false);
		}

		IEnumerator InitializeNodeViews(AsyncLoadingData data, bool reloadExistingNodes) {
			graph.RefreshEventNodes();
			// if(!reloadExistingNodes && _needReloadedViews.Count > 0) {
			// 	reloadExistingNodes = true;
			// }
			float count = (graph.eventNodes != null ? graph.eventNodes.Count : 0) + (graph.nodes != null ? graph.nodes.Count : 0);
			float currentCount = 0;
			if(graph.eventNodes != null) {
				foreach(var node in graph.eventNodes.ToArray()) {
					if(node != null) {
						try {
							RepaintNode(data, node, reloadExistingNodes);
						}
						catch(Exception ex) {
							Debug.LogException(ex, node.owner);
							uNodeDebug.LogException(ex, node);
						}
						if(data.needWait) {
							data.DisplayProgress("Loading graph", "Initialize Node", currentCount / count);
							data.watch.Restart();
							yield return null;
						}
						currentCount++;
					}
				}
			}
			if(graph.nodes != null) {
				foreach(var node in graph.nodes.ToArray()) {
					if(node != null) {
						try {
							RepaintNode(data, node, reloadExistingNodes);
						}
						catch(Exception ex) {
							Debug.LogException(ex, node.owner);
							uNodeDebug.LogException(ex, node);
						}
						if(data.needWait) {
							data.DisplayProgress("Loading graph", "Initialize Node", currentCount / count);
							data.watch.Restart();
							yield return null;
						}
						currentCount++;
					}
				}
			}
			if(graph.regions != null) {
				foreach(var region in graph.regions.ToArray()) {
					if(region != null) {
						try {
							RepaintNode(data, region, reloadExistingNodes);
						}
						catch(Exception ex) {
							Debug.LogException(ex, region.owner);
							uNodeDebug.LogException(ex, region);
						}
						if(data.needWait) {
							data.DisplayProgress("Loading graph", "Initialize Node", 1);
							data.watch.Restart();
							yield return null;
						}
					}
				}
			}
			_needReloadedViews.Clear();
		}

		IEnumerator InitializeEdgeViews(AsyncLoadingData data) {
			float count = nodeViews.Count;
			float currentCount = 0;
			foreach(var nodeView in nodeViews) {
				if(nodeView.targetNode == null) {
					currentCount++;
					continue;
				}
				foreach(var port in nodeView.inputPorts) {
					if(!port.enabledSelf)
						continue;
					var portValue = port.portData?.GetPortValue();
					if(portValue != null) {
						if(portValue.IsTargetingPortOrNode) {
							EdgeView edgeView = null;
							foreach(var p in GraphProcessor) {
								edgeView = p.InitializeEdge(this, port, portValue, nodeView);
								if(edgeView != null) {
									break;
								}
							}
							if(edgeView == null) {
								edgeView = new EdgeView(port, PortUtility.GetOutputPort(portValue, this));
							}
							if(edgeView.input != null && edgeView.output != null) {
								Connect(edgeView, false);
							}
							if(data != null && data.needWait) {
								data.DisplayProgress("Loading graph", "Initialize Edges", currentCount / count);
								data.watch.Restart();
								yield return null;
							}
						}
					}
				}
				foreach(var port in nodeView.outputPorts) {
					if(!port.isFlow || !port.enabledSelf)
						continue;
					var portValue = port.portData?.GetPortValue();
					if(portValue != null) {
						if(portValue.IsTargetingPortOrNode) {
							EdgeView edgeView = null;
							foreach(var p in GraphProcessor) {
								edgeView = p.InitializeEdge(this, port, portValue, nodeView);
								if(edgeView != null) {
									break;
								}
							}
							if(edgeView == null) {
								edgeView = new EdgeView(PortUtility.GetInputPort(portValue, this), port);
							}
							if(edgeView.input != null && edgeView.output != null) {
								Connect(edgeView, false);
							}
							if(data != null && data.needWait) {
								data.DisplayProgress("Loading graph", "Initialize Edges", currentCount / count);
								data.watch.Restart();
								yield return null;
							}
						}
					}
				}
				nodeView.InitializeEdge();
				currentCount++;
			}
			foreach(var pair in transitionViewMaps) {
				var transition = pair.Key;
				if(transition != null && transition.GetTargetNode() != null) {
					var edgeView = new EdgeView(PortUtility.GetInputPort(transition.target, this), pair.Value.output);
					if(edgeView.input != null && edgeView.output != null) {
						Connect(edgeView, false);
					}
				}
				pair.Value.InitializeEdge();
				if(data != null && data.needWait) {
					data.DisplayProgress("Loading graph", "Initialize Edges", currentCount / count);
					data.watch.Restart();
					yield return null;
				}
			}
		}

		#region Repaint
		void RepaintNode(AsyncLoadingData data, NodeComponent node, bool fullReload) {
			foreach(var p in GraphProcessor) {
				if(p.RepaintNode(this, node, fullReload)) {
					return;
				}
			}
			if(cachedNodeMap.TryGetValue(node, out var view) && view != null && view.owner == this) {
				try {
					if(!nodeViewsPerNode.ContainsKey(node)) {
						//Ensure to add element when the node is not in the current scope
						AddElement(view);
						nodeViews.Add(view);
						nodeViewsPerNode[node] = view;
					}
					if(view is IRefreshable) {
						(view as IRefreshable).Refresh();
					}
					if(fullReload || _needReloadedViews.Contains(view) || view.autoReload) {
						view.ReloadView();
						view.MarkDirtyRepaint();
					} else {
						view.expanded = view.targetNode.nodeExpanded;
					}
				}
				catch(Exception ex) {
					Debug.LogException(ex, node);
					if(!nodeViewsPerNode.ContainsKey(node)) {
						//Add a view using default settings
						AddNodeView(node);
					}
				}
			} else {
				AddNodeView(node);
			}
		}

		public bool requiredReload { get; private set; }
		protected bool _fullReload = true;
		protected UnityEngine.Object _reloadedGraphScope;
		protected Action executeAfterReload;
		protected uNodeAsyncOperation repaintProgress;
		private int reloadID;

		public void MarkRepaint() {
			MarkRepaint(false);
		}

		public void FullReload() {
			MarkRepaint(true);
		}

		public void MarkRepaint(bool fullReload) {
			if(fullReload) {
				_fullReload = true;
			}
			if(!requiredReload) {
				requiredReload = true;
				repaintProgress?.Stop();
				uNodeThreadUtility.ExecuteOnce(() => {
					int currID = ++reloadID;
					requiredReload = false;
					autoHideNodes = false;
					AutoHideGraphElement.ResetVisibility(this);
					float currentZoom = graph.zoomScale;
					SetZoomScale(1, true);
					contentViewContainer.SetOpacity(0);
					DisplayProgressBar("Loading graph", "", 0);

					var watch = new System.Diagnostics.Stopwatch();
					watch.Start();
					var data = new AsyncLoadingData() {
						watch = watch,
						maxMilis = uNodePreference.preferenceData.maxReloadMilis,
						displayProgressCallback = DisplayProgressBar,
					};
					var taskProgress = uNodeThreadUtility.Task(ReloadView(data), 
						onFinished: () => {
							isLoading = true;
							watch.Stop();
							SetZoomScale(currentZoom, false);
							contentViewContainer.SetOpacity(1);
							ClearProgressBar();
							uNodeThreadUtility.ExecuteAfter(1, () => {
								executeAfterReload?.Invoke();
								executeAfterReload = null;
								uNodeThreadUtility.ExecuteAfter(1, () => {
									autoHideNodes = true;
									AutoHideGraphElement.UpdateVisibility(this);
									if(reloadID == currID)
										isLoading = false;
								});
							});
						}, 
						onStopped: () => {
							reloadID++;
							data.Stop();
						});
					repaintProgress = taskProgress;
				}, typeof(UGraphView));
			}
		}

		protected bool needReloadNodes;
		protected HashSet<UNodeView> _needReloadedViews = new HashSet<UNodeView>();

		public void MarkRepaint(IEnumerable<UNodeView> views) {
			if(views == null)
				return;
			if(requiredReload) {
				needReloadNodes = false;
				// _needReloadedViews.Clear();
				foreach(var v in views) {
					_needReloadedViews.Add(v);
				}
				return;
			}
			if(!needReloadNodes) {
				needReloadNodes = true;
				uNodeThreadUtility.Queue(() => {
					if(needReloadNodes && !requiredReload) {
						Repaint(_needReloadedViews.ToArray());
						_needReloadedViews.Clear();
					}
					needReloadNodes = false;
				});
			}
			foreach(var v in views) {
				_needReloadedViews.Add(v);
			}
		}

		public void MarkRepaint(params UNodeView[] views) {
			MarkRepaint(views as IEnumerable<UNodeView>);
		}

		public void MarkRepaint(NodeComponent node) {
			if(node == null)
				return;
			UNodeView view;
			if(nodeViewsPerNode.TryGetValue(node, out view)) {
				MarkRepaint(view);
			}
		}

		public void MarkRepaint(TransitionEvent transition) {
			if(transition == null)
				return;
			TransitionView view;
			if(transitionViewMaps.TryGetValue(transition, out view)) {
				MarkRepaint(view);
			}
		}

		public void MarkRepaintEdges() {
			MarkRepaint(new UNodeView[0]);
		}

		void Repaint(params UNodeView[] views) {
			for(int i = 0; i < views.Length; i++) {
				if(views[i] == null || views[i].targetNode == null)
					continue;
				views[i].ReloadView();
			}
			//Update edges.
			RemoveEdges();
			InitializeEdgeViews(null).MoveNext();
		}

		void Repaint(params NodeComponent[] nodes) {
			for(int i = 0; i < nodes.Length; i++) {
				if(nodeViewsPerNode.TryGetValue(nodes[i], out var view) && view != null && view.targetNode != null) {
					view.ReloadView();
				}
			}
			//Update edges.
			RemoveEdges();
			InitializeEdgeViews(null).MoveNext();
		}
		#endregion

		#region ReloadView
		IEnumerator ReloadView(AsyncLoadingData data) {
			isLoading = true;
			graphLayout = editorData.graph != null ? editorData.graph.graphData.graphLayout : GraphLayout.Vertical;
			if(graphLayout == GraphLayout.Vertical) {
				EnableInClassList("vertical-graph", true);
				EnableInClassList("horizontal-graph", false);
			} else {
				EnableInClassList("vertical-graph", false);
				EnableInClassList("horizontal-graph", true);
			}
			if(uNodePreference.GetPreference().forceReloadGraph) {
				_fullReload = true;
				cachedNodeMap.Clear();
			} else if(_reloadedGraphScope != editorData.currentCanvas) {
				_reloadedGraphScope = editorData.currentCanvas;
				if(!_fullReload) {
					//Ensure to remove all node views when the graph scope is different
					RemoveNodeViews(false);
				}
			}
			//Ensure we get the up to date datas
			editorData.Refresh();
			//Remove all edges
			RemoveEdges();
			if(_fullReload) {
				// Remove everything node view
				RemoveNodeViews(true);
			} else {
				foreach(var nodeView in nodeViews) {
					if(nodeView.targetNode == null) {
						RemoveElement(nodeView);
						cachedNodeMap.Remove(nodeView.targetNode);
						nodeViewsPerNode.Remove(nodeView.targetNode);
					}
				}
				nodeViews.RemoveAll(n => n.targetNode == null);
				List<TransitionEvent> removedTransitions = new List<TransitionEvent>();
				foreach(var pair in transitionViewMaps) {
					if(pair.Key == null) {
						RemoveElement(pair.Value);
						cachedNodeMap.Remove(pair.Key);
						removedTransitions.Add(pair.Key);
					}
				}
				foreach(var tr in removedTransitions) {
					transitionViewMaps.Remove(tr);
				}
			}
			// re-add with new up to date datas
			var initNodes = InitializeNodeViews(data, _fullReload);
			while(initNodes.MoveNext()) {
				yield return null;
			}
			data.DisplayProgress("Loading graph", "Initialize Edges", 1);
			var initEdges = InitializeEdgeViews(data);
			while(initEdges.MoveNext()) {
				yield return null;
			}

			ToggleMinimap(UIElementUtility.Theme.enableMinimap);
			ToogleGrid(uNodePreference.GetPreference().showGrid);
			//Mark full reload to false for more faster reload after full reload
			_fullReload = false;
			yield break;
		}
		#endregion

		#region Utility
		private PropertyInfo _keepPixelCacheProperty;
		protected void SetPixelCachedOnBoundChanged(bool value) {
			if(panel != null) {
				if(_keepPixelCacheProperty == null) {
					_keepPixelCacheProperty = panel.GetType().GetProperty("keepPixelCacheOnWorldBoundChange", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				}
				if(_keepPixelCacheProperty != null) {
					_keepPixelCacheProperty.SetValue(panel, value);
				}
			}
		}

		void RemoveNodeViews(bool includingCache) {
			foreach(var nodeView in nodeViews) {
				RemoveElement(nodeView);
				if(includingCache)
					cachedNodeMap.Remove(nodeView.targetNode);
			}
			nodeViews.Clear();
			nodeViewsPerNode.Clear();
			foreach(var pair in transitionViewMaps) {
				RemoveElement(pair.Value);
				if(includingCache)
					cachedNodeMap.Remove(pair.Key);
			}
			transitionViewMaps.Clear();
		}

		public void RemoveEdges() {
			foreach(var edge in edgeViews)
				RemoveElement(edge);
			edgeViews.Clear();
		}
		#endregion
	}
}
