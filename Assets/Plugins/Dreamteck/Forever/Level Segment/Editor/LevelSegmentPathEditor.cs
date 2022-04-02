namespace Dreamteck.Forever.Editor
{
    using UnityEngine;
    using UnityEditor;
    using Dreamteck.Splines;

    public class LevelSegmentCustomPathEditor
    {
        private Spline visualization;
        private ForeverSplineEditor splineEditor;


        internal LevelSegment segment;
        internal LevelSegment.LevelSegmentPath path;
        internal LevelSegmentEditor editor;

        public LevelSegmentCustomPathEditor(LevelSegmentEditor e, LevelSegment s, LevelSegment.LevelSegmentPath p)
        {
            editor = e;
            segment = s;
            path = p;
            splineEditor = new ForeverSplineEditor(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), "Custom Path Editor");
            splineEditor.repaintHandler += OnRepaint;
            splineEditor.undoHandler += RecordUndo;
            splineEditor.evaluate += EvaluateHandler;
            splineEditor.points = path.spline.points;
            visualization = new Spline(path.spline.type, path.spline.sampleRate);
#if UNITY_2019_1_OR_NEWER
            SceneView.beforeSceneGui += BeforeSceneGUI;
#else
            SceneView.onSceneGUIDelegate += BeforeSceneGUI;
#endif
        }

        private void EvaluateHandler(double percent, SplineSample result)
        {
            visualization.Evaluate(result, percent);
        }

        private void BeforeSceneGUI(SceneView current)
        {
            splineEditor.BeforeSceneGUI(current);
        }

        private void OnRepaint()
        {
            SceneView.RepaintAll();
            editor.Repaint();
        }

        private void RecordUndo(string title)
        {
            Undo.RecordObject(segment, title);
        }

        public void DrawInspector()
        {
            path.spline.sampleRate = EditorGUILayout.IntField("Custom Path Sample Rate", path.spline.sampleRate);
            path.spline.type = (Spline.Type)EditorGUILayout.EnumPopup("Custom Path Type", path.spline.type);
            path.confineToBounds = EditorGUILayout.Toggle("Confine To Bounds", path.confineToBounds);
            splineEditor.splineType = path.spline.type;
            splineEditor.sampleRate = path.spline.sampleRate;
            path.Transform();
            splineEditor.DrawInspector();
            path.spline.points = splineEditor.points;
            path.InverseTransform();
        }

        public void DrawScene(SceneView current)
        {
            if (path.spline == null) return;
            path.Transform();
            Splines.Editor.SplineDrawer.DrawSpline(path.spline, path.color);
            splineEditor.DrawScene(current);
            path.spline.points = splineEditor.points;
            path.InverseTransform();
        }
    }
}
