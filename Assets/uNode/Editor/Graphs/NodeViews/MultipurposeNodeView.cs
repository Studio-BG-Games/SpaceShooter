using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace MaxyGames.uNode.Editors {
	[NodeCustomEditor(typeof(MultipurposeNode))]
	public class MultipurposeNodeView : BaseNodeView {
		bool isCompact;
		RichLabel richTitle;
		string m_Title;

		protected override void InitializeView() {
			MultipurposeNode node = targetNode as MultipurposeNode;
			InitializeDefaultPort();
			AddMultipurposeMemberPort(node.target, (o) => {
				node.target = o;
			}, node);
			if(node.IsFlowNode()) {
				AddOutputFlowPort(nameof(node.onFinished), "");
			}
			if(UIElementGraph.richText && richTitle == null) {
				richTitle = new RichLabel {
					name = "title-label"
				};
				titleContainer.Insert(titleContainer.IndexOf(titleIcon) + 1, richTitle);
			}
			if(richTitle != null) {
				titleContainer.AddToClassList("rich-title");
				title = "";
				richTitle.text = uNodeUtility.GetNicelyDisplayName(node.target.target, uNodeUtility.preferredDisplay, true);
				m_Title = uNodeEditorUtility.RemoveHTMLTag(richTitle.text);
			}
			isCompact = false;
			EnableInClassList("compact", false);
			EnableInClassList("compact-value", false);
			if(nodeFlowPort != null || outputControls.Count != 0) {
				if(uNodeUtility.preferredDisplay == DisplayKind.Partial) {
					ConstructCompactTitle();
				}
				return;
			}
			if(inputControls.Count == 0) {
				int valueOutputCount = outputPorts.Count(x => x.orientation == Orientation.Horizontal);
				int valueInputCount = inputPorts.Count(x => x.orientation == Orientation.Horizontal);
				if(valueOutputCount == 1 && valueInputCount == 0) {
					isCompact = true;
					EnableInClassList("compact", true);
					if(nodeValuePort != null) {
						nodeValuePort.SetName("");
					}
				}
			} else if(inputControls.Count > 0 && node.target.target.targetType == MemberData.TargetType.Values) {
				isCompact = true;
				ControlView control = inputControls.First();
				if(control.control as MemberControl != null) {
					var config = control.control.config;
					if(config.filter != null) {
						config.filter.ValidTargetType = MemberData.TargetType.Values;
					}
					if(config.type == typeof(string)) {
						control.control.AddToClassList("multiline");
						control.style.height = new StyleLength(StyleKeyword.Auto);
						control.style.flexGrow = 1;
					}
					(control.control as MemberControl).UpdateControl();
				}
				if(inputControls.Count == 1) {
					EnableInClassList("compact-value", true);
					if(nodeValuePort != null) {
						nodeValuePort.SetName("");
					}
				}
			}
			if(isCompact && nodeValuePort != null) {
				Color c = uNodePreference.GetColorForType(nodeValuePort.GetPortType());
				c.a = 0.8f;
				elementTypeColor = c;
				if (UIElementUtility.Theme.coloredNodeBorder) {
					border.style.SetBorderColor(c);
				}
			} else if(uNodeUtility.preferredDisplay == DisplayKind.Partial) {
				ConstructCompactTitle();
				if (nodeValuePort != null) {
					EnableInClassList("compact-title", true);
					Color c = uNodePreference.GetColorForType(nodeValuePort.GetPortType());
					c.a = 0.8f;
					elementTypeColor = c;
					if (UIElementUtility.Theme.coloredNodeBorder) {
						border.style.SetBorderColor(c);
					}
				}
			}
		}

		protected override void OnCustomStyleResolved(ICustomStyle style) {
			base.OnCustomStyleResolved(style);
			if(isCompact && nodeValuePort != null) {
				Color c = uNodePreference.GetColorForType(nodeValuePort.GetPortType());
				c.a = 0.8f;
				elementTypeColor = c;
				if (UIElementUtility.Theme.coloredNodeBorder) {
					border.style.SetBorderColor(c);
				}
			}
		}

		public override string GetTitle() {
			return richTitle == null ? title : m_Title;
		}
	}
}
