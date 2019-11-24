using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AwsServerFake
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/", async b =>
                {
                    b.Response.StatusCode = StatusCodes.Status200OK;
                    b.Response.ContentType = "text/xml";

                    await b.Response.WriteAsync("<ReceiveMessageResponse><ReceiveMessageResult><Message><MessageId>5fea7756-0ea4-451a-a703-a558b933e274</MessageId><ReceiptHandle>MbZj6wDWli+JvwwJaBV+3dcjk2YW2vA3+STFFljTM8tJJg6HRG6PYSasuWXPJB+CwLj1FjgXUv1uSj1gUPAWV66FU/WeR4mq2OKpEGYWbnLmpRCJVAyeMjeU5ZBdtcQ+QEauMZc8ZRv37sIW2iJKq3M9MFx1YvV11A2x/KSbkJ0=</ReceiptHandle><MD5OfBody>fafb00f5732ab283681e124bf8747ed1</MD5OfBody><Body>This is a test message</Body><Attribute><Name>SenderId</Name><Value>195004372649</Value></Attribute><Attribute><Name>SentTimestamp</Name><Value>1238099229000</Value></Attribute><Attribute><Name>ApproximateReceiveCount</Name><Value>5</Value></Attribute><Attribute><Name>ApproximateFirstReceiveTimestamp</Name><Value>1250700979248</Value></Attribute></Message></ReceiveMessageResult><ResponseMetadata><RequestId>b6633655-283d-45b4-aee4-4e84e0ae6afa</RequestId></ResponseMetadata></ReceiveMessageResponse>");
                });
            });
        }
    }
}
