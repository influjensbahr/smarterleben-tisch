//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Sparrow.Verification
{
    [Serializable]
    public class CheckLayoutGroupSizeFitters : VerifyCheckBase
    {
        public override string description => "Nested UI Layout problems";
        public override string longDescription => "Checks for incorrectly nested ContentSizeFitters or AspectRatioFitters within LayoutGroups in the UI system.";


        public override void PerformCheck(GameObject gameObject)
        {
            ContentSizeFitter csf = gameObject.GetComponent < ContentSizeFitter >();
            AspectRatioFitter arf = gameObject.GetComponent<AspectRatioFitter>();
            if (csf != null || arf != null)
            {
                bool anyHaveLayoutParent = false;
                ILayoutIgnorer ignorer = gameObject.GetComponent(typeof(ILayoutIgnorer)) as ILayoutIgnorer;
                if (ignorer != null && ignorer.ignoreLayout)
                    return;

                RectTransform parent = gameObject.transform.parent as RectTransform;
                if (parent != null)
                {
                    Behaviour layoutGroup = parent.GetComponent(typeof(ILayoutGroup)) as Behaviour;
                    if (layoutGroup != null && layoutGroup.enabled)
                    {
                        anyHaveLayoutParent = true;
                    }
                }
                if (anyHaveLayoutParent)
                    AddFailedCheck($"ContentSizeFitter/AspectRatioFitter component is child of a LayoutGroup - this will lead to unexpected behaviour", gameObject);
            }
        }
    }
}
#endif
