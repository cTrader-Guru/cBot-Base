/*  CTRADER GURU -->

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    GitHub      : https://gist.github.com/cTraderGURU/24c5ef3c715b514f3f1c9086036becf5


    IMPORTANTE, le funzioni da scrivere sono :
    
        _checkClosePositions    <-- opzionale, se vuoi chiudere in determinate condizioni
        _calculateLongFilter    <-- opzionale
        _calculateShortFilter   <-- opzionale
        _calculateLongTrigger   <-- richiesto, opzionale se si desiderano solo operazioni short
        _calculateShortTrigger  <-- richiesto, opzionale se si desiderano solo operazioni long

*/

using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{

    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class cBotBase : Robot
    {

        // --> Parametri di configurazione del cBot

        [Parameter("Label ( Magic Name )", DefaultValue = "cBot Base 1.0.2")]
        public string MyLabel { get; set; }

        [Parameter("Stop Loss", DefaultValue = 5, MinValue = 0, Step = 0.1)]
        public double SL { get; set; }

        [Parameter("Take Profit", DefaultValue = 10, MinValue = 0, Step = 0.1)]
        public double TP { get; set; }

        [Parameter("Break Even From", DefaultValue = 5, MinValue = 0, Step = 0.1)]
        public double BEfrom { get; set; }

        [Parameter("Break Even To", DefaultValue = 1.5, MinValue = 1, Step = 0.1)]
        public double BEto { get; set; }

        [Parameter("Max Spread allowed", DefaultValue = 1.5, MinValue = 0.1, Step = 0.1)]
        public double SpreadToTrigger { get; set; }

        [Parameter("Slippage", DefaultValue = 2.0, MinValue = 0.5, Step = 0.1)]
        public double Slippage { get; set; }

        [Parameter("Capital", DefaultValue = "Balance")]
        public string myCapital { get; set; }

        [Parameter("% Risk", DefaultValue = 1, MinValue = 0.1, Step = 0.1)]
        public double myRisk { get; set; }

        [Parameter("Pips To Calculate ( if no stoploss )", DefaultValue = 9, MinValue = 0, Step = 0.1)]
        public double fakeSL { get; set; }

        [Parameter("Minimum Lots", DefaultValue = 0.01, MinValue = 0.01, Step = 0.01)]
        public double MinLots { get; set; }

        [Parameter("Maximum Lots", DefaultValue = 10, MinValue = 0.01, Step = 0.01)]
        public double MaxLots { get; set; }

        [Parameter("Pause over this time", DefaultValue = 21.3, MinValue = 0, MaxValue = 23.59)]
        public double pauseOver { get; set; }

        [Parameter("Pause under this time", DefaultValue = 3, MinValue = 0, MaxValue = 23.59)]
        public double pauseUnder { get; set; }

        [Parameter("Max GAP Allowed", DefaultValue = 1, MinValue = 0, Step = 0.01)]
        public double GAP { get; set; }

        [Parameter("Max Number of Trades", DefaultValue = 1, MinValue = 1, Step = 1)]
        public int MaxTrades { get; set; }


        // --> VARIABILI DI SERVIZIO

        bool openedInThisBar = false;

        // <-- VARIABILI DI SERVIZIO

        protected override void OnStart()
        {

            // --> INIZIALIZZAZIONE VARIABILI DI SERVIZIO



            // <-- INIZIALIZZAZIONE VARIABILI DI SERVIZIO

        }

        protected override void OnBar()
        {

            openedInThisBar = false;

        }

        protected override void OnTick()
        {

            _checkClosePositions();

            _checkBreakEven();

            // --> Condizione condivisa, non voglio uno spread troppo alto e voglio aprire una sola operazione per volta

            bool sharedCondition = (!openedInThisBar && !_iAmInGAP() && !_iAmInPause() && _getSpreadInformation() <= SpreadToTrigger && Positions.FindAll(MyLabel, Symbol.Name).Length < MaxTrades);

            bool triggerBuy = _calculateLongTrigger(_calculateLongFilter(sharedCondition));

            bool triggerSell = _calculateShortTrigger(_calculateShortFilter(sharedCondition));

            // --> Se ho entrambi i trigger qualcosa non va, lo segno nei log e fermo la routin

            if (triggerBuy && triggerSell)
            {

                Print("{0} {1} ERROR : trigger buy and sell !", MyLabel, Symbol.Name);
                return;

            }

            // --> Calcolo la size automaticamente

            var volumeInUnits = Symbol.QuantityToVolumeInUnits(_calculateSize());

            if (triggerBuy)
            {

                ExecuteMarketRangeOrder(TradeType.Buy, Symbol.Name, volumeInUnits, Slippage, Symbol.Ask, MyLabel, SL, TP);
                openedInThisBar = true;

            }
            else if (triggerSell)
            {

                ExecuteMarketRangeOrder(TradeType.Sell, Symbol.Name, volumeInUnits, Slippage, Symbol.Bid, MyLabel, SL, TP);
                openedInThisBar = true;

            }

        }

        private void _checkClosePositions()
        {

            // --> QUI IL VOSTRO CODICE PER CALCOLARE LE CHIUSURE

            return;

            // <-- QUI IL VOSTRO CODICE PER CALCOLARE LE CHIUSURE

        }

        private bool _calculateLongFilter(bool condition = true)
        {

            if (!condition)
                return false;

            // --> QUI IL VOSTRO CODICE PER CALCOLARE IL FILTRO

            return true;

            // <-- QUI IL VOSTRO CODICE PER CALCOLARE IL FILTRO

        }

        private bool _calculateShortFilter(bool condition = true)
        {

            if (!condition)
                return false;

            // --> QUI IL VOSTRO CODICE PER CALCOLARE IL FILTRO

            return true;

            // <-- QUI IL VOSTRO CODICE PER CALCOLARE IL FILTRO

        }

        private bool _calculateLongTrigger(bool filter = true)
        {

            if (!filter)
                return false;

            // --> QUI IL VOSTRO CODICE PER CALCOLARE IL TRIGGER

            return false;

            // <-- QUI IL VOSTRO CODICE PER CALCOLARE IL TRIGGER

        }

        private bool _calculateShortTrigger(bool filter = true)
        {

            if (!filter)
                return false;

            // --> QUI IL VOSTRO CODICE PER CALCOLARE IL TRIGGER

            return false;

            // <-- QUI IL VOSTRO CODICE PER CALCOLARE IL TRIGGER

        }

        private bool _iAmInGAP()
        {

            double K = 0;

            if (MarketSeries.Close.Last(1) > MarketSeries.Open.LastValue)
            {

                K = Math.Round(((MarketSeries.Close.Last(1) - MarketSeries.Open.LastValue) / Symbol.PipSize), 2);

            }
            else if (MarketSeries.Open.LastValue > MarketSeries.Close.Last(1))
            {

                K = Math.Round(((MarketSeries.Open.LastValue - MarketSeries.Close.Last(1)) / Symbol.PipSize), 2);

            }

            return (K > GAP);

        }

        private bool _iAmInPause()
        {

            string nowHour = (Server.Time.Hour < 10) ? string.Format("0{0}", Server.Time.Hour) : string.Format("{0}", Server.Time.Hour);
            string nowMinute = (Server.Time.Minute < 10) ? string.Format("0{0}", Server.Time.Minute) : string.Format("{0}", Server.Time.Minute);

            double adesso = Convert.ToDouble(string.Format("{0},{1}", nowHour, nowMinute));

            if (pauseOver < pauseUnder && adesso >= pauseOver && adesso <= pauseUnder)
            {

                return true;

            }
            else if (pauseOver > pauseUnder && ((adesso >= pauseOver && adesso <= 23.59) || adesso <= pauseUnder))
            {

                return true;

            }

            return false;

        }

        private double _getSpreadInformation()
        {

            // --> Restituisco lo spread corrente

            return Math.Round(Symbol.Spread / Symbol.PipSize, 2);

        }

        private void _closePositions()
        {

            var MyPositions = Positions.FindAll(MyLabel, Symbol.Name);

            foreach (var position in MyPositions)
            {
                ClosePosition(position);
            }

        }

        private void _closePositions(TradeType myTrade)
        {

            var MyPositions = Positions.FindAll(MyLabel, Symbol.Name, myTrade);

            foreach (var position in MyPositions)
            {
                ClosePosition(position);
            }

        }

        private void _checkBreakEven()
        {

            if (BEfrom == 0)
                return;

            var MyPositions = Positions.FindAll(MyLabel, Symbol.Name);

            foreach (var position in MyPositions)
            {

                switch (position.TradeType)
                {

                    case TradeType.Buy:

                        if ((Symbol.Bid >= (position.EntryPrice + (BEfrom * Symbol.PipSize))) && (position.StopLoss == null || position.StopLoss < position.EntryPrice))
                        {

                            ModifyPosition(position, (position.EntryPrice + (BEto * Symbol.PipSize)), position.TakeProfit);

                        }

                        break;

                    case TradeType.Sell:

                        if ((Symbol.Ask <= (position.EntryPrice - (BEfrom * Symbol.PipSize))) && (position.StopLoss == null || position.StopLoss > position.EntryPrice))
                        {

                            ModifyPosition(position, (position.EntryPrice - (BEto * Symbol.PipSize)), position.TakeProfit);

                        }

                        break;

                }

            }

        }

        private double _calculateSize()
        {

            if (SL > 0)
                return _getLotSize(_getMyCapital(myCapital), SL, myRisk, MinLots, MaxLots);

            if (fakeSL > 0)
                return _getLotSize(_getMyCapital(myCapital), fakeSL, myRisk, MinLots, MaxLots);

            return MinLots;

        }

        private double _getLotSize(double capital, double stoploss, double percentage, double Minim, double Maxi)
        {

            double moneyrisk = ((capital / 100) * percentage);

            double sl_double = (stoploss * Symbol.PipSize);

            // --> In formato 0.01 = microlotto double lots = Math.Round(Symbol.VolumeInUnitsToQuantity(moneyrisk / ((sl_double * Symbol.TickValue) / Symbol.TickSize)), 2);

            // --> In formato volume 1K = 1000 Math.Round((moneyrisk / ((sl_double * Symbol.TickValue) / Symbol.TickSize)), 2);

            double lots = Math.Round(Symbol.VolumeInUnitsToQuantity(moneyrisk / ((sl_double * Symbol.TickValue) / Symbol.TickSize)), 2);

            if (lots < Minim)
                return Minim;

            if (lots > Maxi)
                return Maxi;

            return lots;

        }

        private double _getMyCapital(string x)
        {

            switch (x.ToLower())
            {

                case "free margin":

                    return Account.FreeMargin;

                case "equity":

                    return Account.Equity;
                default:



                    return Account.Balance;

            }

        }

    }

}

