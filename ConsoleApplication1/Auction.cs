using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1 {
    public class Auction {
        public int Sequence {get;set;}
        public string Description { get; set; }
        public string Code { get; set; }
        public string ExternalCode { get; set; }
        public int AuctionLetterNumberNext { get; set; }
        public string EdiEmail { get; set; }
        public Auction(
            int sequence,
            string description,
            string code,
            string externalCode,
            int auctionLetterNumberNext,
            string ediEmail
            ) {
            Sequence = sequence;
            Description = description;
            Code = code;
            ExternalCode = externalCode;
            AuctionLetterNumberNext = auctionLetterNumberNext;
            EdiEmail = ediEmail;
        }
        public override string ToString() {
            var DescShort = Description;
            if(Description.Length > 15)
                DescShort = Description.Substring(0, 15);
            if(Description.Length > 5)
                return String.Format("{0}\t{1}\t{2}\t{3}", DescShort, Code, ExternalCode, EdiEmail);
            else
                return String.Format("{0}\t\t{1}\t{2}\t{3}", DescShort, Code, ExternalCode, EdiEmail);
        }
    }
}
