//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

using OTBT.Framework.Core;
using Sparrow.Verification;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    [CreateAssetMenu(menuName = "OTBT/Utils/AtomCollection", fileName = "AtomCollection", order = 0)]
    public class AtomCollection : ScriptableObjectCollection<VanillaAtom>, IVerify
    {

        public List<VanillaAtom> atoms => m_Objects;

#if UNITY_EDITOR
        public new void Verify(CheckVerifyInterface checker)
        {
            base.Verify(checker);

            foreach(VanillaAtom a in atoms)
            {
                foreach(VanillaAtom b in atoms)
                {
                    if (a == b) continue;
                    checker.Check(!a.identifier.Equals(b.identifier), "Duplicate identifiers found for objects: " + a.name + " and " + b.name, a, () =>
                    {
                        a.RegenerateGUID();
                    });
                }
            }
        }
#endif
    }
}
