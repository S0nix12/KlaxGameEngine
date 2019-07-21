using System;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfSharpDxControl;

namespace KlaxEditor
{
    class CFileStatsViewModel : CToolViewModel
    {
        public CFileStatsViewModel()
            : base("Scene Viewer")
        {
            ContentId = ToolContentId;

            BitmapImage bi = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Tabs/viewport.png"));
            IconSource = bi;
            ViewportContent = new CDX11HostControl();

            m_framesTimer.Tick += (obj, args) =>
            {
                Title = string.Format("Scene Viewer ({0} FPS)", ViewportContent.Renderer.FPS);
            };
            m_framesTimer.Interval = TimeSpan.FromSeconds(1.0);
            m_framesTimer.Start();
        }

        public const string ToolContentId = "FileStatsTool";

        private CDX11HostControl m_viewportContent;
        public CDX11HostControl ViewportContent
        {
            get { return m_viewportContent; }
            set
            {
                m_viewportContent = value;
                RaisePropertyChanged();
            }
        }


        DispatcherTimer m_framesTimer = new DispatcherTimer();
    }
}
