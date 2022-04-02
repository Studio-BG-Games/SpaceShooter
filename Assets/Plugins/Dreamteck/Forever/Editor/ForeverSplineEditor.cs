namespace Dreamteck.Forever.Editor
{
    using Dreamteck.Splines;
    using Dreamteck.Splines.Editor;
    using UnityEditor;
    using UnityEngine;

    public class ForeverSplineEditor : SplineEditor
    {
        public ForeverSplineEditor (Matrix4x4 transformMatrix, string editorName) : base(transformMatrix, editorName)
        {

        }

        public override void BeforeSceneGUI(SceneView current)
        {
            SetupModule(mainModule);
            for (int i = 0; i < moduleCount; i++)
            {
                SetupModule(GetModule(i));
            }
            base.BeforeSceneGUI(current);
        }

        private void SetupModule(PointModule module)
        {
            module.duplicationDirection = Spline.Direction.Forward;
            module.highlightColor = ForeverPrefs.highlightColor;
            module.showPointNumbers = false;
        }
    }
}
