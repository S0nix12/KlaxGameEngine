using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlaxConfig.Console
{
    public static class ConsoleUtility
    {
        public static string GetConsoleStringArgument(string command, int index)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return null;
            }

            bool bInsideQuotation = false;
            char currentCharacter = default(char);
            string currentArgument = "";
            List<string> arguments = new List<string>();

            for (int i = 0; i < command.Length; i++)
            {
                currentCharacter = command[i];

                switch (currentCharacter)
                {
                    case ' ':
                        if (!bInsideQuotation)
                        {
                            if (currentArgument.Length > 0)
                            {
                                arguments.Add(currentArgument);
                                if (arguments.Count > index)
                                {
                                    return arguments[index];
                                }

                                currentArgument = "";
                            }
                        }
                        else
                        {
                            currentArgument += currentCharacter;
                        }
                        break;
                    case '\"':
                        if (bInsideQuotation)
                        {
                            arguments.Add(currentArgument);
                            if (arguments.Count > index)
                            {
                                return arguments[index];
                            }

                            currentArgument = "";
                        }
                        bInsideQuotation = !bInsideQuotation;
                        break;
                    default:
                        currentArgument += currentCharacter;
                        break;
                }
            }

            if (currentArgument.Length > 0)
            {
                return currentArgument;
            }

            return null;
        }

        public static void ProcessConsoleString(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            bool bInsideQuotation = false;
            char currentCharacter = default(char);
            string currentArgument = "";
            List<string> arguments = new List<string>();

            for (int i = 0; i < command.Length; i++)
            {
                currentCharacter = command[i];

                switch (currentCharacter)
                {
                    case ' ':
                        if (!bInsideQuotation)
                        {
                            if (currentArgument.Length > 0)
                            {
                                arguments.Add(currentArgument);
                                currentArgument = "";
                            }
                        }
                        else
                        {
                            currentArgument += currentCharacter;
                        }
                        break;
                    case '\"':
                        if (bInsideQuotation)
                        {
                            arguments.Add(currentArgument);
                            currentArgument = "";
                        }
                        bInsideQuotation = !bInsideQuotation;
                        break;
                    default:
                        currentArgument += currentCharacter;
                        break;
                }
            }

            if (currentArgument.Length > 0)
            {
                arguments.Add(currentArgument);
            }

            CConfigManager manager = CConfigManager.Instance;

            //Search for CVar with first arguments name
            if (arguments.Count == 2)
            {
                if (manager.SetVariable(arguments[0], arguments[1]))
                {
                    return;
                }
            }

            //Try command instead
            string commandName = arguments[0];
            arguments.RemoveRange(0, 1);
            manager.InvokeConsoleCommand(commandName, arguments);
        }
    }
}
