namespace Loupedeck.StockPlugin
{
    using System;
    using System.Net.Http;
    using System.Security.Policy;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    // This class implements an example command that counts button presses.

    public class StockTicker : PluginDynamicCommand
    {
        private Int32 _counter = 0;
        private double _stockPrice = 0;
        private readonly String _ticker = "RBLX";

        // Initializes the command class.
        public StockTicker()
            : base(displayName: "Stock Ticker", description: "Displays the current stock price", groupName: "")
        {
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
            dynamic stockResponse = await this.getStockPrice();
            this._counter++;
            this._stockPrice = stockResponse.price;
            this.ActionImageChanged(); // Notify the Loupedeck service that the command display name and/or image has changed.
            PluginLog.Info($"Counter value is {this._counter}"); // Write the current counter value to the log file.
        }

        // This method is called when Loupedeck needs to show the command on the console or the UI.
        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            $"{this._ticker}{Environment.NewLine}${this._stockPrice}";
    }
}
