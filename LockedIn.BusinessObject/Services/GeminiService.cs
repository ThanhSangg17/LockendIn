using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LockedIn.BusinessObject.DTOs.MealPlans;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.BusinessObject.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(string JsonContent, int TokensUsed)> GenerateMealPlanJsonAsync(GenerateMealPlanRequest request)
    {
        _logger.LogInformation("Bắt đầu gọi AI để sinh thực đơn.");

        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("Thiếu cấu hình Gemini:ApiKey.");
            throw new InvalidOperationException("Cấu hình API Key của Gemini bị thiếu.");
        }

        var model = _configuration["Gemini:Model"];
        if (string.IsNullOrWhiteSpace(model))
        {
            model = "gemini-3.1-flash-lite";
        }

        var prompt = $@"Bạn là một chuyên gia dinh dưỡng. Hãy tạo một thực đơn (meal plan) thân thiện với người Việt Nam dựa trên các thông tin sau:
- Mục tiêu (Goal): {request.Goal}
- Sở thích/Yêu cầu (Preference): {request.Preference ?? "Không có"}
- Lưu ý dị ứng (Allergy Note): {request.AllergyNote ?? "Không có"}

Yêu cầu bắt buộc:
1. Các món ăn phải phổ biến, dễ tìm, phù hợp với văn hóa ẩm thực và thói quen ăn uống của người Việt Nam.
2. Lượng calo và các chất dinh dưỡng (protein, carb, fat) phải phù hợp với mục tiêu đã đề ra.
3. Return valid JSON only.
4. No markdown.
5. No explanation.

Desired JSON Schema:
{{
  ""dailyCalories"": number,
  ""proteinGrams"": number,
  ""carbGrams"": number,
  ""fatGrams"": number,
  ""days"": [
    {{
      ""day"": 1,
      ""meals"": [
        {{
          ""mealType"": ""Breakfast"",
          ""foods"": [
            {{
              ""name"": ""string"",
              ""quantity"": ""string"",
              ""calories"": number,
              ""protein"": number,
              ""carbs"": number,
              ""fat"": number
            }}
          ]
        }}
      ]
    }}
  ]
}}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Do not log the full request URL because the API key is in the query string. Log only the model name.
        _logger.LogInformation("Sending POST request to Gemini API using model: {Model}", model);

        HttpResponseMessage response;
        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            response = await _httpClient.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi kết nối khi gọi Gemini API.");
            throw new Exception("Không thể kết nối đến Gemini API. Chi tiết: " + ex.Message);
        }

        // Log only model name and status code
        _logger.LogInformation("Gemini response status code: {StatusCode} for model: {Model}", response.StatusCode, model);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API call failed with status code: {StatusCode}", response.StatusCode);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new Exception($"Lỗi xác thực với Gemini API (Status Code: {response.StatusCode}). Vui lòng kiểm tra lại API Key.");
            }
            if ((int)response.StatusCode == 429)
            {
                throw new Exception("Yêu cầu quá tải (Rate limit exceeded) với Gemini API. Vui lòng thử lại sau.");
            }
            throw new Exception($"Gemini API trả về lỗi hệ thống (Status Code: {response.StatusCode}).");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        
        using var responseDoc = JsonDocument.Parse(responseBody);
        var root = responseDoc.RootElement;

        // Extract candidates[0].content.parts[0].text
        string? rawText = null;
        if (root.TryGetProperty("candidates", out var candidates) && 
            candidates.ValueKind == JsonValueKind.Array && 
            candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var contentElement) && 
                contentElement.TryGetProperty("parts", out var parts) && 
                parts.ValueKind == JsonValueKind.Array && 
                parts.GetArrayLength() > 0)
            {
                rawText = parts[0].GetProperty("text").GetString();
            }
        }

        if (string.IsNullOrWhiteSpace(rawText))
        {
            _logger.LogError("Không tìm thấy nội dung phản hồi từ Gemini.");
            throw new Exception("Gemini không trả về kết quả thực đơn.");
        }

        // Strip markdown fence (```json / ```) if present
        rawText = rawText.Trim();
        if (rawText.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            rawText = rawText.Substring(7);
        }
        else if (rawText.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            rawText = rawText.Substring(3);
        }

        if (rawText.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            rawText = rawText.Substring(0, rawText.Length - 3);
        }
        rawText = rawText.Trim();

        // Extract token usage safely
        int tokensUsed = 0;
        if (root.TryGetProperty("usageMetadata", out var usageMetadata))
        {
            if (usageMetadata.TryGetProperty("totalTokenCount", out var totalTokens))
            {
                tokensUsed = totalTokens.GetInt32();
            }
        }

        return (rawText, tokensUsed);
    }
}
