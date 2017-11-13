﻿using System;
using System.Runtime.InteropServices;

namespace ServoSharp
{
    public delegate void SimpleCallbackDelegate();
    public delegate void LogCallbackDelegate(string log);
    public delegate void TitleChangedCallbackDelegate(string title);
    public delegate void UrlChangedCallbackDelegate(string url);
    public delegate void HistoryChangedCallbackDelegate(bool canGoBack, bool canGoForward);

    public class Servo
    {
        readonly ServoSharp _servoSharp = new ServoSharp();
        const string Url = "https://servo.org";
        const string ResourcePath = "/sdcard/servo/resources";
        Size _viewSize;
        float _hidpiFactor = 2f;
        public Margins Margins { get; } = new Margins(); 
        public Position Position { get; } = new Position();
        public HostCallbacks HostCallbacks { get; private set; }
        public ViewLayout ViewLayout { get; private set; }

        SimpleCallbackDelegate _wakeUp;
        SimpleCallbackDelegate _flush;
        LogCallbackDelegate _log;
        SimpleCallbackDelegate _loadStarted;
        SimpleCallbackDelegate _loadEnded;
        TitleChangedCallbackDelegate _titleChanged;
        UrlChangedCallbackDelegate _urlChanged;
        HistoryChangedCallbackDelegate _historyChanged;

        public unsafe string ServoVersion => Marshal.PtrToStringAnsi((IntPtr) _servoSharp.ServoVersion());

        public void InitWithEgl()
        {
            CheckServoResult(() => _servoSharp.InitWithEgl(Url, ResourcePath, HostCallbacks, ViewLayout));
        }
        
        public void Resize(uint height, uint width)
        {
            _viewSize = new Size { Height = height, Width = width };
            CheckServoResult(() => _servoSharp.Resize(CreateLayout()));
        }

        public void PerformUpdates()
        {
            CheckServoResult(() => _servoSharp.PerformUpdates());
        }

        public void Scroll(int dx, int dy, uint x, uint y, ScrollState state)
        {
            CheckServoResult(() => _servoSharp.Scroll(dx, dy, x, y, state));
        }

        public void Click(uint x, uint y)
        {
            CheckServoResult(() => _servoSharp.Click(x, y));
        }

        ViewLayout CreateLayout()
        {
            return new ViewLayout
            {
                Margins = Margins,
                Positions = Position,
                ViewSize = _viewSize,
                HidpiFactor = _hidpiFactor,
            };
        }

        public void SetSize(uint height, uint width)
        {
            _viewSize = new Size {Height = height, Width = width};
        }

        public void SetHidpiFactor(float hidpiFactor)
        {
            _hidpiFactor = hidpiFactor;
        }

        public void SetUrlCallback(Action<string> urlChangedCallback)
        {
            _urlChanged = new UrlChangedCallbackDelegate(urlChangedCallback);
        }

        public void SetHostCallbacks(Action<Action> wakeUp, Action flush, Action<string> log, Action loadStarted, Action loadEnded, 
            Action<string> titleChanged, Action<bool, bool> historyChanged)
        {
            _wakeUp = () => wakeUp(PerformUpdates);
            _flush = new SimpleCallbackDelegate(flush);
            _log = new LogCallbackDelegate(log);
            _loadStarted = new SimpleCallbackDelegate(loadStarted);
            _loadEnded = new SimpleCallbackDelegate(loadEnded);
            _titleChanged = new TitleChangedCallbackDelegate(titleChanged);
            _historyChanged = new HistoryChangedCallbackDelegate(historyChanged);
        }

        public void ValidateCallbacks()
        {
            if(_wakeUp == null) throw new ArgumentNullException(nameof(_wakeUp));
            if(_flush == null) throw new ArgumentNullException(nameof(_flush));
            if(_log == null) throw new ArgumentNullException(nameof(_log));
            if(_loadStarted == null) throw new ArgumentNullException(nameof(_loadStarted));
            if(_loadEnded == null) throw new ArgumentNullException(nameof(_loadEnded));
            if(_titleChanged == null) throw new ArgumentNullException(nameof(_titleChanged));
            if(_historyChanged == null) throw new ArgumentNullException(nameof(_historyChanged));
            if(_urlChanged == null) throw new ArgumentNullException(nameof(_urlChanged));

            HostCallbacks = new HostCallbacks
            {
                wakeup = Marshal.GetFunctionPointerForDelegate(_wakeUp),
                flush = Marshal.GetFunctionPointerForDelegate(_flush),
                log = Marshal.GetFunctionPointerForDelegate(_log),
                on_load_started = Marshal.GetFunctionPointerForDelegate(_loadStarted),
                on_load_ended = Marshal.GetFunctionPointerForDelegate(_loadEnded),
                on_title_changed = Marshal.GetFunctionPointerForDelegate(_titleChanged),
                on_url_changed = Marshal.GetFunctionPointerForDelegate(_urlChanged),
                on_history_changed = Marshal.GetFunctionPointerForDelegate(_historyChanged)
            };
        }

        void CheckServoResult(Func<ServoResult> action)
        {
            var result = action();
            if (result != ServoResult.Ok)
                throw new ServoException(result);
        }
    }
}
