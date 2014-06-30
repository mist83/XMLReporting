
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class AsyncExamples
    {
        private static List<string> allCalls = new List<string>();

        public async Task<int> MyTaskAsync()
        {
            Task getStringTask = Task.Run(
                () =>
                {
                    allCalls.Add(string.Format("Starting Task"));
                    for (int i = 0; i < 20; i++)
                    {
                        for (int j = 0; j < int.MaxValue / 10; j++)
                        {
                            int x = 5;
                        }

                        allCalls.Add(string.Format("Task: (call {0})", allCalls.Count));
                    }
                    //Thread.Sleep(50);
                    allCalls.Add(string.Format("Ending Task"));
                });

            allCalls.Add(string.Format("Starting Other Work"));
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < int.MaxValue / 100; j++)
                {
                    int x = 5;
                }
                //Thread.Sleep(50);
                allCalls.Add(string.Format("Other work: (call {0})", allCalls.Count));
            }
            allCalls.Add(string.Format("Ending Other Work"));

            await getStringTask;

            foreach (var item in allCalls)
                Debug.WriteLine(item);

            Debug.WriteLine("Finished function");
            return 2;
        }
    }
}
