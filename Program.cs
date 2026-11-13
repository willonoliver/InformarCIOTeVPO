using System;
using System.Windows.Forms;

/// <summary>
/// Ponto de entrada da aplicação InformarCIOTeVPO
/// 
/// Responsável por inicializar as configurações da aplicação e executar
/// o formulário principal em Single-Threaded Apartment (STA) mode, conforme
/// requerido pelo Windows Forms.
/// </summary>
namespace InformarCIOTeVPO;

internal static class Program
{
	/// <summary>
	/// Ponto de entrada principal da aplicação
	/// </summary>
	[STAThread]
	private static void Main()
	{
		// Inicializa as configurações visuais e de compatibilidade
		ConfiguracaoAplicacao.Inicializar();

		// Executa o formulário principal
		Application.Run((Form)(object)new Formulario());
	}
}
