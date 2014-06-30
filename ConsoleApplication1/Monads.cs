using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Monads
    {
        async static Task<int> AddOne(Task<int> task)
        {
            int unwrapped = await task;
            int result = unwrapped + 1;
            return result;
        }
    }
}
