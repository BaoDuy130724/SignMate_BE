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
        _tmnCode = config["VnPay:TmnCode"] ?? "CGXZLS0Z";
        _hashSecret = config["VnPay:HashSecret"] ?? "XNBCJFAKAZQSGTARRLGCHVZWCIOIGSHN";
        _baseUrl = config["VnPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
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

        var queryString = string.Join("&",
            vnpParams
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{UrlEncode(kv.Key)}={UrlEncode(kv.Value)}"));

        var signData = queryString;

        var secureHash = HmacSha512(_hashSecret, signData);

        var finalUrl = $"{_baseUrl}?{queryString}&vnp_SecureHash={secureHash}";

        try {
            System.IO.File.AppendAllText("vnpay_debug.log", 
                $"\n[{DateTime.Now}] CreatePaymentUrl:\n- TmnCode: {_tmnCode}\n- HashSecret: {_hashSecret}\n- SignData:\n{signData}\n- SecureHash:\n{secureHash}\n- FinalURL:\n{finalUrl}\n");
        } catch {}

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

        var signData = string.Join("&",
            checkParams
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{UrlEncode(kv.Key)}={UrlEncode(kv.Value)}"));

        var computedHash = HmacSha512(_hashSecret, signData);

        result.IsValid = string.Equals(computedHash, receivedHash, StringComparison.OrdinalIgnoreCase);

        if (result.IsValid)
        {
            result.ResponseCode = vnpayParams.TryGetValue("vnp_ResponseCode", out var rc) ? rc : "";
            result.TransactionId = vnpayParams.TryGetValue("vnp_TransactionNo", out var tn) ? tn : "";
            result.IsSuccess = result.ResponseCode == "00";

            // Parse OrderId from TxnRef
            var txnRef = vnpayParams.TryGetValue("vnp_TxnRef", out var tr) ? tr : "";
            // We stored OrderId as first 16 chars of Guid.ToString("N")
            // We need to look up by this reference in the controller
        }

        return result;
    }

    private static string HmacSha512(string key, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA512(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private static string UrlEncode(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        
        // C#'s WebUtility.UrlEncode behaves differently than Java's URLEncoder.
        // We must mimic Java's URLEncoder.encode exactly to pass VNPAY HMAC-SHA512.
        // Java URLEncoder encodes space to +, which VNPAY replaces with %20.
        // Java encodes (, ), and ~ but leaves * unencoded.
        return WebUtility.UrlEncode(value)
            .Replace("+", "%20")
            .Replace("(", "%28")
            .Replace(")", "%29")
            .Replace("!", "%21")
            .Replace("*", "%2A")
            .Replace("~", "%7E");
    }
}
