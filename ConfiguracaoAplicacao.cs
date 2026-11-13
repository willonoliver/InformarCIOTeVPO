using System.Runtime.CompilerServices;
using System.Windows.Forms;

/// <summary>
/// Configuração de inicialização da aplicação Windows Forms
/// 
/// Esta classe configura as opções visuais e de compatibilidade que devem
/// ser aplicadas antes de executar a aplicação.
/// </summary>
namespace InformarCIOTeVPO;

[CompilerGenerated]
internal static class ConfiguracaoAplicacao
{
	/// <summary>
	/// Inicializa as configurações visuais da aplicação
	/// 
	/// - EnableVisualStyles: Habilita os estilos visuais modernos do Windows
	/// - SetCompatibleTextRenderingDefault: Define o modo de renderização de texto
	/// - SetHighDpiMode: Ativa suporte para monitores de alta resolução (DPI)
	/// </summary>
	public static void Inicializar()
	{
		// Habilita estilos visuais modernos do Windows
		Application.EnableVisualStyles();

		// Define compatibilidade de renderização de texto
		Application.SetCompatibleTextRenderingDefault(false);

		// Ativa suporte para High DPI (telas com alta densidade de pixels)
		Application.SetHighDpiMode((HighDpiMode)1);
	}
}
