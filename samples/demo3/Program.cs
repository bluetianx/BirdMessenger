using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using BirdMessenger;
using BirdMessenger.Collections;
using BirdMessenger.Infrastructure;

namespace demo3
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var stream = new MemoryStream(1024 * 1024 * 32);

            for(var i = 0; i < 1024 * 1024 * 32; i++) {
                stream.Write(Encoding.UTF8.GetBytes(BitConverter.ToString(new byte[] { (byte)i }), 0, 2));
            }

            //reset position
            stream.Position = 0;

            // remote tus service
            var hostUri = new Uri(@"http://localhost:5000/files");
            
            // build a standalone tus client instance
            
        }

        
    }
}