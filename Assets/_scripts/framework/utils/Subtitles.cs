// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    [Serializable]
    public class Subtitles
    {
        [Serializable]
        public struct Line
        {
            public int segment;
            public float start;
            public float end;
            public string text;
        }

        [SerializeField] List<Line> m_Lines = new();

        public string GetText(float time)
        {
            var line = GetLine(time);
            return line.text;
        }

        public void AddLine(Line line)
        {
            m_Lines.Add(line);
        }

        public Line GetLine(float time)
        {
            for (int i = 0; i < m_Lines.Count - 1; i++)
            {
                var line = m_Lines[i];
                if (time < line.start || time > line.end) continue;
                return line;
            }

            return new Line() {
                segment = -1
            };
        }
    }

}
