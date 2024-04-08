//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace OTBT.Framework.Utils.DevConsole
{
    public class DevConsole
    {
        static DevConsole instance;

        readonly LinkedList<string> m_CommandHistory = new();
        readonly List<DevConsoleCommand> m_Commands = new();
        readonly string m_Prefix;
        LinkedListNode<string> m_SelectedCommand;
        public event Action<string> onLog;

        public DevConsole(string prefix)
        {
            m_Prefix = prefix;
            instance = this;

            m_CommandHistory.AddLast(m_Prefix);
            m_SelectedCommand = m_CommandHistory.Last;

            DevConsoleCommand commands = new SimpleConsoleCommand("list",
                "List all commands.",
                "[detailed]",
                LogCommands);
            DevConsoleCommand help = new SimpleConsoleCommand("help",
                "Show help for a specific command.",
                "<command:string>",
                HelpCommand);
            DevConsoleCommand joke = new SimpleConsoleCommand("witz",
                "Tells you a good ol' german joke",
                null,
                _ => "Was macht eine Piratin am Computer?\nSie drückt die Enter-Taste.");
        }

        public static void AddCommand(DevConsoleCommand command)
        {
            instance.Add(command);
        }

        void Add(DevConsoleCommand command)
        {
            if (m_Commands.Any(cmd => cmd.ID.Equals(command.ID))) return;
            m_Commands.Add(command);
        }

        string LogCommands(string[] args)
        {
            var detailed = false;
            if (args.Length > 0)
                detailed = args[0].Equals("detailed");

            var output = new StringBuilder();
            foreach (DevConsoleCommand command in m_Commands)
                if (detailed)
                    output.AppendLine(m_Prefix + command.Print());
                else
                    output.AppendLine(m_Prefix + command.PrintSimple());
            return output.ToString();
        }

        string HelpCommand(string[] commandInput)
        {
            var target = "help";
            if (commandInput.Length > 0)
                target = commandInput[0];

            var output = new StringBuilder();

            var commands = m_Commands.Where(command => target.Equals(command.ID));
            foreach (DevConsoleCommand command in commands)
            {
                output.AppendLine(m_Prefix + command.Print());
                return output.ToString();
            }
            return string.Empty;
        }

        public string GetPrevious()
        {
            if (m_SelectedCommand.Previous != null)
                m_SelectedCommand = m_SelectedCommand.Previous;
            return m_SelectedCommand.Value;
        }

        public string GetNext()
        {
            if (m_SelectedCommand.Next != null)
                m_SelectedCommand = m_SelectedCommand.Next;
            return m_SelectedCommand.Value;
        }

        public void ProcessCommand(string input)
        {
            if (!input.StartsWith(m_Prefix))
                return;

            m_CommandHistory.AddLast(input);
            m_SelectedCommand = m_CommandHistory.Last;

            input = input.Remove(0, m_Prefix.Length);
            input = input.Trim();

            string[] splitInput = input.Split(' ');

            string commandInput = splitInput[0];
            string[] arguments = splitInput.Skip(1).ToArray();

            ProcessCommand(commandInput, arguments);
        }

        void ProcessCommand(string commandInput, string[] args)
        {
            var command = m_Commands.FirstOrDefault(cmd => commandInput.Equals(cmd.ID, StringComparison.OrdinalIgnoreCase));
            if (command == null) return;

            var output = command.Process(args);
            onLog?.Invoke(output);
        }

        public string TryAutoComplete(string input)
        {
            if (!input.StartsWith(m_Prefix))
                return m_Prefix;

            input = input.Remove(0, m_Prefix.Length);
            input = input.Trim();

            foreach (DevConsoleCommand command in m_Commands)
                if (command.ID.StartsWith(input))
                    return m_Prefix + command.ID;
            return m_Prefix + input;
        }
    }
}
