using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using NUnit.Framework;

namespace Kraka
{
    public class Logger : IDisposable
    {
        ConcurrentBag<IDisposable> _disposables = new ConcurrentBag<IDisposable>();
        Subject<string> _sub = new Subject<string>();
        
        public Logger()
        {
            var udp = new UdpClient("127.0.0.1", 9999);
            _disposables.Add(udp);
            
            _disposables.Add(_sub
                .Select(s => Encoding.UTF8.GetBytes(s))
                .SelectMany(async r =>
                {
                    try
                    {
                        return await udp.SendAsync(r, r.Length);
                    }
                    catch (SocketException)
                    {
                        return 0;
                    }
                })
                .Subscribe());
        }

        public void Write(string message)
            => _sub.OnNext(message);

        public void Dispose()
            => _disposables.ToList().ForEach(d => d.Dispose());
    }
    
    public static class Log 
    {
        static Logger _logger = new Logger();

        public static void Write(string message)
        {
            TestContext.Write(message);
            _logger.Write(message);
        }
        
        public static void WriteLine(string message = null)
            => Write((message ?? "") + Environment.NewLine);
    }
}