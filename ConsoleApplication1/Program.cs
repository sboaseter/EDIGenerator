using System;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ConsoleApplication1.Helpers;
using Dapper;
using System.Data.SqlClient;

namespace ConsoleApplication1 {
    #region DataModels
    class Assortiment {
        public int Sequence { get; set; }
        public int CABSequence { get; set; }
        public string CABCode { get; set; }
        public string CABDesc { get; set; }
        public decimal? Available { get; set; }
    }

    public class EDIData {

        /*
         *  F_CAB_CD_CAB_CODE, CABC_SEQUENCE
         */
        public decimal ArtCode { get; set; }

        /*
         * F_CAB_GROWER_CASK_MATRIX
         */
        public decimal APE { get; set; }
        public decimal TotalUnits { get; set; }
        public decimal UnitsPerCar { get; set; }

        public string strSortcode1 { get; set; }
        public string strSortcode2 { get; set; }
        public string strSortcode3 { get; set; }
        public string strSortcode4 { get; set; }
        public string strSortcode5 { get; set; }
        public string strSortcode6 { get; set; }
        public string strSortcode7 { get; set; }
        public string strSortcode8 { get; set; }
        public string strSortcode9 { get; set; }
        public string strSortcode10 { get; set; }

        public string strSortCodeZN1 { get; set; }
        public string strSortCodeZN2 { get; set; }
        public string strSortCodeZN3 { get; set; }
        public string strSortCodeZN4 { get; set; }
        public string strSortCodeZN5 { get; set; }
        public string strSortCodeZN6 { get; set; }
        public string strSortCodeZN7 { get; set; }
        public string strSortCodeZN8 { get; set; }
        public string strSortCodeZN9 { get; set; }
        public string strSortCodeZN10 { get; set; }


        /*
         * F_CAB_CD_CAB_CODE -> F_CAB_CAB_DETAIL
         */
        public string strQuality { get; set; }
        public string strMaturity { get; set; }
        public decimal EmbCode { get; set; }

        /*
         * F_CAB_OFFER_DETAILS/F_CAB_OFFER_HEADERS?
         */
        public string strPartySize { get; set; }

        public string strVet1Code { get; set; }
        public string strVet2Code { get; set; }
        public decimal lngNumOfPlates { get; set; }
        public string strSellPrice { get; set; }
        public decimal lngSegmentCount { get; set; }
        public decimal lngPTYCount { get; set; }
        public decimal lngUNHCount { get; set; }
        public decimal SenderId { get; set; } // Debt.nummer LF: CINF_AUCTION_DEBTOR_NO, CAB: ORGA_VBA_NUMMER (Needs to be changed to general debt.nummer pr auction)
        public decimal AuinSeq { get; set; }
        public decimal AUSESeq { get; set; }
        public bool IsTest { get; set; }
        public string ALNumber { get; set; }
        public string ALNumberFirst { get; set; }
        public decimal UNCode { get; set; }
        public string AuctionCode { get; set; }
        public decimal lngMaxParty { get; set; }
        public DateTime dtTransport { get; set; }
        public DateTime dtAuction { get; set; }
        public string AuctionGroupCode { get; set; }
        public string CarNumber { get; set; }
        public bool IsSelfInspect { get; set; }
        public bool RequestEABResponse { get; set; }
        public bool IsGP { get; set; }
        public string CDPPCode { get; set; }
        public decimal NumOfCars { get; set; }
        public string strVersion { get; set; }
        public string strRelease { get; set; }
        public decimal lngLineSequence { get; set; }
        public decimal UNH_ID { get; set; }
        public string IRNNumber { get; set; }
    }

    public enum AuctionCodeEnum {
        [Description("06")]
        MVA,
        [Description("01")]
        VBA,
        [Description("03")]
        BVH

    }
    public class EDIRepository {
        public int _auctionLetterInfo { get; set; }
        public EDIRepository(int auctionLetterInfo) {
            _auctionLetterInfo = auctionLetterInfo;

        }
        public List<EDIData> loadEDIData() {
            List<EDIData> result = new List<EDIData>();
            result.Add(new EDIData());
            result.Add(new EDIData());
            result.Add(new EDIData());
            return result;
        }
    }

