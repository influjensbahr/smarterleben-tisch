//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using System;

namespace OTBT.Framework.Utils.DevConsole
{
    public class SimpleConsoleCommand : DevConsoleCommand
    {
        readonly Func<string[], string> m_Command;
        public SimpleConsoleCommand(string id, string description, string format, Func<string[], string> command) : base(id, description, format)
        {
            m_Command = command;
        }

        public override string Process(string[] args)
        {
            string result;
            result = m_Command?.Invoke(args);
            return result;
        }
    }
}
