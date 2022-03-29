using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class GunWeaponStatsOverrideController : ModuleStatsOverrideController
    {

        [SerializeField]
        protected string infiniteValueDisplay = "-";

        [SerializeField]
        protected List<HealthType> damageStatsHealthTypes = new List<HealthType>();

        protected float maxSpeed;
        protected float maxRange;
        protected float[] maxDamageByHealthType;


        protected virtual void Awake()
        {
            maxDamageByHealthType = new float[damageStatsHealthTypes.Count];
        }

        public override void OnModulesListUpdated (List<Module> moduleList)
        {

            // Reset gun max stats
            maxSpeed = 0;
            maxRange = 0;
            for (int i = 0; i < maxDamageByHealthType.Length; ++i)
            {
                maxDamageByHealthType[i] = 0;
            }

            // Update gun max stats
            for (int i = 0; i < moduleList.Count; ++i)
            {

                GunWeapon gunWeapon = moduleList[i].GetComponent<GunWeapon>();

                if (gunWeapon != null)
                {
                    // Update the max gun speed
                    if (gunWeapon.Speed == Mathf.Infinity)
                    {
                        continue;
                    }
                    else
                    {
                        maxSpeed = Mathf.Max(gunWeapon.Speed, maxSpeed);
                    }

                    // Update the max gun range
                    maxRange = Mathf.Max(gunWeapon.Range, maxRange);

                    // Updeate max damage values for each health type
                    for (int j = 0; j < damageStatsHealthTypes.Count; ++j)
                    {
                        maxDamageByHealthType[j] = Mathf.Max(maxDamageByHealthType[j], gunWeapon.Damage(damageStatsHealthTypes[j]));
                    }
                }
            }
        }

        public override bool ShowStats (Module module)
        {
            GunWeapon gunWeapon = module.GetComponent<GunWeapon>();
            if (gunWeapon == null)
            {
                return false;
            }
            else
            {

                statsController.LabelText.text = module.Label;
                statsController.DescriptionText.text = module.Description;

                // Show speed
                StatsInstance speedStatsInstance = statsController.GetStatsInstance();
                string speedValueDisplay = gunWeapon.Speed == Mathf.Infinity ? infiniteValueDisplay : ((int)(gunWeapon.Speed)).ToString();
                float speedFillBarValue = gunWeapon.Speed == Mathf.Infinity ? 1 : (gunWeapon.Speed / maxSpeed);
                speedStatsInstance.Set("SPEED", speedValueDisplay + " M/S", speedFillBarValue);

                // Show range
                StatsInstance rangeStatsInstance = statsController.GetStatsInstance();
                string rangeValueDisplay = gunWeapon.Range == Mathf.Infinity ? infiniteValueDisplay : ((int)(gunWeapon.Range)).ToString();
                float rangeFillBarValue = gunWeapon.Range == Mathf.Infinity ? 1 : (gunWeapon.Range / maxRange);
                rangeStatsInstance.Set("RANGE", rangeValueDisplay + " M", rangeFillBarValue);

                // Update damage stats
                for (int i = 0; i < damageStatsHealthTypes.Count; ++i)
                {
                    StatsInstance damageStatsInstance = statsController.GetStatsInstance();
                    string damageStatsLabel = damageStatsHealthTypes[i].name.ToUpper() + " DMG";
                    string damageStatsValue = ((int)(gunWeapon.Damage(damageStatsHealthTypes[i]))).ToString() + " DPS";
                    float damageStatsFillBarValue = gunWeapon.Damage(damageStatsHealthTypes[i]) / maxDamageByHealthType[i];
                    damageStatsInstance.Set(damageStatsLabel, damageStatsValue, damageStatsFillBarValue);
                }

                return true;
            }
        }
    }
}

