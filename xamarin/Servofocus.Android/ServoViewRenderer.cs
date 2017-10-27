﻿using System;
using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
using Servofocus;
using Servofocus.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.Runtime.InteropServices;
using Android.Graphics;
using Android.Views;

[assembly: ExportRenderer(typeof(ServoView), typeof(ServoViewRenderer))]
namespace Servofocus.Android
{
    public class ServoViewRenderer : ViewRenderer<ServoView, GLSurfaceView>
    {
        bool _disposed;
        private float _lastY;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void interopDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void logDelegate(string log);

        protected override void OnElementChanged(ElementChangedEventArgs<ServoView> e)
        {
            base.OnElementChanged(e);



            if (e.NewElement != null)
            {
                //e.NewElement.ScrollRequested += OnScrollRequested;

                GLSurfaceView surfaceView = Control;
                if (surfaceView == null)
                {
                    surfaceView = new GLSurfaceView(Context);
                    surfaceView.SetEGLContextClientVersion(3);
                    surfaceView.SetEGLConfigChooser(8, 8, 8, 8, 24, 0);


                    var hostCallbackInstance = ServoSharp.HostCallbacks.__CreateInstance(new ServoSharp.HostCallbacks.__Internal
                    {
                        wakeup = Marshal.GetFunctionPointerForDelegate( new interopDelegate(() => 
                                                                                            Control.QueueEvent(() => {
                            System.Diagnostics.Debug.WriteLine("FOO");
                            var x = ServoSharp.libservobridge.PerformUpdates();
                            System.Diagnostics.Debug.WriteLine("BAR");
                        }))),
                        flush = Marshal.GetFunctionPointerForDelegate(new interopDelegate(() => Control.RequestRender())),
                                    
                        log = Marshal.GetFunctionPointerForDelegate(new logDelegate(str => System.Diagnostics.Debug.WriteLine(str)))
                    });

                    var renderer = new Renderer(
                        hostCallbackInstance
                    );

                    surfaceView.SetRenderer(renderer);
                    SetNativeControl(surfaceView);

                    Touch += OnTouch;
                    Element.RegisterGLCallback(callback => Control.QueueEvent(callback));
                }
                Control.RenderMode = Rendermode.WhenDirty;
            }
        }
        
        private void OnTouch(object sender, TouchEventArgs touchEventArgs)
        {
            var x = touchEventArgs.Event.RawX;
            var y = touchEventArgs.Event.RawY;
            var delta = y - _lastY;
            _lastY = touchEventArgs.Event.RawY;

            // https://developer.android.com/reference/android/view/MotionEvent.html
            System.Diagnostics.Debug.WriteLine(touchEventArgs.Event);

            if(touchEventArgs.Event.Action == MotionEventActions.Up)
                Element.OnTap(x, y);
            else if (touchEventArgs.Event.Action == MotionEventActions.Move)
            {
                // need to find gesture status
                // https://github.com/mozilla-mobile/focus-android/blob/a61745794f28f4ac924fed5d9b62d1a03fea3613/app/src/gecko/java/org/mozilla/focus/web/NestedGeckoView.java
                Element.OnScroll(GestureStatus.Started, delta);
            }
        }

        private void OnScrollRequested(object sender, EventArgs args)
        {
            var e = (ScrollArgs)args;
            Control.QueueEvent(() =>
            {
                if (e.status == GestureStatus.Started) {
                    ServoSharp.libservobridge.Scroll(e.dx, e.dy, e.x, e.y, ServoSharp.ScrollState.ScrollStateStart);
                } else if (e.status == GestureStatus.Running) {
                    ServoSharp.libservobridge.Scroll(e.dx, e.dy, e.x, e.y, ServoSharp.ScrollState.ScrollStateMove);
                } else {
                    ServoSharp.libservobridge.Scroll(e.dx, e.dy, e.x, e.y, ServoSharp.ScrollState.ScrollStateEnd);
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        class Renderer : Java.Lang.Object, GLSurfaceView.IRenderer
        {
            ServoSharp.HostCallbacks _callbacks;

            public Renderer(ServoSharp.HostCallbacks callbacks)
			{
                _callbacks = callbacks;
			}

			public void OnDrawFrame(IGL10 gl)
			{
			}

			public void OnSurfaceChanged(IGL10 gl, int width, int height)
			{
                System.Diagnostics.Debug.WriteLine("Resize:" + width + "x" + height);
			}


			public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
            {
                //Interop.InitWithEgl(
                    //() => _wakeup(),
                    //() => _flush(),
                    //(str) => {}, // System.Diagnostics.Debug.WriteLine("[servo] " + Marshal.PtrToStringAnsi(str)),
                    //540, 740);


                var viewLayout = ServoSharp.ViewLayout.__CreateInstance(new ServoSharp.ViewLayout.__Internal
                {
                    hidpi_factor = 1f,
                    margins = new ServoSharp.Margins.__Internal(),
                    position = new ServoSharp.Position.__Internal(),
                    view_size = new ServoSharp.Size.__Internal()
                });


                ServoSharp.libservobridge.InitWithEgl(_callbacks, viewLayout);
			}
        }

    }
}
