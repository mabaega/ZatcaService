// Models/InvoiceLog.cs
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace ZatcaService.Models
{
    public class ApprovedInvoice
    {
        [Key]
        public string InvoiceUUID { get; set; }
        public string InvoiceType { get; set; }
        public string InvoiceSubType { get; set; }
        public string Reference { get; set; }
        public string IssueDate { get; set; }
        public string PartyName { get; set; }
        public string CurrencyCode { get; set; } = "SAR";
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; } = 0;
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal TaxAmount { get; set; } = 0;
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal TotalAmount { get; set; } = 0;

        public int ICV { get; set; }
        public string RequestType { get; set; }
        public string ClearanceStatus { get; set; }
        public string ReportingStatus { get; set; }
        public string PIH { get; set; }
        public string InvoiceHash { get; set; }

        public string EditData { get; set; }
        public string Base64Invoice { get; set; }
        public string Referrer { get; set; }
        
        public string ServerResult { get; set; }
        public string Base64SignedInvoice { get; set; }
        public string Base64QrCode { get; set; }
        public string XmlFileName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

    }
}