namespace InputProcessor
{
    class Program
    {
        public async static void Main(string[] args)
        {
            await new Processor().Run();
        }
    }
}
