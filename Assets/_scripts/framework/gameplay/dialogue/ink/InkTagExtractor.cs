
//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer:Alex Brühl
//

#if OTBT_INK

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

namespace OTBT.Framework.Ink
{
    public class InkTagExtractor
    {
        public static Dictionary<string,string> ExtractTagsFromStory(Story story)
        {
            Dictionary<string,string> tags = new();
            foreach(string tag in story.currentTags) { 
                tags.Add(tag.Split(':')[0], tag.Split(':')[1]);
            }
            return tags;
        }

    }
}

#endif