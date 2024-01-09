namespace Loupedeck.StockPlugin
{
    using System;
    using System.Globalization;
    using System.Net.Http;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    // This class implements an example command that counts button presses.

    public class StockTicker : PluginDynamicCommand
    {
        public event EventHandler<EventArgs> Tick;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private Double _stockPrice = 0;
        private String _ticker = "RBLX";

        private async void Timer()
        {
            while (true && !this._cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(1000);
                Tick?.Invoke(this, new EventArgs());
            }

        }

        // Initializes the command class.
        public StockTicker()
            : base(displayName: "Stock Ticker", description: "Displays the current stock price", groupName: "")
        {
        }

        protected override Boolean OnLoad()
        {
            this.Timer();
            this.MakeProfileAction("text;Enter the ticker you want to use");

            this.Tick += (sender, e) => this.ActionImageChanged("");
            return base.OnLoad();
        }

        protected async Task<Object> getStockPrice()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync("http://api.londonmarket.xyz/stockInfo/" + this._ticker);
                if (response != null)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<object>(jsonString);
                }
            }

            return null;
        }

        // This method is called when the user executes the command.
        protected override async void RunCommand(String actionParameter)
        {
            this._ticker = actionParameter;
            dynamic stockResponse = await this.getStockPrice();
            this._stockPrice = stockResponse.price;
            this.ActionImageChanged(); // Notify the Loupedeck service that the command display name and/or image has changed.
            PluginLog.Info($"{this._ticker} price is {this._stockPrice}");
            this.Tick += (sender, e) => this.ActionImageChanged("");
            this.ActionImageChanged(actionParameter);
        }

        // This method is called when Loupedeck needs to show the command on the console or the UI.
        //protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
        //    $"{actionParameter}{Environment.NewLine}${this._stockPrice}";

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

            Int32 idx = actionParameter.LastIndexOf("/");
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.Clear(BitmapColor.Black);
                if (!String.IsNullOrEmpty(actionParameter))
                {
                    var x1 = bitmapBuilder.Width * 0.1;
                    var w = bitmapBuilder.Width * 0.8;
                    var y1 = bitmapBuilder.Height * 0.42;
                    var y2 = bitmapBuilder.Height * 0.62;
                    var y3 = bitmapBuilder.Height * 0.05;
                    var h = bitmapBuilder.Height * 0.3;

                    bitmapBuilder.DrawText(actionParameter.Substring(idx + 1).Replace("_", " "), (Int32)x1, (Int32)y3, (Int32)w, (Int32)h, new BitmapColor(255, 255, 255, 200), imageSize == PluginImageSize.Width90 ? 13 : 9, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                    bitmapBuilder.DrawText(this._stockPrice.ToString(), (Int32)x1, (Int32)y2, (Int32)w, (Int32)h, new BitmapColor(255, 255, 255, 200), imageSize == PluginImageSize.Width90 ? 13 : 9, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                }
                return bitmapBuilder.ToImage();
            }
        }

    }
}
