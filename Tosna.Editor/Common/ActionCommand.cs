using System;
using System.Windows.Input;
using Tosna.Core.Common;

namespace Tosna.Editor.Common
{
	public class ActionCommand : ICommand
	{
		private readonly Action executeAction;
		private readonly Func<bool> canExecuteFunc;
		private readonly ILogger logger;
		
		public string PublicName { get; }

		public ActionCommand(Action executeAction, Func<bool> canExecuteFunc, ILogger logger = null)
		{
			this.executeAction = executeAction;
			this.canExecuteFunc = canExecuteFunc;
			this.logger = logger;
		}
		
		public ActionCommand(Action executeAction, Func<bool> canExecuteFunc, string publicName, ILogger logger = null)
		{
			this.executeAction = executeAction;
			this.canExecuteFunc = canExecuteFunc;
			this.logger = logger;

			PublicName = publicName;
		}

		public bool CanExecute(object parameter)
		{
			return canExecuteFunc();
		}

		public void Execute(object parameter)
		{
			try
			{
				executeAction();
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