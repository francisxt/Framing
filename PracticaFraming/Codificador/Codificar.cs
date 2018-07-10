using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace Codificador
{
    public class Codificar
    {
        public class ConstantesCodificadorTexto
        {
            public static readonly String CODIFICACION_POR_DEFECTO = "ascii";
            public static readonly int LONG_MAX_FLUJO = 1024;
        }

        public class ConstantesCodificadorBinario
        {
            public static readonly String CODIFICACION_POR_DEFECTO = "ascii";
            public static readonly byte BANDERA_DESCUENTO = 1 << 7;
            public static readonly byte BANDERA_EN_STOCK = 1 << 0;
            public static readonly int LONG_MAX_DESCRIPCION = 255;
            public static readonly int LONG_MAX_FLUJO = 1024;
        }

        public class CodificadorTexto : Codificador.Elemento.CodificadorElemento
        {
            public Encoding codificador;
            public CodificadorTexto() : this(ConstantesCodificadorTexto.CODIFICACION_POR_DEFECTO) { }
            public CodificadorTexto(string datos)
            {
                codificador = Encoding.GetEncoding(datos);
            }
            public byte[] Codificar(Elemento elemento)
            {
                String cadenaCodificada = elemento.numeroElemento + " ";
                if (elemento.descripcion.IndexOf('\n') != -1)
                    throw new IOException("Descripcion no valida (contiene un salto de linea)");
                cadenaCodificada = cadenaCodificada + elemento.descripcion + "\n";
                cadenaCodificada = cadenaCodificada + elemento.cantidad + " ";
                cadenaCodificada = cadenaCodificada + elemento.precio + " ";

                if (elemento.tieneDescuento) 
                    cadenaCodificada = cadenaCodificada + "d";
                if (elemento.enStock) 
                    cadenaCodificada = cadenaCodificada + "s";
                cadenaCodificada = cadenaCodificada + "\n";
                if (cadenaCodificada.Length > ConstantesCodificadorTexto.LONG_MAX_FLUJO)
                    throw new IOException("Longitud codificada demasiado grande");
                byte[] bufer = codificador.GetBytes(cadenaCodificada);
                return bufer;
            }
        }

        public class DecodificadorTexto : Codificador.Elemento.DecodificadorElemento
        {
            public Encoding decodificador;
            public DecodificadorTexto() : this(ConstantesCodificadorTexto.CODIFICACION_POR_DEFECTO) { }
            public DecodificadorTexto(String datoCodificado)
            {
                decodificador = Encoding.GetEncoding(datoCodificado);
            }
            public Elemento Decodificar(Stream flujo)
            {
                String noElemento, descripcion, cant, precio, banderas;
                byte[] espacios = decodificador.GetBytes(" ");
                byte[] saltoLinea = decodificador.GetBytes("\n");
                noElemento = decodificador.GetString(Entramar.SiguienteToken(flujo, espacios));
                descripcion = decodificador.GetString(Entramar.SiguienteToken(flujo, saltoLinea));
                cant = decodificador.GetString(Entramar.SiguienteToken(flujo, espacios));
                precio = decodificador.GetString(Entramar.SiguienteToken(flujo, espacios));
                banderas = decodificador.GetString(Entramar.SiguienteToken(flujo, saltoLinea));
                return new Elemento(Int64.Parse(noElemento), descripcion, Int32.Parse(cant), Int32.Parse(precio), (banderas.IndexOf('d') != -1), (banderas.IndexOf('s') != -1));
            }
            public Elemento Decodificar(byte[] paquete)
            {
                Stream cargaUtil = new MemoryStream(paquete, 0, paquete.Length, false);
                return Decodificar(cargaUtil);
            }
        }

        public class CodificadorBinario : Codificador.Elemento.CodificadorElemento
        {
            public Encoding codificador;
            public CodificadorBinario() : this(ConstantesCodificadorBinario.CODIFICACION_POR_DEFECTO) { }
            public CodificadorBinario(String datos)
            {
                codificador = Encoding.GetEncoding(datos);
            }
            public byte[] Codificar(Elemento elemento)
            {
                MemoryStream flujoMemoria = new MemoryStream();
                BinaryWriter escritorBinario = new BinaryWriter(new BufferedStream(flujoMemoria));
                escritorBinario.Write(IPAddress.HostToNetworkOrder(elemento.numeroElemento));
                escritorBinario.Write(IPAddress.HostToNetworkOrder(elemento.cantidad));
                escritorBinario.Write(IPAddress.HostToNetworkOrder(elemento.precio));
                byte banderas = 0;
                if (elemento.tieneDescuento)
                    banderas |= ConstantesCodificadorBinario.BANDERA_DESCUENTO;

                if (elemento.enStock)
                    banderas |= ConstantesCodificadorBinario.BANDERA_EN_STOCK;
                escritorBinario.Write(banderas);
                byte[] bytesDescripcion = codificador.GetBytes(elemento.descripcion);
                if (bytesDescripcion.Length > ConstantesCodificadorBinario.LONG_MAX_DESCRIPCION)
                    throw new IOException("La descripcion del elemento excede el límite establecido");

                escritorBinario.Write((byte)bytesDescripcion.Length);
                escritorBinario.Write(bytesDescripcion);
                escritorBinario.Flush();

                return flujoMemoria.ToArray();
            }
        }
        public class DecodificadorBinario : Codificador.Elemento.DecodificadorElemento
        {
            public Encoding decodificador;
            public DecodificadorBinario() : this(ConstantesCodificadorBinario.CODIFICACION_POR_DEFECTO) { }
            public DecodificadorBinario(String datos) { decodificador = Encoding.GetEncoding(datos); }
            public Elemento Decodificar(Stream flujo)
            {
                BinaryReader lectorBinario = new BinaryReader(new BufferedStream(flujo)); 
                long noElemento = IPAddress.NetworkToHostOrder(lectorBinario.ReadInt64()); 
                int cant = IPAddress.NetworkToHostOrder(lectorBinario.ReadInt32()); 
                int precio = IPAddress.NetworkToHostOrder(lectorBinario.ReadInt32());
                byte banderas = lectorBinario.ReadByte();
                int longCadena = lectorBinario.Read(); 
                if (longCadena == -1) 
                    throw new EndOfStreamException();
                byte[] buferDescripcion = new byte[longCadena]; 
                lectorBinario.Read(buferDescripcion, 0, longCadena);
                String descripcion = decodificador.GetString(buferDescripcion); 
                return new Elemento(noElemento, descripcion, cant, precio, ((banderas & ConstantesCodificadorBinario.BANDERA_DESCUENTO) == ConstantesCodificadorBinario.BANDERA_DESCUENTO), ((banderas & ConstantesCodificadorBinario.BANDERA_EN_STOCK) == ConstantesCodificadorBinario.BANDERA_EN_STOCK));
            }
            public Elemento Decodificar(byte[] paquete)
            { Stream cargaUtil = new MemoryStream(paquete, 0, paquete.Length, false); return Decodificar(cargaUtil); }

        }
    }
}
