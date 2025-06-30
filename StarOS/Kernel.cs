using System;
using Sys = Cosmos.System;
using Cosmos.Core.Memory;

namespace StarOS
{
    public class Kernel : Sys.Kernel
    {
        private bool RunGUI = false;
        private bool isBooted = false;
        private int lastHeapCollect = 0;

        private Terminal terminal = new Terminal();

        protected override void BeforeRun()
        {
            Console.Clear();
            Console.SetWindowSize(90, 30);
            Console.OutputEncoding = Cosmos.System.ExtendedASCII.CosmosEncodingProvider.Instance.GetEncoding(437);

            ShowBootMenu();
        }

        private void ShowBootMenu()
        {
            Console.WriteLine("StarOS Boot Manager");
            Console.WriteLine("1. Start StarOS normally");
            Console.WriteLine("2. Start GUI");
            Console.WriteLine("3. Shutdown");
            Console.Write("Select option: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    Console.WriteLine("Starting StarOS normally...");
                    isBooted = true;
                    RunGUI = false;
                    terminal.Reset();
                    break;
                case "2":
                    Console.WriteLine("Starting GUI...");
                    Boot.OnBoot();
                    isBooted = true;
                    RunGUI = true;
                    break;
                case "3":
                    Console.WriteLine("Shutting down...");
                    Sys.Power.Shutdown();
                    break;
                default:
                    Console.WriteLine("Invalid option");
                    ShowBootMenu();
                    break;
            }
        }

        protected override void Run()
        {
            if (!isBooted)
                return;

            if (!RunGUI)
            {
                if (terminal.IsRunning)
                {
                    terminal.Run();
                }
                else
                {
                    isBooted = false;
                    ShowBootMenu();
                }
            }
            else
            {
                Gui.Update();
                //Thread.Sleep(10);
            }

            if (lastHeapCollect >= 20)
            {
                Heap.Collect();
                lastHeapCollect = 0;
            }
            else
            {
                lastHeapCollect++;
            }
        }
    }
}
