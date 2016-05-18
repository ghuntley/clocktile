using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace ClockTile.BackgroundTasks
{
    public sealed class UpdateApplicationTile : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var cancel = new CancellationTokenSource();
            taskInstance.Canceled += (s, e) =>
            {
                cancel.Cancel();
                cancel.Dispose();
            };

            var deferral = taskInstance.GetDeferral();

            try
            {
                var template = await LoadTemplate("PrimaryTile.xml");
                var applicationTile = string.Format(template, DateTime.Now.TimeOfDay, DateTime.Now.Date);
                UpdatePrimaryWith(applicationTile);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                deferral.Complete();
            }
        }

        private static async Task<string> LoadTemplate(string fileName)
        {
            var folder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            var file = await folder.GetFileAsync(fileName);
            return await FileIO.ReadTextAsync(file);
        }

        private static void ResetTileToDefault()
        {
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.Clear();
        }

        private static void SetPrimary(XmlDocument xmlDocument)
        {
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();

            // clear everything, display default values from application manifest.
            tileUpdater.Clear();

            // always display the latest version.
            tileUpdater.EnableNotificationQueue(false);
            tileUpdater.EnableNotificationQueueForSquare150x150(false);
            tileUpdater.EnableNotificationQueueForSquare310x310(false);
            tileUpdater.EnableNotificationQueueForWide310x150(false);

            tileUpdater.Update(new TileNotification(xmlDocument));
        }

        private static void UpdatePrimaryWith(string tileXml)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(tileXml);

            SetPrimary(xmlDocument);
        }
    }
}