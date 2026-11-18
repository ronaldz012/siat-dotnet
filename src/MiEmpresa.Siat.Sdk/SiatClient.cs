namespace MiEmpresa.Siat.Sdk;

using MiEmpresa.Siat.Sdk.Clients;
using MiEmpresa.Siat.Sdk.Models.Requests;
using MiEmpresa.Siat.Sdk.Models.Responses;
using MiEmpresa.Siat.Sdk.Operations;
using MiEmpresa.Siat.Sdk.Exceptions;

public interface ISiatSdk
{
    Task<CuisResponse> ObtenerCuisAsync(CuisRequest request, string token, CancellationToken ct);
    Task<CufdResponse> ObtenerCufdAsync(CufdRequest request, string token, CancellationToken ct);
    Task<EmisionResponse> EmitirFacturaComputarizadaAsync(FacturaRequest request, string token, CancellationToken ct);
    Task<SincronizacionResponse> SincronizarCatalogosAsync(SincronizacionRequest request, string token, CancellationToken ct);
    Task<RegistroEventoResponse> RegistrarEventoSignificativoAsync(EventoSignificativoRequest request, string token, CancellationToken ct);
}

public sealed class SiatClient : ISiatSdk
{
    private readonly ISiatClient _httpClient;
    private readonly ObtenerCuisOperation _obtenerCuis;
    private readonly ObtenerCufdOperation _obtenerCufd;
    private readonly EmitirFacturaComputarizadaOperation _emitirFactura;
    private readonly SincronizarCatalogosOperation _sincronizarCatalogos;
    private readonly RegistrarEventoOperation _registrarEvento;

    public SiatClient(
        ISiatClient httpClient,
        ObtenerCuisOperation obtenerCuis,
        ObtenerCufdOperation obtenerCufd,
        EmitirFacturaComputarizadaOperation emitirFactura,
        SincronizarCatalogosOperation sincronizarCatalogos,
        RegistrarEventoOperation registrarEvento)
    {
        _httpClient = httpClient;
        _obtenerCuis = obtenerCuis;
        _obtenerCufd = obtenerCufd;
        _emitirFactura = emitirFactura;
        _sincronizarCatalogos = sincronizarCatalogos;
        _registrarEvento = registrarEvento;
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
}