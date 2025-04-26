using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2MemoramaHTTP.Models
{
	public class SesionMemorama
	{
		public int Id { get; set; }
		public string Jugador1 { get; set; } = "";
		public string Ip1 { get; set; } = "";
		public string Jugador2 { get; set; } = "";
		public string Ip2 { get; set; } = "";

		public int[] Cartas { get; private set; } = new int[0];
		public string Turno { get; private set; } = "";
		public int ParesJugador1 { get; private set; } = 0;
		public int ParesJugador2 { get; private set; } = 0;
		public string Estado { get; private set; } = "jugando";

		private int? cartaSeleccionada = null;
		public bool EstaCompleto => Jugador1 != "" && Jugador2 != "";
		public DateTime InicioJuego { get; private set; }  // Guarda cuándo arrancó la partida

		public TimeSpan Duracion => Estado == "finalizado"
			? (DateTime.Now - InicioJuego)
			: TimeSpan.Zero;



		private static readonly Random rnd = new Random();

		public SesionMemorama(int id)
		{
			Id = id;
			GenerarCartas();
		}

		public void AgregarJugador(string nombre, string ip)
		{
			if (Jugador1 == "")
			{
				Jugador1 = nombre;
				Ip1 = ip;
				Turno = nombre;
				Estado = "Esperando";
				return; //salirme para no revisar al segundo jugador
			}
			if (Jugador2 == "")
			{
				if (nombre == Jugador1)
				{
					throw new ArgumentException("Los jugadores no puede tener el mismo nombre");
				}

				Jugador2 = nombre;
				Ip2 = ip;

				Estado = "Jugando"; //1 Jugando
			}
			if (Jugador2 != "" && Estado == "Jugando")
			{
				InicioJuego = DateTime.Now;
			}
		}

		private int[] GenerarCartasMezcladas(int pares)
		{
			List<int> cartas = new();
			for (int i = 0; i < pares; i++)
			{
				cartas.Add(i);
				cartas.Add(i);
			}

			return cartas.OrderBy(x => rnd.Next()).ToArray();
		}
		public void GenerarCartas()
		{
			Cartas = GenerarCartasMezcladas(6); //6 pares (12 cartas)
		}

		public bool ValidarTurno(string nombre)
		{
			return nombre == Turno;
		}

		public bool ValidarMovimiento(int indice) =>
			indice >= 0 && indice < Cartas.Length && Cartas[indice] != -1;

		public bool ValidarMovimientoDoble(int indice1, int indice2)
		{
			return indice1 != indice2 &&
				   ValidarMovimiento(indice1) &&
				   ValidarMovimiento(indice2);
		}


		public void RealizarMovimiento(int indice1, int indice2)
		{
			// Verifica si es pareja
			if (EsPareja(indice1, indice2))
			{
				VoltearCartas(indice1, indice2); // Marca como emparejadas
				if (Turno == Jugador1)
					ParesJugador1++;
				else
					ParesJugador2++;
			}
			else
			{
				// Si no son pareja, cambia el turno
				Turno = (Turno == Jugador1) ? Jugador2 : Jugador1;
			}

			if (VerificarJuegoCompleto())
			{
				Estado = "finalizado";
			}
		}
		
		public bool VerificarJuegoCompleto()
		{
			return Cartas.All(c => c == -1);
		}

		public string VerificarGanador()
		{
			if (Estado != "finalizado") return "sin finalizar";

			if (ParesJugador1 > ParesJugador2)
				return Jugador1 ?? "Jugador1";
			else if (ParesJugador2 > ParesJugador1)
				return Jugador2 ?? "Jugador2";
			else
				return "empate";

		}
		
		public bool EsPareja(int indice1, int indice2)
		{
			return Cartas[indice1] == Cartas[indice2] && indice1 != indice2;
		}


		///Ver si puedo agregar lo de seleccionar si quieres jugar solo o multijugador(EXTRA)
		public void VoltearCartas(int indice1, int indice2)
		{
			Cartas[indice1] = -1;
			Cartas[indice2] = -1;

		}

	}
}

