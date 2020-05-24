using System;
using System.IO;

namespace KlaxIO.Log
{
    public class CLogger : IDisposable
    {
        public event Action<string, string> Logged;

        public CLogger(string filePath, bool bRelativeToUserFolder, bool bThreadSafe, bool bDeferInitialization)
        {
			if (LogUtility.Logger == null)
			{
				LogUtility.Logger = this;
			}

			m_rawPath = filePath;
			m_bIsDeferredInit = bDeferInitialization;
			m_bRelativeToUserFolder = bRelativeToUserFolder;
			m_bThreadSafe = bThreadSafe;
            m_lock = new object();

			if (!bDeferInitialization)
			{
				if (m_bRelativeToUserFolder)
				{
					filePath = Paths.UserDirectory + "\\" + filePath;
				}

				FetchLogFileStream(filePath);
				InternalInit();
			}
        }

		public void Init()
		{
			if (m_bIsDeferredInit)
			{
				string filePath = m_rawPath;

				if (m_bRelativeToUserFolder)
				{
					filePath = Paths.UserDirectory + "\\" + filePath;
				}

				FetchLogFileStream(filePath);
				InternalInit();
			}
		}

        public void Dispose()
        {
            if (m_bThreadSafe)
            {
                lock (m_lock)
                {
                    if (m_writer != null)
                    {
                        m_writer.Dispose();
                        m_writer = null;
                    }
                }
            }
            else
            {
                if (m_writer != null)
                {
                    m_writer.Dispose();
                    m_writer = null;
                }
            }
        }

        public void Log(string text, params object[] arguments)
        {
            if (m_bThreadSafe)
            {
                lock (m_lock)
                {
                    if (CurrentVerbosity >= TargetVerbosity)
                    {
                        InternalLog(text, arguments);
                    }
                }
            }
            else
            {
                if (CurrentVerbosity >= TargetVerbosity)
                {
                    InternalLog(text, arguments);
                }
            }
        }

        public void Log(string text, ELogVerbosity verbosity, params object[] arguments)
        {
            if (m_bThreadSafe)
            {
                lock (m_lock)
                {
                    if (verbosity >= TargetVerbosity)
                    {
                        InternalLog(text, arguments);
                    }
                }
            }
            else
            {
                if (verbosity >= TargetVerbosity)
                {
                    InternalLog(text, arguments);
                }
            }
        }

        private void InternalLog(string text, params object[] arguments)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Console.WriteLine(text, arguments);

            if (m_writer != null)
            {
                string timestamp = DateTime.Now.ToString("hh:mm:ss.fff");
                string filledInText = string.Format(text, arguments);
                string timestampedText = string.Format("[{0}] {1}", timestamp, filledInText);
                m_writer.WriteLine(timestampedText);

                //todo valentin: expose logic to flush at start of render loop
                m_writer.Flush();

                Logged?.Invoke(filledInText, timestampedText);
            }
        }

        private void InternalInit()
        {
#if NDEBUG
            CurrentVerbosity = ELogVerbosity.Release;
#else
            CurrentVerbosity = ELogVerbosity.Debug;
#endif //NDEBUG 

            TargetVerbosity = CurrentVerbosity;
        }

        private void FetchLogFileStream(string path)
        {
            m_writer = File.CreateText(path);
        }

        public ELogVerbosity CurrentVerbosity { get; set; }
        public ELogVerbosity TargetVerbosity { get; set; }

        private StreamWriter m_writer;
        private object m_lock;
        private readonly bool m_bThreadSafe;

		private bool m_bIsDeferredInit;
		private bool m_bRelativeToUserFolder;
		private string m_rawPath;
    }
}
