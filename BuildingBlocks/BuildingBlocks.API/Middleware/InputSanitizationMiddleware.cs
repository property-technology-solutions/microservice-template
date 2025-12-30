using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.API.Middleware;

/// <summary>
/// Middleware that sanitizes input data to prevent XSS attacks.
/// Processes JSON request bodies and encodes potentially dangerous HTML characters.
/// </summary>
public partial class InputSanitizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputSanitizationMiddleware> _logger;
    private readonly InputSanitizationOptions _options;

    public InputSanitizationMiddleware(
        RequestDelegate next,
        ILogger<InputSanitizationMiddleware> logger,
        InputSanitizationOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new InputSanitizationOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only process requests with JSON content
        if (context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true &&
            context.Request.ContentLength > 0)
        {
            try
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 4096,
                    leaveOpen: true);

                var originalBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(originalBody))
                {
                    var sanitizedBody = SanitizeJson(originalBody);

                    if (originalBody != sanitizedBody)
                    {
                        _logger.LogWarning("Input was sanitized for potential XSS. Path: {Path}", context.Request.Path);

                        var sanitizedBytes = Encoding.UTF8.GetBytes(sanitizedBody);
                        context.Request.Body = new MemoryStream(sanitizedBytes);
                        context.Request.ContentLength = sanitizedBytes.Length;
                    }
                    else
                    {
                        context.Request.Body.Position = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during input sanitization");
                context.Request.Body.Position = 0;
            }
        }

        await _next(context);
    }

    private string SanitizeJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = false
            });

            SanitizeElement(document.RootElement, writer);
            writer.Flush();

            return Encoding.UTF8.GetString(stream.ToArray());
        }
        catch (JsonException)
        {
            // If not valid JSON, return original
            return json;
        }
    }

    private void SanitizeElement(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);
                    SanitizeElement(property.Value, writer);
                }
                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    SanitizeElement(item, writer);
                }
                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                var value = element.GetString();
                var sanitizedValue = SanitizeString(value);
                writer.WriteStringValue(sanitizedValue);
                break;

            case JsonValueKind.Number:
                if (element.TryGetInt64(out var longValue))
                    writer.WriteNumberValue(longValue);
                else if (element.TryGetDouble(out var doubleValue))
                    writer.WriteNumberValue(doubleValue);
                else
                    writer.WriteRawValue(element.GetRawText());
                break;

            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;

            default:
                writer.WriteRawValue(element.GetRawText());
                break;
        }
    }

    private string? SanitizeString(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sanitized = input;

        // Remove potentially dangerous script patterns
        if (_options.RemoveScriptTags)
        {
            sanitized = ScriptTagRegex().Replace(sanitized, string.Empty);
            sanitized = EventHandlerRegex().Replace(sanitized, string.Empty);
        }

        // HTML encode dangerous characters
        if (_options.HtmlEncode)
        {
            sanitized = HttpUtility.HtmlEncode(sanitized);
        }

        // Remove null bytes
        sanitized = sanitized.Replace("\0", string.Empty);

        return sanitized;
    }

    [GeneratedRegex(@"<script[^>]*>[\s\S]*?</script>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptTagRegex();

    [GeneratedRegex(@"on\w+\s*=\s*(['""]).*?\1", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerRegex();
}

/// <summary>
/// Configuration options for input sanitization.
/// </summary>
public class InputSanitizationOptions
{
    /// <summary>
    /// Whether to remove script tags from input.
    /// Default: true
    /// </summary>
    public bool RemoveScriptTags { get; set; } = true;

    /// <summary>
    /// Whether to HTML encode special characters.
    /// Default: true
    /// </summary>
    public bool HtmlEncode { get; set; } = true;
}

/// <summary>
/// Attribute to allow raw HTML in specific properties.
/// Use with caution - only for trusted input.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AllowHtmlAttribute : Attribute
{
}

