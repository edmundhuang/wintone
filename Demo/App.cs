namespace Demo
{
    public class App
    {
        public void Run()
        {
            Console.WriteLine("Press X to exit, any key to continue.");
            while (true)
            {
                var key = Console.ReadKey();

                switch (key.Key)
                {
                    case ConsoleKey.X:
                        return;
                    default:
                        break;
                }

                Thread.Sleep(1000);
            }
        }
    }
}
