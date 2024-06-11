using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ZatcaService.Models
{
    public class GatewaySetting
    {
        [Key]
        public int RowId { get; set; }

        // Business Info
        [Display(Name = "Business ID")]
        public string ID { get; set; }

        [Display(Name = "Scheme ID")]
        [JsonProperty("schemeID")]
        public string SchemeID { get; set; }

        [Display(Name = "Street Name")]
        public string StreetName { get; set; }

        [Display(Name = "Building Number")]
        public string BuildingNumber { get; set; }

        [Display(Name = "City Subdivision Name")]
        public string CitySubdivisionName { get; set; }

        [Display(Name = "City Name")]
        public string CityName { get; set; }

        [Display(Name = "Postal Zone")]
        public string PostalZone { get; set; }

        [Display(Name = "Country Identification Code")]
        public string CountryIdentificationCode { get; set; }

        [Display(Name = "Company ID")]
        public string CompanyID { get; set; }

        [Display(Name = "Tax Scheme ID")]
        public string TaxSchemeID { get; set; }

        [Display(Name = "Registration Name")]
        public string RegistrationName { get; set; }

        // Business Data Custom Fields
        [Display(Name = "Api Access Token")]
        public string AccessToken { get; set; }

        [Display(Name = "Business Name GUID")]
        public string BusinessNameGuid { get; set; }

        [Display(Name = "Base Currency GUID")]
        public string BaseCurrencyGuid { get; set; }

        [Display(Name = "Invoice Subtype GUID")]
        public string InvoiceSubTypeGuid { get; set; }

        [Display(Name = "Item Tax Category GUID")]
        public string ItemTaxCategoryGuid { get; set; }

        [Display(Name = "Payment Means Code GUID")]
        public string PaymentMeansCodeGuid { get; set; }

        [Display(Name = "Instruction Note GUID")]
        public string InstructionNoteGuid { get; set; }

        [Display(Name = "Party Tax Info GUID")]
        public string PartyTaxInfoGuid { get; set; }

        [Display(Name = "QR Code GUID")]
        public string QrCodeGuid { get; set; }

        // Zatca Connection Setting

        [Display(Name = "EC Secp256k1 Private Key (PEM)")]
        public string EcSecp256k1Privkeypem { get; set; }

        [Display(Name = "CCSID Compliance Request ID")]
        public string CCSIDComplianceRequestId { get; set; }

        [Display(Name = "CCSID Binary Token")]
        public string CCSIDBinaryToken { get; set; }


        [Display(Name = "CCSID Secret")]
        public string CCSIDSecret { get; set; }

        [Display(Name = "Compliance Check URL")]
        public string ComplianceCheckUrl { get; set; }

        [Display(Name = "PCSID Binary Token")]
        public string PCSIDBinaryToken { get; set; }


        [Display(Name = "PCSID Secret")]
        public string PCSIDSecret { get; set; }

        [Display(Name = "Reporting URL")]
        public string ReportingUrl { get; set; }

        [Display(Name = "Clearance URL")]
        public string ClearanceUrl { get; set; }
    }
}