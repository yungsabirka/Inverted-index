using System.Diagnostics;
using InvertedIndex.Root.Index;
using InvertedIndex.Root.Services;

namespace InvertedIndex.Root;

public class Program
{
    static async Task Main()
    {
        var server  = new Server();
        server.Init();
        await server.StartServer();
        //TestIndexBuilder();
    }

    private static void TestIndexBuilder()
    {
        for (int i = 1; i < 5000; i*=2)
        {
            var watch = new Stopwatch();
            watch.Start();
            var reader = new FilesReader();
            var index = new Index.InvertedIndex(i);
            var builder = new IndexBuilder(reader, index);
            builder.RebuildIndex(null);
            while(index.IsBuilded == false)
                continue;
            
            watch.Stop();
            Console.WriteLine("Time for " + i + " threads: " + watch.ElapsedMilliseconds);
        }
    }
}