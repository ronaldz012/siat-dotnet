namespace MiEmpresa.Siat.Sdk.Tests;

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Operations;
using MiEmpresa.Siat.Sdk.Options;
using Xunit;

public class ObtenerCuisOperationTests
{
    private const string CuisExitosoXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <ns:cuisResponse xmlns:ns="https://siat.impuestos.gob.bo/">
              <RespuestaCuis>
                <codigo>1234567890</codigo>
                <fechaVigencia>2025-12-31T23:59:59</fechaVigencia>
                <codigoControl>ABCDEF123456</codigoControl>
                <transaccion>true</transaccion>
                <mensajesList>
                  <codigo>901</codigo>
                  <descripcion>CUIS generado correctamente</descripcion>
                </mensajesList>
              </RespuestaCuis>
            </ns:cuisResponse>
          </soap:Body>
        </soap:Envelope>
        """;

    private const string CuisConErrorXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <ns:cuisResponse xmlns:ns="https://siat.impuestos.gob.bo/">
              <RespuestaCuis>
                <transaccion>false</transaccion>
                <mensajesList>
                  <codigo>1001</codigo>
                  <descripcion>Token inválido</descripcion>
                </mensajesList>
              </RespuestaCuis>
            </ns:cuisResponse>
          </soap:Body>
        </soap:Envelope>
        """;

    private const string CuisConNamespacePorDefectoXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <cuisResponse xmlns="https://siat.impuestos.gob.bo/">
              <RespuestaCuis>
                <codigo>987654321</codigo>
                <fechaVigencia>2027-03-01T00:00:00</fechaVigencia>
                <transaccion>true</transaccion>
              </RespuestaCuis>
            </cuisResponse>
          </soap:Body>
        </soap:Envelope>
        """;

    private const string CuisConOffsetXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <ns:cuisResponse xmlns:ns="https://siat.impuestos.gob.bo/">
              <RespuestaCuis>
                <codigo>OFFSET1</codigo>
                <fechaVigencia>2027-11-17T15:58:05.619-04:00</fechaVigencia>
                <transaccion>true</transaccion>
              </RespuestaCuis>
            </ns:cuisResponse>
          </soap:Body>
        </soap:Envelope>
        """;

