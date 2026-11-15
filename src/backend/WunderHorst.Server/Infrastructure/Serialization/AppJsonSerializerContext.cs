using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace WunderHorst.Server.Infrastructure.Serialization;

/// <summary>
/// Source-generated JSON serializer context for Native AOT compatibility.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ProblemDetails))]
public sealed partial class AppJsonSerializerContext : JsonSerializerContext;
