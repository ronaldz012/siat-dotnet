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

public class ConsultaPuntosVentaOperationTests
{
    [Fact]
    public async Task Consultar_ParseaLaListaDePuntosVenta()
    {
        var (operation, handler) = Crear("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:consultaPuntoVentaResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaConsultaPuntoVenta>
                    <listaPuntosVentas>
                      <codigoPuntoVenta>0</codigoPuntoVenta>
                      <nombrePuntoVenta>Casa matriz</nombrePuntoVenta>
                      <tipoPuntoVenta>Punto de Venta Cajeros</tipoPuntoVenta>
                    </listaPuntosVentas>
                    <listaPuntosVentas>
                      <codigoPuntoVenta>1</codigoPuntoVenta>
                      <nombrePuntoVenta>Caja 1</nombrePuntoVenta>
                      <tipoPuntoVenta>Punto de Venta Cajeros</tipoPuntoVenta>
                    </listaPuntosVentas>
                    <transaccion>true</transaccion>
                  </RespuestaConsultaPuntoVenta>
                </ns2:consultaPuntoVentaResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.EjecutarAsync(Solicitud(), "token", CancellationToken.None);

        Assert.True(resultado.Exitoso);
        Assert.Equal(2, resultado.PuntosVenta.Count);
        Assert.Equal(0, resultado.PuntosVenta[0].CodigoPuntoVenta);
        Assert.Equal("Casa matriz", resultado.PuntosVenta[0].NombrePuntoVenta);
        Assert.Equal("Caja 1", resultado.PuntosVenta[1].NombrePuntoVenta);
        Assert.Equal("Punto de Venta Cajeros", resultado.PuntosVenta[1].TipoPuntoVenta);

        Assert.Contains("<ns:consultaPuntoVenta>", handler.LastRequestBody);
        Assert.Contains("<SolicitudConsultaPuntoVenta>", handler.LastRequestBody);
        Assert.Equal(
            "https://pilotosiatservicios.impuestos.gob.bo/v2/FacturacionOperaciones",
            handler.LastRequestUri!.ToString());
    }

    [Fact]
    public async Task Consultar_SinPuntosVenta_DevuelveMensaje982()
    {
        var (operation, _) = Crear("""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ns2:consultaPuntoVentaResponse xmlns:ns2="https://siat.impuestos.gob.bo/">
                  <RespuestaConsultaPuntoVenta>
                    <transaccion>false</transaccion>
                    <mensajesList>
                      <codigo>982</codigo>
                      <descripcion>NO EXISTEN PUNTOS DE VENTA REGISTRADOS</descripcion>
                    </mensajesList>
                  </RespuestaConsultaPuntoVenta>
                </ns2:consultaPuntoVentaResponse>
              </soap:Body>
            </soap:Envelope>
            """);

        var resultado = await operation.EjecutarAsync(Solicitud(), "token", CancellationToken.None);

        Assert.False(resultado.Exitoso);
        Assert.Empty(resultado.PuntosVenta);
        var mensaje = Assert.Single(resultado.Mensajes);
        Assert.Equal(982, mensaje.Codigo);
    }

    private static PuntosVentaRequest Solicitud() => new()
    {
        Nit = 7388296012,
        CodigoSistema = "TEST-SISTEMA",
        Cuis = "CUIS-TEST",
        Ambiente = Ambiente.Pruebas,
        CodigoSucursal = 0,
    };

    private static (ConsultaPuntosVentaOperation Operation, MockHttpMessageHandler Handler) Crear(
        string responseXml)
    {
        var handler = new MockHttpMessageHandler(responseXml);
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new SiatOptions { TimeoutSeconds = 30 });
        var client = new SiatHttpClient(httpClient, options, NullLogger<SiatHttpClient>.Instance);
        return (new ConsultaPuntosVentaOperation(client), handler);
    }
}
