// 
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jannik Boysen
// 
using System;
using System.Globalization;
using System.IO;
namespace OTBT.Framework.Utils
{
    public class SubtitleParserSRT
    {
        enum State
        {
            Segment,
            Timestamp,
            Text,
        }

        Subtitles m_Subtitles;
        Subtitles.Line m_CurrentLine;
        State m_State;

        public Subtitles ParseString(string input)
        {
            m_Subtitles = new Subtitles();

            StringReader reader = new StringReader(input);
            m_CurrentLine = new Subtitles.Line();
            while (reader.Peek() >= 0)
            {
                ParseLine(reader.ReadLine());
            }

            return m_Subtitles;
        }

        void ParseLine(string readLine)
        {
            switch (m_State)
            {
                case State.Segment:
                    ParseSegment(readLine);
                    m_State = State.Timestamp;
                    break;
                case State.Timestamp:
                    ParseTimeStamp(readLine);
                    m_State = State.Text;
                    break;
                case State.Text:
                    if (string.IsNullOrWhiteSpace(readLine))
                    {
                        m_Subtitles.AddLine(m_CurrentLine);
                        m_CurrentLine = new Subtitles.Line();
                        m_State = State.Segment;
                        break;
                    }
                    ParseText(readLine);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void ParseText(string text)
        {
            m_CurrentLine.text += $"{text}\n";
        }

        void ParseSegment(string text)
        {
            if (!int.TryParse(text, out int result)) return;
            m_CurrentLine.segment = result;
        }

        void ParseTimeStamp(string readLine)
        {
            const string k_TimeSeparator = " --> ";
            string[] times = readLine.Split(k_TimeSeparator);

            if (times.Length != 2)
            {
                m_CurrentLine.start = -1;
                m_CurrentLine.end = -1;
                return;
            }

            m_CurrentLine.start = ParseTime(times[0]);
            m_CurrentLine.end = ParseTime(times[1]);
        }

        float ParseTime(string time)
        {
            time = time.Trim();
            if (TimeSpan.TryParseExact(time, "hh\\:mm\\:ss\\,fff", CultureInfo.InvariantCulture, out TimeSpan result))
            {
                return (float)result.TotalSeconds;
            }

            return -1;
        }
    }
}
