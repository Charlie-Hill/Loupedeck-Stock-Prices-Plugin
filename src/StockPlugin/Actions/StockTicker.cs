namespace Loupedeck.StockPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    public class StockTicker : PluginDynamicCommand
    {
        private StockPlugin _plugin;
        private readonly ConcurrentDictionary<String, dynamic> _tickers;
        private readonly Timer _updateTimer;
        private bool _hasLoaded;

        public StockTicker() : base(displayName: "Stock Ticker", description: "Displays the current stock price", groupName: "")
        {
            this.MakeProfileAction("text;Enter the ticker you want to use");
            this._tickers = new ConcurrentDictionary<String, dynamic>();

            this._updateTimer = new Timer(this.UpdateStockPrices, null, TimeSpan.Zero, TimeSpan.FromMinutes(3));
        }

        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as StockPlugin;
            this._plugin.Tick += (sender, e) => this.OnPluginTick();
            return base.OnLoad();
        }

        public void OnPluginTick()
        {
            if (!this._hasLoaded && (this._tickers.Count() > 0))
            {
                this.UpdateStockPrices(null);
            }
            base.ActionImageChanged();
        }

        private async void UpdateStockPrices(object state)
        {
            PluginLog.Info($"Called UpdateStockPrices");

            foreach (var ticker in this._tickers.Keys.ToArray())
            {
                await this.getStockPrice(ticker);
            }

            if (!this._hasLoaded && (this._tickers.Count() > 0))
            {
                this._hasLoaded = true;
            }

            this.OnPluginTick();
        }

        protected async Task<dynamic> getStockPrice(String ticker)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("http://api.londonmarket.xyz/stockInfo/" + ticker);
                if (response != null)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var tickerData = JsonConvert.DeserializeObject<dynamic>(jsonString);
                    this._tickers[ticker] = tickerData;
                    base.ActionImageChanged(ticker);

                    return tickerData;
                }
            }
            return null;
        }

        protected override void RunCommand(String actionParameter) => base.ActionImageChanged(actionParameter);

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (!String.IsNullOrEmpty(actionParameter))
            {
                if (this._tickers.TryGetValue(actionParameter, out dynamic tickerData))
                {
                    if (tickerData != null)
                    {
                        CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                        var idx = actionParameter.LastIndexOf("/");
                        var _ticker = actionParameter.Substring(idx + 1).Replace("_", " ");
                        using (var bitmapBuilder = new BitmapBuilder(imageSize))
                        {
                            var backgroundColor = (tickerData.price < tickerData.previousClose) ? new BitmapColor(255, 0, 0) : new BitmapColor(8, 170, 63);
                            //bitmapBuilder.Clear(new BitmapColor(0, 255, 0));
                            bitmapBuilder.Clear(backgroundColor);
                            var x1 = bitmapBuilder.Width * 0.1;
                            var w = bitmapBuilder.Width * 0.8;
                            var y1 = bitmapBuilder.Height * 0.42;
                            var y2 = bitmapBuilder.Height * 0.62;
                            var y3 = bitmapBuilder.Height * 0.05;
                            var h = bitmapBuilder.Height * 0.3;
                            //var color = tickerData.price < tickerData.previousClose ? new BitmapColor(255, 0, 0) : new BitmapColor(0, 255, 0);
                            bitmapBuilder.DrawText(_ticker, (Int32)x1, (Int32)y3, (Int32)w, (Int32)h, BitmapColor.White, imageSize == PluginImageSize.Width90 ? 20 : 9, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                            bitmapBuilder.DrawText(Math.Round((Single)tickerData.price, 2).ToString(), (Int32)x1, (Int32)y2, (Int32)w, (Int32)h, BitmapColor.White, imageSize == PluginImageSize.Width90 ? 25 : 9, imageSize == PluginImageSize.Width90 ? 2 : 0, 10);
                            return bitmapBuilder.ToImage();
                        }
                    }
                }
                else
                {
                    // Add the ticker to the list of tickers to track its price.
                    this._tickers[actionParameter] = null;
                }
            }
            return base.GetCommandImage(actionParameter, imageSize);
        }
    }
    }