using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using U2MemoramaHTTP.Services;

namespace U2MemoramaHTTP.ViewModels
{
	public class MemoramaViewModel
	{
		MemoramaServer server = new();
		public ICommand IniciarCommand { get; set; }
		public ICommand DetenerCommand { get; set; }

		public MemoramaViewModel()
		{
			IniciarCommand = new RelayCommand(Iniciar);
			DetenerCommand = new RelayCommand(Detener);
		}

		private void Detener()
		{
			server.Detener();
		}

		private void Iniciar()
		{
			try
			{
				server.Iniciar();
			}
			catch (HttpListenerException ex)
			{
				if (ex.Message.StartsWith("Acceso denegado"))
				{
					ProcessStartInfo p = new ProcessStartInfo
					{
						FileName = "netsh",
						Arguments = "http add urlacl url=http://*:10000/memorama/ user=Todos",
						UseShellExecute = true,
						CreateNoWindow = false,
						Verb = "runas" //correr como administrador
					};
					var res = Process.Start(p);
					res.WaitForExit();
					if (res.ExitCode == 0)
					{
						server.Iniciar();
					}
				}
				
			}
		}
	}
}
