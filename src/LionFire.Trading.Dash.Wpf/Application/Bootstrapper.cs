using LionFire.Applications.Hosting;
using LionFire.Assets;
using LionFire.Execution;
using LionFire.Extensions.Logging;
using LionFire.Templating;
using LionFire.Trading.Applications;
using LionFire.Trading.Bots;
using LionFire.Trading.Dash.Wpf;
using LionFire.Trading.Proprietary.Bots;
using LionFire.Trading.Spotware.Connect;
using LionFire.Trading.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LionFire.Parsing.String;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.DataGrid;
using Newtonsoft.Json;
using LionFire.Trading.Backtesting;
using Newtonsoft.Json.Linq;
using Caliburn.Micro;
using IEventAggregator = Caliburn.Micro.IEventAggregator;
using EventAggregator = Caliburn.Micro.EventAggregator;
using LionFire.Structures;

namespace LionFire.Trading.Dash.Wpf
{
    public class HelloBootstrapper : BootstrapperBase
    {
        public HelloBootstrapper()
        {
            Initialize();
        }

        private readonly SimpleContainer _container = new SimpleContainer();

        protected override void Configure()
        {
            base.Configure();


            _container.Singleton<IEventAggregator, EventAggregator>();
            ManualSingleton<IEventAggregator>.Instance = _container.GetInstance<IEventAggregator>();

            // http://caliburnmicro.codeplex.com/discussions/287228
            MessageBinder.SpecialValues.Add("$orignalsourcecontext", context =>
            {
                var args = context.EventArgs as RoutedEventArgs;
                if (args == null)
                {
                    return null;
                }

                var fe = args.OriginalSource as FrameworkElement;
                if (fe == null)
                {
                    return null;
                }

                return fe.DataContext;
            });

            MessageBinder.SpecialValues.Add("$name", context =>
            {
                var args = context.EventArgs as RoutedEventArgs;
                if (args == null)
                {
                    return null;
                }

                var fe = args.OriginalSource as FrameworkElement;
                if (fe == null)
                {
                    return null;
                }

                return fe.Name;
            });

            ConventionManager.AddElementConvention<Calendar>(Calendar.SelectedDateProperty, "DataContext", "DateChanged");
            
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            TypeResolverTemp.Register();
            InitApp();
            DisplayRootViewFor<ShellViewModel>();
        }

        public static string AppDataRoot { get { return @"C:\programdata\lionfire\Trading"; } } // HARDPATH

        private IAppHost app;
        private void InitApp()
        {
            LionFireEnvironment.ProgramName = "Trading Dash";
            LionFireEnvironment.AppDataDirName = "Trading";
            //LionEnvironment.ProgramDataDir = @"C:\programdata";
            
            LionFire.Extensions.Logging.NLog.NLogConfig.LoadDefaultConfig();

            //tWorkspace.Assemblies = new List<string> { "LionFire.Trading.Proprietary" };

            //StatusText = "Initializing";

            app = new AppHost()

            #region Bootstrap
                                .AddJsonAssetProvider(AppDataRoot)
                                .Bootstrap()
            #endregion

            #region Logging
                                                    .ConfigureServices(sc => sc.AddLogging())
                                                    .AddInit(a => a
                                                            .ServiceProvider.GetService<ILoggerFactory>()
                                                            .AddNLog()
                                                        //.AddConsole()
                                                        )
            #endregion

                                //.AddSpotwareConnectClient("LionFire.Trading.Sandbox")
                                .AddSpotwareConnectClient("LionFire.Trading.App")
                                ;

            //StatusText = "Starting app";
            app.Run();
            //StatusText = "App started";
        }


    }
}
