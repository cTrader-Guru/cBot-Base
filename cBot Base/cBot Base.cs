/*  CTRADER GURU

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    Twitter     : https://twitter.com/cTraderGURU/
    Facebook    : https://www.facebook.com/ctrader.guru/
    YouTube     : https://www.youtube.com/channel/UCKkgbw09Fifj65W5t5lHeCQ
    GitHub      : https://github.com/ctrader-guru

*/

using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Estensioni che rendono il codice più scorrevole con metodi non previsti dalla libreria cAlgo
    /// </summary>
    public static class Extensions
    {

        #region Enum

        /// <summary>
        /// Enumeratore per esporre il nome del colore nelle opzioni
        /// </summary>
        public enum ColorNameEnum
        {

            AliceBlue,
            AntiqueWhite,
            Aqua,
            Aquamarine,
            Azure,
            Beige,
            Bisque,
            Black,
            BlanchedAlmond,
            Blue,
            BlueViolet,
            Brown,
            BurlyWood,
            CadetBlue,
            Chartreuse,
            Chocolate,
            Coral,
            CornflowerBlue,
            Cornsilk,
            Crimson,
            Cyan,
            DarkBlue,
            DarkCyan,
            DarkGoldenrod,
            DarkGray,
            DarkGreen,
            DarkKhaki,
            DarkMagenta,
            DarkOliveGreen,
            DarkOrange,
            DarkOrchid,
            DarkRed,
            DarkSalmon,
            DarkSeaGreen,
            DarkSlateBlue,
            DarkSlateGray,
            DarkTurquoise,
            DarkViolet,
            DeepPink,
            DeepSkyBlue,
            DimGray,
            DodgerBlue,
            Firebrick,
            FloralWhite,
            ForestGreen,
            Fuchsia,
            Gainsboro,
            GhostWhite,
            Gold,
            Goldenrod,
            Gray,
            Green,
            GreenYellow,
            Honeydew,
            HotPink,
            IndianRed,
            Indigo,
            Ivory,
            Khaki,
            Lavender,
            LavenderBlush,
            LawnGreen,
            LemonChiffon,
            LightBlue,
            LightCoral,
            LightCyan,
            LightGoldenrodYellow,
            LightGray,
            LightGreen,
            LightPink,
            LightSalmon,
            LightSeaGreen,
            LightSkyBlue,
            LightSlateGray,
            LightSteelBlue,
            LightYellow,
            Lime,
            LimeGreen,
            Linen,
            Magenta,
            Maroon,
            MediumAquamarine,
            MediumBlue,
            MediumOrchid,
            MediumPurple,
            MediumSeaGreen,
            MediumSlateBlue,
            MediumSpringGreen,
            MediumTurquoise,
            MediumVioletRed,
            MidnightBlue,
            MintCream,
            MistyRose,
            Moccasin,
            NavajoWhite,
            Navy,
            OldLace,
            Olive,
            OliveDrab,
            Orange,
            OrangeRed,
            Orchid,
            PaleGoldenrod,
            PaleGreen,
            PaleTurquoise,
            PaleVioletRed,
            PapayaWhip,
            PeachPuff,
            Peru,
            Pink,
            Plum,
            PowderBlue,
            Purple,
            Red,
            RosyBrown,
            RoyalBlue,
            SaddleBrown,
            Salmon,
            SandyBrown,
            SeaGreen,
            SeaShell,
            Sienna,
            Silver,
            SkyBlue,
            SlateBlue,
            SlateGray,
            Snow,
            SpringGreen,
            SteelBlue,
            Tan,
            Teal,
            Thistle,
            Tomato,
            Transparent,
            Turquoise,
            Violet,
            Wheat,
            White,
            WhiteSmoke,
            Yellow,
            YellowGreen

        }
        
        /// <summary>
        /// Enumeratore per esporre nei parametri una scelta con menu a tendina
        /// </summary>
        public enum CapitalTo
        {

            Balance,
            Equity

        }

        #endregion

        #region Class

        /// <summary>
        /// Classe per monitorare le posizioni di una specifica strategia
        /// </summary>
        public class Monitor
        {

            private Positions _allPositions = null;

            /// <summary>
            /// Standard per la raccolta di informazioni nel Monitor
            /// </summary>
            public class Information
            {

                public double TotalNetProfit = 0;
                public double MinVolumeInUnits = 0;
                public double MaxVolumeInUnits = 0;
                public double MidVolumeInUnits = 0;
                public int BuyPositions = 0;
                public int SellPositions = 0;
                public Position FirstPosition = null;
                public Position LastPosition = null;

            }

            /// <summary>
            /// Valore univoco che identifica la strategia
            /// </summary>
            public readonly string Label;

            /// <summary>
            /// Il Simbolo da monitorare in relazione alla Label
            /// </summary>
            public readonly Symbol Symbol;

            /// <summary>
            /// Le informazioni raccolte dopo la chiamata .Update()
            /// </summary>
            public Information Info { get; private set; }

            /// <summary>
            /// Le posizioni filtrate
            /// </summary>
            public Position[] Positions { get; private set; }

            /// <summary>
            /// Monitor per la raccolta d'informazioni inerenti la strategia in corso
            /// </summary>
            /// <param name="NewLabel">Valore univoco che identifica la strategia</param>
            /// <param name="NewSymbolName">Il Simbolo che si dersidera monitorare</param>
            /// <param name="AllPositions">Le posizioni da filtrare con il quale verranno raccolte le informazioni</param>
            public Monitor(string NewLabel, Symbol NewSymbol, Positions AllPositions)
            {

                Label = NewLabel;
                Symbol = NewSymbol;

                _allPositions = AllPositions;

                // --> Rendiamo sin da subito disponibili le informazioni
                Update();

            }

            /// <summary>
            /// Filtra e rende disponibili le informazioni per la strategia monitorata
            /// </summary>
            public Information Update()
            {

                // --> Raccolgo le informazioni che mi servono per avere il polso della strategia
                Positions = _allPositions.FindAll(Label, Symbol.Name);

                // --> Resetto le informazioni
                Info = new Information();

                double tmpVolume = 0;

                foreach (Position position in Positions)
                {

                    Info.TotalNetProfit += position.NetProfit;
                    tmpVolume += position.VolumeInUnits;

                    switch (position.TradeType)
                    {
                        case TradeType.Buy:

                            Info.BuyPositions++;
                            break;

                        case TradeType.Sell:

                            Info.SellPositions++;
                            break;

                    }

                    if (Info.FirstPosition == null || position.EntryTime < Info.FirstPosition.EntryTime)
                        Info.FirstPosition = position;

                    if (Info.LastPosition == null || position.EntryTime > Info.LastPosition.EntryTime)
                        Info.LastPosition = position;

                    if (Info.MinVolumeInUnits == 0 || position.VolumeInUnits < Info.MinVolumeInUnits)
                        Info.MinVolumeInUnits = position.VolumeInUnits;

                    if (Info.MaxVolumeInUnits == 0 || position.VolumeInUnits > Info.MaxVolumeInUnits)
                        Info.MaxVolumeInUnits = position.VolumeInUnits;

                }

                // --> Restituisce una Exception Overflow di una operazione aritmetica, da approfondire
                //     Info.MidVolumeInUnits = Symbol.NormalizeVolumeInUnits(tmpVolume / Positions.Length,RoundingMode.ToNearest);
                Info.MidVolumeInUnits = Math.Round(tmpVolume / Positions.Length, 0);

                return Info;

            }

        }

        public class MomenyManagement
        {

            private readonly double _minSize = 0.01;
            private double _percentage = 0;
            private double _fixedSize = 0;
            private double _pipToCalc = 30;

            
            private IAccount _account = null;
            private CapitalTo _capitalType = CapitalTo.Balance;
            private Monitor _monitor = null;

            public double Percentage
            {

                get
                {

                    return _percentage;

                }

                set
                {
                    _percentage = (value > 0 && value <= 100) ? value : 0;

                }

            }

            public double FixedSize
            {
                get
                {

                    return _fixedSize;

                }

                set
                {

                    _fixedSize = (value >= _minSize) ? value : 0;

                }

            }

            public double PipToCalc
            {
                get
                {

                    return _pipToCalc;

                }

                set
                {

                    _pipToCalc = (value > 0) ? value : 30;

                }

            }

            public CapitalTo CapitalType
            {

                get
                {

                    return _capitalType;

                }

                set
                {

                    _capitalType = value;

                }

            }

            public double Capital
            {

                get
                {

                    switch (_capitalType)
                    {

                        case CapitalTo.Equity:

                            return _account.Equity;

                        default:

                            return _account.Balance;

                    }

                }

            }

            public MomenyManagement(IAccount NewAccount, CapitalTo NewCapitalTo, double NewPercentage, double NewFixedSize, double NewPipToCalc, Monitor NewMonitor)
            {

                _account = NewAccount;
                _capitalType = NewCapitalTo;
                _monitor = NewMonitor;

                Percentage = NewPercentage;
                FixedSize = NewFixedSize;
                PipToCalc = NewPipToCalc;

            }

            /// <summary>
            /// Restituisce il numero di lotti in formato 0.01
            /// </summary>
            public double GetLotSize()
            {

                // --> Hodeciso di usare una size fissa
                if (FixedSize > 0) return FixedSize;

                // --> La percentuale di rischio in denaro
                double moneyrisk = Capital / 100 * Percentage;

                // --> Traduco lo stoploss o il suo riferimento in double
                double sl_double = PipToCalc * _monitor.Symbol.PipSize;

                // --> In formato 0.01 = microlotto double lots = Math.Round(_monitor.Symbol.VolumeInUnitsToQuantity(moneyrisk / ((sl_double * _monitor.Symbol.TickValue) / _monitor.Symbol.TickSize)), 2);
                // --> In formato volume 1K = 1000 Math.Round((moneyrisk / ((sl_double * _monitor.Symbol.TickValue) / _monitor.Symbol.TickSize)), 2);
                double lots = Math.Round(_monitor.Symbol.VolumeInUnitsToQuantity(moneyrisk / ((sl_double * _monitor.Symbol.TickValue) / _monitor.Symbol.TickSize)), 2);

                if (lots < _minSize)
                    return _minSize;

                return lots;

            }

        }

        #endregion

        #region Helper

        /// <summary>
        /// Restituisce il colore corrispondente a partire dal nome
        /// </summary>
        /// <returns>Il colore corrispondente</returns>
        public static API.Color ColorFromEnum(ColorNameEnum colorName)
        {

            return API.Color.FromName(colorName.ToString("G"));

        }

        #endregion

        #region Bars

        /// <summary>
        /// Si ottiene l'indice della candela partendo dal suo orario di apertura
        /// </summary>
        /// <param name="MyTime">La data e l'ora di apertura della candela</param>
        /// <returns></returns>
        public static int GetIndexByDate(this Bars thisBars, DateTime thisTime)
        {

            for (int i = thisBars.ClosePrices.Count - 1; i >= 0; i--)
            {

                if (thisTime == thisBars.OpenTimes[i])
                    return i;

            }

            return -1;

        }

        #endregion

        #region Bar

        /// <summary>
        /// Misura la grandezza di una candela, tenendo conto della sua direzione
        /// </summary>
        /// <returns>Il corpo della candela, valore uguale o superiore a zero</returns>
        public static double Body(this Bar thisBar)
        {

            return thisBar.IsBullish() ? thisBar.Close - thisBar.Open : thisBar.Open - thisBar.Close;


        }

        /// <summary>
        /// Verifica la direzione rialzista di una candela
        /// </summary>
        /// <returns>True se la candela è rialzista</returns>        
        public static bool IsBullish(this Bar thisBar)
        {

            return thisBar.Close > thisBar.Open;

        }

        /// <summary>
        /// Verifica la direzione ribassista di una candela
        /// </summary>
        /// <returns>True se la candela è ribassista</returns>        
        public static bool IsBearish(this Bar thisBar)
        {

            return thisBar.Close < thisBar.Open;

        }

        /// <summary>
        /// Verifica se una candela ha un open uguale al close
        /// </summary>
        /// <returns>True se la candela è una doji con Open e Close uguali</returns>        
        public static bool IsDoji(this Bar thisBar)
        {

            return thisBar.Close == thisBar.Open;

        }

        #endregion

        #region Symbol

        /// <summary>
        /// Converte il numero di pips corrente da digits a double
        /// </summary>
        /// <param name="Pips">Il numero di pips nel formato Digits</param>
        /// <returns></returns>
        public static double DigitsToPips(this Symbol thisSymbol, double Pips)
        {

            return Math.Round(Pips / thisSymbol.PipSize, 2);

        }

        /// <summary>
        /// Converte il numero di pips corrente da double a digits
        /// </summary>
        /// <param name="Pips">Il numero di pips nel formato Double (2)</param>
        /// <returns></returns>
        public static double PipsToDigits(this Symbol thisSymbol, double Pips)
        {

            return Math.Round(Pips * thisSymbol.PipSize, thisSymbol.Digits);

        }

        #endregion

        #region Chart

        /// <summary>
        /// Determina se ci sono le condizioni per disegnare sul grafico
        /// </summary>
        public static bool CanDraw(this Chart thisChart, RunningMode thisRunning)
        {

            return thisRunning == RunningMode.RealTime || thisRunning == RunningMode.VisualBacktesting;

        }

        #endregion

        #region TimeFrame

        /// <summary>
        /// Restituisce in minuti il timeframe corrente
        /// </summary>
        public static int ToMinutes(this TimeFrame thisTimeFrame)
        {

            if (thisTimeFrame == TimeFrame.Daily)
                return 60 * 24;
            if (thisTimeFrame == TimeFrame.Day2)
                return 60 * 24 * 2;
            if (thisTimeFrame == TimeFrame.Day3)
                return 60 * 24 * 3;
            if (thisTimeFrame == TimeFrame.Hour)
                return 60;
            if (thisTimeFrame == TimeFrame.Hour12)
                return 60 * 12;
            if (thisTimeFrame == TimeFrame.Hour2)
                return 60 * 2;
            if (thisTimeFrame == TimeFrame.Hour3)
                return 60 * 3;
            if (thisTimeFrame == TimeFrame.Hour4)
                return 60 * 4;
            if (thisTimeFrame == TimeFrame.Hour6)
                return 60 * 6;
            if (thisTimeFrame == TimeFrame.Hour8)
                return 60 * 8;
            if (thisTimeFrame == TimeFrame.Minute)
                return 1;
            if (thisTimeFrame == TimeFrame.Minute10)
                return 10;
            if (thisTimeFrame == TimeFrame.Minute15)
                return 15;
            if (thisTimeFrame == TimeFrame.Minute2)
                return 2;
            if (thisTimeFrame == TimeFrame.Minute20)
                return 20;
            if (thisTimeFrame == TimeFrame.Minute3)
                return 3;
            if (thisTimeFrame == TimeFrame.Minute30)
                return 30;
            if (thisTimeFrame == TimeFrame.Minute4)
                return 4;
            if (thisTimeFrame == TimeFrame.Minute45)
                return 45;
            if (thisTimeFrame == TimeFrame.Minute5)
                return 5;
            if (thisTimeFrame == TimeFrame.Minute6)
                return 6;
            if (thisTimeFrame == TimeFrame.Minute7)
                return 7;
            if (thisTimeFrame == TimeFrame.Minute8)
                return 8;
            if (thisTimeFrame == TimeFrame.Minute9)
                return 9;
            if (thisTimeFrame == TimeFrame.Monthly)
                return 60 * 24 * 30;
            if (thisTimeFrame == TimeFrame.Weekly)
                return 60 * 24 * 7;

            return 0;

        }

        #endregion

    }

}

