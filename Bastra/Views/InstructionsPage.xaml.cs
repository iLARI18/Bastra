using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Bastra.Models;
using Bastra.ModelsLogic;
using Bastra.ViewModels;
using Bastra.Views;

namespace Bastra.Views
{
    public partial class InstructionsPage : ContentPage
    {
        public InstructionsPage()
        {
            InitializeComponent();
            BindingContext = new InstructionsPageVM(); // Set the BindingContext

        }      
    }
}
