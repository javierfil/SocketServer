using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{

    public enum TiposLOG
    {
        ALL,
        ALUTRACK,
        HH,
        LENEL
    }

    public enum EstadosCiviles
    {
        Soltero,
        Casado,
        Viudo
    }

    public enum CategoríasLibretasConducir
    {
        NoTiene,
        CategoriaA,
        CategoriaB
    }

    public enum TiposAcceso
    {
        Entrada,
        Salida,
        EINVALIDO,
        SINVALIDO,
        INVALIDO
    }

    public enum GateAccessType
    {
        Entrance,             // Puerta de entrada
        Exit,                 // Puerta de salida
        Granted,              // Puerta para acceder a la zona
        Forbidden             // Puerta para definir zonas de acceso prohibidas

    }
}
