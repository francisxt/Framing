using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codificador
{
    public class Elemento
    {
        public long numeroElemento;
        public String descripcion;
        public int cantidad;
        public int precio;
        public Boolean tieneDescuento;
        public Boolean enStock;
        public Elemento(long id, string descripcion, int cant, int precio, bool tieneDescuento, bool enStock)
        {
            numeroElemento = id; this.descripcion = descripcion; cantidad = cant; this.precio = precio; this.tieneDescuento = tieneDescuento; this.enStock = enStock;
        }
        public override string ToString()
        {
            String separador = "\n"; String valor = "ID#=" + numeroElemento + separador + "Descripcion=" + descripcion + separador + "Cantidad=" + cantidad + separador + "Precio=" + precio + separador + "Precio Total=" + (cantidad * precio);
            if (tieneDescuento) valor += " (descuento)";
            if (enStock) valor += separador + "En Stock" + separador;
            else valor += separador + "No en Stock" + separador;
            return valor;
        }

        public interface CodificadorElemento
        {
            byte[] Codificar(Elemento elemento);
        }
        public interface DecodificadorElemento
        {
            Elemento Decodificar(Stream dato);
            Elemento Decodificar(byte[] paquete);

        }
    }
}



    
