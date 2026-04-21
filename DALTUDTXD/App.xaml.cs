using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DALTUDTXD.Views;

namespace DALTUDTXD
{
    public partial class App : Application
    {
        protected void ApplicationStart(object sender, StartupEventArgs e)
        {
            var loginView = new LoginView();
            loginView.Show();
        }
    }
}

