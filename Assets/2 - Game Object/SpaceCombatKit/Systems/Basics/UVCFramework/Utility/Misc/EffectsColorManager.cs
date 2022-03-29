using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Control the color of a set of effects materials.
    /// </summary>
    public class EffectsColorManager : MonoBehaviour
    {

        [SerializeField]
        protected Color effectsColor = Color.white;

        [SerializeField]
        protected string colorID = "_Color";

        [SerializeField]
        protected bool preserveIndividualAlpha = true;

        [SerializeField]
        protected float startingGroupAlpha = 1;

        protected float groupAlpha;

        [Header("Effects Elements")]

        [SerializeField]
        protected List<Renderer> effectsRenderers = new List<Renderer>();
        protected List<Color> effectsOriginalColors = new List<Color>();


        private void Awake()
        {
            groupAlpha = startingGroupAlpha;

            // Cache the materials
            for (int i = 0; i < effectsRenderers.Count; ++i)
            {
                Color c = effectsRenderers[i].material.GetColor(colorID);
                effectsOriginalColors.Add(c);
            }

            for (int i = 0; i < effectsRenderers.Count; ++i)
            {
                UpdateColor(effectsRenderers[i], effectsOriginalColors[i], effectsColor, groupAlpha);
            }
        }


        void UpdateColor(Renderer renderer, Color originalColor, Color targetColor, float alpha)
        {
            float h_Original, s_Original, v_Original;
            Color.RGBToHSV(originalColor, out h_Original, out s_Original, out v_Original);

            float h_Target, s_Target, v_Target;
            Color.RGBToHSV(targetColor, out h_Target, out s_Target, out v_Target);

            Color newColor = Color.HSVToRGB(h_Target, s_Target, v_Original);
            newColor.a = originalColor.a * alpha;

            renderer.material.SetColor(colorID, newColor);
        }


        public void SetAlpha(float alpha)
        {
            groupAlpha = alpha;
        }


        private void Update()
        {
            for (int i = 0; i < effectsRenderers.Count; ++i)
            {
                UpdateColor(effectsRenderers[i], effectsOriginalColors[i], effectsColor, groupAlpha);
            }
        }
    }
}