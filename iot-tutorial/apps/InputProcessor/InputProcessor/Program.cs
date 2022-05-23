using System;

namespace InputProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            await new Processor().Run();
        }
    }
}
