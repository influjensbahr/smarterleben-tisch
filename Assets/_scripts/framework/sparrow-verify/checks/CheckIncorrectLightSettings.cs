//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckIncorrectLightSettings : VerifyCheckBase
    {
        public override string description => "Incorrect light settings";
        public override string longDescription => "Checks for obvious inconsistencies in the lighting settings.";


        /*
         *  Directional lights with shadows enabled but very low shadow resolution.
            Point lights with a very large range but low shadow resolution.
            Spot lights with a large range or spot angle but low shadow resolution.
            Any light with intensity set too high, which may cause overexposure.
        */
        public override void PerformCheck(GameObject gameObject)
        {
            Light light = gameObject.GetComponent<Light>();
            if (light == null) return;

            // Check for directional light settings
            if (light.type == LightType.Directional)
            {
                if (light.shadows != LightShadows.None && light.shadowResolution == LightShadowResolution.Low)
                {
                    AddFailedCheck("Directional light with shadows enabled has very low shadow resolution", gameObject);
                }
            }

            // Check for point light settings
            if (light.type == LightType.Point)
            {
                if (light.range > 50 && light.shadowResolution == LightShadowResolution.Low)
                {
                    AddFailedCheck("Point light with a large range has low shadow resolution", gameObject);
                }
            }

            // Check for spot light settings
            if (light.type == LightType.Spot)
            {
                if ((light.range > 50 || light.spotAngle > 60) && light.shadowResolution == LightShadowResolution.Low)
                {
                    AddFailedCheck("Spot light with large range or spot angle has low shadow resolution", gameObject);
                }
            }

            // Check for excessive light intensity
            if (light.intensity > 8.0f)
            {
                AddFailedCheck("Light intensity is too high and may cause overexposure", gameObject);
            }
        }
    }
}
#endif
