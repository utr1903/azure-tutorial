using System.Threading.Tasks;

namespace InputProcessor
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await new Processor().Run();
        }
    }
}
