using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tosna.Core;

namespace Tosna.Editor.IDE.Verification
{
	public class VerificationService
	{
		private readonly FilesManager filesManager;
		private readonly ILogger logger;

		private readonly HashSet<SingleFileManager> updatingQueue = new HashSet<SingleFileManager>();

		private readonly object sync = new object();

		private bool active;

		public VerificationService(FilesManager filesManager, ILogger logger)
		{
			this.filesManager = filesManager;
			this.logger = logger;
		}

		public void EnqueueFileChangesVerification(IEnumerable<SingleFileManager> updates)
		{
			lock (sync)
			{
				foreach (var update in updates)
				{
					updatingQueue.Add(update);
				}
				if (!active)
				{
					Task.Run(UpdateAll);
				}
			}
		}

		public void EnqueueFileChangesVerification(SingleFileManager update)
		{
			EnqueueFileChangesVerification(new[] { update });
		}
		
		public void EnqueueDependenciesVerification()
		{
			EnqueueFileChangesVerification(new SingleFileManager[] { });
		}

		private async Task UpdateAll()
		{
			lock (sync)
			{
				if (active)
				{
					return;
				}
				active = true;
			}

			try
			{
				var dependenciesCheckNeeded = true;

				while (true)
				{
					IReadOnlyCollection<SingleFileManager> tasks;
					lock (sync)
					{
						tasks = updatingQueue.ToArray();
						updatingQueue.Clear();

						if (!tasks.Any() && !dependenciesCheckNeeded)
						{
							active = false;
							return;
						}
					}

					if (tasks.Any())
					{
						await Task.WhenAll(tasks.Select(singleFileManager =>
							Task.Run(singleFileManager.Verify)));
						dependenciesCheckNeeded = true;
					}
					else
					{
						await filesManager.VerifyDependenciesAsync();
						dependenciesCheckNeeded = false;
					}

					await Task.Delay(500);
				}
			}
			catch (Exception e)
			{
				active = false;
				logger.LogError("Verification failure", e);
			}
		}
	}
}
