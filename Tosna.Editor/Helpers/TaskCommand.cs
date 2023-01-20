using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Tosna.Core;

namespace Tosna.Editor.Helpers
{
	public class TaskCommand : ICommand
	{
		private readonly Func<Task> taskFunc;
		private readonly Func<bool> canExecuteFunc;
		private readonly ILogger logger;

		public TaskCommand(Func<Task> taskFunc, Func<bool> canExecuteFunc, ILogger logger = null)
		{
			this.taskFunc = taskFunc;
			this.canExecuteFunc = canExecuteFunc;
			this.logger = logger;
		}

		public bool CanExecute(object parameter)
		{
			return canExecuteFunc();
		}

		public async void Execute(object parameter)
		{
			try
			{
				await taskFunc.Invoke();
			}
			catch (Exception e)
			{
				logger?.LogError($"Execution error: {e.Message}", e);
			}
		}

		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler CanExecuteChanged;
	}
}