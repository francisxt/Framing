using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Codificador;

namespace Cliente
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(500);
            IPAddress servidor = IPAddress.Parse("127.0.0.1"); 
            int puerto = 8080; 
            IPEndPoint extremo = new IPEndPoint(servidor, puerto);
            TcpClient cliente = new TcpClient();
            cliente.Connect(extremo);
            NetworkStream flujoRed = cliente.GetStream();
            Elemento elemento = new Elemento(1234567890987654L, "Cadena de Bicicleta", 18, 1000, true, false);
            Elemento elemento2 = new Elemento(1245789632589, "Bujia de Motor", 4, 250, false, true);
            Codificador.Codificar.CodificadorTexto codificador = new Codificador.Codificar.CodificadorTexto(); 
            byte[] datosCodificados = codificador.Codificar(elemento);
            byte[] datosCodificados2 = codificador.Codificar(elemento2);
            Console.WriteLine("Enviando elemento codificado en texto (" + datosCodificados.Length + " bytes): ");
            Console.WriteLine("Enviando elemento codificado en texto (" + datosCodificados2.Length + " bytes): ");
            Console.WriteLine(elemento);
            Console.WriteLine(elemento2);
            flujoRed.Write(datosCodificados, 0, datosCodificados.Length);
            flujoRed.Write(datosCodificados2, 0, datosCodificados.Length);
            Codificador.Codificar.DecodificadorBinario decodificador = new Codificador.Codificar.DecodificadorBinario(); 
            Elemento elementoRecibido = decodificador.Decodificar(cliente.GetStream());
            Console.WriteLine("Se recibio un elemento codificado en formato binario:"); 
            Console.WriteLine(elementoRecibido);
            Console.Read();
            flujoRed.Close();
            cliente.Close();
        }
    }
}
