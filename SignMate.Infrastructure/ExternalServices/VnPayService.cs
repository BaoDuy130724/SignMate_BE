using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SignMate.Application.Interfaces;

namespace SignMate.Infrastructure.ExternalServices;

public class VnPayService : IVnPayService
{
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _baseUrl;
    private readonly string _version = "2.1.0";

    public VnPayService(IConfiguration config)
    {
        _tmnCode = config["VnPay:TmnCode"]
            ?? throw new Exception("Missing VnPay:TmnCode");

        _hashSecret = config["VnPay:HashSecret"]
            ?? throw new Exception("Missing VnPay:HashSecret");

        _baseUrl = config["VnPay:BaseUrl"]
            ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    }

    public string CreatePaymentUrl(VnPayPaymentRequest request, string ipAddress)
    {
        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            { "vnp_Version", _version },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _tmnCode },
            { "vnp_Amount", ((long)(request.Amount * 100)).ToString() },
            { "vnp_CreateDate", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", ipAddress },
            { "vnp_Locale", "vn" },
            { "vnp_OrderInfo", request.OrderInfo },
            { "vnp_OrderType", "billpayment" },
            { "vnp_ReturnUrl", request.ReturnUrl },
            { "vnp_TxnRef", request.OrderId.ToString("N").ToUpper() },
            { "vnp_ExpireDate", DateTime.UtcNow.AddHours(7).AddMinutes(15).ToString("yyyyMMddHHmmss") },
        };

        // === KEY FIX: VNPay hashes the URL-encoded query string ===
        // Both signData and queryString must be IDENTICAL (both URL-encoded)
        var data = new StringBuilder();
        foreach (var kv in vnpParams.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            if (data.Length > 0) data.Append('&');
            data.Append(WebUtility.UrlEncode(kv.Key));
            data.Append('=');
            data.Append(WebUtility.UrlEncode(kv.Value));
        }

        var queryString = data.ToString();
        var secureHash = HmacSha512(_hashSecret, queryString);

        var finalUrl = $"{_baseUrl}?{queryString}&vnp_SecureHash={secureHash}";

        try
        {
            System.IO.File.AppendAllText("vnpay_debug.log",
                $"\n[{DateTime.Now}] CreatePaymentUrl:\n- QueryString (= SignData):\n{queryString}\n- SecureHash:\n{secureHash}\n- FinalURL:\n{finalUrl}\n");
        }
        catch { }

        return finalUrl;
    }

    public VnPayCallbackResult ValidateCallback(IDictionary<string, string> vnpayParams)
    {
        var result = new VnPayCallbackResult();

        if (!vnpayParams.TryGetValue("vnp_SecureHash", out var receivedHash))
            return result;

        // Remove hash fields for validation
        var checkParams = new SortedDictionary<string, string>(
            vnpayParams
                .Where(kv => kv.Key.StartsWith("vnp_") &&
                             kv.Key != "vnp_SecureHash" &&
                             kv.Key != "vnp_SecureHashType")
                .ToDictionary(kv => kv.Key, kv => kv.Value),
            StringComparer.Ordinal
        );

        // Build sign data with URL-encoded values (same as CreatePaymentUrl)
        var data = new StringBuilder();
        foreach (var kv in checkParams.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            if (data.Length > 0) data.Append('&');
            data.Append(WebUtility.UrlEncode(kv.Key));
            data.Append('=');
            data.Append(WebUtility.UrlEncode(kv.Value));
        }

        var computedHash = HmacSha512(_hashSecret, data.ToString());

        result.IsValid = string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);

        if (result.IsValid)
        {
            result.ResponseCode = vnpayParams.TryGetValue("vnp_ResponseCode", out var rc) ? rc : "";
            result.TransactionId = vnpayParams.TryGetValue("vnp_TransactionNo", out var tn) ? tn : "";
            result.IsSuccess = result.ResponseCode == "00";
        }

        return result;
    }

    private static string HmacSha512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);

        var sb = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
            sb.Append(b.ToString("x2"));    // lowercase hex — matches VNPay demo
        return sb.ToString();
    }
}
