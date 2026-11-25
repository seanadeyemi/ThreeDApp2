using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ThreeDApp2
{
    /// <summary>
    /// GPU renderer using OpenGL through opengl32.dll, which every Windows PC has, so no extra packages
    /// are needed. It draws the faces and lines of a <see cref="GpuFrame"/> with a real depth buffer and
    /// 4x multisampling into an off-screen framebuffer, then reads the picture back as a bitmap. The form
    /// paints that bitmap and draws its usual overlays (handles, snap markers, gizmo) on top, so every
    /// tool works exactly the same in both renderers.
    /// </summary>
    public sealed class GlRenderer : IDisposable
    {
        #region Native

        const uint GL_COLOR_BUFFER_BIT = 0x4000, GL_DEPTH_BUFFER_BIT = 0x0100;
        const uint GL_DEPTH_TEST = 0x0B71, GL_BLEND = 0x0BE2, GL_CULL_FACE = 0x0B44, GL_POLYGON_OFFSET_FILL = 0x8037;
        const uint GL_MULTISAMPLE = 0x809D, GL_LINE_SMOOTH = 0x0B20, GL_DITHER = 0x0BD0;
        const uint GL_TRIANGLES = 4, GL_LINES = 1, GL_FLOAT = 0x1406, GL_UNSIGNED_BYTE = 0x1401;
        const uint GL_VERTEX_ARRAY = 0x8074, GL_COLOR_ARRAY = 0x8076;
        const uint GL_PROJECTION = 0x1701, GL_MODELVIEW = 0x1700;
        const uint GL_LEQUAL = 0x0203, GL_SRC_ALPHA = 0x0302, GL_ONE_MINUS_SRC_ALPHA = 0x0303, GL_ONE = 1;
        const uint GL_RENDERER = 0x1F01, GL_VERSION = 0x1F02, GL_VENDOR = 0x1F00;
        const uint GL_FRAMEBUFFER = 0x8D40, GL_READ_FRAMEBUFFER = 0x8CA8, GL_DRAW_FRAMEBUFFER = 0x8CA9, GL_RENDERBUFFER = 0x8D41;
        const uint GL_COLOR_ATTACHMENT0 = 0x8CE0, GL_DEPTH_ATTACHMENT = 0x8D00, GL_RGBA8 = 0x8058, GL_DEPTH_COMPONENT24 = 0x81A6;
        const uint GL_FRAMEBUFFER_COMPLETE = 0x8CD5, GL_NEAREST = 0x2600, GL_BGRA = 0x80E1, GL_PACK_ALIGNMENT = 0x0D05;
        const uint GL_ARRAY_BUFFER = 0x8892, GL_STATIC_DRAW = 0x88E4;

        [StructLayout(LayoutKind.Sequential)]
        struct PIXELFORMATDESCRIPTOR
        {
            public ushort nSize, nVersion;
            public uint dwFlags;
            public byte iPixelType, cColorBits, cRedBits, cRedShift, cGreenBits, cGreenShift, cBlueBits, cBlueShift;
            public byte cAlphaBits, cAlphaShift, cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits, cAccumAlphaBits;
            public byte cDepthBits, cStencilBits, cAuxBuffers, iLayerType, bReserved;
            public uint dwLayerMask, dwVisibleMask, dwDamageMask;
        }

        [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("gdi32.dll")] static extern bool SetPixelFormat(IntPtr hdc, int format, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("opengl32.dll")] static extern IntPtr wglCreateContext(IntPtr hdc);
        [DllImport("opengl32.dll")] static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);
        [DllImport("opengl32.dll")] static extern bool wglDeleteContext(IntPtr hglrc);
        [DllImport("opengl32.dll")] static extern IntPtr wglGetProcAddress(string name);
        [DllImport("opengl32.dll")] static extern IntPtr wglGetCurrentContext();
        [DllImport("opengl32.dll")] static extern void glViewport(int x, int y, int w, int h);
        [DllImport("opengl32.dll")] static extern void glClearColor(float r, float g, float b, float a);
        [DllImport("opengl32.dll")] static extern void glClear(uint mask);
        [DllImport("opengl32.dll")] static extern void glEnable(uint cap);
        [DllImport("opengl32.dll")] static extern void glDisable(uint cap);
        [DllImport("opengl32.dll")] static extern void glDepthFunc(uint func);
        [DllImport("opengl32.dll")] static extern void glDepthMask(byte flag);
        [DllImport("opengl32.dll")] static extern void glBlendFunc(uint s, uint d);
        [DllImport("opengl32.dll")] static extern void glMatrixMode(uint mode);
        [DllImport("opengl32.dll")] static extern void glLoadMatrixd(double[] m);
        [DllImport("opengl32.dll")] static extern void glEnableClientState(uint array);
        [DllImport("opengl32.dll")] static extern void glDisableClientState(uint array);
        [DllImport("opengl32.dll")] static extern void glVertexPointer(int size, uint type, int stride, IntPtr ptr);
        [DllImport("opengl32.dll")] static extern void glColorPointer(int size, uint type, int stride, IntPtr ptr);
        [DllImport("opengl32.dll")] static extern void glDrawArrays(uint mode, int first, int count);
        [DllImport("opengl32.dll")] static extern void glLineWidth(float w);
        [DllImport("opengl32.dll")] static extern void glPolygonOffset(float factor, float units);
        [DllImport("opengl32.dll")] static extern void glReadPixels(int x, int y, int w, int h, uint format, uint type, IntPtr data);
        [DllImport("opengl32.dll")] static extern void glPixelStorei(uint pname, int param);
        [DllImport("opengl32.dll")] static extern IntPtr glGetString(uint name);
        [DllImport("opengl32.dll")] static extern void glFinish();

        // Framebuffer functions are not exported by opengl32.dll; they come from the driver.
        delegate void GenDel(int n, out uint id);
        delegate void DelDel(int n, ref uint id);
        delegate void BindDel(uint target, uint id);
        delegate void FbRbDel(uint target, uint attachment, uint rbTarget, uint rb);
        delegate void RbStorageDel(uint target, uint format, int w, int h);
        delegate void RbStorageMsDel(uint target, int samples, uint format, int w, int h);
        delegate uint StatusDel(uint target);
        delegate void BlitDel(int sx0, int sy0, int sx1, int sy1, int dx0, int dy0, int dx1, int dy1, uint mask, uint filter);
        delegate void BlendSepDel(uint srcRgb, uint dstRgb, uint srcA, uint dstA);
        delegate void BufferDataDel(uint target, IntPtr size, IntPtr data, uint usage);

        GenDel glGenFramebuffers, glGenRenderbuffers;
        DelDel glDeleteFramebuffers, glDeleteRenderbuffers;
        BindDel glBindFramebuffer, glBindRenderbuffer;
        FbRbDel glFramebufferRenderbuffer;
        RbStorageDel glRenderbufferStorage;
        RbStorageMsDel glRenderbufferStorageMultisample;
        StatusDel glCheckFramebufferStatus;
        BlitDel glBlitFramebuffer;
        BlendSepDel glBlendFuncSeparate;
        GenDel glGenBuffers;
        DelDel glDeleteBuffers;
        BindDel glBindBuffer;
        BufferDataDel glBufferData;

        T Proc<T>(string name) where T : class
        {
            foreach (string n in new[] { name, name + "EXT", name + "ARB" })
            {
                IntPtr p = wglGetProcAddress(n);
                long v = p.ToInt64();
                if (v != 0 && v != 1 && v != 2 && v != 3 && v != -1)
                    return Marshal.GetDelegateForFunctionPointer(p, typeof(T)) as T;
            }
            return null;
        }

        #endregion

        readonly NativeWindow mWindow = new NativeWindow();
        IntPtr mDC, mContext;
        uint mFbo, mColorRb, mDepthRb;          // Drawing target (multisampled when possible)
        uint mResolveFbo, mResolveRb;           // Plain target the multisampled picture is resolved into
        int mSamples, mWidth, mHeight;
        Bitmap mBitmap;
        uint mTriBuffer;            // Face positions kept on the graphics card between frames
        int mTriBufferStamp;
        byte[] mPixels = new byte[0];

        /// <summary>The graphics card and driver, e.g. "NVIDIA Quadro K1100M (OpenGL 4.6)".</summary>
        public string Description { get; private set; }

        GlRenderer() { }

        /// <summary>Sets up OpenGL, or returns null with the reason if this PC can't do it.</summary>
        public static GlRenderer TryCreate(out string error)
        {
            GlRenderer r = new GlRenderer();
            try
            {
                r.Init();
                error = null;
                return r;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                r.Dispose();
                return null;
            }
        }

        void Init()
        {
            CreateParams cp = new CreateParams { Caption = "ThreeDApp2 GL", Style = unchecked((int)0x80000000) | 0x06000000, Width = 1, Height = 1 };
            mWindow.CreateHandle(cp);   // Hidden: WS_POPUP | WS_CLIPCHILDREN | WS_CLIPSIBLINGS, never shown
            mDC = GetDC(mWindow.Handle);
            if (mDC == IntPtr.Zero) throw new InvalidOperationException("No device context for OpenGL.");
            PIXELFORMATDESCRIPTOR pfd = new PIXELFORMATDESCRIPTOR
            {
                nSize = (ushort)Marshal.SizeOf(typeof(PIXELFORMATDESCRIPTOR)),
                nVersion = 1,
                dwFlags = 0x04 | 0x20 | 0x01,   // Draw to window, support OpenGL, double buffer
                cColorBits = 32,
                cAlphaBits = 8,
                cDepthBits = 24,
            };
            int format = ChoosePixelFormat(mDC, ref pfd);
            if (format == 0 || !SetPixelFormat(mDC, format, ref pfd)) throw new InvalidOperationException("No OpenGL pixel format is available.");
            mContext = wglCreateContext(mDC);
            if (mContext == IntPtr.Zero || !wglMakeCurrent(mDC, mContext)) throw new InvalidOperationException("Could not start OpenGL.");

            string renderer = Marshal.PtrToStringAnsi(glGetString(GL_RENDERER)) ?? "?";
            string version = Marshal.PtrToStringAnsi(glGetString(GL_VERSION)) ?? "?";
            Description = renderer + " (OpenGL " + version.Split(' ')[0] + ")";

            glGenFramebuffers = Proc<GenDel>("glGenFramebuffers");
            glGenRenderbuffers = Proc<GenDel>("glGenRenderbuffers");
            glDeleteFramebuffers = Proc<DelDel>("glDeleteFramebuffers");
            glDeleteRenderbuffers = Proc<DelDel>("glDeleteRenderbuffers");
            glBindFramebuffer = Proc<BindDel>("glBindFramebuffer");
            glBindRenderbuffer = Proc<BindDel>("glBindRenderbuffer");
            glFramebufferRenderbuffer = Proc<FbRbDel>("glFramebufferRenderbuffer");
            glRenderbufferStorage = Proc<RbStorageDel>("glRenderbufferStorage");
            glRenderbufferStorageMultisample = Proc<RbStorageMsDel>("glRenderbufferStorageMultisample");
            glCheckFramebufferStatus = Proc<StatusDel>("glCheckFramebufferStatus");
            glBlitFramebuffer = Proc<BlitDel>("glBlitFramebuffer");
            glBlendFuncSeparate = Proc<BlendSepDel>("glBlendFuncSeparate");
            glGenBuffers = Proc<GenDel>("glGenBuffers");
            glDeleteBuffers = Proc<DelDel>("glDeleteBuffers");
            glBindBuffer = Proc<BindDel>("glBindBuffer");
            glBufferData = Proc<BufferDataDel>("glBufferData");
            if (glGenFramebuffers == null || glBindFramebuffer == null || glGenRenderbuffers == null || glBindRenderbuffer == null ||
                glFramebufferRenderbuffer == null || glRenderbufferStorage == null || glCheckFramebufferStatus == null)
                throw new NotSupportedException("The graphics driver (" + Description + ") has no off-screen framebuffers. Update the graphics driver, or use the Standard renderer.");
            mSamples = glRenderbufferStorageMultisample != null && glBlitFramebuffer != null ? 4 : 0;
        }

        void Resize(int w, int h)
        {
            if (w == mWidth && h == mHeight && mFbo != 0) return;
            FreeTargets();
            mWidth = w;
            mHeight = h;
            if (!MakeTargets(mSamples))
            {
                FreeTargets();
                mSamples = 0;   // Some drivers refuse multisampling; fall back to plain
                if (!MakeTargets(0)) throw new InvalidOperationException("The graphics driver could not make an off-screen picture of " + w + "x" + h + ".");
            }
            if (mBitmap != null) mBitmap.Dispose();
            mBitmap = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
            mPixels = new byte[w * h * 4];
        }

        bool MakeTargets(int samples)
        {
            glGenFramebuffers(1, out mFbo);
            glBindFramebuffer(GL_FRAMEBUFFER, mFbo);
            glGenRenderbuffers(1, out mColorRb);
            glBindRenderbuffer(GL_RENDERBUFFER, mColorRb);
            if (samples > 0) glRenderbufferStorageMultisample(GL_RENDERBUFFER, samples, GL_RGBA8, mWidth, mHeight);
            else glRenderbufferStorage(GL_RENDERBUFFER, GL_RGBA8, mWidth, mHeight);
            glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_RENDERBUFFER, mColorRb);
            glGenRenderbuffers(1, out mDepthRb);
            glBindRenderbuffer(GL_RENDERBUFFER, mDepthRb);
            if (samples > 0) glRenderbufferStorageMultisample(GL_RENDERBUFFER, samples, GL_DEPTH_COMPONENT24, mWidth, mHeight);
            else glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT24, mWidth, mHeight);
            glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, mDepthRb);
            if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE) return false;

            if (samples > 0)
            {
                glGenFramebuffers(1, out mResolveFbo);
                glBindFramebuffer(GL_FRAMEBUFFER, mResolveFbo);
                glGenRenderbuffers(1, out mResolveRb);
                glBindRenderbuffer(GL_RENDERBUFFER, mResolveRb);
                glRenderbufferStorage(GL_RENDERBUFFER, GL_RGBA8, mWidth, mHeight);
                glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_RENDERBUFFER, mResolveRb);
                if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE) return false;
            }
            return true;
        }

        void FreeTargets()
        {
            if (glDeleteFramebuffers != null)
            {
                if (mFbo != 0) glDeleteFramebuffers(1, ref mFbo);
                if (mResolveFbo != 0) glDeleteFramebuffers(1, ref mResolveFbo);
            }
            if (glDeleteRenderbuffers != null)
            {
                if (mColorRb != 0) glDeleteRenderbuffers(1, ref mColorRb);
                if (mDepthRb != 0) glDeleteRenderbuffers(1, ref mDepthRb);
                if (mResolveRb != 0) glDeleteRenderbuffers(1, ref mResolveRb);
            }
            mFbo = mResolveFbo = mColorRb = mDepthRb = mResolveRb = 0;
        }

        /// <summary>Draws the frame and returns it as a bitmap with a transparent background.</summary>
        public Bitmap Render(GpuFrame f)
        {
            if (wglGetCurrentContext() != mContext) wglMakeCurrent(mDC, mContext);
            Resize(f.Width, f.Height);

            glBindFramebuffer(GL_FRAMEBUFFER, mFbo);
            glViewport(0, 0, f.Width, f.Height);
            glClearColor(0, 0, 0, 0);
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            if (mSamples > 0) glEnable(GL_MULTISAMPLE);
            glDisable(GL_CULL_FACE);   // Faces are seen from both sides
            glDisable(GL_DITHER);
            glEnable(GL_DEPTH_TEST);
            glDepthFunc(GL_LEQUAL);
            glMatrixMode(GL_PROJECTION);
            glLoadMatrixd(f.Projection);
            glMatrixMode(GL_MODELVIEW);
            glLoadMatrixd(f.ModelView);
            glEnable(GL_BLEND);
            // Keep the picture premultiplied so its soft edges blend correctly over the sky and grid.
            if (glBlendFuncSeparate != null) glBlendFuncSeparate(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
            else glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            glEnableClientState(GL_VERTEX_ARRAY);
            glEnableClientState(GL_COLOR_ARRAY);

            // Faces, pushed back a little so their outlines draw cleanly on top.
            glEnable(GL_POLYGON_OFFSET_FILL);
            glPolygonOffset(1f, 1f);
            if (f.TrianglesTranslucent) glDepthMask(0);
            DrawTriangles(f);
            glDepthMask(1);
            glDisable(GL_POLYGON_OFFSET_FILL);

            if (mSamples == 0) glEnable(GL_LINE_SMOOTH);
            foreach (GpuLines lines in f.Lines)
            {
                if (lines.VertexCount == 0) continue;
                glLineWidth(lines.Width);
                Draw(GL_LINES, lines.Positions, lines.Colors, lines.VertexCount);
            }
            glDisable(GL_LINE_SMOOTH);
            glDisableClientState(GL_COLOR_ARRAY);
            glDisableClientState(GL_VERTEX_ARRAY);

            // Resolve the multisampled picture, then read it back.
            if (mSamples > 0)
            {
                glBindFramebuffer(GL_READ_FRAMEBUFFER, mFbo);
                glBindFramebuffer(GL_DRAW_FRAMEBUFFER, mResolveFbo);
                glBlitFramebuffer(0, 0, f.Width, f.Height, 0, 0, f.Width, f.Height, GL_COLOR_BUFFER_BIT, GL_NEAREST);
                glBindFramebuffer(GL_FRAMEBUFFER, mResolveFbo);
            }
            glPixelStorei(GL_PACK_ALIGNMENT, 4);
            GCHandle h = GCHandle.Alloc(mPixels, GCHandleType.Pinned);
            try { glReadPixels(0, 0, f.Width, f.Height, GL_BGRA, GL_UNSIGNED_BYTE, h.AddrOfPinnedObject()); }
            finally { h.Free(); }
            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            // OpenGL rows run bottom to top; bitmaps run top to bottom.
            BitmapData data = mBitmap.LockBits(new Rectangle(0, 0, f.Width, f.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            try
            {
                int row = f.Width * 4;
                for (int y = 0; y < f.Height; y++)
                    Marshal.Copy(mPixels, (f.Height - 1 - y) * row, data.Scan0 + y * data.Stride, row);
            }
            finally { mBitmap.UnlockBits(data); }
            return mBitmap;
        }

        /// <summary>
        /// Faces: positions stay on the graphics card until the shapes change (only colours are sent
        /// each frame), which matters for big models. Falls back to plain arrays on old drivers.
        /// </summary>
        void DrawTriangles(GpuFrame f)
        {
            if (f.TriVertexCount == 0) return;
            bool vbo = glGenBuffers != null && glBindBuffer != null && glBufferData != null && f.GeometryStamp > 0;
            if (!vbo) { Draw(GL_TRIANGLES, f.TriPositions, f.TriColors, f.TriVertexCount); return; }
            if (mTriBuffer == 0) glGenBuffers(1, out mTriBuffer);
            glBindBuffer(GL_ARRAY_BUFFER, mTriBuffer);
            if (mTriBufferStamp != f.GeometryStamp)
            {
                GCHandle hp = GCHandle.Alloc(f.TriPositions, GCHandleType.Pinned);
                try { glBufferData(GL_ARRAY_BUFFER, new IntPtr((long)f.TriVertexCount * 12), hp.AddrOfPinnedObject(), GL_STATIC_DRAW); }
                finally { hp.Free(); }
                mTriBufferStamp = f.GeometryStamp;
            }
            glVertexPointer(3, GL_FLOAT, 0, IntPtr.Zero);
            glBindBuffer(GL_ARRAY_BUFFER, 0);
            GCHandle hc = GCHandle.Alloc(f.TriColors, GCHandleType.Pinned);
            try
            {
                glColorPointer(4, GL_UNSIGNED_BYTE, 0, hc.AddrOfPinnedObject());
                glDrawArrays(GL_TRIANGLES, 0, f.TriVertexCount);
                glFinish();
            }
            finally { hc.Free(); }
        }

        static void Draw(uint mode, float[] positions, byte[] colors, int count)
        {
            if (count == 0) return;
            GCHandle hp = GCHandle.Alloc(positions, GCHandleType.Pinned);
            GCHandle hc = GCHandle.Alloc(colors, GCHandleType.Pinned);
            try
            {
                glVertexPointer(3, GL_FLOAT, 0, hp.AddrOfPinnedObject());
                glColorPointer(4, GL_UNSIGNED_BYTE, 0, hc.AddrOfPinnedObject());
                glDrawArrays(mode, 0, count);
                glFinish();   // The arrays are unpinned right after, so let the driver finish reading them
            }
            finally { hp.Free(); hc.Free(); }
        }

        public void Dispose()
        {
            try
            {
                if (mContext != IntPtr.Zero)
                {
                    wglMakeCurrent(mDC, mContext);
                    FreeTargets();
                    if (mTriBuffer != 0 && glDeleteBuffers != null) glDeleteBuffers(1, ref mTriBuffer);
                    wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                    wglDeleteContext(mContext);
                }
            }
            catch (Exception) { }
            mContext = IntPtr.Zero;
            if (mDC != IntPtr.Zero && mWindow.Handle != IntPtr.Zero) ReleaseDC(mWindow.Handle, mDC);
            mDC = IntPtr.Zero;
            if (mWindow.Handle != IntPtr.Zero) mWindow.DestroyHandle();
            if (mBitmap != null) { mBitmap.Dispose(); mBitmap = null; }
        }
    }
}
