using Dreamteck.Forever;
using Dreamteck.Splines;
using UnityEngine;

namespace DefaultNamespace
{
    public static class GlobalHelp
    {
        public static void SetOffsetProjectTile(Runner r, Vector3 targetPoint)
        {
            // Раннер ввсегда начинает с нулевым оффестом. Здесь мы вычисляем нужный оффсет для ранера. Сначала 
            // Вычисляем направление от точки с нулевым оффестом к месту стрельбы
            // Мы получаем оффест в глобальных кординатах, затем через  InverseTransformVector делаем оффет локальный и уже его устанавливаем в оффест раннера
            SplineSample sample = new SplineSample();
            LevelGenerator.instance.Project(targetPoint, sample);
            var globalOffset = targetPoint-sample.position;
            var localOffset = r.transform.InverseTransformVector(globalOffset);
            r.motion.offset = localOffset;
        }
    }
}