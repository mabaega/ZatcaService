using Microsoft.AspNetCore.Mvc;
using System.Net;
using ZatcaService.Models;
using Zatca.eInvoice.Models;
using ZatcaService.Helpers;
using System.Text;
using Zatca.eInvoice;
using Newtonsoft.Json;
using ZatcaService.Services;
using System.Net.Http.Headers;
using System.Xml.Serialization;


namespace ZatcaService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelayController : Controller // Mengubah dari ControllerBase menjadi Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<RelayController> _logger;
        private readonly GatewaySetting _gatewaySetting;
        private readonly HttpClient _httpClient = new HttpClient();
        public RelayController(AppDbContext dbContext, IGatewaySettingService gatewaySettingService, ILogger<RelayController> logger)
        {
            _dbContext = dbContext;
            _gatewaySetting = gatewaySettingService.GetGatewaySetting();
            _logger = logger;
        }

        [HttpPost("process-form")]
        public IActionResult ProcessFormData([FromForm] Dictionary<string, string> formData)
        {
            try
            {
                _logger.LogInformation("Received form data: {FormData}", JsonConvert.SerializeObject(formData));

                var relayData = new RelayData
                {
                    Referrer = formData.GetValueOrDefault("Referrer"),
                    Key = formData.GetValueOrDefault("Key"),
                    View = formData.GetValueOrDefault("View"),
                    Edit = formData.GetValueOrDefault("Edit"),
                    HiddenFields = formData
                                 .Where(x => x.Key != "Referrer" && x.Key != "Key" && x.Key != "View" && x.Key != "Edit")
                                 .ToDictionary(x => x.Key, x => x.Value)
                };

                ApprovedInvoice approvedInvoice = _dbContext.ApprovedInvoices.FirstOrDefault(invoice => invoice.InvoiceUUID == relayData.Key);

                if (approvedInvoice != null)
                {
                    approvedInvoice.RequestType = approvedInvoice.RequestType + " --- FROM INVOICE LOG ---";
                    return View("Index", approvedInvoice);
                }
                else
                {
                    (int ICV, string PIH) = InvoiceHelper.GetLastICVandPIH(_dbContext);
                    Invoice invoice = RelayToInvoiceMapper.GenerateInvoiceObject(relayData, _gatewaySetting, ICV, PIH);

                    InvoiceGenerator ig = new(
                        invoice,
                        Encoding.UTF8.GetString(Convert.FromBase64String(_gatewaySetting.PCSIDBinaryToken)),
                        _gatewaySetting.EcSecp256k1Privkeypem
                    );

                    ig.GetSignedInvoiceXML(out string base64SignedInvoice, out string base64QrCode, out string XmlFileName, out string requestApi);

                    ZatcaRequestApi zatcaRequestApi = JsonConvert.DeserializeObject<ZatcaRequestApi>(requestApi);
                    string invoiceHash = zatcaRequestApi.InvoiceHash;

                    var managerInvoicejson = InvoiceHelper.GetManagerInvoiceJson(relayData);
                    ManagerInvoice managerInvoice = JsonConvert.DeserializeObject<ManagerInvoice>(managerInvoicejson);

                    var editData = InvoiceHelper.ModifyQrInEditData(relayData.Edit, _gatewaySetting.QrCodeGuid, base64QrCode);
                    
                    var amount = invoice.LegalMonetaryTotal.TaxExclusiveAmount.NumericValue;
                    var totalAmount = invoice.LegalMonetaryTotal.TaxInclusiveAmount.NumericValue;
                    var taxAmount = totalAmount - amount;

                    approvedInvoice = new ApprovedInvoice
                    {
                        InvoiceUUID = relayData.Key,
                        InvoiceType = invoice.InvoiceTypeCode?.Value,
                        InvoiceSubType = invoice.InvoiceTypeCode?.Name,
                        Reference = managerInvoice?.Reference,
                        IssueDate = managerInvoice?.IssueDate.ToString("yyyy-MM-dd"),
                        PartyName = managerInvoice?.InvoiceParty?.Name,
                        CurrencyCode = managerInvoice?.InvoiceParty?.Currency?.Code ?? "SAR",
                        Amount = amount,
                        TaxAmount = taxAmount,
                        TotalAmount = totalAmount,
                        Base64Invoice = Convert.ToBase64String(Encoding.UTF8.GetBytes(managerInvoicejson)),
                        Referrer = relayData.Referrer,
                        ICV = ICV,
                        PIH = PIH,
                        InvoiceHash = invoiceHash,
                        Base64SignedInvoice = base64SignedInvoice,
                        Base64QrCode = base64QrCode,
                        XmlFileName = XmlFileName,
                        Timestamp = DateTime.Now,
                        EditData = editData,
                    };

                    return View("Index", approvedInvoice);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessFormData");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("compliance-check")]
        public async Task<IActionResult> ComplianceCheck([FromForm] ApprovedInvoice model)
        {
            try
            {
                ZatcaRequestApi zatcaRequestApi = new()
                {
                    Uuid = model.InvoiceUUID,
                    InvoiceHash = model.InvoiceHash,
                    Invoice = model.Base64SignedInvoice
                };

                var jsonContent = JsonConvert.SerializeObject(zatcaRequestApi);

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
                _httpClient.DefaultRequestHeaders.Add("Accept-Version", "V2");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_gatewaySetting.CCSIDBinaryToken}:{_gatewaySetting.CCSIDSecret}")));

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_gatewaySetting.ComplianceCheckUrl, content);

                var resultContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ServerResult>(resultContent);

                apiResponse.RequestType = "Invoice Compliant Check";
                apiResponse.StatusCode = response.StatusCode.ToString();

                model.RequestType = apiResponse.RequestType;
                model.ClearanceStatus = apiResponse.ClearanceStatus;
                model.ReportingStatus = apiResponse.ReportingStatus;

                model.ServerResult = JsonConvert.SerializeObject(apiResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ComplianceCheck");
                model.ServerResult = ex.Message;
                return View("Index", model);
            }
        }

        [HttpPost("clearance")]
        public async Task<IActionResult> Clearance([FromForm] ApprovedInvoice model)
        {
            try
            {
                ApprovedInvoice approvedInvoice = _dbContext.ApprovedInvoices.FirstOrDefault(invoice => invoice.InvoiceUUID == model.InvoiceUUID);

                if (approvedInvoice != null)
                {
                    var serverResult = JsonConvert.DeserializeObject<ServerResult>(approvedInvoice.ServerResult);
                    serverResult.RequestType = serverResult.RequestType + " --- FROM INVOICE LOG ---";
                    approvedInvoice.ServerResult = JsonConvert.SerializeObject(serverResult);

                    return View("Index", approvedInvoice);
                }

                ZatcaRequestApi zatcaRequestApi = new()
                {
                    Uuid = model.InvoiceUUID,
                    InvoiceHash = model.InvoiceHash,
                    Invoice = model.Base64SignedInvoice
                };

                var jsonContent = JsonConvert.SerializeObject(zatcaRequestApi);

                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
                _httpClient.DefaultRequestHeaders.Add("Clearance-Status", "1");
                _httpClient.DefaultRequestHeaders.Add("Accept-Version", "V2");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_gatewaySetting.PCSIDBinaryToken}:{_gatewaySetting.PCSIDSecret}")));

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_gatewaySetting.ClearanceUrl, content);

                var resultContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ServerResult>(resultContent);

                if (apiResponse != null && apiResponse.ClearedInvoice != null)
                {
                    var clearedInvoiceXml = Encoding.UTF8.GetString(Convert.FromBase64String(apiResponse.ClearedInvoice));

                    model.Base64SignedInvoice = apiResponse.ClearedInvoice;

                    XmlSerializer serializer = new(typeof(Invoice));
                    using (StringReader reader = new(clearedInvoiceXml))
                    {
                        var clearedInvoice = (Invoice)serializer.Deserialize(reader);

                        var qrCodeNode = clearedInvoice?.AdditionalDocumentReference?
                            .FirstOrDefault(docRef => docRef.ID.Value == "QR")?.Attachment?.EmbeddedDocumentBinaryObject;

                        if (qrCodeNode != null)
                        {
                            model.Base64QrCode = qrCodeNode.Value;
                            model.EditData = InvoiceHelper.ModifyQrInEditData(model.EditData, _gatewaySetting.QrCodeGuid, qrCodeNode.Value);
                        }

                    }
                }

                apiResponse.RequestType = "Invoice Clearance";
                apiResponse.StatusCode = response.StatusCode.ToString();

                model.RequestType = apiResponse.RequestType;
                model.ClearanceStatus = apiResponse.ClearanceStatus;
                model.ReportingStatus = apiResponse.ReportingStatus;
                model.ServerResult = JsonConvert.SerializeObject(apiResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                if (model.ClearanceStatus == "CLEARED")
                {
                    //model.Timestamp = apiResponse.Timestamp;
                    _dbContext.ApprovedInvoices.Add(model);
                    await _dbContext.SaveChangesAsync();
                }

                return View("Index", model);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ComplianceCheck");
                model.ServerResult = ex.Message;
                return View("Index", model);
            }
        }

        [HttpPost("reporting")]
        public async Task<IActionResult> Reporting([FromForm] ApprovedInvoice model)
        {
            {
                try
                {
                    ApprovedInvoice approvedInvoice = _dbContext.ApprovedInvoices.FirstOrDefault(invoice => invoice.InvoiceUUID == model.InvoiceUUID);

                    if (approvedInvoice != null)
                    {
                        var serverResult = JsonConvert.DeserializeObject<ServerResult>(approvedInvoice.ServerResult);
                        serverResult.RequestType = serverResult.RequestType + " --- FROM INVOICE LOG ---";
                        approvedInvoice.ServerResult = JsonConvert.SerializeObject(serverResult);

                        return View("Index", approvedInvoice);
                    }

                    ZatcaRequestApi zatcaRequestApi = new()
                    {
                        Uuid = model.InvoiceUUID,
                        InvoiceHash = model.InvoiceHash,
                        Invoice = model.Base64SignedInvoice
                    };

                    var jsonContent = JsonConvert.SerializeObject(zatcaRequestApi);

                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
                    _httpClient.DefaultRequestHeaders.Add("Clearance-Status", "0");
                    _httpClient.DefaultRequestHeaders.Add("Accept-Version", "V2");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_gatewaySetting.PCSIDBinaryToken}:{_gatewaySetting.PCSIDSecret}")));

                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(_gatewaySetting.ReportingUrl, content);

                    var resultContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ServerResult>(resultContent);

                    apiResponse.RequestType = "Invoice Reporting";
                    apiResponse.StatusCode = response.StatusCode.ToString();

                    model.RequestType = apiResponse.RequestType;
                    model.ClearanceStatus = apiResponse.ClearanceStatus;
                    model.ReportingStatus = apiResponse.ReportingStatus;
                    model.ServerResult = JsonConvert.SerializeObject(apiResponse, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    if (model.ReportingStatus == "REPORTED")
                    {
                        //model.Timestamp = apiResponse.Timestamp;
                        _dbContext.ApprovedInvoices.Add(model);
                        await _dbContext.SaveChangesAsync();
                    }

                    return View("Index", model);
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ComplianceCheck");
                    model.ServerResult = ex.Message;
                    return View("Index", model);
                }
            }
        }

        [HttpPost("update-invoice")]
        public async Task<IActionResult> UpdateInvoice([FromForm] ApprovedInvoice model)
        {
            try
            {
                var apiUrl = ConstructApiUrl(model.Referrer, model.InvoiceUUID);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-KEY", _gatewaySetting.AccessToken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var payload = model.EditData;
                    var content = new StringContent(payload, Encoding.UTF8, "application/json");

                    var response = await client.PutAsync(apiUrl, content);
                    ServerResult serverResult = new();
                    if (response.IsSuccessStatusCode)
                    {
                        serverResult.Message = "Manager invoice successfully updated. Please return to the manager to view the results.";
                    }
                    else
                    {
                        serverResult.Message = "Failed to update manager invoice. Please ensure that the Access Token recorded in the Gateway Settings is correct and still valid.";
                    }

                    model.ServerResult = JsonConvert.SerializeObject(serverResult, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateInvoice");
                model.ServerResult = ex.Message;
                return View("Index", model);
            }
        }

        private string ConstructApiUrl(string referrer, string invoiceUUID)
        {
            var uri = new Uri(referrer);
            var baseUrl = $"{uri.Scheme}://{uri.Host}";

            if (referrer.Contains("purchase-invoice-view"))
            {
                return $"{baseUrl}/api2/purchase-invoice-form/{invoiceUUID}";
            }
            else if (referrer.Contains("sales-invoice-view"))
            {
                return $"{baseUrl}/api2/sales-invoice-form/{invoiceUUID}";
            }
            else if (referrer.Contains("debit-note-view"))
            {
                return $"{baseUrl}/api2/debit-note-form/{invoiceUUID}";
            }
            else if (referrer.Contains("credit-note-view"))
            {
                return $"{baseUrl}/api2/credit-note-form/{invoiceUUID}";
            }

            throw new ArgumentException("Invalid referrer URL");
        }

    }
}



