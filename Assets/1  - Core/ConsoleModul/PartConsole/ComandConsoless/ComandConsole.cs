using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleModul.Logger;
using UnityEngine;
using ILogger = ConsoleModul.Logger.ILogger;

namespace ConsoleModul.PartConsole.ComandConsoless
{
    public class ComandConsole : IPartConsole
    {
        public ILogger LoggerConsole => _logger;
        private readonly ILogger _logger;
        private List<CommandBase> _commands;

        public event Action Cleaned;

        public ComandConsole(ILogger logger)
        {
            _logger = logger;
            _commands = new List<CommandBase>();
            _commands.Add(CreateHelpCommand());
        }

        public void Add(CommandBase commandBase) => _commands.Add(commandBase);

        public void ClearCommands()
        {
            _commands.Clear();
            _commands.Add(CreateHelpCommand());
            Cleaned?.Invoke();
        }

        public void Remove(CommandBase commandBase) => _commands.Remove(commandBase);

        public void Invoke(string comandTxt)
        {
            var partComand = comandTxt.Split(' ');
            var comand = _commands.FirstOrDefault(x => x.Id == partComand[0]);
            _logger.Log(new ConsoleMessage().SetMessage(comandTxt).SetColor(Color.green).SetBolt(true));
            if(comand==null)
                _logger.Log(new ConsoleMessage().SetMessage($"no comand by this id {partComand[0]}").SetColor(Color.red));
            if (comand is Command)
            {
                _logger.Log(new ConsoleMessage().SetMessage($"Start command - {comand.Id}; {comand.Description}"));
                ((Command)comand).Invoke();
            }
            else if (comand is ArgumentCommand)
            {
                _logger.Log(new ConsoleMessage().SetMessage($"Start arg command  - {comand.Id}; {comand.Description}"));
                try
                {
                    var listTemo = partComand.ToList();
                    listTemo.RemoveAt(0);
                    ((ArgumentCommand)comand).Invoke(listTemo.ToArray());
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    _logger.LogError(e.StackTrace);
                }
            }
        }

        private Command CreateHelpCommand()
        {
            var result = new Command($"output all comand in cosloe", "Help",()=>
            {
                foreach (var commandBase in _commands)
                    _logger.Log(new ConsoleMessage().SetMessage($"Format commnad: ({commandBase.Format}); What do it - ({commandBase.Description})"));
            });
            return result;
        }
    }
}