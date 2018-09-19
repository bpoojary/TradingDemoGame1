using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace IndiaHacksGame.Data
{
    public class AskBidData : INotifyPropertyChanged
    {
        private int _bidQty;
        private int _askQty;
        private double _bid;
        private double _ask;
        private double _askPercentage;
        private double _bidPercentage;
        public int BidQty
        {
            get { return _bidQty; }
            set
            {
                _bidQty = value;
                _bidPercentage = value / 50;
                OnPropertyChanged("BidQty");
            }
        }
        public int AskQty
        {
            get { return _askQty; }
            set
            {
                _askQty = value;
                _askPercentage = value / 50;
                OnPropertyChanged("AskQty");
            }
        }
        public double Bid
        {
            get { return _bid; }
            set
            {
                _bid = value;
                OnPropertyChanged("Bid");
            }
        }
        public double Ask
        {
            get { return _ask; }
            set
            {
                _ask = value;
                OnPropertyChanged("Ask");
            }
        }
        public double BidPercentage
        {
            get { return _bidPercentage; }
            set
            {
                _bidPercentage = value;
                OnPropertyChanged("BidPercentage");
            }
        }
        public double AskPercentage
        {
            get { return _askPercentage; }
            set
            {
                _askPercentage = value;
                OnPropertyChanged("AskPercentage");
            }
        }
        public static List<AskBidData> GetAskBidCollection(double seed)
        {
            double ask = seed;
            double bid = seed + 0.5;
            List<AskBidData> data = new List<AskBidData>();
            Random random = new Random();
            for (int i =0;i<8;i++)
            {
                if(i!=0)
                {
                    ask = Math.Round(ask + Math.Round(random.NextDouble() / 2, 2),2);
                    bid = Math.Round(bid - Math.Round(random.NextDouble() / 2, 2),2);
                }
                data.Add(new AskBidData() { Bid = bid, Ask = ask, AskQty = GetRandomSize(random), BidQty = GetRandomSize(random) });
            }
            return data;
        }
        private static int GetRandomSize(Random random)
        {
            int size = (int)(5000 * random.NextDouble() + 100 * random.NextDouble() + 10 * random.NextDouble());
            if (size > 5000)
                size = size % 5000;
            if (size == 0)
                size = 1124;
            if (size < 1000)
                size = size + 1000;
            return size;
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
