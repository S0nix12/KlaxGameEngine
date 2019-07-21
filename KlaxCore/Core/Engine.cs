using KlaxConfig;
using KlaxRenderer.Graphics;
using KlaxShared;
using KlaxShared.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KlaxCore.GameFramework;
using KlaxCore.GameFramework.Assets;
using KlaxCore.KlaxScript.Interfaces;
using KlaxIO.AssetManager.Assets;
using KlaxIO.Input;
using KlaxRenderer;
using KlaxShared.Attributes;
using KlaxIO.Log;

namespace KlaxCore.Core
{
    public interface IEngineSubsystem
    {
        void Init(CInitializer initializer);
    }

    public enum EEngineUpdatePriority
    {
        BeginFrame,
        EndFrame
    }

    public class CEngine : IDispatchable<EEngineUpdatePriority>
	{
		[CVar] public static int EnableEngineUpdate { get; private set; } = 1;
        public static CEngine Instance { get; private set; }
        public static void Create(CInitializer initializer, bool bRunOnSeparateThread)
        {
            if (Instance != null)
            {
                throw new Exception("You cannot create multiple engines at the same time!");
            }

            Input.InitInput();

            if (bRunOnSeparateThread)
            {
                Thread engineThread = new Thread(() =>
                {
                    Instance.Run();
                });

                Instance = new CEngine(engineThread);
                Instance.Init(initializer);
                engineThread.Start();
            }
            else
            {
                Instance = new CEngine(Thread.CurrentThread);
                Instance.Init(initializer);
                Instance.Run();
            }
        }

        private CEngine(Thread owningThread)
        {
            m_owningThread = owningThread;

            Config = new CConfigManager();
			Config.Init();
            Updater = new CEngineUpdater();

			CAssetRegistry.LoadInstance();
		}

        public void Init(CInitializer initializer)
        {
            Updater.Init(initializer);

			CLogger logger = initializer.Get<CLogger>();
			if (logger != null)
			{
				logger.Init();
			}

            IRenderSurface surface = initializer.Get<IRenderSurface>();
            if (surface != null)
            {
				CRenderer.Instance.Init(surface);
            }

			RegisterAssetTypes();
        }

		private void RegisterAssetTypes()
		{
			CAssetRegistry registry = CAssetRegistry.Instance;
			registry.RegisterAssetType<CMaterialAsset>();
			registry.RegisterAssetType<CMeshAsset>();
			registry.RegisterAssetType<CModelAsset>();
			registry.RegisterAssetType<CShaderAsset>();
			registry.RegisterAssetType<CTextureAsset>();
			registry.RegisterAssetType<CEntityAsset<CEntity>>();
			registry.RegisterAssetType<CKlaxScriptInterfaceAsset>();
		}

        private void Run()
        {
            while (!m_bShutdown)
            {
                m_dispatcherQueue.Execute((int)EEngineUpdatePriority.BeginFrame);

                float deltaTime = Updater.UpdateDeltaTime();

                // Update Input first poll input is thread safe but we should normally only poll here
                Input.PollInput();

				if (EnableEngineUpdate > 0)
				{
					CRenderer.Instance.BeginFrame(deltaTime);
					Config.Update(deltaTime);
					CurrentWorld?.Update(deltaTime);
					CRenderer.Instance.RenderFrame(deltaTime);
				}

                m_dispatcherQueue.Execute((int)EEngineUpdatePriority.EndFrame);
            }

            CurrentWorld?.Shutdown();
			CRenderer.Instance.Dispose();
            Input.Shutdown();
			CAssetRegistry.Instance.Dispose();
		}

		public void LoadWorld(CInitializer initializer, Action<CWorld> loadedCallback)
        {
            CurrentWorld = new CWorld();
            CurrentWorld.Init(initializer);
            loadedCallback(CurrentWorld);
        }

        public void Shutdown()
        {
            m_bShutdown = true;
        }

        public void Dispatch(EEngineUpdatePriority priority, Action action)
        {
            m_dispatcherQueue.Add((int)priority, action);
        }

        public bool IsInAuthoritativeThread()
        {
            return Thread.CurrentThread == m_owningThread;
        }

        public CConfigManager Config { get; private set; }
        public CWorld CurrentWorld { get; set; }
        public CEngineUpdater Updater { get; set; }

        private DispatcherQueue m_dispatcherQueue = new DispatcherQueue((int)EEngineUpdatePriority.EndFrame + 1);
        private Thread m_owningThread;
        private bool m_bShutdown;
    }
}
