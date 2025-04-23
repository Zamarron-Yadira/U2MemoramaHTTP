using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2MemoramaHTTP.Models
{
	public class MemoramaDTO
	{
		public int IdSesion { get; set; }
		public string? Jugador1 { get; set; }
		public string? Jugador2 { get; set; }
		public int[] Cartas { get; set; } = Array.Empty<int>(); // -1 si ya fue emparejada
		public string? Turno { get; set; }
		public int ParesJugador1 { get; set; }
		public int ParesJugador2 { get; set; }
		public int Movimientos { get; set; }
		public string Estado { get; set; } = "jugando"; // o "finalizado"
		public string? Ganador { get; set; } // Se llena cuando el estado es "finalizado"

	}
}
