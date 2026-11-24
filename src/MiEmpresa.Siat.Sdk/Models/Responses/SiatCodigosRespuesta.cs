namespace MiEmpresa.Siat.Sdk.Models.Responses;

/// <summary>
/// Códigos de respuesta del SIN que disparan lógica. El resto de los códigos se
/// muestra tal cual llega en <see cref="SiatMensaje.Descripcion"/>; la tabla completa
/// vive en la documentación del proyecto consumidor (docs/siat-codigos-error.md).
/// </summary>
public static class SiatCodigosRespuesta
{
    // --- Comunicación / disponibilidad ---
    public const int ComunicacionExitosa = 926;
    public const int TokenInvalido = 989;
    public const int ErrorBaseDatos = 991;
    public const int ErrorServicioPadron = 992;
    public const int NitInexistente = 994;
    public const int ServicioNoDisponible = 995;
    public const int ErrorEjecucionServicio = 999;

    // --- CUIS ---
    public const int CuisVigenteNoSolicitable = 970; // "se encuentra vigente, no puede solicitar otro"
    public const int ExisteCuisVigente = 980;        // advertencia: devuelve el vigente
    public const int CuisNoVigente = 929;
    public const int CuisNoCorrespondeSucursal = 930;
    public const int CuisNoAsociado = 979;
    public const int CuisProximoACaducar = 3008;

    // --- CUFD ---
    public const int CufdNoVigente = 953;

    // --- NIT ---
    public const int NitActivo = 986;
    public const int NitInactivo = 987;

    // --- Puntos de venta ---
    public const int NoExistePuntosVenta = 982;

    // --- Estados de recepción / anulación ---
    public const int RecepcionPendiente = 901;
    public const int RecepcionRechazada = 902;
    public const int RecepcionProcesada = 903;
    public const int RecepcionObservada = 904;
    public const int AnulacionConfirmada = 905;
    public const int AnulacionRechazada = 906;
    public const int RecepcionValidada = 908;
}
