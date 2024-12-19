using System.Net;
using InvertedIndex.Root.Index;
using InvertedIndex.Root.Services;
using FilesReader = InvertedIndex.Root.Services.FilesReader;

namespace InvertedIndex.Root;

public class Server
{
    private HttpListener _listener = new();
    private ThreadPool.ThreadPool _pool = new(4, 2);
    private Index.InvertedIndex _invertedIndex = new();
    private FilesReader _filesReader = new();
    private ResponseFormatter _formatter = new();
    private IndexBuilder _indexBuilder;
    private bool _isRunning;
    private Timer _timer;

    public void Init()
    {
        _indexBuilder = new IndexBuilder(2, _filesReader, _invertedIndex);
        _pool.InitiateThreads();
        _indexBuilder.RebuildIndex(null);
        _listener.Prefixes.Add("http://localhost:8080/");
        _timer = new Timer(_indexBuilder.RebuildIndex, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
    }

    public async Task StartServer()
    {
        _isRunning = true;
        _listener.Start();
        _pool.StartExecuting();
        while (_isRunning)
        {
            var context = await _listener.GetContextAsync();
            // Console.WriteLine($"Client connected: {context.Request.RemoteEndPoint}");
            await Task.Run(() => HandleClientRequestAsync(context));
        }
    }

    private async Task HandleClientRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        
        if (request.HttpMethod == "OPTIONS")
        {
            response.StatusCode = 204; 
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Methods", "GET, OPTIONS");
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Access-Control-Max-Age");
            response.Close();
            return;
        }

        try
        {
            if (request.HttpMethod == "GET")
            {
                var input = request.QueryString["input"];
                if (string.IsNullOrEmpty(input) == false)
                {
                    _pool.AddTask(() => SendIndexesAsync(response, input).Wait());
                }
                else
                {
                    await WriteResponseAsync(response, 400, "Invalid input");
                }
            }
            else
            {
                await WriteResponseAsync(response, 405, "Method not allowed");
            }
        }
        catch (Exception ex)
        {
            await WriteResponseAsync(response, 500, $"Internal server error: {ex.Message}");
            throw new Exception($"Internal server error: {ex.Message}");
        }
    }
    
    private async Task SendIndexesAsync(HttpListenerResponse response, string word)
    {
        try
        {
            var indexes = _invertedIndex.GetIndex(word);
            if (indexes == null || indexes.Count == 0)
            {
                await WriteResponseAsync(response, 200, "Nothing found");
            }
            else
            {
                //var message = string.Join("\n", indexes); // old
                var message = _formatter.FormatResponse(indexes, word);
                await WriteResponseAsync(response, 200, message);
            }
        }
        catch (Exception ex)
        {
            await WriteResponseAsync(response, 500, $"Error processing request: {ex.Message}");
        }
    }

    private async Task WriteResponseAsync(HttpListenerResponse response, int statusCode, string message)
    {
        var buffer = System.Text.Encoding.UTF8.GetBytes(message);
        response.StatusCode = statusCode;
        response.ContentType = "text/plain; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.AddHeader("Access-Control-Allow-Origin", "*");
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
    }


    public void StopServer()
    {
        _isRunning = false;
    }
}