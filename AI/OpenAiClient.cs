using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AI_CAD_ENGINEER.AI;

public class OpenAiClient
{
    private const string ResponsesEndpoint =
        "https://api.openai.com/v1/responses";

    private const string ModelName =
        "gpt-5.6";

    private readonly HttpClient
        _httpClient;

    private readonly string
        _apiKey;

    public OpenAiClient()
    {
        _apiKey =
            Environment.GetEnvironmentVariable(
                "OPENAI_API_KEY")
            ?? throw new InvalidOperationException(
                "Не найдена переменная OPENAI_API_KEY.");

        _httpClient =
            new HttpClient
            {
                Timeout =
                    TimeSpan.FromMinutes(
                        2)
            };
    }

    public async Task<string> SendAsync(
        string prompt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            prompt);

        using HttpRequestMessage request =
            new(
                HttpMethod.Post,
                ResponsesEndpoint);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _apiKey);

        object requestBody =
            new
            {
                model =
                    ModelName,

                input =
                    prompt
            };

        string requestJson =
            JsonSerializer.Serialize(
                requestBody);

        request.Content =
            new StringContent(
                requestJson,
                Encoding.UTF8,
                "application/json");

        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request);

        string responseJson =
            await response.Content
                .ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"OpenAI API вернул ошибку " +
                $"{(int)response.StatusCode} " +
                $"({response.StatusCode})." +
                Environment.NewLine +
                responseJson);
        }

        return ExtractOutputText(
            responseJson);
    }

    private static string ExtractOutputText(
        string responseJson)
    {
        using JsonDocument document =
            JsonDocument.Parse(
                responseJson);

        JsonElement root =
            document.RootElement;

        if (root.TryGetProperty(
                "output_text",
                out JsonElement outputTextElement))
        {
            string? outputText =
                outputTextElement.GetString();

            if (!string.IsNullOrWhiteSpace(
                    outputText))
            {
                return outputText;
            }
        }

        if (root.TryGetProperty(
                "output",
                out JsonElement outputElement) &&
            outputElement.ValueKind ==
            JsonValueKind.Array)
        {
            foreach (JsonElement outputItem
                     in outputElement.EnumerateArray())
            {
                if (!outputItem.TryGetProperty(
                        "content",
                        out JsonElement contentElement) ||
                    contentElement.ValueKind !=
                    JsonValueKind.Array)
                {
                    continue;
                }

                foreach (JsonElement contentItem
                         in contentElement.EnumerateArray())
                {
                    if (!contentItem.TryGetProperty(
                            "text",
                            out JsonElement textElement))
                    {
                        continue;
                    }

                    string? text =
                        textElement.GetString();

                    if (!string.IsNullOrWhiteSpace(
                            text))
                    {
                        return text;
                    }
                }
            }
        }

        return responseJson;
    }
}