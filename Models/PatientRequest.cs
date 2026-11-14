using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MedicalApi.Models;

public class PatientRequest
{
    [Required(ErrorMessage = "Укажите имя пациента.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя должно содержать от 2 до 100 символов.")]
    public string Name { get; set; } = string.Empty;

    [JsonConverter(typeof(BirthDateJsonConverter))]
    public DateOnly BirthDate { get; set; }

    [Phone(ErrorMessage = "Укажите телефон в корректном формате.")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Укажите корректный email.")]
    public string? Email { get; set; }

    [StringLength(10, ErrorMessage = "Группа крови слишком длинная.")]
    public string? BloodType { get; set; }
}

public sealed class BirthDateJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && reader.TryGetDateTime(out var value))
            return DateOnly.FromDateTime(value);

        throw new JsonException("Укажите дату рождения в формате ГГГГ-ММ-ДД.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
