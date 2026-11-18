namespace MiEmpresa.Siat.Sdk.Tests;

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Operations;
using MiEmpresa.Siat.Sdk.Options;
using Xunit;

public class ObtenerCuisOperationTests
{
private const string CuisExitosoXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RespuestaCuis xmlns=""https://siat.impuestos.gob.bo/"">
  <codigo>1234567890</codigo>
  <fechaVigencia>2025-12-31T23:59:59</fechaVigencia>
  <codigoControl>ABCDEF123456</codigoControl>
  <transaccion>true</transaccion>
  <mensajesList>
    <mensaje>
      <codigo>901</codigo>
      <descripcion>CUIS generado correctamente</descripcion>
    </mensaje>
  </mensajesList>
</RespuestaCuis>";

private const string CuisConErrorXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<RespuestaCuis xmlns=""https://siat.impuestos.gob.bo/"">
  <transaccion>false</transaccion>
  <mensajesList>
    <mensaje>
      <codigo>1001</codigo>
      <descripcion>Token inválido</descripcion>
    </mensaje>
  </mensajesList>
</RespuestaCuis>";

    [Fact]
    public async Task ObtenerCuisAsync_RespuestaExitosa_RetornaCuisValido()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(CuisExitosoXml);
        var httpClient = new HttpClient(handler);
        
        var options = Options.Create(new SiatOptions
        {
            DefaultAmbiente = "Pruebas",
            TimeoutSeconds = 30
        });
        
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<SiatHttpClient>();
        
        var siatClient = new SiatHttpClient(httpClient, options, logger);
        var operation = new ObtenerCuisOperation(siatClient);
        
        var request = new CuisRequest
        {
            Nit = 123456789,
            CodigoSistema = "TEST-SISTEMA",
            Ambiente = MiEmpresa.Siat.Sdk.Models.Ambiente.Pruebas,
            Modalidad = MiEmpresa.Siat.Sdk.Models.Modalidad.ComputarizadaEnLinea,
            CodigoSucursal = 0,
            CodigoPuntoVenta = 0
        };

        // Act
        var resultado = await operation.EjecutarAsync(request, "token-de-prueba", CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
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
        // Arrange
        var handler = new MockHttpMessageHandler(CuisConErrorXml);
        var httpClient = new HttpClient(handler);
        
        var options = Options.Create(new SiatOptions
        {
            DefaultAmbiente = "Pruebas",
            TimeoutSeconds = 30
        });
        
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<SiatHttpClient>();
        
        var siatClient = new SiatHttpClient(httpClient, options, logger);
        var operation = new ObtenerCuisOperation(siatClient);
        
        var request = new CuisRequest
        {
            Nit = 123456789,
            CodigoSistema = "TEST-SISTEMA",
            Ambiente = MiEmpresa.Siat.Sdk.Models.Ambiente.Pruebas,
            Modalidad = MiEmpresa.Siat.Sdk.Models.Modalidad.ComputarizadaEnLinea
        };

        // Act
        var resultado = await operation.EjecutarAsync(request, "token-invalido", CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.False(resultado.Transaccion);
        Assert.False(resultado.Exitoso);
        Assert.Single(resultado.Mensajes);
        Assert.Equal(1001, resultado.Mensajes[0].Codigo);
        Assert.Equal("Token inválido", resultado.Mensajes[0].Descripcion);
        Assert.True(resultado.Mensajes[0].EsError);
    }

    [Fact]
    public async Task ObtenerCuisAsync_ErrorHttp_LanzaSiatCommunicationException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler);
        
        var options = Options.Create(new SiatOptions
        {
            DefaultAmbiente = "Pruebas",
            TimeoutSeconds = 30
        });
        
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<SiatHttpClient>();
        
        var siatClient = new SiatHttpClient(httpClient, options, logger);
        var operation = new ObtenerCuisOperation(siatClient);
        
        var request = new CuisRequest
        {
            Nit = 123456789,
            CodigoSistema = "TEST-SISTEMA",
            Ambiente = MiEmpresa.Siat.Sdk.Models.Ambiente.Pruebas,
            Modalidad = MiEmpresa.Siat.Sdk.Models.Modalidad.ComputarizadaEnLinea
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<MiEmpresa.Siat.Sdk.Exceptions.SiatCommunicationException>(
            () => operation.EjecutarAsync(request, "token", CancellationToken.None));
        
        Assert.Equal((int)HttpStatusCode.InternalServerError, ex.HttpStatusCode);
    }
}