using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ZatcaService.Models;

namespace ZatcaService.Helpers
{
    public class InvoiceHelper
    {
        public static string GetManagerInvoiceJson(RelayData relayData)
        {
            var edit = JsonConvert.DeserializeObject<JObject>(relayData.Edit);
            ReplaceGuidsWithHiddenFieldValues(edit, relayData.HiddenFields);
            var Ret = JsonConvert.SerializeObject(edit, Formatting.Indented);
            Ret = Ret.Replace("Customer", "InvoiceParty")
                     .Replace("Supplier", "InvoiceParty")
                     .Replace("SalesInvoice", "RefInvoice")
                     .Replace("PurchaseInvoice", "RefInvoice")
                     .Replace("SalesUnitPrice", "UnitPrice")
                     .Replace("PurchaseUnitPrice", "UnitPrice");

            return Ret;
        }

        public static void ReplaceGuidsWithHiddenFieldValues(JObject edit, System.Collections.Generic.Dictionary<string, string> hiddenFields)
        {
            var propertiesToReplace = hiddenFields.Keys.ToList();
            foreach (var property in edit.Descendants().OfType<JProperty>())
            {
                if (propertiesToReplace.Contains(property.Value.ToString()))
                {
                    var guidValue = property.Value.ToString();
                    if (hiddenFields.ContainsKey(guidValue))
                    {
                        property.Value = JsonConvert.DeserializeObject<JObject>(hiddenFields[guidValue]);
                    }
                }
            }
        }

        public static void UpdateOrAddValueInCustomFields2Strings(JObject jObject, string fieldGuid, string newValue)
        {
           
            var strings = (JObject)jObject["CustomFields2"]["Strings"];

            if (strings.ContainsKey(fieldGuid))
            {
                strings[fieldGuid] = newValue;
            }
            else
            {
                strings.Add(fieldGuid, newValue);
            }
        }
        public static (int ICV, string PIH) GetLastICVandPIH(AppDbContext _dbContext)
        {
            var lastInvoice = _dbContext.ApprovedInvoices
                                               .OrderByDescending(invoice => invoice.Timestamp)
                                               .FirstOrDefault();

            if (lastInvoice == null)
            {
                return (1, "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==");
            }

            return (lastInvoice.ICV + 1, lastInvoice.PIH);
        }

        public static string ModifyQrInEditData(string editData, string qrFieldGuid, string newValue)
        {
            JObject jsonObject = JObject.Parse(editData);
            if (jsonObject["CustomFields2"]["Strings"][qrFieldGuid] != null)
            {
                jsonObject["CustomFields2"]["Strings"][qrFieldGuid] = newValue;
            }
            else
            {
                jsonObject["CustomFields2"]["Strings"][qrFieldGuid] = newValue;
            }
            string updatedJsonString = jsonObject.ToString(Formatting.Indented);

            return updatedJsonString;
        }
    }
}