    private const string FaultXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <soap:Fault>
              <faultcode>soap:Server</faultcode>
              <faultstring>No se pudo procesar la solicitud</faultstring>
            </soap:Fault>
          </soap:Body>
        </soap:Envelope>
        """;

    [Fact]
    public async Task ObtenerCuisAsync_RespuestaExitosa_RetornaCuisValido()
    {
        var (client, _) = Crear(CuisExitosoXml);

        var resultado = await Ejecutar(client, "token-de-prueba");

        Assert.Equal("1234567890", resultado.Codigo);
        Assert.Equal(new DateTime(2025, 12, 31, 23, 59, 59), resultado.FechaVigencia);
        Assert.Equal("ABCDEF123456", resultado.CodigoControl);
        Assert.True(resultado.Transaccion);
        Assert.True(resultado.Exitoso);
        Assert.Single(resultado.Mensajes);
        Assert.Equal(901, resultado.Mensajes[0].Codigo);
        Assert.Equal("CUIS generado correctamente", resultado.Mensajes[0].Descripcion);
        Assert.False(resultado.Mensajes[0].EsError);
    }

    [Fact]
    public async Task ObtenerCuisAsync_RespuestaConError_RetornaTransaccionFalse()
    {
        var (client, _) = Crear(CuisConErrorXml);

        var resultado = await Ejecutar(client, "token-invalido");

        Assert.False(resultado.Transaccion);
        Assert.False(resultado.Exitoso);
        Assert.Single(resultado.Mensajes);
        Assert.Equal(1001, resultado.Mensajes[0].Codigo);
        Assert.Equal("Token inválido", resultado.Mensajes[0].Descripcion);
        Assert.True(resultado.Mensajes[0].EsError);
    }

    [Fact]
    public async Task ObtenerCuisAsync_RespuestaConNamespacePorDefecto_TambienParsea()
    {
        var (client, _) = Crear(CuisConNamespacePorDefectoXml);

        var resultado = await Ejecutar(client, "token");

        Assert.Equal("987654321", resultado.Codigo);
        Assert.True(resultado.Transaccion);
        Assert.True(resultado.Exitoso);
    }

    [Fact]
    public async Task ObtenerCuisAsync_FechaConOffset_SeNormalizaAUtc()
    {
        var (client, _) = Crear(CuisConOffsetXml);

        var resultado = await Ejecutar(client, "token");

        Assert.Equal(DateTimeKind.Utc, resultado.FechaVigencia.Kind);
        Assert.Equal(new DateTime(2027, 11, 17, 19, 58, 5, 619, DateTimeKind.Utc), resultado.FechaVigencia);
    }

    [Fact]
    public async Task ObtenerCuisAsync_TransaccionFalse_LogueaElCuerpoCrudoDelSin()
    {
        var logger = new CapturingLogger<SiatHttpClient>();
        var (client, _) = Crear(CuisConErrorXml, logger: logger);

        await Ejecutar(client, "token");

        Assert.Contains(logger.Messages,
            m => m.Contains("transaccion=false") && m.Contains("<RespuestaCuis>"));
    }

    [Fact]
    public async Task ObtenerCuisAsync_ErrorHttp_LanzaSiatCommunicationException()
    {
        var (client, _) = Crear("", HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsAsync<MiEmpresa.Siat.Sdk.Exceptions.SiatCommunicationException>(
            () => Ejecutar(client, "token"));

        Assert.Equal((int)HttpStatusCode.InternalServerError, ex.HttpStatusCode);
        Assert.Contains("HTTP 500", ex.Message);
    }

    [Fact]
    public async Task ObtenerCuisAsync_SoapFault_LanzaExcepcionConElMensajeDelSin()
    {
        var (client, _) = Crear(FaultXml, HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsAsync<MiEmpresa.Siat.Sdk.Exceptions.SiatCommunicationException>(
            () => Ejecutar(client, "token"));

        Assert.Equal("soap:Server", ex.SoapFaultCode);
        Assert.Equal("No se pudo procesar la solicitud", ex.SoapFaultString);
        Assert.Contains("No se pudo procesar la solicitud", ex.Message);
    }

    [Fact]
    public async Task ObtenerCuisAsync_EnviaElElementoDeLaOperacionSinWrapperPropio()
    {
        var (client, handler) = Crear(CuisExitosoXml);

        await Ejecutar(client, "token");

        var body = handler.LastRequestBody!;
        Assert.Contains("<ns:cuis", body);
        Assert.Contains("<SolicitudCuis>", body);
        Assert.Contains("<codigoSistema>TEST-SISTEMA</codigoSistema>", body);
        Assert.DoesNotContain("<Content", body);
        Assert.Equal(
            "https://pilotosiatservicios.impuestos.gob.bo/v2/FacturacionCodigos",
            handler.LastRequestUri!.ToString());
    }

    private static (SiatHttpClient Client, MockHttpMessageHandler Handler) Crear(
        string responseXml,
        HttpStatusCode status = HttpStatusCode.OK,
        ILogger<SiatHttpClient>? logger = null)
    {
        var handler = new MockHttpMessageHandler(responseXml, status);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new SiatOptions { TimeoutSeconds = 30 });
        var client = new SiatHttpClient(httpClient, options, logger ?? NullLogger<SiatHttpClient>.Instance);
        return (client, handler);
    }

    private static Task<MiEmpresa.Siat.Sdk.Models.Responses.CuisResponse> Ejecutar(
        SiatHttpClient client, string token)
    {
        var operation = new ObtenerCuisOperation(client);

        var request = new CuisRequest
        {
            Nit = 123456789,
            CodigoSistema = "TEST-SISTEMA",
            Ambiente = Ambiente.Pruebas,
            Modalidad = Modalidad.ComputarizadaEnLinea,
            CodigoSucursal = 0,
            CodigoPuntoVenta = 0
        };

        return operation.EjecutarAsync(request, token, CancellationToken.None);
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));
    }
}
