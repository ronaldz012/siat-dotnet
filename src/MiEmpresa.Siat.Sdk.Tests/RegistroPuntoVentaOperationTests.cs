namespace MiEmpresa.Siat.Sdk.Tests;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Operations;
using MiEmpresa.Siat.Sdk.Options;
using Xunit;

public class RegistroPuntoVentaOperationTests
{
    [Fact]
    public async Task Registrar_ParseaElCodigoAsignado()
    {
        var (operation, handler) = Crear("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:registroPuntoVentaResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaRegistroPuntoVenta>
                    <codigoPuntoVenta>7</codigoPuntoVenta>
                    <transaccion>true</transaccion>
                  </RespuestaRegistroPuntoVenta>
                </ns2:registroPuntoVentaResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.EjecutarAsync(Solicitud(), "token", CancellationToken.None);

        Assert.True(resultado.Exitoso);
        Assert.Equal(7, resultado.CodigoPuntoVenta);

        Assert.Contains("<ns:registroPuntoVenta>", handler.LastRequestBody);
        Assert.Contains("<SolicitudRegistroPuntoVenta>", handler.LastRequestBody);
        Assert.Contains("<codigoTipoPuntoVenta>5</codigoTipoPuntoVenta>", handler.LastRequestBody);
        Assert.Contains("<nombrePuntoVenta>CAJA 1</nombrePuntoVenta>", handler.LastRequestBody);
        Assert.Equal(
            "https://pilotosiatservicios.impuestos.gob.bo/v2/FacturacionOperaciones",
            handler.LastRequestUri!.ToString());
    }

    [Fact]
    public async Task Registrar_RechazoDelSin_DevuelveMensajes()
    {
        var (operation, _) = Crear("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:registroPuntoVentaResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaRegistroPuntoVenta>
                    <transaccion>false</transaccion>
                    <mensajesList>
                      <codigo>970</codigo>
                      <descripcion>ERROR DE VALIDACION</descripcion>
                    </mensajesList>
                  </RespuestaRegistroPuntoVenta>
                </ns2:registroPuntoVentaResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.EjecutarAsync(Solicitud(), "token", CancellationToken.None);

        Assert.False(resultado.Exitoso);
        var mensaje = Assert.Single(resultado.Mensajes);
        Assert.Equal(970, mensaje.Codigo);
    }

    private static RegistroPuntoVentaRequest Solicitud() => new()
    {
        Nit = 7388296012,
        CodigoSistema = "TEST-SISTEMA",
        Cuis = "CUIS-TEST",
        Ambiente = Ambiente.Pruebas,
        Modalidad = Modalidad.ComputarizadaEnLinea,
        CodigoSucursal = 0,
        CodigoTipoPuntoVenta = 5,
        NombrePuntoVenta = "CAJA 1",
        Descripcion = "CAJA 1",
    };

    private static (RegistroPuntoVentaOperation Operation, MockHttpMessageHandler Handler) Crear(
        string responseXml)
    {
        var handler = new MockHttpMessageHandler(responseXml);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new SiatOptions { TimeoutSeconds = 30 });
        var client = new SiatHttpClient(httpClient, options, NullLogger<SiatHttpClient>.Instance);
        return (new RegistroPuntoVentaOperation(client), handler);
    }
}
