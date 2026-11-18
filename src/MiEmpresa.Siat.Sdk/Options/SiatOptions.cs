namespace MiEmpresa.Siat.Sdk.Options;

public sealed class SiatOptions
{
    public const string SectionName = "Siat";

    public string BaseUrlPruebas { get; set; } = "https://pilotosiatservicios.impuestos.gob.bo/v2";
    public string BaseUrlProduccion { get; set; } = "https://siatrest.impuestos.gob.bo/v2";

    public int TimeoutSeconds { get; set; } = 30;

    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;

    public string UserAgent { get; set; } = "MiEmpresa.Siat.Sdk/1.0";

    public bool ValidateXmlAgainstXsd { get; set; } = true;

    public string DefaultAmbiente { get; set; } = "Pruebas";

    internal string GetBaseUrl(string ambiente) =>
        ambiente.Equals("Produccion", StringComparison.OrdinalIgnoreCase)
            ? BaseUrlProduccion
            : BaseUrlPruebas;
}