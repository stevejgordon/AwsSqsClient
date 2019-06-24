using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace HighPerfCloud.Aws.Sqs.Core
{
    /// <summary>
    /// Abstraction of AWS API invocation.
    /// </summary>
    public abstract class AwsApiInvoker
    {
        


    }

    public class HttpClientAwsApiInvoker : AwsApiInvoker
    {
        private readonly HttpClient _httpClient;

        public HttpClientAwsApiInvoker(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


    }

    public class AwsSqsResponseMessage //: IDisposable
    {
        private readonly HttpResponseMessage _responseMessage;

        public AwsSqsResponseMessage(HttpResponseMessage responseMessage)
        {
            _responseMessage = responseMessage;
        }


    }
}
