
using ZatcaService.Models;

namespace ZatcaService.Helpers
{
    //"StreetName": "صلاح الدين | Salah Al-DinX"
    //"BuildingNumber": "1111"
    //"CitySubdivisionName": "المروج | Al-Murooj"
    //"CityName": "الرياض | Riyadh"
    //"PostalZone": "12222"
    //"CountryIdentificationCode": "SA"
    //"TaxSchemeCompanyID": "399999999800003"
    //"TaxSchemeID": "VAT"
    //"RegistrationName": "شركة نماذج فاتورة المحدودة | Fatoora Samples LTD"

    public static class PartyTaxInfoParser
    {
        private static readonly string[] separator = new[] { "\n" };
        private static readonly string[] separatorArray = new[] { ":" };

        public static PartyTaxInfo ParsePartyInfo(string PartyTaxInfo)
        {
            var partyInfo = new PartyTaxInfo();
            var keyValuePairs = PartyTaxInfo.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(pair => pair.Split(separatorArray, 2, StringSplitOptions.RemoveEmptyEntries))
                                             .Where(pair => pair.Length == 2)
                                             .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim().Trim('"'));

            foreach (var pair in keyValuePairs)
            {
                switch (pair.Key)
                {
                    case "StreetName":
                        partyInfo.StreetName = pair.Value;
                        break;
                    case "BuildingNumber":
                        partyInfo.BuildingNumber = pair.Value;
                        break;
                    case "CitySubdivisionName":
                        partyInfo.CitySubdivisionName = pair.Value;
                        break;
                    case "CityName":
                        partyInfo.CityName = pair.Value;
                        break;
                    case "PostalZone":
                        partyInfo.PostalZone = pair.Value;
                        break;
                    case "CountryIdentificationCode":
                        partyInfo.CountryIdentificationCode = pair.Value;
                        break;
                    case "TaxSchemeCompanyID":
                        partyInfo.CompanyID = pair.Value;
                        break;
                    case "TaxSchemeID":
                        partyInfo.TaxSchemeID = pair.Value;
                        break;
                    case "RegistrationName":
                        partyInfo.RegistrationName = pair.Value;
                        break;
                }
            }

            return partyInfo;
        }
    }
}