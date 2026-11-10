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

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
