/*  CTRADER GURU -->

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    Twitter     : https://twitter.com/cTraderGURU/
    Facebook    : https://www.facebook.com/ctrader.guru/
    YouTube     : https://www.youtube.com/channel/UCKkgbw09Fifj65W5t5lHeCQ
    GitHub      : https://github.com/cTraderGURU/
    TOS         : https://ctrader.guru/termini-del-servizio/

*/

using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using cAlgo.API;
using cAlgo.API.Internals;

// --> Microsoft Visual Studio 2017 --> Strumenti --> Gestione pacchetti NuGet --> Gestisci pacchetti NuGet per la soluzione... --> Installa
using Newtonsoft.Json;

namespace cAlgo.Robots
{

    /// <summary>
    /// Enumeratore per esporre nei parametri una scelta con menu a tendina
    /// </summary>
    public enum CapitalTo
    {

        Balance,
        Equity

    }

    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class CBOTBASE : Robot
    {

        /// <summary>
        /// ID prodotto, identificativo ctrader.guru
        /// </summary>
        public const int ID = 60886;

        /// <summary>
        /// Nome del cBot, identificativo
        /// </summary>
        public const string NAME = "cBot Base";

        /// <summary>
        /// La versione del prodotto, progressivo, utilie per controllare gli aggiornamenti
        /// </summary>
        public const string VERSION = "0.0.1";


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

        [Parameter("Capital", Group = "Money Management", DefaultValue = CapitalTo.Balance)]
        public CapitalTo MyCapital { get; set; }

        [Parameter("% Risk", Group = "Money Management", DefaultValue = 1, MinValue = 0.1, Step = 0.1)]
        public double MyRisk { get; set; }

        [Parameter("Pips To Calculate ( if no stoploss )", Group = "Money Management", DefaultValue = 9, MinValue = 0, Step = 0.1)]
        public double FakeSL { get; set; }

        [Parameter("Minimum Lots", Group = "Money Management", DefaultValue = 0.01, MinValue = 0.01, Step = 0.01)]
        public double MinLots { get; set; }

        [Parameter("Maximum Lots", Group = "Money Management", DefaultValue = 10, MinValue = 0.01, Step = 0.01)]
        public double MaxLots { get; set; }

        [Parameter("Pause over this time", Group = "Filters", DefaultValue = 21.3, MinValue = 0, MaxValue = 23.59)]
        public double PauseOver { get; set; }

        [Parameter("Pause under this time", Group = "Filters", DefaultValue = 3, MinValue = 0, MaxValue = 23.59)]
        public double PauseUnder { get; set; }

        [Parameter("Max GAP Allowed (pips)", Group = "Filters", DefaultValue = 1, MinValue = 0, Step = 0.01)]
        public double GAP { get; set; }

        [Parameter("Max Number of Trades", Group = "Filters", DefaultValue = 1, MinValue = 1, Step = 1)]
        public int MaxTrades { get; set; }

        /// <summary>
        /// Flag che scandisce il cambio candela
        /// </summary>
        bool openedInThisBar = false;

        /// <summary>
        /// Evento generato quando viene avviato il cBot
        /// </summary>
        protected override void OnStart()
        {

            // --> Stampo nei log la versione corrente
            Print("{0} : {1}", NAME, VERSION);

            // --> Se viene settato l'ID effettua un controllo per verificare eventuali aggiornamenti
            _checkProductUpdate();

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
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(_calculateSize());

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

        /// <summary>
        /// Effettua un controllo sul sito ctrader.guru per mezzo delle API per verificare la presenza di aggiornamenti
        /// </summary>
        private void _checkProductUpdate()
        {

            // --> Organizzo i dati per la richiesta degli aggiornamenti
            Guru.API.RequestProductInfo Request = new Guru.API.RequestProductInfo
            {

                MyProduct = new Guru.Product
                {

                    ID = ID,
                    Name = NAME,
                    Version = VERSION

                },
                AccountBroker = Account.BrokerName,
                AccountNumber = Account.Number

            };

            // --> Effettuo la richiesta
            Guru.API Response = new Guru.API(Request);

            // --> Controllo per prima cosa la presenza di errori di comunicazioni
            if (Response.ProductInfo.Exception != "")
            {

                Print("{0} Exception : {1}", NAME, Response.ProductInfo.Exception);

            }// --> Chiedo conferma della presenza di nuovi aggiornamenti
            else if (Response.HaveNewUpdate())
            {

                string updatemex = string.Format("{0} : Updates available {1} ( {2} )", NAME, Response.ProductInfo.LastProduct.Version, Response.ProductInfo.LastProduct.Updated);

                // --> Informo l'utente con un messaggio sul grafico e nei log del cbot
                Chart.DrawStaticText(NAME + "Updates", updatemex, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Red);
                Print(updatemex);

            }

        }

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
            if (!condition) return false;

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
            if (!condition) return false;

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
            if (!filter) return false;

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
            if (!filter) return false;

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
            if (BEfrom == 0) return;

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

        /// <summary>
        /// Calcola la size da utilizzare secondo i parametri stabiliti
        /// </summary>
        /// <returns>I lotti da investire</returns>
        private double _calculateSize()
        {

            // --> Se ho inserito uno stoploss questo verrà utilizzato per calcolare la size
            if (SL > 0) return _getLotSize(_getMyCapital(MyCapital), SL, MyRisk, MinLots, MaxLots);

            // --> Se non ho settato uno stoploss controllo se ho settato un valore fittizio di riferimento per il calcolo
            if (FakeSL > 0) return _getLotSize(_getMyCapital(MyCapital), FakeSL, MyRisk, MinLots, MaxLots);

            // --> A questo punto desidero lavorare solo con la size minima
            return MinLots;

        }

        /// <summary>
        /// Restituisce la size da investire tenendo conto dei criteri di calcolo stabiliti
        /// </summary>
        /// <param name="capital">La base sul quale fare il conteggio delle proporzioni</param>
        /// <param name="stoploss">Il riferimento per il calcolo</param>
        /// <param name="percentage">La percentuale in base al "capital"</param>
        /// <param name="Minim">Il valore minimo accettabile</param>
        /// <param name="Maxi">Il valore massimo accettabile</param>
        /// <returns></returns>
        private double _getLotSize(double capital, double stoploss, double percentage, double Minim, double Maxi)
        {

            // --> La percentuale di rischio in denaro
            double moneyrisk = capital / 100 * percentage;

            // --> Traduco lo stoploss o il suo riferimento in double
            double sl_double = stoploss * Symbol.PipSize;

            // --> In formato 0.01 = microlotto double lots = Math.Round(Symbol.VolumeInUnitsToQuantity(moneyrisk / ((sl_double * Symbol.TickValue) / Symbol.TickSize)), 2);

            // --> In formato volume 1K = 1000 Math.Round((moneyrisk / ((sl_double * Symbol.TickValue) / Symbol.TickSize)), 2); // *

            double lots = Math.Round(Symbol.VolumeInUnitsToQuantity(moneyrisk / ((sl_double * Symbol.TickValue) / Symbol.TickSize)), 2);

            if (lots < Minim) return Minim;

            if (lots > Maxi) return Maxi;

            return lots;

        }

        private double _getMyCapital(CapitalTo x)
        {

            switch (x)
            {

                case CapitalTo.Equity:

                    return Account.Equity;

                default:

                    return Account.Balance;

            }

        }

    }

}

/// <summary>
/// NameSpace che racchiude tutte le feature ctrader.guru
/// </summary>
namespace Guru
{
    /// <summary>
    /// Classe che definisce lo standard identificativo del prodotto nel marketplace ctrader.guru
    /// </summary>
    public class Product
    {

