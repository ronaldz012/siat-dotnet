namespace MiEmpresa.Siat.Sdk.Clients;

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiEmpresa.Siat.Sdk.Models;
using MiEmpresa.Siat.Sdk.Options;
using MiEmpresa.Siat.Sdk.Exceptions;
using MiEmpresa.Siat.Sdk.Xml;

public interface ISiatClient
{
    Task<TResponse> PostSoapAsync<TRequest, TResponse>(
        string serviceName,
        TRequest request,
        string token,
        Ambiente ambiente,
        CancellationToken ct)
        where TRequest : class
        where TResponse : class;

    Task<TResponse> PostSoapXmlAsync<TResponse>(
        string serviceName,
        string xml,
        string token,
        Ambiente ambiente,
        bool payloadOnly,
        CancellationToken ct)
        where TResponse : class;
}

public sealed class SiatHttpClient : ISiatClient
{
    /// <summary>El cuerpo de un fault se loguea truncado: no queremos volcar respuestas enormes.</summary>
    private const int MaxLoggedBodyLength = 2000;

    private readonly HttpClient _httpClient;
    private readonly SiatOptions _options;
    private readonly ILogger<SiatHttpClient>? _logger;

    public SiatHttpClient(
        HttpClient httpClient,
        IOptions<SiatOptions> options,
        ILogger<SiatHttpClient>? logger = null)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task<TResponse> PostSoapAsync<TRequest, TResponse>(
        string serviceName,
        TRequest request,
        string token,
        Ambiente ambiente,
        CancellationToken ct)
        where TRequest : class
        where TResponse : class
        => PostSoapXmlAsync<TResponse>(
            serviceName,
            SoapEnvelopeBuilder.BuildEnvelope(request),
            token,
            ambiente,
            payloadOnly: false,
            ct);

    public async Task<TResponse> PostSoapXmlAsync<TResponse>(
        string serviceName,
        string xml,
        string token,
        Ambiente ambiente,
        bool payloadOnly,
        CancellationToken ct)
        where TResponse : class
    {
        // El ambiente viene por parámetro (el SDK es stateless): decide la URL base.
        var baseUrl = _options.GetBaseUrl(ambiente == Ambiente.Produccion ? "Produccion" : "Pruebas");
        var url = $"{baseUrl}/{serviceName}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(xml, Encoding.UTF8, "application/xml")
        };

        httpRequest.Headers.Add("apiKey", $"TokenApi {token}");
        httpRequest.Headers.UserAgent.ParseAdd(_options.UserAgent);

        _logger?.LogDebug("Enviando SOAP a {Url}: {Xml}", url, xml);

        var response = await _httpClient.SendAsync(httpRequest, ct);
        var responseXml = await response.Content.ReadAsStringAsync(ct);

        var fault = SoapEnvelopeBuilder.TryReadFault(responseXml);

        if (!response.IsSuccessStatusCode || fault is not null)
        {
            // El SIN explica el rechazo en el SOAP Fault; lo dejamos en el log para soporte.
            _logger?.LogWarning(
                "El SIN rechazó {Servicio} (HTTP {Status}) en {Url}. Fault: {Fault}. Respuesta: {Respuesta}",
                serviceName,
                (int)response.StatusCode,
                url,
                fault?.ToString() ?? "(sin fault)",
                Truncate(responseXml));

            throw new SiatCommunicationException(
                fault is null
                    ? $"El SIN respondió HTTP {(int)response.StatusCode} al llamar {serviceName}."
                    : $"El SIN rechazó {serviceName}: {fault}",
                httpStatusCode: (int)response.StatusCode,
                soapFaultCode: fault?.Code,
                soapFaultString: fault?.Message);
        }

        _logger?.LogDebug("Respuesta SOAP de {Url}: {Xml}", url, responseXml);

        if (SoapEnvelopeBuilder.IsUnsuccessful(responseXml))
        {
            // Rechazo de negocio del SIN (HTTP 200, transaccion=false): el cuerpo crudo
            // es lo único que explica el motivo, así que va al log visible.
            _logger?.LogWarning(
                "El SIN respondió {Servicio} con transaccion=false en {Url}. Respuesta: {Respuesta}",
                serviceName,
                url,
                Truncate(responseXml));
        }

        var innerXml = payloadOnly
            ? SoapEnvelopeBuilder.ExtractPayloadContent(responseXml)
            : SoapEnvelopeBuilder.ExtractBodyContent(responseXml);

        var serializer = new XmlSerializer(typeof(TResponse));
        using var stringReader = new StringReader(innerXml);
        using var xmlReader = XmlReader.Create(stringReader);
        var obj = serializer.Deserialize(xmlReader);
        return obj is TResponse parsed
            ? parsed
            : throw new SiatXmlException($"La respuesta de {serviceName} no se pudo interpretar como {typeof(TResponse).Name}.");
    }

    private static string Truncate(string value) =>
        value.Length <= MaxLoggedBodyLength ? value : value[..MaxLoggedBodyLength] + "…";
}