    public class EDIProvider {
        public EDIData EDI { get; set; }
        public List<String> Body { get; set; }
        public EDIProvider() {
            Body = new List<String>();
        }
        public bool IsEDIDataValid(EDIData edi) {
            if (Int32.Parse(edi.strPartySize) == 0)
                return false;
            return true;
        }
        public void InsertData(EDIData edi, List<string> EDIBody, bool UseAsMVAInvoice) {
            EDIBody.Add(String.Format("Test: {0}", UseAsMVAInvoice));
        }
        public void InsertData(ref String line, bool increaseSgmCount) {
            Body.Add(line);
            Console.WriteLine(String.Format("{0}: {1}", Body.Count, line));
            line = String.Empty;
            if (increaseSgmCount)
                EDI.lngSegmentCount++;
            
        }
    }
    public static class MeObj {
        public static bool EDIExport { get; set; }
    }
    public class AuctionProvider {
        private string ConnectionString;
        public AuctionProvider() {
            this.ConnectionString = ConfigurationManager.ConnectionStrings["CAB"].ToString();
        }
        public List<Auction> Get() {
            using (var conn = new SqlConnection(this.ConnectionString))
            {
                conn.Open();
                var res = conn.Query<Auction>(@"
                    SELECT 
                    CDAU_SEQUENCE as sequence,
                    CDAU_DESCRIPTION as description,
                    CDAU_CODE as code,
                    CDAU_EXTERNAL_CODE as externalCode,
                    CDAU_AUCTION_LETTER_NUMBER_TEXT as auctionLetterNumberNext,
                    CDAU_EDI_EMAIL as ediEmail
                    FROM F_CAB_CD_AUCTION");
                return res.ToList();
            }
        }
    }

    #endregion
    class Program {
         
