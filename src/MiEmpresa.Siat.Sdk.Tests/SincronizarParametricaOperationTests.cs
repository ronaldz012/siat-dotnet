namespace MiEmpresa.Siat.Sdk.Tests;

using System.Net;
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

public class SincronizarParametricaOperationTests
{
    private const string TipoPuntoVentaXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <ns2:sincronizarParametricaTipoPuntoVentaResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
              <RespuestaListaParametricas>
                <transaccion>true</transaccion>
                <listaCodigos>
                  <codigoClasificador>1</codigoClasificador>
                  <descripcion>Punto Venta Comisionista</descripcion>
                </listaCodigos>
                <listaCodigos>
                  <codigoClasificador>5</codigoClasificador>
                  <descripcion>Punto de Venta Cajeros</descripcion>
                </listaCodigos>
              </RespuestaListaParametricas>
            </ns2:sincronizarParametricaTipoPuntoVentaResponse>
          </soap:Body>
        </soap:Envelope>
        """;

    [Fact]
    public async Task SincronizarParametrica_ParseaLaListaDeCodigos()
    {
        var (operation, _) = Crear(TipoPuntoVentaXml);

        var resultado = await operation.EjecutarAsync(
            ParametricaSiat.TipoPuntoVenta, Solicitud(), "token", CancellationToken.None);

        Assert.True(resultado.Transaccion);
        Assert.True(resultado.Exitoso);
        Assert.Equal(2, resultado.Parametros.Count);
        Assert.Equal(1, resultado.Parametros[0].Codigo);
        Assert.Equal("Punto Venta Comisionista", resultado.Parametros[0].Descripcion);
        Assert.Equal(5, resultado.Parametros[1].Codigo);
    }

    [Fact]
    public async Task SincronizarParametrica_EnviaElNombreDeOperacionYElPayloadCorrecto()
    {
        var (operation, handler) = Crear(TipoPuntoVentaXml);

        await operation.EjecutarAsync(
            ParametricaSiat.TipoPuntoVenta, Solicitud(), "token", CancellationToken.None);

        var body = handler.LastRequestBody!;
        Assert.Contains("<ns:sincronizarParametricaTipoPuntoVenta>", body);
        Assert.Contains("<SolicitudSincronizacion>", body);
        Assert.Contains("<cuis>CUIS-TEST</cuis>", body);
        Assert.DoesNotContain("<codigoModalidad>", body); // el payload de sincronización no la lleva
        Assert.Equal(
            "https://pilotosiatservicios.impuestos.gob.bo/v2/FacturacionSincronizacion",
            handler.LastRequestUri!.ToString());
    }

    private static ParametricaRequest Solicitud() => new()
    {
        Nit = 7388296012,
        CodigoSistema = "TEST-SISTEMA",
        Cuis = "CUIS-TEST",
        Ambiente = Ambiente.Pruebas,
        CodigoSucursal = 0,
        CodigoPuntoVenta = 0,
    };

    private static (SincronizarParametricaOperation Operation, MockHttpMessageHandler Handler) Crear(
        string responseXml)
    {
        var handler = new MockHttpMessageHandler(responseXml);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new SiatOptions { TimeoutSeconds = 30 });
        var client = new SiatHttpClient(httpClient, options, NullLogger<SiatHttpClient>.Instance);
        return (new SincronizarParametricaOperation(client), handler);
    }
}
