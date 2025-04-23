using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2MemoramaHTTP.Models
{
	public class CartaDTO
	{

		//REVISAR
		public int Id { get; set; }
		public string ImagenUrl { get; set; }
		public int Indice { get; set; }
		public bool EstaVolteada { get; set; }
		public bool EstaEmparejada { get; set; }

	}
}
