// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Net.Tests
{
    public class HttpRequestStreamTests : IDisposable
    {
        private HttpListenerFactory _factory;
        private HttpListener _listener;
        private GetContextHelper _helper;

        public HttpRequestStreamTests()
        {
            _factory = new HttpListenerFactory();
            _listener = _factory.GetListener();
            _helper = new GetContextHelper(_listener, _factory.ListeningUrl);
        }

        public void Dispose()
        {
            _factory.Dispose();
            _helper.Dispose();
        }

        //[ActiveIssue(22110, TargetFrameworkMonikers.UapAot)]
        [Theory]
        [InlineData(true, "")]
        [InlineData(false, "")]
        [InlineData(true, "Non-Empty")]
        [InlineData(false, "Non-Empty")]
        public async Task Read_FullLengthAsynchronous_PadBuffer_Success(bool transferEncodingChunked, string text)
        {
            byte[] expected = Encoding.UTF8.GetBytes(text);
            Task<HttpListenerContext> contextTask = _listener.GetContextAsync();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TransferEncodingChunked = transferEncodingChunked;
                Task<HttpResponseMessage> clientTask = client.PostAsync(_factory.ListeningUrl, new StringContent(text));

                HttpListenerContext context = await contextTask;
                if (transferEncodingChunked)
                {
                    Assert.Equal(-1, context.Request.ContentLength64);
                    Assert.Equal("chunked", context.Request.Headers["Transfer-Encoding"]);
                }
                else
                {
                    Assert.Equal(expected.Length, context.Request.ContentLength64);
                    Assert.Null(context.Request.Headers["Transfer-Encoding"]);
                }

                const int pad = 128;

                // Add padding at beginning and end to test for correct offset/size handling
                byte[] buffer = new byte[pad + expected.Length + pad];
                int bytesRead = await context.Request.InputStream.ReadAsync(buffer, pad, expected.Length);
                Assert.Equal(expected.Length, bytesRead);
                Assert.Equal(expected, buffer.Skip(pad).Take(bytesRead));

                // Subsequent reads don't do anything.
                Assert.Equal(0, await context.Request.InputStream.ReadAsync(buffer, pad, 1));

                context.Response.Close();
                using (HttpResponseMessage response = await clientTask)
                {
                    Assert.Equal(200, (int)response.StatusCode);
                }
            }
        }

        //[ActiveIssue(22110, TargetFrameworkMonikers.UapAot)]
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Read_TooMuchAsynchronous_Success(bool transferEncodingChunked)
        {
            const string Text = "Some-String";
            byte[] expected = Encoding.UTF8.GetBytes(Text);
            Task<HttpListenerContext> contextTask = _listener.GetContextAsync();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TransferEncodingChunked = transferEncodingChunked;
                Task<HttpResponseMessage> clientTask = client.PostAsync(_factory.ListeningUrl, new StringContent(Text));

                HttpListenerContext context = await contextTask;

                byte[] buffer = new byte[expected.Length + 5];
                int bytesRead = await context.Request.InputStream.ReadAsync(buffer, 0, buffer.Length);
                Assert.Equal(expected.Length, bytesRead);
                Assert.Equal(expected.Concat(new byte[5]), buffer);

                context.Response.Close();
            }
        }

        //[ActiveIssue(22110, TargetFrameworkMonikers.UapAot)]
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Read_TooMuchSynchronous_Success(bool transferEncodingChunked)
        {
            const string Text = "Some-String";
            byte[] expected = Encoding.UTF8.GetBytes(Text);
            Task<HttpListenerContext> contextTask = _listener.GetContextAsync();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TransferEncodingChunked = transferEncodingChunked;
                Task<HttpResponseMessage> clientTask = client.PostAsync(_factory.ListeningUrl, new StringContent(Text));

                HttpListenerContext context = await contextTask;

                byte[] buffer = new byte[expected.Length + 5];
                int bytesRead = context.Request.InputStream.Read(buffer, 0, buffer.Length);
                Assert.Equal(expected.Length, bytesRead);
                Assert.Equal(expected.Concat(new byte[5]), buffer);

                context.Response.Close();
            }
        }

        //[ActiveIssue(22110, TargetFrameworkMonikers.UapAot)]
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Read_Disposed_ReturnsZero(bool transferEncodingChunked)
        {
            const string Text = "Some-String";
            int bufferSize = Encoding.UTF8.GetByteCount(Text);
            Task<HttpListenerContext> contextTask = _listener.GetContextAsync();
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TransferEncodingChunked = transferEncodingChunked;
                Task<HttpResponseMessage> clientTask = client.PostAsync(_factory.ListeningUrl, new StringContent(Text));

                HttpListenerContext context = await contextTask;
                context.Request.InputStream.Close();

                byte[] buffer = new byte[bufferSize];
                Assert.Equal(0, context.Request.InputStream.Read(buffer, 0, buffer.Length));
                Assert.Equal(new byte[bufferSize], buffer);

                IAsyncResult result = context.Request.InputStream.BeginRead(buffer, 0, buffer.Length, null, null);
                Assert.Equal(0, context.Request.InputStream.EndRead(result));
                Assert.Equal(new byte[bufferSize], buffer);

                context.Response.Close();
            }
        }

        //[ActiveIssue(22110, TargetFrameworkMonikers.UapAot)]
        [Fact]
        public async Task CanRead_Get_ReturnsTrue()
        {
            HttpListenerRequest request = await _helper.GetRequest(chunked: true);
            using (Stream inputStream = request.InputStream)
            {
                Assert.True(inputStream.CanRead);
            }
        }

        //[ActiveIssue(22110, TargetFrameworkMonikers.UapAot)]
        [Theory]
        [InlineData(-1, true)]
        [InlineData(3, true)]
        [InlineData(-1, false)]
        [InlineData(3, false)]
        public async Task Read_InvalidOffset_ThrowsArgumentOutOfRangeException(int offset, bool chunked)
        {
            HttpListenerRequest request = await _helper.GetRequest(chunked);
            using (Stream inputStream = request.InputStream)
            {
                AssertExtensions.Throws<ArgumentOutOfRangeException>("offset", () => inputStream.Read(new byte[2], offset, 0));
                await AssertExtensions.ThrowsAsync<ArgumentOutOfRangeException>("offset", () => inputStream.ReadAsync(new byte[2], offset, 0));
            }
        }

        //[ActiveIssue(22110, TargetFrameworkMonikers.UapAot)]
        [Theory]
        [InlineData(0, 3, true)]
        [InlineData(1, 2, true)]
        [InlineData(2, 1, true)]
        [InlineData(0, 3, false)]
        [InlineData(1, 2, false)]
        [InlineData(2, 1, false)]
        public async Task Read_InvalidOffsetSize_ThrowsArgumentOutOfRangeException(int offset, int size, bool chunked)
        {
            HttpListenerRequest request = await _helper.GetRequest(chunked);
            using (Stream inputStream = request.InputStream)
            {
                AssertExtensions.Throws<ArgumentOutOfRangeException>("size", () => inputStream.Read(new byte[2], offset, size));
                await AssertExtensions.ThrowsAsync<ArgumentOutOfRangeException>("size", () => inputStream.ReadAsync(new byte[2], offset, size));
            }
        }
    }
}