        static void Main(string[] args) {
            string ConnectionString = ConfigurationManager.ConnectionStrings["CAB"].ToString();
            
            /*
             * Setup test-data or fetch data.
             */ 
            #region EDI_Initialization

            //Console.WriteLine("");

            //Sender sender = new SenderProvider().Get()

            List<Auction> auctions = new AuctionProvider().Get();
            Console.WriteLine("Auctions");
            Console.WriteLine("==================================================");
            for (int i = 0; i < auctions.Count; i++) {
                Console.WriteLine(auctions[i].ToString());
            }



            Console.WriteLine();
            Console.WriteLine("Generating FLOWAV 2.8: {0} ({1})", auctions.FirstOrDefault(x => x.Code == "VBA").Description, auctions.FirstOrDefault(x => x.Code == "VBA").EdiEmail);
            Console.WriteLine("==================================================");
            Console.WriteLine();



            MeObj.EDIExport = false;
            EDIProvider _ediProvider = new EDIProvider();
            _ediProvider.EDI = new EDIData();
            decimal lngSenderId = 0;
            bool EDIExport = false;

            int auctionLetterInfo = 12;
            _ediProvider.EDI.AuctionCode = auctions.FirstOrDefault(x => x.Code == "VBA").ExternalCode;
            string SenderIDDebtNr = "";
            using (var conn = new SqlConnection(ConnectionString)) {
                conn.Open();
                SenderIDDebtNr = conn.Query<string>(@"SELECT ORGA_VBA_NUMBER FROM F_CAB_ORGANISATION WHERE ORGA_SEQUENCE = @Id", new { Id = 1155 }).First();                
            }
            _ediProvider.EDI.SenderId = Decimal.Parse(SenderIDDebtNr);
            _ediProvider.EDI.lngUNHCount = 0;
            _ediProvider.EDI.strVersion = "2";
            _ediProvider.EDI.strRelease = "8";
            _ediProvider.EDI.IsTest = true;
            _ediProvider.EDI.RequestEABResponse = false;
            _ediProvider.EDI.APE = 80;
            _ediProvider.EDI.EmbCode = 577;
            //_ediProvider.EDI.ArtCode = 
//            using (CAB cab = new CAB()) {
//                _ediProvider.EDI.ArtCode = Decimal.Parse(cab.F_CAB_GROWER_ASSORTIMENT.FirstOrDefault(x => x.GRAS_FK_GROWER == 141).F_CAB_CD_CAB_CODE.F_CAB_CAB_VBN_MATRIX.FirstOrDefault().F_CAB_CD_VBN_CODE.VBNC_VBN_CODE);
//            }
            _ediProvider.EDI.ArtCode = 0;
            String strLine = "";

            // OfferSequence or AuctionLetterSequence in local DB.
            decimal uniqueIdentifier = 20707174102437;
            #endregion

            //Segment: UNB Interchange Header
            //Position: 010
            //Group:
            //Level: 0
            //Usage: Conditional (Required)
            //Max Use: 1
            //Dependency Notes:
            //Comments:
            //Notes: Functie: begin van een berichtentransmissie.
            #region UNB_Interchange_Header
            switch (_ediProvider.EDI.AuctionCode) {
                case "MVA":
                strLine += String.Format("UNB+UNOA:2+{0:000000.##}:ZZ+{1:00}:ZZ+", _ediProvider.EDI.SenderId, 1);
                _ediProvider.EDI.strVersion = "002";
                _ediProvider.EDI.strRelease = "008";
                break;
                case "VBA":
                strLine += String.Format("UNB+UNOA:2+{0:000000.##}:ZZ+{1:00}:ZZ+", _ediProvider.EDI.SenderId, _ediProvider.EDI.AuctionCode);
                _ediProvider.EDI.strVersion = "002";
                _ediProvider.EDI.strRelease = "008";
                break;
                default:
                strLine += String.Format("UNB+UNOA:2+{0:000000.##}:ZZ+{1:00}:ZZ+", _ediProvider.EDI.SenderId, _ediProvider.EDI.AuctionCode);
                break;
            }
            strLine += String.Format("{0:yyMMdd}:{1:hhmm}+{2:00000000000000}++{3}++{4}", DateTime.Now, DateTime.Now, uniqueIdentifier, _ediProvider.EDI.RequestEABResponse ? 1 : 0, _ediProvider.EDI.IsTest ? 1 : 0);
            _ediProvider.InsertData(ref strLine, false);
            #endregion

            //Segment: UNH Message Header
            //Position: 020
            //Group:
            //Level: 0
            //Usage: Mandatory
            //Max Use: 1
            //Dependency Notes:
            //Comments:
            //Notes: Functie: start van een bericht.
            #region UNH_Message_Header

            _ediProvider.EDI.UNH_ID = 42;  // Unique number for the edi-message
            strLine += String.Format("UNH+{0}+FLOWAV:{1}:{2}:EF'", _ediProvider.EDI.UNH_ID, _ediProvider.EDI.strVersion, _ediProvider.EDI.strRelease);
            _ediProvider.InsertData(ref strLine, true);
            _ediProvider.EDI.lngUNHCount += 1;
            #endregion

            //Segment: BGM Beginning of Message
            //Position: 030
            //Group:
            //Level: 0
            //Usage: Mandatory
            //Max Use: 1
            //Dependency Notes:
            //Comments:
            //Notes: Functie: wordt gebruikt om het type bericht te specificeren.
            #region BGM_Beginning_of_Message

            _ediProvider.EDI.UNCode = 9;
            if (EDIExport)
                strLine += String.Format("BGM+103++{0}'", _ediProvider.EDI.UNCode);// 'UNCode = 3,5 of 9 (9 = origineel, 5 = vervang voorgaande, 3 = ?)
            else
                strLine += String.Format("BGM+102++{0}'", _ediProvider.EDI.UNCode); //'UNCode = 3,5 of 9 (9 = origineel, 5 = vervang voorgaande, 3 = ?)
            _ediProvider.InsertData(ref strLine, true);
            _ediProvider.EDI.lngPTYCount = 0;
            #endregion

            //Segment: PTY Partij
            //Position: 035 (Trigger Segment)
            //Group: Segment Group 1 Mandatory
            //Level: 1
            //Usage: Mandatory
            //Max Use: 1
            //Dependency Notes:
            //Comments:
            //Notes: Identificatie van de partij door het aanvoerbriefvolgnummer en letter. Bij FloraH
            #region PTY_Partij
            //foreach (EDIData edi in _ediProvider.) {
            for(int i =0;i<2;i++) {
                Console.WriteLine("====For each Articlegroup : {0}====", i);
                if (_ediProvider.EDI.ALNumber == "BVH".ToString()) {
                    strLine += String.Format("PTY+{0:000000}+{1}", String.Format("{0}{1}", _ediProvider.EDI.ALNumber, "W"), _ediProvider.EDI.lngPTYCount);
                } else {
                    strLine += String.Format("PTY+{0}+{1}", _ediProvider.EDI.ALNumber, _ediProvider.EDI.lngPTYCount.ToString().ToUpper());
                }
                _ediProvider.InsertData(ref strLine, true);
                if (MeObj.EDIExport) {
                    strLine += String.Format("NAD+DO+{0:00}'", 1);
                } else {
                    strLine += String.Format("NAD+DO+{0:00}'", _ediProvider.EDI.AuctionCode.ToString());
                }
                _ediProvider.InsertData(ref strLine, true);
                strLine += String.Format("NAD+MF+{0:000000}'", lngSenderId);
                _ediProvider.InsertData(ref strLine, true);

                if (MeObj.EDIExport) {
                    strLine += String.Format("NAD+BY+{0:000000}'", _ediProvider.EDI.SenderId);
                    _ediProvider.InsertData(ref strLine, true);
                }
                

                #region PTY_Start
                
                bool UseAsMVAInvoice = false; // TODO: Real data

                if (_ediProvider.EDI.strQuality != "A1" && _ediProvider.EDI.strQuality != "A2" && _ediProvider.EDI.strQuality != "B1")
                    _ediProvider.EDI.strQuality = "A1";
                _ediProvider.EDI.strPartySize = "3";
                if (!_ediProvider.IsEDIDataValid(_ediProvider.EDI)) {
                    System.Console.WriteLine("Partijgrootte (PartySize) is onbekend. Aanmaken bestand.");
                } else {
                    if (MeObj.EDIExport) {
                        strLine = String.Empty;
                        strLine += "FTX+ABM++6'";
                        _ediProvider.InsertData(ref strLine, true);
                    } else if (_ediProvider.EDI.AuctionCode == "BVH" || _ediProvider.EDI.AuctionCode == "VBA") {
                        int intPartySize = Int32.Parse(_ediProvider.EDI.strPartySize);
                        int intNumOfCars = (int)_ediProvider.EDI.NumOfCars;
                        strLine = String.Empty;
                        if (intPartySize == 2 && intNumOfCars > 2) {
                            strLine += "FTX+ABM++8'";
                        } else if (intPartySize == 4 && intNumOfCars > 2) {
                            strLine += "FTX+ABM++9'";
                        } else {
                            strLine += String.Format("FTX+ABM++{0}'", intPartySize);
                        }
                        _ediProvider.InsertData(ref strLine, true);
                    } else {
                        int intPartySize = Int32.Parse(_ediProvider.EDI.strPartySize);
                        strLine += String.Format("FTX+ABM++{0}'", intPartySize);
                        _ediProvider.InsertData(ref strLine, true);
                    }
                    _ediProvider.EDI.lngSegmentCount++;

                    if (MeObj.EDIExport) {
                        strLine += "FTX+COI++J'";
                        _ediProvider.InsertData(ref strLine, true);
                    }

                    if (_ediProvider.EDI.AuctionCode != "MVA" && _ediProvider.EDI.AuctionCode != "BVH") { // Refine
                        strLine = String.Format("FTX+KWA++{0}'", _ediProvider.EDI.strQuality);
                        _ediProvider.InsertData(ref strLine, true);
                    }
                    if (_ediProvider.EDI.AuctionCode != "MVA") {
                        strLine = String.Format("FTX+PRD++{0}'", _ediProvider.EDI.AuctionGroupCode);
                        _ediProvider.InsertData(ref strLine, true);
                    }
                    strLine += String.Format("DTM+97:{0:yyMMdd}:102'", _ediProvider.EDI.dtAuction);
                    _ediProvider.InsertData(ref strLine, true);

                    strLine += String.Format("DTM+50:{0:yyMMdd}:102'", _ediProvider.EDI.dtTransport);
                    _ediProvider.InsertData(ref strLine, true);

                    strLine += String.Format("DTM+50:{0:hhmm}:401", _ediProvider.EDI.dtTransport);
                    _ediProvider.InsertData(ref strLine, true);

                    if (_ediProvider.EDI.AuctionCode != "VBA")
                        strLine += String.Format("GDS+{0:00}'", _ediProvider.EDI.ArtCode);
                    else
                        strLine += String.Format("GDS+{0:00000}'", _ediProvider.EDI.ArtCode);
                    _ediProvider.InsertData(ref strLine, true);

                #endregion
                    //Segment: IMD Item Description
                    //Position: 175
                    //Group: Segment Group 1 Mandatory
                    //Level: 2
                    //Usage: Conditional (Optional)
                    //Max Use: 99
                    //Dependency Notes:
                    //Comments:
                    //Notes: Optioneel segment. Functie: opgeven van kenmerktype en waarde van:
                    //- partijsorteringen (S-codes)
                    //- negatieve keurcodes (K-codes)
                    //- positieve keurcodes (P-codes)
                    //- Informatiecodes (I-codes, Alleen FloraHolland)
                    #region IMD_Item_Description


                    //'### PTY.IMD (Sorteercode)
                    //'LENGTE (S-code zit al in de variabele)
                    strLine = String.Empty;
                    if (_ediProvider.EDI.AuctionCode != "VBA") {
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortCodeZN1);
                    } else {
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortcode1);
                    }
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.strSortcode1)) {
                        _ediProvider.InsertData(ref strLine, true);
                    }
                    //'### PTY.IMD (Sorteercode)
                    //'GEWICHT (S-code zit al in de variabele)
                    strLine = String.Empty;
                    if (_ediProvider.EDI.AuctionCode != "VBA")
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortCodeZN2);
                    else
                        strLine = String.Format("IMD++{0}'", _ediProvider.EDI.strSortcode2);
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.strSortcode2)) {
                        _ediProvider.InsertData(ref strLine, true);
                    }

                    //'### PTY.IMD (Sorteercode)
                    //'KNOPPEN (S-code zit al in de variabele)
                    strLine = String.Empty;
                    if (_ediProvider.EDI.AuctionCode != "VBA")
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortCodeZN3);
                    else
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortcode3);
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.strSortcode3)) {
                        _ediProvider.InsertData(ref strLine, true);
                    }

                    //'### PTY.IMD (Sorteercode)
                    //'DIAMETER (S-code zit al in de variabele)
                    strLine = String.Empty;
                    if (_ediProvider.EDI.AuctionCode != "VBA")
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortCodeZN4);
                    else
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortcode4);
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.strSortcode4)) {
                        _ediProvider.InsertData(ref strLine, true);
                    }

                    //'### PTY.IMD (Sorteercode)
                    //'KLEUR (S-code zit al in de variabele)
                    strLine = String.Empty;
                    if (_ediProvider.EDI.AuctionCode != "BVH") { // != auBVH_Bleiswijk
                        if (_ediProvider.EDI.AuctionCode != "VBA") {
                            strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortCodeZN5);
                        } else {
                            strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortcode5);
                        }
                        if (!String.IsNullOrEmpty(_ediProvider.EDI.strSortcode5)) {
                            _ediProvider.InsertData(ref strLine, true);
                        }
                    }

                    //'### PTY.IMD (Sorteercode)
                    //'POTMAAT (S-code zit al in de variabele)
                    strLine = String.Empty;
                    if (_ediProvider.EDI.AuctionCode != "VBA")
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortCodeZN6);
                    else
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortcode6);
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.strSortcode6)) {
                        _ediProvider.InsertData(ref strLine, true);
                    }
          
                    //'### PTY.IMD (Sorteercode)
                    //'Aantal per bos (rozen)
                    strLine = String.Empty;
                    if (_ediProvider.EDI.AuctionCode != "VBA")
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortCodeZN7);
                    else
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortcode7);
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.strSortcode7)) {
                        _ediProvider.InsertData(ref strLine, true);
                    }

                    //'### PTY.IMD (Sorteercode)
                    //'Aantal knoppen (trosrozen)
                    strLine = String.Empty;
                    if (_ediProvider.EDI.AuctionCode != "VBA")
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortCodeZN8);
                    else
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortcode8);
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.strSortcode8)) {
                        _ediProvider.InsertData(ref strLine, true);
                    }

                    //'### PTY.IMD (Sorteercode)
                    //'Vertakkingen (Hypericum)
                    strLine = String.Empty;
                    if (_ediProvider.EDI.AuctionCode != "VBA")
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortCodeZN9);
                    else
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortcode9);
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.strSortcode9)) {
                        _ediProvider.InsertData(ref strLine, true);
                    }

                    //'### PTY.IMD (Sorteercode)
                    //'Aantal kleuren (Hypericum gemengd.) S55
                    strLine = String.Empty;
                    if (_ediProvider.EDI.AuctionCode != "VBA")
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortCodeZN10);
                    else
                        strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strSortcode10);
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.strSortcode10)) {
                        _ediProvider.InsertData(ref strLine, true);
                    }
                    //'### PTY.IMD (Sorteercode)
                    //'RIJPHEIDSSTADIUM (S-code zit al in de variabele)
                    strLine = String.Empty;
                    strLine += String.Format("IMD++{0}'", _ediProvider.EDI.strMaturity);
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.strMaturity)) {
                        _ediProvider.InsertData(ref strLine, true);
                    }
                    //'### PTY.IMD K02 (Sorteercode, negatieve keurcode) VBA: niet verplicht
                    //  '4-7-2002/MVD: Negatieve Keurcodes ook in IMD. Voorlopig 1 regel, later max drie
                    //  '           regels mogelijk maken. Indien geen neg. keurcode aanwezig, 0.00 meegeven.
                    //  '           later eventueel ook positieve codes mogelijk maken
                    strLine = String.Empty;
                    if (_ediProvider.EDI.strVet1Code != "000") {
                        strLine += String.Format("IMD++K01+{0}'", _ediProvider.EDI.strVet1Code);
                        _ediProvider.InsertData(ref strLine, true);
                    }
                    //'### PTY.IMD K02 (Sorteercode, negatieve keurcode) VBA: niet verplicht
                    strLine = String.Empty;
                    if (_ediProvider.EDI.strVet2Code != "000") {
                        strLine += String.Format("IMD++K02+{0}'", _ediProvider.EDI.strVet2Code);
                        _ediProvider.InsertData(ref strLine, true);
                    }

                    #endregion

                    #region PTY_End

                    strLine = String.Empty;
                    //'### PTY PAC (Fustcode) VBA: niet verplicht
                    if (_ediProvider.EDI.EmbCode == 0) _ediProvider.EDI.EmbCode = 999;
                    strLine += String.Format("PAC+++{0}'", _ediProvider.EDI.EmbCode);
                    _ediProvider.InsertData(ref strLine, true);
                    // 'PTY QTY 52 (Aantal per fust)
                    strLine += String.Format("QTY+52:{0}'", _ediProvider.EDI.APE);// 'voor GP als KP hetzelfde: APE.
                    _ediProvider.InsertData(ref strLine, true);
                    // 'PTY QTY 46 (Aantal per partij: vervallen vanaf Flowav2.0, hebben wij nooit ondersteund)
                    // 'PTY RFF IRN foto referentie..
                    strLine = String.Empty;
                    if (!String.IsNullOrEmpty(_ediProvider.EDI.IRNNumber)) {
                        strLine += String.Format("RFF+IRN:{0}::1'", _ediProvider.EDI.IRNNumber);
                        _ediProvider.InsertData(ref strLine, true);
                    }
                    strLine = String.Empty;
                    if (MeObj.EDIExport) {
                        //'PRI INV prijs in euro verkoopprijs.
                        //'Dit element is nodig voor factureren via heb bemiddelingsbureau.
                        strLine += String.Format("PRI+INV:{0}'", _ediProvider.EDI.strSellPrice);
                        _ediProvider.InsertData(ref strLine, true);
                    }
                    #endregion
                }
            }
            #endregion

            #region EDIEnd
            if (!_ediProvider.EDI.IsGP && (_ediProvider.EDI.AuctionCode == "VBA" || _ediProvider.EDI.AuctionCode == "MVA"))
                // add EQD-data
            if(_ediProvider.EDI.lngNumOfPlates > 0 && _ediProvider.EDI.AuctionCode != "VBA") {
                strLine += String.Format("EQN+{0}:19'", _ediProvider.EDI.lngNumOfPlates);
                _ediProvider.InsertData(ref strLine, true);
            }
            strLine += String.Format("UNT+{0}+{1:00000000000000}'", _ediProvider.EDI.lngSegmentCount, uniqueIdentifier);
            _ediProvider.InsertData(ref strLine, true);

            strLine += String.Format("UNZ+{0}+{1:00000000000000}'", _ediProvider.EDI.lngUNHCount, uniqueIdentifier);
            _ediProvider.InsertData(ref strLine, true);
            #endregion

            #region EntityTest
            /*
             using (CAB cab = new CAB()) {
                var assortiment = cab.F_CAB_GROWER_ASSORTIMENT.Where(x => x.GRAS_FK_GROWER == 141).Select(x => new Assortiment {
                    Sequence = x.GRAS_SEQUENCE,
                    CABSequence = x.GRAS_FK_CAB_CODE,
                    CABCode = x.F_CAB_CD_CAB_CODE.CABC_CAB_CODE,
                    CABDesc = x.F_CAB_CD_CAB_CODE.CABC_CAB_DESC,
                    Available = x.GRAS_NUM_OF_ITEMS
                });
                foreach (Assortiment a in assortiment) {
                    System.Console.WriteLine(String.Format("CC: {0}, A: {1}\t, CD: {2}", a.CABCode, a.Available, a.CABDesc));
                }

                var test1 = cab.V_OFFER_HEADERS.Where(x => x.GROWER == "Kweker TEST");
                foreach (V_OFFER_HEADERS ofhd in test1) {
                    var test2 = cab.V_OFFER_DETAILS.Where(x => x.OFHD_SEQUENCE == ofhd.OFHD_SEQUENCE);
                    Console.WriteLine(String.Format("OFHD_NO: {0}, Buyer: {1}", ofhd.OFHD_NO, ofhd.BUYER));
                    foreach (V_OFFER_DETAILS ofdt in test2) {
                        V_FIXED_CAB_PROPS props = cab.V_FIXED_CAB_PROPS.FirstOrDefault(x => x.CABC_SEQUENCE == ofdt.OFDT_FK_CAB_CODE);
                        Console.WriteLine(String.Format("\tDesc: {0}", props.DESC_PROPS));
                    }
                    
                }

             }
             */
            #endregion

            System.Console.ReadLine();
        }
    }
}
