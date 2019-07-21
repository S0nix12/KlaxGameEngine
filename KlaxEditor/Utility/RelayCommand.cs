using System;
using System.Windows.Input;

namespace KlaxEditor
{
    public delegate void Executor(object argument);
    public delegate bool ExecutorCondition(object argument);

    public class CRelayCommand : ICommand
    {
        public CRelayCommand(Executor executor)
            : this(executor, null)
        {
        }

        public CRelayCommand(Executor executor, ExecutorCondition executorCondition)
        {
            m_executor = executor ?? throw new ArgumentNullException("Executor must not be null!");
            m_executorCondition = executorCondition;
        }

        public bool CanExecute(object parameter)
        {
            return m_executorCondition == null ? true : m_executorCondition(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            m_executor(parameter);
        }

        private readonly Executor m_executor;
        private readonly ExecutorCondition m_executorCondition;
    }
}
