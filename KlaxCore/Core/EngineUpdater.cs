using KlaxShared;
using System;
using System.Diagnostics;

namespace KlaxCore.Core
{
    public class CEngineUpdater : IEngineSubsystem
    {
        public CEngineUpdater()
        {
        }

        public void Init(CInitializer initializer)
        {
            m_stopWatch.Start();
        }

        public float UpdateDeltaTime()
        {
            TimeSpan stopWatch = m_stopWatch.Elapsed;
            DeltaTime = (float)(stopWatch - m_lastFrameTime).TotalSeconds;

            m_lastFrameTime = stopWatch;
            return DeltaTime;
        }

        public float DeltaTime { get; private set; }

        private Stopwatch m_stopWatch = new Stopwatch();
        private TimeSpan m_lastFrameTime;
    }
}
