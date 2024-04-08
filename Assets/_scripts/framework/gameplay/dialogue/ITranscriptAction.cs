//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if OTBT_AC
using AC;

namespace OTBT.Framework.Gameplay
{
    public interface ITranscriptAction 
    {
#if UNITY_EDITOR
        public string ToTranscript(int i = 0);
        public Action FollowAction(int i = 0);
#endif
    }
}
#endif