        public int ID = 0;
        public string Name = "";
        public string Version = "";
        public string Updated = "";

    }

    /// <summary>
    /// Offre la possibilità di utilizzare le API messe a disposizione da ctrader.guru per verificare gli aggiornamenti del prodotto.
    /// Permessi utente "AccessRights = AccessRights.FullAccess" per accedere a internet ed utilizzare JSON
    /// </summary>
    public class API
    {
        /// <summary>
        /// Costante da non modificare, corrisponde alla pagina dei servizi API
        /// </summary>
        private const string Service = "https://ctrader.guru/api/product_info/";

        /// <summary>
        /// Costante da non modificare, utilizzata per filtrare le richieste
        /// </summary>
        private const string UserAgent = "cTrader Guru";

        /// <summary>
        /// Variabile dove verranno inserite le direttive per la richiesta
        /// </summary>
        private RequestProductInfo RequestProduct = new RequestProductInfo();

        /// <summary>
        /// Variabile dove verranno inserite le informazioni identificative dal server dopo l'inizializzazione della classe API
        /// </summary>
        public ResponseProductInfo ProductInfo = new ResponseProductInfo();

        /// <summary>
        /// Classe che formalizza i parametri di richiesta, vengono inviate le informazioni del prodotto e di profilazione a fini statistici
        /// </summary>
        public class RequestProductInfo
        {

