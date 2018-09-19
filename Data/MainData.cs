using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndiaHacksGame
{
    public class MainData
    {
        bool _isPrimary = false;
        public void Initialize(string stockName, SymbolData symbolDataObject, bool isPrimary)
        {
            _isPrimary = isPrimary;
            StockName = stockName;
            CalculatePrice(symbolDataObject);
        }
        private void CalculatePrice(SymbolData symbolDataObject)
        {
            if (symbolDataObject == null)
                return;
            if(Price !=null)
                Price.Clear();
            else
                Price = new Queue<double>(MainData.GetDurationInSeconds()*2);
            double currentValue = GetStartingPrice();
            Random currentPriceRandom = new Random((int)DateTime.Now.Ticks);
            currentValue = currentValue + currentPriceRandom.Next(23);
            Random random = new Random((int)DateTime.Now.Ticks);
            List<double> priceCollection = new List<double>();
            for (int i =1; i< MainData.GetDurationInSeconds() * 2; i++)
            {
                if(_isPrimary)
                {
                    if(i>=14 && i<=19)
                    {
                        if(symbolDataObject.News.isPositive)
                        {
                            currentValue = currentValue + currentValue * symbolDataObject.News.SwingPercentage / 600;
                        }
                        else
                        {
                            currentValue = currentValue - currentValue * symbolDataObject.News.SwingPercentage / 600;
                        }
                        priceCollection.Add(currentValue);
                        continue;
                    }
                }
                else
                {
                    if (i >= 27 && i <= 32)
                    {
                        if (symbolDataObject.News.isPositive)
                        {
                            currentValue = currentValue + currentValue * symbolDataObject.News.SwingPercentage / 600;
                        }
                        else
                        {
                            currentValue = currentValue - currentValue * symbolDataObject.News.SwingPercentage / 600;
                        }
                        priceCollection.Add(currentValue);
                        continue;
                    }
                }
                double nrand = random.NextDouble();
                if (((int)(nrand * 10)) % 2 == 0)
                    priceCollection.Add(currentValue + Math.Round(nrand / 2, 2));
                else
                    priceCollection.Add(currentValue - Math.Round(nrand / 2, 2));
            }
            foreach(double item in priceCollection)
            {
                Price.Enqueue(item);
            }
        }
        public static double GetStartingPrice()
        {
            return Math.Round(Convert.ToDouble(ConfigurationManager.AppSettings["StartingPrice"]), 2);
        }
        public static double GetUpdateTickerInSeconds()
        {
            return Math.Round(Convert.ToDouble(ConfigurationManager.AppSettings["UpdateTickerInSeconds"]), 2);
        }
        public string StockName { get;private set; }
        public Queue<double> Price { get; private set; }

        internal static int GetDurationInSeconds()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["DurationInSeconds"]);
        }

        internal static string GetStockName()
        {
            return Convert.ToString(ConfigurationManager.AppSettings["StockName"]);
        }
        internal static int GetCash()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["Cash"]);
        }
        internal static int GetHideYourProfitAfterSec()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["HideYourProfitAfterSec"]);
        }
        internal static int GetMaximumOrderSizeInBuySellDlg()
        {
            try
            {
                int max = Convert.ToInt32(ConfigurationManager.AppSettings["MaximumOrderSizeInBuySellDlg"]);
                return max;
            }
            catch
            {
                return 100000;
            }
        }
        
    }
}
