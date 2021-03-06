/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

#[cfg(target_os = "windows")]
extern crate winapi;


#[cfg(not(target_os = "macos"))]
#[allow(non_camel_case_types)]
pub mod egl {

    // From glutin

    use libc;
    use servo::gl;
    use std::ffi::CString;
    use std::os::raw::c_void;
    use std::rc::Rc;


    #[cfg(target_os = "windows")]
    pub type EGLNativeWindowType = winapi::HWND;
    #[cfg(target_os = "linux")]
    pub type EGLNativeWindowType = *const libc::c_void;
    #[cfg(target_os = "android")]
    pub type EGLNativeWindowType = *const libc::c_void;
    #[cfg(any(target_os = "dragonfly", target_os = "freebsd", target_os = "openbsd"))]
    pub type EGLNativeWindowType = *const libc::c_void;

    pub type khronos_utime_nanoseconds_t = khronos_uint64_t;
    pub type khronos_uint64_t = libc::uint64_t;
    pub type khronos_ssize_t = libc::c_long;
    pub type EGLint = libc::int32_t;
    pub type EGLNativeDisplayType = *const libc::c_void;
    pub type EGLNativePixmapType = *const libc::c_void;     // FIXME: egl_native_pixmap_t instead
    pub type NativeDisplayType = EGLNativeDisplayType;
    pub type NativePixmapType = EGLNativePixmapType;
    pub type NativeWindowType = EGLNativeWindowType;

    include!(concat!(env!("OUT_DIR"), "/egl_bindings.rs"));

    pub fn init() -> Rc<gl::Gl> {
        info!("init_egl");
        unsafe {
            gl::GlesFns::load_with(|addr| {
                let addr = CString::new(addr.as_bytes()).unwrap();
                let addr = addr.as_ptr();
                let egl = Egl;
                egl.GetProcAddress(addr) as *const c_void
            })
        }
    }
}

#[cfg(target_os = "macos")]
pub mod gl {
    use servo::gl;
    use std::os::raw::c_void;
    use std::rc::Rc;
    use std::str;
    use core_foundation::base::TCFType;
    use core_foundation::string::CFString;
    use core_foundation::bundle::{CFBundleGetBundleWithIdentifier, CFBundleGetFunctionPointerForName};

    #[cfg(target_os = "macos")]
    pub fn init() -> Rc<gl::Gl> {
        info!("init_gl");
        unsafe {
            gl::GlFns::load_with(|addr| {
                let symbol_name: CFString = str::FromStr::from_str(addr).unwrap();
                let framework_name: CFString = str::FromStr::from_str("com.apple.opengl").unwrap();
                let framework = CFBundleGetBundleWithIdentifier(framework_name.as_concrete_TypeRef());
                let symbol = CFBundleGetFunctionPointerForName(framework, symbol_name.as_concrete_TypeRef());
                symbol as *const c_void
            })
        }
    }
}
