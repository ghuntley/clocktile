using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace ClockTile
{
    public sealed partial class MainPage : Page
    {
        private readonly ApplicationTrigger _applicationTrigger = new ApplicationTrigger();

        public MainPage()
        {
            this.InitializeComponent();
        }

        public static void UnregisterBackgroundTask(string taskName)
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    task.Value.Unregister(true);
                }
            }
        }

        public async Task UnregisterAndRegisterTask(string taskName, string taskEntryPoint)
        {
            UnregisterBackgroundTask(taskName);

            await BackgroundExecutionManager.RequestAccessAsync();

            var builder = new BackgroundTaskBuilder();

            builder.Name = taskName;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(new TimeTrigger(freshnessTime: 15, oneShot: false));

            BackgroundTaskRegistration task = builder.Register();

        }

        public void RunBackgroundTask(string taskName, string taskEntryPoint)
        {
            var taskBuilder = new BackgroundTaskBuilder
            {
                Name = taskName,
                TaskEntryPoint = taskEntryPoint,
                IsNetworkRequested = true
            };

            try
            {
                taskBuilder.SetTrigger(_applicationTrigger);
                taskBuilder.Register();
            }
            catch (Exception ex)
            {
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            const string taskName = "updateApplicationTile";
            const string taskEntryPoint = "ClockTile.BackgroundTasks.UpdateApplicationTile";

            Task.Run(async () => await UnregisterAndRegisterTask(taskName, taskEntryPoint));
            RunBackgroundTask(taskName, taskEntryPoint);

            base.OnNavigatedTo(e);
        }
    }
}