using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2MemoramaHTTP.Models
{
	public class JugadorDTO
	{
		public string Nombre { get; set; } = "";
		public int IdSesion { get; set; } 
		public  int Puntaje { get; set; }
		public int CantidadPares { get; set; }
		public TimeSpan TiempoTotal { get; set; }
		public DateTime? TiempoInicioJugada { get; set; }
	}
}
