//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//


using OTBT.Framework.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Core
{
    [CreateAssetMenu(menuName = "OTBT/Core/Scene Collection Collection", fileName = "New Scene Collection Collection")]
    public class SceneCollectionCollection : ScriptableObjectCollection<SceneCollection>
    {
        public List<SceneCollection> sceneCollections => m_Objects;
    }
}
