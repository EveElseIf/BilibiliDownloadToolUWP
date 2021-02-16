using BilibiliDownloadTool.Services;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BilibiliDownloadTool
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetLogger("Main");
        public static SqliteData SqliteData { get; private set; } = new Func<SqliteData>(() =>
        {
            var sql = new SqliteData();
            sql.InitializeDb().Wait();
            return sql;
        })();
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            NLog.GlobalDiagnosticsContext.Set("LogPath", ApplicationData.Current.LocalFolder.Path + "\\");
            _logger.Info("\n程序启动");
            Current.UnhandledException += (s, e) =>
            {
                _logger.Fatal(e.Exception, e.Message);
            };
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        #region AppService Codes
        private static BackgroundTaskDeferral _appServiceDeferral;
        public static AppServiceConnection Connection { get; set; }
        public static event EventHandler<AppServiceTriggerDetails> AppServiceConnected;
        public static event EventHandler AppServiceDisconnected;
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                if (details.CallerPackageFamilyName == Package.Current.Id.FamilyName)
                {
                    try
                    {
                        _appServiceDeferral = args.TaskInstance.GetDeferral();
                        args.TaskInstance.Canceled += (s, e) =>
                        {
                            _appServiceDeferral?.Complete();
                            _appServiceDeferral = null;
                            Connection = null;
                            AppServiceDisconnected?.Invoke(this, null);
                        };
                        Connection = details.AppServiceConnection;
                        AppServiceConnected?.Invoke(this, args.TaskInstance.TriggerDetails as AppServiceTriggerDetails);
                        _logger.Info("辅助进程服务连接成功");
                    }
                    catch(Exception ex)
                    {
                        _logger.Error("辅助进程服务连接失败");
                        _logger.Error(ex);
                    }
                }
            }
            else
            {
                _logger.Error("辅助进程服务连接失败");
                _logger.Error(args.TaskInstance.TriggerDetails.GetType());
            }
        }
        #endregion
    }
}
