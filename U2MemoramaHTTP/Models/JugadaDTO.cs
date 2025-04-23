using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2MemoramaHTTP.Models
{
	public class JugadaDTO
	{
		public int IdSesion { get; set; }       // Para identificar en qué sesión está jugando
		public string Nombre { get; set; } = ""; // Nombre del jugador que hace la jugada
		public int Indice1 { get; set; }         // Índice de la carta seleccionada
		public int Indice2 { get; set; }         // Índice de la carta seleccionada
	}
}
