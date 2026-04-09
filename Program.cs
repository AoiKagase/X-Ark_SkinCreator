using System.Windows.Forms;
using SkinCreator.UI;

namespace SkinCreator;

static class Program
{
	[STAThread]
	static void Main()
	{
		ApplicationConfiguration.Initialize();
		Application.Run(new MainEditorForm());
	}
}
