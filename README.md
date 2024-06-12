# Manager IO - Zatca Integration

This guide provides detailed instructions for integrating Manager IO with Zatca's e-invoicing system using the provided SDK.

## Installation

1. **Download SDK**:
   - Visit [Zatca SDK Download](https://sandbox.zatca.gov.sa/downloadSDK) and download version 238-R3.3.3.
   - Select "Version History", choose the latest version, and click the "Download SDK" button.
   - Extract the downloaded file to `C:\`.

2. **Configuration**:
   - Copy `C:\zatca-einvoicing-sdk-238-R3.3.3\Data\Input\csr-config-example-EN.properties` to `C:\zatca-einvoicing-sdk-238-R3.3.3\Lib\.Net\csr-config-example-EN.properties`.
   - Edit the copied file using Notepad and save.
   - Rename the edited file to `csr.config`.

3. **Generate CSR**:
   - Open Command Prompt and run the following commands:
     ```bash
     C:
     cd \
     cd C:\zatca-einvoicing-sdk-238-R3.3.3\Lib\.Net\Test
     fatooranet.exe csr -csrConfig ..\csr.config -generatedCsr ..\cert.pem -privateKey ..\ec-secp256k1-priv-key.pem -nonprod true
     ```
   - Use `-nonprod true` for sandbox testing. Use `-sim true` for Simulation Server (not tested), and skip this option for production (not tested).

   - This command will generate two files:
     - `C:\zatca-einvoicing-sdk-238-R3.3.3\Lib\.Net\cert.pem`
     - `C:\zatca-einvoicing-sdk-238-R3.3.3\Lib\.Net\ec-secp256k1-priv-key.pem`

## CCSID and PCSID Generation

1. **Generate CCSID**:
   - In your browser, go to [Zatca Integration Sandbox](https://sandbox.zatca.gov.sa/IntegrationSandbox).
   - Select "Compliance CSID", click "POST", and then "Try it Out".
   - Use the default valid token "123456" for sandbox testing.
   - Copy the content of `cert.pem` and paste it into the `{ "csr": "csr file content" }` field.
   - Click "Execute" to receive a response containing the `binarySecurityToken` and `secret`.

2. **Generate PCSID**:
   - Select "Production CSID (Onboarding) API".
   - Click "Authorize" and use the `binarySecurityToken` and `secret` from the CCSID generation step for authentication.
   - Click "POST" and "Try it Out", using the CCSID `requestID` in the request body `{"compliance_request_id": "1234567890123"}`.
   - Click "Execute" to receive a response with the PCSID details.

## API Endpoints

Use the following endpoints for sandbox testing:
- **Reporting**: `https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/invoices/reporting/single`
- **Clearance**: `https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/invoices/clearance/single`
- **Compliance Check**: `https://gw-fatoora.zatca.gov.sa/e-invoicing/developer-portal/compliance/invoices`

## Manager Business Data Setup

1. **Generate New Business or Open Existing Business Data**.
2. **Create Custom Fields**:
   - Navigate to `Settings` -> `Custom Fields` -> `Text Custom Fields`.
   - Create the following custom fields:
     
     - **PartyTaxInfo**:
        Type : Paragraph text
        Placement : Customer, Supplier
        
     - **Item TaxCategory Info**:
        Type : Dropdown list
        Placement : Inventory Item, Non Inventory Item, Inventory Kit
        Options:
        ```
        S
        E | VATEX-SA-29
        E | VATEX-SA-29-7
        E | VATEX-SA-30
        Z | VATEX-SA-32
        Z | VATEX-SA-33
        Z | VATEX-SA-34-1
        Z | VATEX-SA-34-2
        Z | VATEX-SA-34-3
        Z | VATEX-SA-34-4
        Z | VATEX-SA-34-5
        Z | VATEX-SA-35
        Z | VATEX-SA-36
        Z | VATEX-SA-EDU
        Z | VATEX-SA-HEA
        Z | VATEX-SA-MLTRY
        O | VATEX-SA-OOS
        ```
    - **Create four custom fields for transactions:**
       - **Invoice Sub Type**
       - **Payment Means**
       - **Instruction Note** (Skip Sales Invoice)
       - **QR Code** (Type: Paragraph text)

3. **`Edit` each custom field to get its `GUID`.**



## Footer Script for Sales Documents

Add the following script to the footer to generate and insert a QR code into the printed document:

```html
<script>
    document.addEventListener('DOMContentLoaded', () => {
        var qrCodeContent = '%%QR Code%%';
        
        if (qrCodeContent != '-') {
            var mainTable = document.querySelector('#printable-content > table');
            if (mainTable) {
                var newRow = mainTable.insertRow();
                var newCell = newRow.insertCell();
                newCell.colSpan = 99;
                newCell.innerHTML += '<div id="signedQrCode" style="padding: 20px"></div>';
            }
    
            new QRCode(document.getElementById("signedQrCode"), {
                text: qrCodeContent,
                width: 160,
                height: 160,
                colorDark: "#000000",
                colorLight: "#fafafa",
                correctLevel: QRCode.CorrectLevel.L
            });
    
            var qrcodeDiv = document.getElementById('qrcode');
            if (qrcodeDiv) {
                qrcodeDiv.remove();
            }
        }
    });
</script>
```


### Add PartyTaxInfo for All Customer and Supplier

Add the following information for each Customer and Supplier:

```json
"StreetName": "صلاح الدين | Salah Al-DinX",
"BuildingNumber": "1111",
"CitySubdivisionName": "المروج | Al-Murooj",
"CityName": "الرياض | Riyadh",
"PostalZone": "12222",
"CountryIdentificationCode": "SA",
"TaxSchemeCompanyID": "399999999800003",
"TaxSchemeID": "VAT",
"RegistrationName": "شركة نماذج فاتورة المحدودة | Fatoora Samples LTD"
```

### Transaction Form Default Setup
1. Checklist and Add Footer for QR Code.
2. Checklist and Add Relay URL: http://localhost:4455/process-form.
3. Add Other Settings as Needed.

## ZatcaWebServices Application Setup
1. Download Latest Release from GitHub.
2. Extract to Desired Folder.
3. Run ZatcaService.exe and Open http://localhost:4455 in Browser.
4. Populate Gateway Setting with Data from Previous Steps and Save Settings.

ZatcaWebServices ready to serve Manager for Invoicing


If you encounter any shortcomings or issues with the web API that need to be added or fixed, you can ask questions or provide feedback through the discussion section of this project. 
Your feedback and suggestions are highly appreciated to help improve the quality and functionality of this project.


