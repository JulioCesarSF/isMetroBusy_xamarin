using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using isMetroBusy.ViewModels;
using Xamarin.Forms;

namespace isMetroBusy.Views
{
    public partial class LinhasView : ContentPage
    {
        public LinhasView()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            var vm = BindingContext as LinhasViewModel;
            vm._cancelTask = new CancellationTokenSource();
            vm.StartAutoUpdate();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            //var vm = BindingContext as LinhasViewModel;
            //vm._cancelTask.Cancel();
            base.OnDisappearing();
        }
    }
}
