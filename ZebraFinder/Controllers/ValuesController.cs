using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Mvc;
using Zebra.Sdk.Printer.Discovery;

namespace ZebraFinder.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // Test value for office network. 
        public static string SubnetIpRange { get; set; } = "10.0.20.*";

        public static volatile HashSet<string> Printers = new HashSet<string>()
        {
            "dummy"
        };

        public ValuesController()
        {
            // TODO: Discovery at interval in background instead of on request. 
            // This didn't work.

            //var timer = new System.Timers.Timer
            //{
            //    Interval = 60000,
            //    Enabled = true
            //};
            //timer.Elapsed += (s, e) => FindNetworkZebras(10).Start();
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            // TODO: (Last stopping point). Correct async behavior.
            // Currently runs discovery on this request, so two requests are required.
            var res = FindNetworkZebras();
            return Ok(res);
        }

        public static IEnumerable<string> FindNetworkZebras()
        {
            var discoveryHandler = new NetworkDiscoveryHandler();
            var timer = new System.Timers.Timer
            {
                Interval = 10,
                Enabled = true
            };
            timer.Elapsed += discoveryHandler.End;

            var printers = new List<string>() {"dummy2"};

            discoveryHandler.WhatDo = p =>
            {
                var ip = p.DiscoveryDataMap["ADDRESS"];
                var name = p.DiscoveryDataMap["SYSTEM_NAME"];
                var strToAdd = $"Name: {name} IP: {ip}";
                printers.Add(strToAdd);
            };

            NetworkDiscoverer.SubnetSearch(discoveryHandler, SubnetIpRange);
            Thread.Sleep(10 * 256);

            return printers;
        }

        public class NetworkDiscoveryHandler : DiscoveryHandler
        {
            public Action<DiscoveredPrinter> WhatDo { get; set; }
            public bool DiscoveryComplete { get; private set; }

            public void DiscoveryError(string message)
            {
                Console.WriteLine($@"An error occurred during discovery: {message}.");
                DiscoveryComplete = true;
            }

            public void DiscoveryFinished()
            {
                DiscoveryComplete = true;
            }

            public void FoundPrinter(DiscoveredPrinter printer)
            {
                WhatDo(printer);
            }

            public void End(object source, ElapsedEventArgs e)
            {
                DiscoveryComplete = true;
            }
        }
    }
}
