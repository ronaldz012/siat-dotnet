namespace MiEmpresa.Siat.Sdk;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Operations;

public interface ISiatSdk
{
    Task<CuisResponse> ObtenerCuisAsync(CuisRequest request, string token, CancellationToken ct);
    Task<CufdResponse> ObtenerCufdAsync(CufdRequest request, string token, CancellationToken ct);
    Task<EmisionResponse> EmitirFacturaComputarizadaAsync(FacturaRequest request, string token, CancellationToken ct);
    Task<SincronizacionResponse> SincronizarCatalogosAsync(SincronizacionRequest request, string token, CancellationToken ct);
    Task<RegistroEventoResponse> RegistrarEventoSignificativoAsync(EventoSignificativoRequest request, string token, CancellationToken ct);
    Task<SincronizacionParametricaResponse> SincronizarParametricaAsync(ParametricaSiat catalogo, ParametricaRequest request, string token, CancellationToken ct);
    Task<CatalogoResponse<ActividadEconomicaSiat>> SincronizarActividadesAsync(ParametricaRequest request, string token, CancellationToken ct);
    Task<CatalogoResponse<ProductoServicioSiat>> SincronizarProductosServiciosAsync(ParametricaRequest request, string token, CancellationToken ct);
    Task<CatalogoResponse<LeyendaFacturaSiat>> SincronizarLeyendasFacturaAsync(ParametricaRequest request, string token, CancellationToken ct);
    Task<CatalogoResponse<ActividadDocumentoSectorSiat>> SincronizarActividadesDocumentoSectorAsync(ParametricaRequest request, string token, CancellationToken ct);
    Task<FechaHoraResponse> ObtenerFechaHoraAsync(ParametricaRequest request, string token, CancellationToken ct);
    Task<ConsultaPuntosVentaResponse> ConsultarPuntosVentaAsync(PuntosVentaRequest request, string token, CancellationToken ct);
}

public sealed class SiatClient : ISiatSdk
{
    private readonly ObtenerCuisOperation _obtenerCuis;
    private readonly ObtenerCufdOperation _obtenerCufd;
    private readonly EmitirFacturaComputarizadaOperation _emitirFactura;
    private readonly SincronizarCatalogosOperation _sincronizarCatalogos;
    private readonly RegistrarEventoOperation _registrarEvento;
    private readonly SincronizarParametricaOperation _sincronizarParametrica;
    private readonly SincronizarCatalogosEspecialesOperation _sincronizarEspeciales;
    private readonly ObtenerFechaHoraOperation _obtenerFechaHora;
    private readonly ConsultaPuntosVentaOperation _consultaPuntosVenta;

    public SiatClient(
        ObtenerCuisOperation obtenerCuis,
        ObtenerCufdOperation obtenerCufd,
        EmitirFacturaComputarizadaOperation emitirFactura,
        SincronizarCatalogosOperation sincronizarCatalogos,
        RegistrarEventoOperation registrarEvento,
        SincronizarParametricaOperation sincronizarParametrica,
        SincronizarCatalogosEspecialesOperation sincronizarEspeciales,
        ObtenerFechaHoraOperation obtenerFechaHora,
        ConsultaPuntosVentaOperation consultaPuntosVenta)
    {
        _obtenerCuis = obtenerCuis;
        _obtenerCufd = obtenerCufd;
        _emitirFactura = emitirFactura;
        _sincronizarCatalogos = sincronizarCatalogos;
        _registrarEvento = registrarEvento;
        _sincronizarParametrica = sincronizarParametrica;
        _sincronizarEspeciales = sincronizarEspeciales;
        _obtenerFechaHora = obtenerFechaHora;
        _consultaPuntosVenta = consultaPuntosVenta;
    }

    public async Task<CuisResponse> ObtenerCuisAsync(CuisRequest request, string token, CancellationToken ct)
        => await _obtenerCuis.EjecutarAsync(request, token, ct);

    public async Task<CufdResponse> ObtenerCufdAsync(CufdRequest request, string token, CancellationToken ct)
        => await _obtenerCufd.EjecutarAsync(request, token, ct);

    public async Task<EmisionResponse> EmitirFacturaComputarizadaAsync(FacturaRequest request, string token, CancellationToken ct)
        => await _emitirFactura.EjecutarAsync(request, token, ct);

    public async Task<SincronizacionResponse> SincronizarCatalogosAsync(SincronizacionRequest request, string token, CancellationToken ct)
        => await _sincronizarCatalogos.EjecutarAsync(request, token, ct);

    public async Task<RegistroEventoResponse> RegistrarEventoSignificativoAsync(EventoSignificativoRequest request, string token, CancellationToken ct)
        => await _registrarEvento.EjecutarAsync(request, token, ct);

    public async Task<SincronizacionParametricaResponse> SincronizarParametricaAsync(
        ParametricaSiat catalogo, ParametricaRequest request, string token, CancellationToken ct)
        => await _sincronizarParametrica.EjecutarAsync(catalogo, request, token, ct);

    public async Task<CatalogoResponse<ActividadEconomicaSiat>> SincronizarActividadesAsync(
        ParametricaRequest request, string token, CancellationToken ct)
        => await _sincronizarEspeciales.ActividadesAsync(request, token, ct);

    public async Task<CatalogoResponse<ProductoServicioSiat>> SincronizarProductosServiciosAsync(
        ParametricaRequest request, string token, CancellationToken ct)
        => await _sincronizarEspeciales.ProductosServiciosAsync(request, token, ct);

    public async Task<CatalogoResponse<LeyendaFacturaSiat>> SincronizarLeyendasFacturaAsync(
        ParametricaRequest request, string token, CancellationToken ct)
        => await _sincronizarEspeciales.LeyendasFacturaAsync(request, token, ct);

    public async Task<CatalogoResponse<ActividadDocumentoSectorSiat>> SincronizarActividadesDocumentoSectorAsync(
        ParametricaRequest request, string token, CancellationToken ct)
        => await _sincronizarEspeciales.ActividadesDocumentoSectorAsync(request, token, ct);

    public async Task<FechaHoraResponse> ObtenerFechaHoraAsync(
        ParametricaRequest request, string token, CancellationToken ct)
        => await _obtenerFechaHora.EjecutarAsync(request, token, ct);

    public async Task<ConsultaPuntosVentaResponse> ConsultarPuntosVentaAsync(
        PuntosVentaRequest request, string token, CancellationToken ct)
        => await _consultaPuntosVenta.EjecutarAsync(request, token, ct);
}
