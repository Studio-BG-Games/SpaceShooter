namespace Dreamteck.Forever.Editor
{
    using UnityEngine;
    using UnityEditor;
    using Splines;
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CustomPathGenerator))]
    public class CustomPathGeneratorEditor : PathGeneratorEditor
    {
        Spline spline;
        SplineSample drawResult = new SplineSample();
        ForeverSplineEditor splineEditor;

        private void Awake()
        {
            CustomPathGenerator gen = (CustomPathGenerator)target;
            splineEditor = new ForeverSplineEditor(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), "Custom Path Editor");
            spline = new Spline(gen.customPathType, gen.customPathSampleRate);
            splineEditor.repaintHandler += OnRepaint;
            splineEditor.undoHandler += RecordUndo;
            splineEditor.evaluate += EvaluateHandler;
#if UNITY_2019_1_OR_NEWER
            SceneView.beforeSceneGui += BeforeSceneGUI;
#else
            SceneView.onSceneGUIDelegate += BeforeSceneGUI;
#endif
            CustomPathGenerator generator = (CustomPathGenerator)target;
            splineEditor.points = generator.points;
        }

        private void EvaluateHandler(double percent, SplineSample result)
        {
            spline.Evaluate(result, percent);
        }

        private void BeforeSceneGUI(SceneView current)
        {
            splineEditor.BeforeSceneGUI(current);
        }

        private void OnRepaint()
        {
            SceneView.RepaintAll();
            Repaint();
        }

        private void RecordUndo(string title)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                Undo.RecordObject((CustomPathGenerator)targets[i], title);
            }
        }

        protected override void OnInspector()
        {
            base.OnInspector();
            CustomPathGenerator gen = (CustomPathGenerator)target;
            Undo.RecordObject(gen, gen.name + " - Edit Properties");
            gen.customPathSampleRate = EditorGUILayout.IntField("Custom Path Sample Rate", gen.customPathSampleRate);
            gen.customPathType = (Spline.Type)EditorGUILayout.EnumPopup("Custom Path Type", gen.customPathType);
            splineEditor.splineType = gen.customPathType;
            splineEditor.sampleRate = gen.customPathSampleRate;
            splineEditor.DrawInspector();
            if (GUI.changed) EditorUtility.SetDirty(gen);
        }

        public override void DrawScene(SceneView current)
        {
            CustomPathGenerator generator = (CustomPathGenerator)target;
            splineEditor.splineType = generator.customPathType;
            splineEditor.DrawScene(current);

            generator.points = splineEditor.points;
            spline.sampleRate = generator.customPathSampleRate;
            spline.type = generator.customPathType;
            spline.points = generator.points;
            if (generator.loop && generator.points.Length > 3) spline.Close();
            else if (spline.isClosed) spline.Break(splineEditor.selectedPoints.Count > 0 ? splineEditor.selectedPoints[0] : 0);
            if(!(splineEditor.currentModule is Dreamteck.Splines.Editor.CreatePointModule) &&  spline.points.Length > 1 && spline.iterations > 0)
            {
                Splines.Editor.SplineDrawer.DrawSpline(spline, Color.white);
            }
        }
    }
}
