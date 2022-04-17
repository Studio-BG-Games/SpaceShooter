using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Dreamteck.Forever;
using Dreamteck.Splines;
using MaxyGames;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Services
{
    public class WeaponLaser : MonoBehaviour
    {
        [Header("Laser setting")]
        [Min(1)]public float LenghtLaser;
        public Runner RunnerShip;
        [Min(0.1f)]public float LenghtSegment;
        public Transform PointLaset;
        public LineRenderer LineRenderer;

        [Header("Attack setting")]
        public Collider[] ColiiderIgnore;
        public LayerMask Mask;
        
        [ShowInInspector]public int CountPoint =>(int)(LenghtLaser / LenghtSegment);

        private void Start()
        {
            
        }

        public Collider TryFire()
        {
            if (gameObject.activeSelf && enabled)
            {
                var ar = GetPoints(PointLaset.position, out var coolider).ToArray();
                LineRenderer.positionCount = ar.Length;
                LineRenderer.SetPositions(ar);
                return coolider;
            }

            return null;
        }

        public void Zero()
        {
            LineRenderer.positionCount = 0;
            LineRenderer.SetPositions(new Vector3[]{});
        }

        private List<Vector3> GetPoints(Vector3 startPoint, out Collider hited)
        {
            hited = null;
            var offset = GlobalHelp.GetOffsetByPath(RunnerShip, startPoint);
            
            List<Vector3> points= new List<Vector3>();

            Vector3 currentPoint = startPoint;
            var prevPoint = startPoint;
            SplineSample sample = new SplineSample();
            for (int i = 0; i < CountPoint; i++)
            {
                LevelGenerator.instance.Project(currentPoint, sample);
                points.Add(sample.position + offset);

                var r = new Ray(prevPoint+offset, (prevPoint+offset)-(currentPoint+offset));
                Debug.DrawRay(r.origin, r.direction*LenghtSegment, RandomColor(), 10);
                if (Physics.Raycast(r, out var hit, LenghtSegment, Mask))
                {
                    if (!ColiiderIgnore.Contains(hit.collider))
                    {
                        hited = hit.collider;
                        return points;
                    }
                }       
                
                prevPoint = currentPoint;
                currentPoint = sample.position + sample.forward * LenghtSegment;
            }

            return points;
        }

        private Color RandomColor() => new Color(Random.Range(0,1f),Random.Range(0,1f),Random.Range(0,1f));
    }
}