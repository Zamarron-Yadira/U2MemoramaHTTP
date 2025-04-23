using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2MemoramaHTTP.Models
{
	public class PartidaHistorial
	{
		public string Jugador1 { get; set; }
		public string Jugador2 { get; set; }
		public int PuntajeJugador1 { get; set; }
		public int PuntajeJugador2 { get; set; }
		public string Resultado { get; set; }
		public TimeSpan Duracion { get; set; }
		public DateTime Fecha { get; set; }

	}


} 
