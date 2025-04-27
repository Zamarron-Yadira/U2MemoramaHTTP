using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using U2MemoramaHTTP.Models;

namespace U2MemoramaHTTP.Services
{
	public class MemoramaServer
	{
		HttpListener listener = new HttpListener();
		byte[] PaginaIndex;
		List<SesionMemorama> sesiones = new();



		public MemoramaServer()
		{
				PaginaIndex = File.ReadAllBytes("assets/index.html");
		}

		public void Iniciar()
		{
			if (!listener.IsListening)
			{
				listener = new();
				listener.Prefixes.Add("http://*:10000/memorama/");
				listener.Start();
				new Thread(EscucharPeticiones) { IsBackground = true }.Start();
			}
		}

		public void Detener()
		{
			if (listener.IsListening)
			{
				listener.Stop();
			}
		}
	


		private void EscucharPeticiones()
		{
			try
			{
				var context = listener.GetContext();
				new Thread(EscucharPeticiones) { IsBackground = true }.Start();

				if (context.Request.HttpMethod == "GET")
				{
					string path = context.Request.Url.AbsolutePath;

					if (path.StartsWith("/memorama/assets/"))
					{
						string filePath = Path.Combine("assets", path.Substring("/memorama/assets/".Length));

						if (File.Exists(filePath))
						{
							string extension = Path.GetExtension(filePath).ToLower();
							string contentType = extension switch
							{
								".png" => "image/png",
								".jpg" => "image/jpeg",
								".jpeg" => "image/jpeg",
								".gif" => "image/gif",
								_ => "application/octet-stream"
							};

							context.Response.ContentType = contentType;
							byte[] fileBytes = File.ReadAllBytes(filePath);
							context.Response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
						}
						else
						{
							context.Response.StatusCode = 404;
							byte[] buffer = Encoding.UTF8.GetBytes("Archivo no encontrado.");
							context.Response.OutputStream.Write(buffer, 0, buffer.Length);
						}

						context.Response.OutputStream.Close();
						return;
					}


					switch (context.Request.RawUrl)
					{
						case "/memorama":
							PaginaIndex = File.ReadAllBytes("assets/index.html");
							context.Response.ContentLength64 = PaginaIndex.Length;
							context.Response.ContentType = "text/html";
							context.Response.StatusCode = 200;
							context.Response.OutputStream.Write(PaginaIndex);

						break;
					

						default:
							context.Response.StatusCode = 404;
						break;

					}
				}

				else if (context.Request.HttpMethod == "POST")
				{
					// Verificar si es una solicitud para un archivo estático
					if (context.Request.RawUrl.StartsWith("/memorama/assets/"))
					{
						string archivo = context.Request.RawUrl.Substring("/memorama/assets/".Length);

						// Obtener la ruta absoluta completa al archivo
						string ruta = Path.Combine(Directory.GetCurrentDirectory(), "assets", archivo);

						if (File.Exists(ruta))
						{
							byte[] contenido = File.ReadAllBytes(ruta);
							context.Response.ContentType = GetMimeType(ruta);
							context.Response.ContentLength64 = contenido.Length;
							context.Response.OutputStream.Write(contenido, 0, contenido.Length);
							context.Response.StatusCode = 200;
						}
						else
						{
							context.Response.StatusCode = 404;
						}
						return;
					}

					switch (context.Request.RawUrl)
					{
						case "/memorama/quierojugar":

							//Recuperar el nombre del jugador y su ip:
							var buffer = new byte[context.Request.ContentLength64];
							context.Request.InputStream.Read(buffer);
							var json = Encoding.UTF8.GetString(buffer);
							var jugador = JsonSerializer.Deserialize<JugadorDTO>(json);

							if (jugador != null)
							{
								var ip = context.Request.RemoteEndPoint.ToString();

								//Verificar si el jugador ya está en una sesión
								SesionMemorama? sesion = sesiones.FirstOrDefault(x =>
								  x.Jugador1 == jugador.Nombre && x.Ip1 == ip || x.Jugador2 == jugador.Nombre &&
								  x.Ip2 == ip);



								//if (sesion != null)
								//{
								//	context.Response.StatusCode = 400;
								//}

								// Verificar si ya existe un jugador con el mismo nombre en alguna sesión
								if (sesiones.Any(x => x.Jugador1 == jugador.Nombre || x.Jugador2 == jugador.Nombre))
								{
									// Si ya hay un jugador con el mismo nombre en la sesión, rechazar la solicitud
									context.Response.StatusCode = 400; // Bad Request
									var errorResponse = new { error = "Los jugadores no pueden tener el mismo nombre." };
									byte[] errorData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorResponse));
									context.Response.ContentLength64 = errorData.Length;
									context.Response.ContentType = "application/json";
									context.Response.OutputStream.Write(errorData);
								}
								else
								{
									sesion = sesiones.FirstOrDefault(x => x.EstaCompleto == false);

									if (sesion == null)
									{
										sesion = new SesionMemorama(sesiones.Count + 1, jugador.CantidadPares);
										
										sesion.AgregarJugador(jugador.Nombre, ip);
										sesiones.Add(sesion);
									}
									else
									{
										sesion.AgregarJugador(jugador.Nombre, ip);
									}
									if (sesion.EstaCompleto)
									{
										sesion.GenerarCartas(jugador.CantidadPares); // Solo se generan una vez
									}

									Console.WriteLine($"Jugador1: {sesion.Jugador1}, Jugador2: {sesion.Jugador2}, Completa: {sesion.EstaCompleto}");

									//Long polling para primer jugador
									while (sesion.EstaCompleto == false)
									{
										Thread.Sleep(500);
									}

									var dto = new MemoramaDTO() //Prepar dto para enviarlo a ambos
									{
										Jugador1 = sesion.Jugador1,
										Jugador2 = sesion.Jugador2,
										Cartas = sesion.Cartas,
										Turno = sesion.Turno,
										IdSesion = sesion.Id,
										ParesJugador1 = sesion.ParesJugador1,
										ParesJugador2 = sesion.ParesJugador2,
										Estado = sesion.Estado
									};

									byte[] dato = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));
									context.Response.ContentLength64 = dato.Length;
									context.Response.ContentType = "application/json";
									context.Response.OutputStream.Write(dato);
									context.Response.StatusCode = 200;
								}
							}
							break;

						
						case "/memorama/esperando":
							buffer = new byte[context.Request.ContentLength64];
							context.Request.InputStream.Read(buffer);
							json = Encoding.UTF8.GetString(buffer);
							jugador = JsonSerializer.Deserialize<JugadorDTO>(json);

