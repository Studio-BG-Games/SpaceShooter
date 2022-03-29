using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class MissileWeaponStatsOverrideController : ModuleStatsOverrideController
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

        public override void OnModulesListUpdated(List<Module> moduleList)
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

                MissileWeapon missileWeapon = moduleList[i].GetComponent<MissileWeapon>();

                if (missileWeapon != null)
                {
                    // Update the max gun speed
                    maxSpeed = Mathf.Max(missileWeapon.Speed, maxSpeed);

                    // Update the max gun range
                    maxRange = Mathf.Max(missileWeapon.Range, maxRange);

                    // Updeate max damage values for each health type
                    for (int j = 0; j < damageStatsHealthTypes.Count; ++j)
                    {
                        maxDamageByHealthType[j] = Mathf.Max(maxDamageByHealthType[j], missileWeapon.Damage(damageStatsHealthTypes[j]));
                    }
                }
            }
        }


        public override bool ShowStats(Module module)
        {
            MissileWeapon missileWeapon = module.GetComponent<MissileWeapon>();
            if (missileWeapon == null)
            {
                return false;
            }
            else
            {
                statsController.LabelText.text = module.Label;
                statsController.DescriptionText.text = module.Description;

                // Show speed
                StatsInstance speedStatsInstance = statsController.GetStatsInstance();
                string speedValueDisplay = missileWeapon.Speed == Mathf.Infinity ? infiniteValueDisplay : ((int)(missileWeapon.Speed)).ToString();
                float speedFillBarValue = missileWeapon.Speed == Mathf.Infinity ? 1 : (missileWeapon.Speed / maxSpeed);
                speedStatsInstance.Set("SPEED", speedValueDisplay + " M/S", speedFillBarValue);

                // Show range
                StatsInstance rangeStatsInstance = statsController.GetStatsInstance();
                string rangeValueDisplay = missileWeapon.Range == Mathf.Infinity ? infiniteValueDisplay : ((int)(missileWeapon.Range)).ToString();
                float rangeFillBarValue = missileWeapon.Range == Mathf.Infinity ? 1 : (missileWeapon.Range / maxRange);
                rangeStatsInstance.Set("RANGE", rangeValueDisplay + " M", rangeFillBarValue);

                // Update damage stats
                for (int i = 0; i < damageStatsHealthTypes.Count; ++i)
                {
                    StatsInstance damageStatsInstance = statsController.GetStatsInstance();
                    string damageStatsLabel = damageStatsHealthTypes[i].name.ToUpper() + " DMG";
                    string damageStatsValue = ((int)(missileWeapon.Damage(damageStatsHealthTypes[i]))).ToString() + " DPS";
                    float damageStatsFillBarValue = missileWeapon.Damage(damageStatsHealthTypes[i]) / maxDamageByHealthType[i];
                    damageStatsInstance.Set(damageStatsLabel, damageStatsValue, damageStatsFillBarValue);
                }

                return true;
            }
        }
    }
}

