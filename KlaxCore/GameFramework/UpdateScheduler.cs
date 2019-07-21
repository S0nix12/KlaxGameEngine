using System.Collections.Generic;
using KlaxShared.Attributes;
using KlaxShared.Utilities;

namespace KlaxCore.GameFramework
{
	public delegate void UpdateCallback(float deltaTime);

	public enum EUpdatePriority
	{
		Editor,
		ResourceLoading,
		Earliest,
		PrePhysics,
		PostPhysics,
		Default,
		Latest,
		Count
	}
	public class CUpdateScope
	{
		internal CUpdateScope(UpdateCallback callback)
		{
			Callback = callback;
			m_bIsConnected = true;
		}

		public bool IsConnected() { return m_bIsConnected; }

		public void Disconnect() { m_bIsConnected = false; }

		public UpdateCallback Callback { get; set; }
		private bool m_bIsConnected;
	}

	public class CUpdateScheduler
	{
		const int MAX_PRIORITY = (int) EUpdatePriority.Count;
        const int DEFAULT_BUCKET_SIZE = 64;

        public CUpdateScheduler()
        {
            for (int i = 0; i < MAX_PRIORITY; i++)
            {
                m_priorityUpdateBuckets[i] = new List<CUpdateScope>(DEFAULT_BUCKET_SIZE);
                m_oneTimeUpdateBucket[i] = new List<CUpdateScope>(DEFAULT_BUCKET_SIZE);
            }
        }

		public void Update(float deltaTime, EUpdatePriority startPriority, EUpdatePriority endPriority)
		{
			int startIndex = (int) startPriority;
			int endIndex = (int) endPriority;
			for (int i = startIndex; i <= endIndex; ++i)
			{
				List<CUpdateScope> updates = m_priorityUpdateBuckets[i];
				for (int j = updates.Count - 1; j >= 0; --j)
				{
					CUpdateScope update = updates[j];
					if (update.IsConnected())
					{
						update.Callback(deltaTime);
					}
					else
					{
						// If the update was disconnected remove it from the list
						ContainerUtilities.RemoveSwapAt(updates, j);
					}
				}

				List<CUpdateScope> singleUpdates = m_oneTimeUpdateBucket[i];
				for (int j = 0; j < singleUpdates.Count; j++)
				{
					CUpdateScope update = singleUpdates[j];
					if (update.IsConnected())
					{
						update.Callback(deltaTime);
					}
					update.Disconnect();
				}
				singleUpdates.Clear();
			}
		}

		/// <summary>
		/// Register a new UpdateCallback that is called every frame until disconnected
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="priority"></param>
		/// <returns></returns>
		public CUpdateScope Connect(UpdateCallback callback, EUpdatePriority priority)
		{
			CUpdateScope outScope = new CUpdateScope(callback);

			// Add the scope to the priority bucket
			m_priorityUpdateBuckets[(int)priority].Add(outScope);

			return outScope;
		}

		/// <summary>
		/// Register a new UpdateCallback that is called one time
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="priority"></param>
		/// <returns></returns>
		public CUpdateScope ConnectOneTimeUpdate(UpdateCallback callback, EUpdatePriority priority)
		{
			CUpdateScope outScope = new CUpdateScope(callback);
			m_oneTimeUpdateBucket[(int)priority].Add(outScope);
			return outScope;
		}

		public void Disconnect(CUpdateScope scope)
		{
			scope?.Disconnect();
		}

		private readonly List<CUpdateScope>[] m_priorityUpdateBuckets = new List<CUpdateScope>[(int) EUpdatePriority.Count];
		private readonly List<CUpdateScope>[] m_oneTimeUpdateBucket = new List<CUpdateScope>[(int) EUpdatePriority.Count];
	}
}
