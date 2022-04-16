using Sirenix.Utilities;
using UnityEngine;

namespace DefaultNamespace
{
    public class SetAlphaMaterial : MonoBehaviour
    {
        public MeshRenderer[] Meshes;

        public void SetAlpha(float a)
        {
            a = Mathf.Clamp(a, 0, 1);
            Meshes.ForEach(mesh =>
            {
                mesh.materials.ForEach(mat =>
                {
                    var color = mat.GetColor("_Color");
                    color.a = a;
                    mat.SetColor("_Color", color);
                });
            });
        }
    }
}