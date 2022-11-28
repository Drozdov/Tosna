using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tosna.Core.Common;

namespace Tosna.Editor.IDE.Verification
{
	public class VerificationService
	{
		private readonly FilesManager filesManager;
		private readonly IInfoLogger logger;

		private readonly HashSet<SingleFileManager> updatingQueue = new HashSet<SingleFileManager>();

		private readonly object sync = new object();

		private bool active;

		public VerificationService(FilesManager filesManager, IInfoLogger logger)
		{
			this.filesManager = filesManager;
			this.logger = logger;
		}

		public void EnqueueUpdate(SingleFileManager singleFileManager)
		{
			lock (sync)
			{
				updatingQueue.Add(singleFileManager);
				if (!active)
				{
					Task.Factory.StartNew(UpdateAll);
				}
			}
		}

		private void UpdateAll()
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
						foreach (var singleFileManager in tasks)
						{
							singleFileManager.Verify();
						}
						dependenciesCheckNeeded = true;
					}
					else
					{
						filesManager.VerifyDependencies();
						dependenciesCheckNeeded = false;
					}

					Thread.Sleep(500);
				}
			}
			catch (Exception e)
			{
				active = false;
				logger.LogError("Configurator", e);
			}
		}
	}
}
