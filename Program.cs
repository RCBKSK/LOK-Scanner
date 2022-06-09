using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace lok_wss
{
    internal class Program
    {
        //private static lokContext _context;
        private static IServiceProvider _services;

        private static void Main()
        {
            _services = ConfigureServices();

            Thread c15Thread = new Thread(() =>
            {
                ContinentScanner continentScanner = new ContinentScanner(15);
            });
            c15Thread.Start();

            Thread cvcThread = new Thread(() =>
            {
                ContinentScanner continentScanner = new ContinentScanner(100002, true);
            });
            cvcThread.Start();

            Thread c24Thread = new Thread(() =>
            {
                ContinentScanner continentScanner = new ContinentScanner(24);
            });
            c24Thread.Start();

           

            var thread = new Thread(() => { while (true) { Thread.Sleep(300000); } });
            thread.Start();
        }

        private static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<Program>()
                .AddDbContext<lokContext>(ServiceLifetime.Scoped)
                .BuildServiceProvider();
        }
    }
}

