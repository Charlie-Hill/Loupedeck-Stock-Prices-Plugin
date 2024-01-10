namespace Loupedeck.StockPlugin
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class StockPlugin : Plugin
    {
        // Gets a value indicating whether this is an API-only plugin.
        public override Boolean UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is a Universal plugin or an Application plugin.
        public override Boolean HasNoApplication => true;

        public event EventHandler<EventArgs> Tick;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // Initializes a new instance of the plugin class.
        public StockPlugin()
        {
            // Initialize the plugin log.
            PluginLog.Init(this.Log);

            // Initialize the plugin resources.
            PluginResources.Init(this.Assembly);
        }

        private async void Timer()
        {
            while (true && !this._cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(1000);
                Tick?.Invoke(this, new EventArgs());
            }
        }

        // This method is called when the plugin is loaded during the Loupedeck service start-up.
        public override void Load()
        {
            this.Timer();
        }

        // This method is called when the plugin is unloaded during the Loupedeck service shutdown.
        public override void Unload()
        {
        }
    }
}
