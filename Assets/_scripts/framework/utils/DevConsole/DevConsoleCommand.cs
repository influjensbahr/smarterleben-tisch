//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

namespace OTBT.Framework.Utils.DevConsole
{
    public abstract class DevConsoleCommand
    {
        readonly string m_CommandID;
        readonly string m_CommandDescription;
        readonly string m_CommandFormat;

        public string ID => m_CommandID;
        public string Description => m_CommandDescription;
        public string Format => m_CommandFormat;

        public DevConsoleCommand(string id, string description, string format)
        {
            m_CommandID = id;
            m_CommandDescription = description;
            m_CommandFormat = format;

            if (string.IsNullOrEmpty(m_CommandFormat))
            {
                m_CommandFormat = "<no parameters>";
            }

            DevConsole.AddCommand(this);
        }

        public string Print()
        {
            return $"{m_CommandID}: {m_CommandDescription}\n\t{m_CommandFormat}";
        }
        public string PrintSimple()
        {
            return $"{m_CommandID}: {m_CommandDescription}";
        }

        public abstract string Process(string[] args);
    }
}