            /// <summary>
            /// Il prodotto corrente per il quale richiediamo le informazioni
            /// </summary>
            public Product MyProduct = new Product();

            /// <summary>
            /// Broker con il quale effettiamo la richiesta
            /// </summary>
            public string AccountBroker = "";

            /// <summary>
            /// Il numero di conto con il quale chiediamo le informazioni
            /// </summary>
            public int AccountNumber = 0;

        }

        /// <summary>
        /// Classe che formalizza lo standard per identificare le informazioni del prodotto
        /// </summary>
        public class ResponseProductInfo
        {

            /// <summary>
            /// Il prodotto corrente per il quale vengono fornite le informazioni
            /// </summary>
            public Product LastProduct = new Product();

            /// <summary>
            /// Eccezioni in fase di richiesta al server, da utilizzare per controllare l'esito della comunicazione
            /// </summary>
            public string Exception = "";

            /// <summary>
            /// La risposta del server
            /// </summary>
            public string Source = "";

        }

        /// <summary>
        /// Richiede le informazioni del prodotto richiesto
        /// </summary>
        /// <param name="Request"></param>
        public API(RequestProductInfo Request)
        {

            RequestProduct = Request;

            // --> Non controllo se non ho l'ID del prodotto
            if (Request.MyProduct.ID <= 0) return;

            // --> Dobbiamo supervisionare la chiamata per registrare l'eccexione
            try
            {

                // --> Strutturo le informazioni per la richiesta POST
                NameValueCollection data = new NameValueCollection
                {
                    { "account_broker", Request.AccountBroker },
                    { "account_number", Request.AccountNumber.ToString() },
                    { "my_version", Request.MyProduct.Version },
                    { "productid", Request.MyProduct.ID.ToString() }
                };

                // --> Autorizzo tutte le pagine di questo dominio
                Uri myuri = new Uri(Service);
                string pattern = string.Format("{0}://{1}/.*", myuri.Scheme, myuri.Host);

                Regex urlRegEx = new Regex(@pattern);
                WebPermission p = new WebPermission(NetworkAccess.Connect, urlRegEx);
                p.Assert();

                // --> Protocollo di sicurezza https://
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;

                // -->> Richiedo le informazioni al server
                using (var wb = new WebClient())
                {

                    wb.Headers.Add("User-Agent", UserAgent);

                    var response = wb.UploadValues(myuri, "POST", data);
                    ProductInfo.Source = Encoding.UTF8.GetString(response);

                }

                // -->>> Nel cBot necessita l'attivazione di "AccessRights = AccessRights.FullAccess"
                ProductInfo.LastProduct = JsonConvert.DeserializeObject<Product>(ProductInfo.Source);

            }
            catch (Exception Exp)
            {

                // --> Qualcosa è andato storto, registro l'eccezione
                ProductInfo.Exception = Exp.Message;

            }

        }

        /// <summary>
        /// Esegue un confronto tra le versioni per determinare la presenza di aggiornamenti
        /// </summary>
        /// <returns></returns>
        public bool HaveNewUpdate()
        {

            // --> Voglio essere sicuro che stiamo lavorando con le informazioni giuste
            return (ProductInfo.LastProduct.ID == RequestProduct.MyProduct.ID && ProductInfo.LastProduct.Version != "" && RequestProduct.MyProduct.Version != "" && new Version(RequestProduct.MyProduct.Version).CompareTo(new Version(ProductInfo.LastProduct.Version)) < 0);

        }

    }

}
