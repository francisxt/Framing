using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Codificador;

namespace Servidor
{
    class Program
    {
        static void Main(string[] args)
        {
            int puerto = 8080;
            TcpListener socketEscucha = new TcpListener(IPAddress.Any, puerto); 
            socketEscucha.Start();
            TcpClient cliente = socketEscucha.AcceptTcpClient();
            Codificador.Codificar.DecodificadorTexto decodificador = new Codificador.Codificar.DecodificadorTexto(); 
            Elemento elemento = decodificador.Decodificar(cliente.GetStream());
            Console.WriteLine("Se recibio un elemento codificado en texto:");
            Console.WriteLine(elemento);
            Codificador.Codificar.CodificadorBinario codificador = new Codificador.Codificar.CodificadorBinario(); 
            elemento.precio += 10;
            Console.Write("Enviando elemento en binario..."); 
            byte[] bytesParaEnviar = codificador.Codificar(elemento); 
            Console.WriteLine("(" + bytesParaEnviar.Length + " bytes): ");
            Console.Read();
            cliente.GetStream().Write(bytesParaEnviar, 0, bytesParaEnviar.Length);
            cliente.Close();
            socketEscucha.Stop();
        }
    }
}
