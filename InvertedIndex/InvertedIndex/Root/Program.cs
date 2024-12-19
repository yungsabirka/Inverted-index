
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
    }

    private void TestIndexBuilder()
    {
        for (int i = 1; i < 1025; i*=2)
        {
            var reader = new FilesReader();
            var index = new Index.InvertedIndex();
            var builder = new IndexBuilder(i, reader, index);
            Console.WriteLine(i);
            builder.RebuildIndex(null);   
        }
    }
}