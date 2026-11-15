using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WunderHorst.Server.Features.Auth.GetCurrentUser;
using WunderHorst.Server.Features.Auth.Login;
using WunderHorst.Server.Features.Auth.SetupAdmin;
using WunderHorst.Server.Features.System.GetHealth;

namespace WunderHorst.Server.Infrastructure.Serialization;

/// <summary>
/// Source-generated JSON serializer context for Native AOT compatibility.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
[JsonSerializable(typeof(GetHealthResponse))]
[JsonSerializable(typeof(SetupAdminRequest))]
[JsonSerializable(typeof(SetupAdminResponse))]
[JsonSerializable(typeof(SetupStatusResponse))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(CurrentUserResponse))]
public sealed partial class AppJsonSerializerContext : JsonSerializerContext;
