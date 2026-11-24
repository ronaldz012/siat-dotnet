namespace MiEmpresa.Siat.Sdk.Tests;

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public MockHttpMessageHandler(string responseXml, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseXml, System.Text.Encoding.UTF8, "application/xml")
        };
    }

    public string? LastRequestBody { get; private set; }
    public Uri? LastRequestUri { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequestUri = request.RequestUri;

        if (request.Content is not null)
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

        return _response;
    }
}