namespace cAlgo.Robots
{

    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class CBOTBASE : Robot
    {

        #region Enums

        

        #endregion

        #region Identity

        /// <summary>
        /// Nome del prodotto, identificativo, da modificare con il nome della propria creazione
        /// </summary>
        public const string NAME = "cBot Base";

        /// <summary>
        /// La versione del prodotto, progressivo, utilie per controllare gli aggiornamenti se viene reso disponibile sul sito ctrader.guru
        /// </summary>
        public const string VERSION = "1.0.6";

        #endregion

        #region Params

        [Parameter(NAME + " " + VERSION, Group = "Identity", DefaultValue = "https://ctrader.guru/product/cbot-base/")]
        public string ProductInfo { get; set; }

        [Parameter("Label ( Magic Name )", Group = "Identity", DefaultValue = NAME)]
        public string MyLabel { get; set; }

        [Parameter("Stop Loss (pips)", Group = "Money Management", DefaultValue = 5, MinValue = 0, Step = 0.1)]
        public double SL { get; set; }

        [Parameter("Take Profit (pips)", Group = "Money Management", DefaultValue = 10, MinValue = 0, Step = 0.1)]
        public double TP { get; set; }

        [Parameter("Break Even From (pips)", Group = "Money Management", DefaultValue = 5, MinValue = 0, Step = 0.1)]
        public double BEfrom { get; set; }

        [Parameter("Break Even To (pips)", Group = "Money Management", DefaultValue = 1.5, MinValue = 1, Step = 0.1)]
        public double BEto { get; set; }

        [Parameter("Max Spread allowed", Group = "Money Management", DefaultValue = 1.5, MinValue = 0.1, Step = 0.1)]
        public double SpreadToTrigger { get; set; }

        [Parameter("Slippage (pips)", Group = "Money Management", DefaultValue = 2.0, MinValue = 0.5, Step = 0.1)]
        public double Slippage { get; set; }

        [Parameter("Fixed Lots", Group = "Money Management", DefaultValue = 0, MinValue = 0, Step = 0.01)]
        public double FixedLots { get; set; }

        [Parameter("Capital", Group = "Money Management", DefaultValue = Extensions.CapitalTo.Balance)]
        public Extensions.CapitalTo MyCapital { get; set; }

        [Parameter("% Risk", Group = "Money Management", DefaultValue = 1, MinValue = 0.1, Step = 0.1)]
        public double MyRisk { get; set; }

        [Parameter("Pips To Calculate ( if no stoploss )", Group = "Money Management", DefaultValue = 9, MinValue = 0, Step = 0.1)]
        public double FakeSL { get; set; }

        [Parameter("Minimum Lots", Group = "Money Management", DefaultValue = 0.01, MinValue = 0.01, Step = 0.01)]
        public double MinLots { get; set; }

        [Parameter("Pause over this time", Group = "Filters", DefaultValue = 21.3, MinValue = 0, MaxValue = 23.59)]
        public double PauseOver { get; set; }

        [Parameter("Pause under this time", Group = "Filters", DefaultValue = 3, MinValue = 0, MaxValue = 23.59)]
        public double PauseUnder { get; set; }

        [Parameter("Max GAP Allowed (pips)", Group = "Filters", DefaultValue = 1, MinValue = 0, Step = 0.01)]
        public double GAP { get; set; }

        [Parameter("Max Number of Trades", Group = "Filters", DefaultValue = 1, MinValue = 1, Step = 1)]
        public int MaxTrades { get; set; }

        [Parameter("Color Text", Group = "Styles", DefaultValue = Extensions.ColorNameEnum.Coral, MinValue = 1, Step = 1)]
        public Extensions.ColorNameEnum TextColor { get; set; }

        #endregion

        #region Property

        Extensions.Monitor Monitor1;
        Extensions.MomenyManagement MomenyManagement1;

        /// <summary>
        /// Flag che scandisce il cambio candela
        /// </summary>
        bool openedInThisBar = false;

        #endregion

        #region cBot Events

        /// <summary>
        /// Evento generato quando viene avviato il cBot
        /// </summary>
        protected override void OnStart()
        {

            // --> Stampo nei log la versione corrente
            Print("{0} : {1}", NAME, VERSION);

            // --> Messaggio di avvertimento nel caso incui si eseguisse senza modifiche logiche
            if (Chart.CanDraw(RunningMode))
                Chart.DrawStaticText(NAME, "ATTENTION : CBOT BASE, EDIT THIS TEMPLATE ONLY", VerticalAlignment.Top, HorizontalAlignment.Left, Extensions.ColorFromEnum(TextColor));

            // --> Inizializzo il Monitor
            Monitor1 = new Extensions.Monitor(MyLabel, Symbol, Positions);

            // --> Inizializzo il MoneyManagement
            MomenyManagement1 = new Extensions.MomenyManagement( Account, MyCapital, MyRisk, FixedLots, SL > 0 ? SL : FakeSL, Monitor1 );
            
        }

        /// <summary>
        /// Evento generato ad ogni cambio candela
        /// </summary>
        protected override void OnBar()
        {

            // --> Resetto il flag del controllo candela
            openedInThisBar = false;

        }

        /// <summary>
        /// Evento generato a ogni tick
        /// </summary>
        protected override void OnTick()
        {

            // --> Aggiorno le informazioni necessarie per gestire la strategia
            Monitor1.Update();

            // --> Controllo se ci sono posizioni da chiudere prima di procedere con la logica
            _checkClosePositions();

            // --> Controllo se devo aggiornare/modificare le posizioni secondo la logica di breakeven
            _checkBreakEven();

            // --> Condizione condivisa, filtri generali, segnano il perimetro di azione limitando l'ingresso
            bool sharedCondition = (!openedInThisBar && !_iAmInGAP() && !_iAmInPause() && _getSpreadInformation() <= SpreadToTrigger && Positions.FindAll(MyLabel, Symbol.Name).Length < MaxTrades);

            // --> Controllo la presenza di trigger d'ingresso tenendo conto i filtri
            bool triggerBuy = _calculateLongTrigger(_calculateLongFilter(sharedCondition));
            bool triggerSell = _calculateShortTrigger(_calculateShortFilter(sharedCondition));

            // --> Se ho entrambi i trigger qualcosa non va, lo segnalo nei log e fermo la routin
            if (triggerBuy && triggerSell)
            {

                Print("{0} {1} ERROR : trigger buy and sell !", MyLabel, Symbol.Name);
                return;

            }

            // --> Calcolo la size in base al money management stabilito
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(MomenyManagement1.GetLotSize());

            // --> Se ho il segnale d'ingresso considerando i filtri allora procedo con l'ordine a mercato
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

        #endregion

        #region Private Methods

        /// <summary>
        /// Controlla la presenza di posizioni da chiudere secondo i criteri stabiliti
        /// </summary>
        private void _checkClosePositions()
        {

            // --> Criteri da stabilire
            return;

        }

        /// <summary>
        /// Conferma se i criteri di filtraggio long sono stati soddisfatti
        /// </summary>
        /// <param name="condition">Filtro globale, condizione condivisa</param>
        /// <returns>I filtri long sono stati soddisfatti</returns>
        private bool _calculateLongFilter(bool condition = true)
        {

            // --> La condizione primaria deve essere presente altrimenti non serve continuare
            if (!condition)
                return false;

            // --> Criteri da stabilire
            return true;

        }

        /// <summary>
        /// Conferma se i criteri di filtraggio short sono stati soddisfatti
        /// </summary>
        /// <param name="condition">Filtro globale, condizione condivisa</param>
        /// <returns>I filtri short sono stati soddisfatti</returns>
        private bool _calculateShortFilter(bool condition = true)
        {

            // --> La condizione primaria deve essere presente altrimenti non serve continuare
            if (!condition)
                return false;

            // --> Criteri da stabilire
            return true;

        }

        /// <summary>
        /// Conferma se i criteri d'ingresso long sono stati soddisfatti
        /// </summary>
        /// <param name="filter">Filtro long, condizione condivisa</param>
        /// <returns>É presente una condizione di apertura long</returns>
        private bool _calculateLongTrigger(bool filter = true)
        {

            // --> Il filtro primario deve essere presente altrimenti non serve continuare
            if (!filter)
                return false;

            // --> Criteri da stabilire
            return false;

        }

        /// <summary>
        /// Conferma se i criteri d'ingresso short sono stati soddisfatti
        /// </summary>
        /// <param name="filter">Filtro short, condizione condivisa</param>
        /// <returns>É presente una condizione di apertura short</returns>
        private bool _calculateShortTrigger(bool filter = true)
        {

            // --> Il filtro primario deve essere presente altrimenti non serve continuare
            if (!filter)
                return false;

            // --> Criteri da stabilire
            return false;

        }

        /// <summary>
        /// Controlla lo stato in cui vi è un GAP tra la chiusura e l'apertura della nuova candela
        /// </summary>
        /// <returns>Conferma la presenza di un GAP secondo i parametri stabiliti</returns>
        private bool _iAmInGAP()
        {

            double K = 0;

            if (Bars.ClosePrices.Last(1) > Bars.OpenPrices.LastValue)
            {

                K = Math.Round((Bars.ClosePrices.Last(1) - Bars.OpenPrices.LastValue) / Symbol.PipSize, 2);

            }
            else if (Bars.OpenPrices.LastValue > Bars.ClosePrices.Last(1))
            {

                K = Math.Round((Bars.OpenPrices.LastValue - Bars.ClosePrices.Last(1)) / Symbol.PipSize, 2);

            }

            return (K > GAP);

        }

        /// <summary>
        /// Controlla la fascia oraria per determinare se rientra in quella di pausa, utilizza dati double 
        /// perchè la ctrader non permette di esporre dati time, da aggiornare non appena la ctrader lo permette
        /// </summary>
        /// <returns>Conferma la presenza di una fascia oraria in pausa</returns>
        private bool _iAmInPause()
        {

            // -->> Poichè si utilizzano dati double per esporre i parametri dobbiamo utilizzare meccanismi per tradurre l'orario
            string nowHour = (Server.Time.Hour < 10) ? string.Format("0{0}", Server.Time.Hour) : string.Format("{0}", Server.Time.Hour);
            string nowMinute = (Server.Time.Minute < 10) ? string.Format("0{0}", Server.Time.Minute) : string.Format("{0}", Server.Time.Minute);

            // --> Stabilisco il momento di controllo in formato double
            double adesso = Convert.ToDouble(string.Format("{0},{1}", nowHour, nowMinute));

            // --> Confronto elementare per rendere comprensibile la logica
            if (PauseOver < PauseUnder && adesso >= PauseOver && adesso <= PauseUnder)
            {

                return true;

            }
            else if (PauseOver > PauseUnder && ((adesso >= PauseOver && adesso <= 23.59) || adesso <= PauseUnder))
            {

                return true;

            }

            return false;

        }

        /// <summary>
        /// Restituisce lo spread corrente
        /// </summary>
        private double _getSpreadInformation()
        {

            // --> Restituisco lo spread corrente
            return Math.Round(Symbol.Spread / Symbol.PipSize, 2);

        }

        /// <summary>
        /// Chiude le posizioni sulla coppia corrente aperte dal cbot
        /// </summary>
        private void _closePositions()
        {

            var MyPositions = Positions.FindAll(MyLabel, Symbol.Name);

            foreach (var position in MyPositions)
            {
                ClosePosition(position);
            }

        }

        /// <summary>
        /// Chiude le posizioni sulla coppia corrente aperte dal cbot, filtrando solo alcune posizioni, short o long
        /// </summary>
        /// <param name="myTrade">Tipo di trade da chiudere long o short</param>
        private void _closePositions(TradeType myTrade)
        {

            var MyPositions = Positions.FindAll(MyLabel, Symbol.Name, myTrade);

            foreach (var position in MyPositions)
            {
                ClosePosition(position);
            }

        }

        /// <summary>
        /// Controlla ed effettua la modifica in break-even se le condizioni le permettono
        /// </summary>
        private void _checkBreakEven()
        {

            // --> Se l'attivazione non è impostato vuol dire che non si vuole utilizzare questa funzione
            if (BEfrom == 0)
                return;

            // --> Agisco solo sulle operazioni che abbiamo aperto con il cbot
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

        #endregion

    }

}
