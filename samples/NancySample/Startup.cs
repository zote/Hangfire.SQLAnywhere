using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SQLAnywhere;
using Hangfire.SQLAnywhere.Msmq;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(NancySample.Startup))]

namespace NancySample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfire(config =>
            {
                //use SQLAnywhere embedded with MSMQ
                config
                    .UseSQLAnywhereStorage(@"User=SYSDBA;Password=masterkey;Database=S:\Source\Hangfire.SQLAnywhere\HANGFIRE_SAMPLE.FDB;Packet Size=8192;DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;ServerType=1;ClientLibrary=S:\Source\Hangfire.SQLAnywhere\SQLAnywhere\fbembed.dll;")
                    .UseMsmqQueues(@".\private$\hangfire-{0}");
                config.UseServer();
            });

            app.UseNancy();

            RecurringJob.AddOrUpdate(
                () => TextBuffer.WriteLine("Recurring Job completed successfully!"),
                Cron.Minutely);
        }
    }
}