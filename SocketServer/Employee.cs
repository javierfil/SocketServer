using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class Employee
    {
        public const int NODEFINIDO = -1;               // Id de usuario no definido

        public int Id;          
        public string Nombre;
        public string Apellido;
        public bool Sexo;       // TRUE: Masculino, FALSE: Femenino
        public string EMail;
        public string Direccion;
        public string Nacionalidad;
        public string Ciudad;
        public string Departamento;
        public DateTime FechaNacimiento;
        public string Telefono;
        public string Celular;
        public string TipoDocumento;
        public string NumeroDocumento;
        public DateTime FechaExpedicionDocumento;
        public DateTime FechaVencimientoDocumento;
        public DateTime FechaVencimientoCarnetSalud;
        public DateTime FechaIngresoBPS;
        public string Empresa;
        public string Imagen;
        public int imageVersion;
        public DateTime ultimaActualizacion;

        public int VersionEmpleado;
        public int PersonID;

        public byte[] imageDataBytes;      // byte array correspondiente a la imagen del empleado

        public Employee()
        {
            imageDataBytes = null;
            FechaNacimiento = new DateTime(2012,1,1);
            FechaVencimientoCarnetSalud = new DateTime(2012, 1, 1);
            FechaExpedicionDocumento = new DateTime(2012, 1, 1);
            FechaVencimientoDocumento = new DateTime(2012, 1, 1);
            //VencimientoLibretaConducir = new DateTime(2012, 1, 1);
            FechaIngresoBPS = new DateTime(2012, 1, 1); 
            //VigenciaBSE = new DateTime(2012, 1, 1);
        }

        public void attachImage(byte[] v_imagen)
        {
            imageDataBytes = v_imagen;
        }

        public bool hasImage()
        {
            return imageDataBytes != null;
        }
        public byte[] getImage()
        {
            return imageDataBytes;
        }

    }
}
