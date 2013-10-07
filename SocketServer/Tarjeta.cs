using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    
    
    public class Tarjeta
    {
        
        public int idTarjeta;
        public string numerodetarjeta;
        public int idEmpleado;
        public bool estado;
        public string accessLevels;
        public DateTime ultimaActualizacion;
        public int OrgID;

        // NOTA: el -1 en idEmpleado es para identificar un empleado con tarjeta no definida
        public Tarjeta(int pidOrg, int pidTarjeta, string pnumerodetarjeta, int pidEmpleado, bool pestado, string v_accessLevels, DateTime v_ultAct)
        {
            numerodetarjeta = pnumerodetarjeta;            
            OrgID = pidOrg; 
            idTarjeta = pidTarjeta;
            idEmpleado = pidEmpleado;
            estado = pestado;
            accessLevels = v_accessLevels;
            ultimaActualizacion = v_ultAct;
        }
        public Tarjeta()
        {
            idEmpleado = -1;
            accessLevels = string.Empty;
        }
        public int IDTARJETA
        {
            get
            {
                return idTarjeta;
            }
            set
            {
                idTarjeta = value;
            }
        }
        public string NUMERODETARJETA
        {
            get
            {
                return numerodetarjeta;
            }
            set
            {
                numerodetarjeta = value;
            }
        }
        public int IDEMPLEADO
        {
            get
            {
                return idEmpleado;
            }
            set
            {
                idEmpleado = value;
            }
        }
        public bool ESTADO
        {
            get
            {
                return estado;
            }
            set
            {
                estado = value;
            }
        }

        public string ACCESSLEVELS
        {
            get
            {
                return accessLevels;
            }
            set
            {
                accessLevels = value;
            }
        }

        public DateTime ULTIMAACTUALIZACION
        {
            get
            {
                return ultimaActualizacion;
            }
            set
            {
                ultimaActualizacion = value;
            }
        }

        public int ORGID
        {
            get
            {
                return OrgID;
            }
            set
            {
                OrgID = value;
            }
        }




    }

}
