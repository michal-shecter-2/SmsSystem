using System.Text.Json;
using System.Text.Json.Serialization;
using Sms.Api.Data;
using Sms.Api.Providers;
using Sms.Api.Services;
namespace Sms.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient<HttpSmsProvider>(client =>
            {
                var baseUrl = builder.Configuration["SmsProvider:BaseUrl"]
                    ?? throw new InvalidOperationException(
                        "SMS provider URL is missing.");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(3);
            });
            builder.Services.AddScoped<MessageRepository>();
            builder.Services.AddScoped<EmployeeRepository>();
            builder.Services.AddScoped<MessageService>();
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}
