using UnityEngine;
using Dreamteck.Splines;

namespace Dreamteck.Forever
{
    public class WavyPathGenerator : HighLevelPathGenerator
    {
        public float angle = 45f;
        public float turnRate = 0f;
        public Vector3 turnAxis = Vector3.up;

        private float currentAngle = 0f;
        private bool positive = true;
        Vector3 continueOrientation = Vector3.zero;


        public override void Continue(LevelPathGenerator previousGenerator)
        {
            base.Continue(previousGenerator);
            continueOrientation = orientation;
        }

        protected override void GeneratePoint(ref Point point, int pointIndex)
        {
            base.GeneratePoint(ref point, pointIndex);
            if (isFirstPoint) return;
            if (positive && currentAngle == angle) positive = false;
            else if (currentAngle == -angle) positive = true;
            currentAngle = MoveAngle(currentAngle);
            SetOrientation(continueOrientation + currentAngle * turnAxis.normalized);
            point.position = GetPointPosition();
            point.autoRotation = false;
            point.rotation = Vector3.Lerp(orientation, orientation + MoveAngle(currentAngle) * turnAxis.normalized, 0.5f);
        }

        float MoveAngle(float current)
        {
            if (positive)
            {
                return Mathf.MoveTowards(current, angle, turnRate);
            }
            else
            {
                return Mathf.MoveTowards(current, -angle, turnRate);
            }
        }


        float MoveTargetAngle(float input, float minStep, float maxStep, bool restrict, float minTarget, float maxTarget)
        {
            float direction = Random.Range(0, 100) > 50f ? 1f : -1f;
            float move = Random.Range(minStep, maxStep) * direction;
            input += move;
            if (restrict) input = Mathf.Clamp(input, minTarget, maxTarget);
            else
            {
                if (input > 360f) input -= Mathf.FloorToInt(input / 360f) * 360f;
                else if (input < 0f) input += Mathf.FloorToInt(-input / 360f) * 360f;
            }
            return input;
        }

        public override void Initialize(LevelGenerator input)
        {
            base.Initialize(input);
            currentAngle = 0f;
            continueOrientation = orientation;
        }

        protected void OffsetPoints(SplinePoint[] points, Vector3 offset, Space space)
        {
            if (offset != Vector3.zero) segment.stitch = false;
            SplineSample result = new SplineSample();
            if (space == Space.Self && segment.previous != null) segment.previous.Evaluate(1.0, result);
            for (int i = 0; i < points.Length; i++) points[i].SetPosition(points[i].position + offset);
        }
    }
}
