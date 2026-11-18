namespace MiEmpresa.Siat.Sdk.Exceptions;

public abstract class SiatException : Exception
{
    protected SiatException(string message) : base(message) { }
    protected SiatException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class SiatConfigurationException : SiatException
{
    public SiatConfigurationException(string message) : base(message) { }
    public SiatConfigurationException(string message, Exception inner) : base(message, inner) { }
}

public sealed class SiatCertificateException : SiatException
{
    public SiatCertificateException(string message) : base(message) { }
    public SiatCertificateException(string message, Exception inner) : base(message, inner) { }
}

public sealed class SiatCommunicationException : SiatException
{
    public int? HttpStatusCode { get; }
    public string? SoapFaultCode { get; }
    public string? SoapFaultString { get; }

    public SiatCommunicationException(string message, int? httpStatusCode = null, string? soapFaultCode = null, string? soapFaultString = null)
        : base(message)
    {
        HttpStatusCode = httpStatusCode;
        SoapFaultCode = soapFaultCode;
        SoapFaultString = soapFaultString;
    }

    public SiatCommunicationException(string message, Exception inner, int? httpStatusCode = null, string? soapFaultCode = null, string? soapFaultString = null)
        : base(message, inner)
    {
        HttpStatusCode = httpStatusCode;
        SoapFaultCode = soapFaultCode;
        SoapFaultString = soapFaultString;
    }
}

public sealed class SiatAuthenticationException : SiatException
{
    public SiatAuthenticationException(string message) : base(message) { }
    public SiatAuthenticationException(string message, Exception inner) : base(message, inner) { }
}

public sealed class SiatValidationException : SiatException
{
    public IReadOnlyList<SiatErrorDetail> Errors { get; }

    public SiatValidationException(string message, IEnumerable<SiatErrorDetail> errors)
        : base(message)
    {
        var list = errors?.ToList();
        Errors = list?.Count > 0 ? list.AsReadOnly() : Array.Empty<SiatErrorDetail>();
    }

    public SiatValidationException(string message, Exception inner, IEnumerable<SiatErrorDetail> errors)
        : base(message, inner)
    {
        var list = errors?.ToList();
        Errors = list?.Count > 0 ? list.AsReadOnly() : Array.Empty<SiatErrorDetail>();
    }
}

public sealed class SiatXmlException : SiatException
{
    public SiatXmlException(string message) : base(message) { }
    public SiatXmlException(string message, Exception inner) : base(message, inner) { }
}

public sealed record SiatErrorDetail
{
    public int Codigo { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public bool EsWarning { get; init; }
}