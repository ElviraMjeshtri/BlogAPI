namespace BlogApi.Services.Helpers;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private HttpResponseMessage _response;
    private Exception _exception;

    public void SetResponse(HttpResponseMessage response)
    {
        _response = response;
    }

    public void SetException(Exception exception)
    {
        _exception = exception;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_exception != null)
        {
            throw _exception;
        }

        return await Task.FromResult(_response);
    }
}