							if (jugador != null)
							{
								var ip = context.Request.RemoteEndPoint.ToString();

								
								SesionMemorama? sesion = sesiones.FirstOrDefault(x => x.Id == jugador.IdSesion && (x.Jugador1 == jugador.Nombre || x.Jugador2 == jugador.Nombre));

								if (sesion == null)
								{
									context.Response.StatusCode = 404;
								}
								else
								{
									
									while (sesion.Turno != jugador.Nombre && sesion.Estado != "finalizado")
									{
										Thread.Sleep(500);
										
									}
								
										var dto = new MemoramaDTO()//enviarlo a ambos
										{
											Estado = sesion.Estado,
											Jugador1 = sesion.Jugador1,
											Jugador2 = sesion.Jugador2,
											Cartas = sesion.Cartas,
											Turno = sesion.Turno,
											IdSesion = sesion.Id,
											
											ParesJugador1 = sesion.ParesJugador1,
											ParesJugador2 = sesion.ParesJugador2
											
										};

										byte[] datos = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));
										context.Response.ContentLength64 = datos.Length;
										context.Response.ContentType = "application/json";
										context.Response.OutputStream.Write(datos);
										context.Response.StatusCode = 200;
								}
							}
							break;


						case "/memorama/jugada":
							buffer = new byte[context.Request.ContentLength64];
							context.Request.InputStream.Read(buffer);
							json = Encoding.UTF8.GetString(buffer);
							var jugada = JsonSerializer.Deserialize<JugadaDTO>(json);

							if (jugada != null)
							{
								var sesion = sesiones.FirstOrDefault(x => x.Id == jugada.IdSesion);

								if (sesion == null)
								{
									context.Response.StatusCode = 404;
									return;
								}

								if (sesion.ValidarTurno(jugada.Nombre) && sesion.ValidarMovimiento(jugada.Indice1) &&
									sesion.ValidarMovimiento(jugada.Indice2))
								{
									sesion.RealizarMovimiento(jugada.Indice1, jugada.Indice2);


									var dto = new MemoramaDTO() //enviar a ambos
									{
										Estado = sesion.Estado,
										Jugador1 = sesion.Jugador1,
										Jugador2 = sesion.Jugador2,
										Cartas = sesion.Cartas,
										Turno = sesion.Turno,
										IdSesion = sesion.Id,
										ParesJugador1 = sesion.ParesJugador1,
										ParesJugador2 = sesion.ParesJugador2
										
									};


									byte[] datos = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));
									context.Response.ContentLength64 = datos.Length;
									context.Response.ContentType = "application/json";
									context.Response.StatusCode = 200;
									context.Response.OutputStream.WriteAsync(datos, 0, datos.Length);

								

								}
								else
								{
									context.Response.StatusCode = 400;
									var error = new { error = "Movimiento inválido o no es tu turno." };
									var errorJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(error));
									context.Response.ContentType = "application/json";
									context.Response.ContentLength64 = errorJson.Length;
									context.Response.OutputStream.WriteAsync(errorJson, 0, errorJson.Length);
									context.Response.OutputStream.Close();
								}
							}
							break;

						//case "/memorama/abandonar":
						//	var bufferAbandonar = new byte[context.Request.ContentLength64];
						//	context.Request.InputStream.Read(bufferAbandonar);
						//	var jsonAbandonar = Encoding.UTF8.GetString(bufferAbandonar);
						//	var abandono = JsonSerializer.Deserialize<JugadorDTO>(jsonAbandonar);

						//	if (abandono != null)
						//	{
						//		// Buscar la sesión donde estaba este jugador
						//		var sesionAbandonada = sesiones.FirstOrDefault(x =>
						//			x.Jugador1 == abandono.Nombre || x.Jugador2 == abandono.Nombre);

						//		if (sesionAbandonada != null)
						//		{
						//			sesionAbandonada.MarcarComoAbandonado(); 
						//		}
						//	}

						//	context.Response.StatusCode = 200;
						//	break;




					}
				}
				context.Response.Close(); 
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
		}

		private static string GetMimeType(string filePath)
		{
			string extension = Path.GetExtension(filePath).ToLower();
			return extension switch
			{
				".png" => "image/png",
				".jpg" => "image/jpeg",
				".jpeg" => "image/jpeg",
				".gif" => "image/gif",
				".css" => "text/css",
				".js" => "application/javascript",
				".html" => "text/html",
				_ => "application/octet-stream",
			};
		}



		
	}
}
