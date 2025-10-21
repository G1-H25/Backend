using System;
using System.Net.Http;

public class TestFixture : IDisposable
{
    public HttpClient Client { get; }

    public TestFixture()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")  // backend base URL
        };
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}
