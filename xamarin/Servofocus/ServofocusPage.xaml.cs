﻿using Xamarin.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.ES30;

namespace Servofocus
{
    public partial class ServofocusPage : ContentPage
    {


        public ServofocusPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UrlField.Text = Marshal.PtrToStringAnsi(Interop.ServoVersion());
            Debug.WriteLine("OnAppearing");
        }


		protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Debug.WriteLine("OnDisappearing");
        }
    }
}
