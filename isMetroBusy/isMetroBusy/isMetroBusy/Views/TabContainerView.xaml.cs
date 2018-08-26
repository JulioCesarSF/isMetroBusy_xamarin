using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace isMetroBusy.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TabContainerView : TabbedPage
	{
		public TabContainerView ()
		{
			InitializeComponent ();
		}
	}
}