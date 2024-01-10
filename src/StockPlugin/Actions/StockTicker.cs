namespace Loupedeck.StockPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    // Main Stock Ticker command class.

    public class StockTicker : PluginDynamicCommand
    {
        private StockPlugin _plugin;

        private Double _stockPrice = 0;
        private Double _previousClosePrice = 0;
        private String _ticker = "UA";

        // Initializes the command class.
        public StockTicker() : base(displayName: "Stock Ticker", description: "Displays the current stock price", groupName: "")
        {
            this.MakeProfileAction("text;Enter the ticker you want to use");
        }

        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as StockPlugin;

            this._plugin.Tick += (sender, e) => this.ActionImageChanged("");

            this.RunCommand(this._ticker);

            return base.OnLoad();
        }

        // Method to make the API call to fetch stock info
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
            this._previousClosePrice = stockResponse.previousClose;
            this.ActionImageChanged(); // Notify the Loupedeck service that the command display name and/or image has changed.
            PluginLog.Info($"{this._ticker} price is {this._stockPrice}");
            this._plugin.Tick += (sender, e) => this.ActionImageChanged("");
            this.ActionImageChanged(actionParameter);
        }

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

                    BitmapColor color = new BitmapColor();
                    if (this._stockPrice < this._previousClosePrice)
                    {
                        color = new BitmapColor(255, 0, 0);
                    } else
                    {
                        color = new BitmapColor(0, 255, 0);
                    }

                    bitmapBuilder.DrawText(actionParameter.Substring(idx + 1).Replace("_", " "), (Int32)x1, (Int32)y3, (Int32)w, (Int32)h, color, imageSize == PluginImageSize.Width90 ? 20 : 9, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                    bitmapBuilder.DrawText(this._stockPrice.ToString(), (Int32)x1, (Int32)y2, (Int32)w, (Int32)h, color, imageSize == PluginImageSize.Width90 ? 25 : 9, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                }
                return bitmapBuilder.ToImage();
            }
        }

    }
}
