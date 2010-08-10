/* 
 * 
 * Authors:
 *  Dmitry Kolchev <dmitrykolchev@msn.com>
 *
 * Copyright (C) 2010 Dmitry Kolchev
 *
 * This sourcecode is licenced under The GNU Lesser General Public License
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
 * NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Managed.Graphics.Direct2D;
using Managed.Graphics.DirectWrite;
using Managed.Graphics.Imaging;

namespace WpfTextBenchmark
{
	public class Direct2DControl : Control
	{
		private Direct2DFactory _factory;
		private DirectWriteFactory _directWriteFactory;
		private WicImagingFactory _imagingFactory;
		private WindowRenderTarget _renderTarget;
		private bool _deviceIndependedResourcesCreated;

		public Direct2DControl()
		{
			SetStyle(
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.Opaque |
				ControlStyles.UserPaint, true);
		}

		protected override void Dispose(bool disposing)
		{
			CleanUpDeviceIndependentResources();
			base.Dispose(disposing);
		}
		
		public Direct2DFactory Direct2DFactory
		{
			get
			{
				return this._factory;
			}
		}

		public DirectWriteFactory DirectWriteFactory
		{
			get
			{
				if (this._directWriteFactory == null)
				{
					this._directWriteFactory = DirectWriteFactory.Create(DirectWriteFactoryType.Shared);
				}
				return this._directWriteFactory;
			}
		}

		protected WicImagingFactory ImagingFactory
		{
			get
			{
				if (this._imagingFactory == null)
				{
					this._imagingFactory = WicImagingFactory.Create();
				}
				return this._imagingFactory;
			}
		}

		protected WindowRenderTarget RenderTarget
		{
			get
			{
				return this._renderTarget;
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			CreateDeviceIndependentResources();
			base.OnHandleCreated(e);
		}

		private void CreateDeviceIndependentResources()
		{
			try {
				this._factory = Direct2DFactory.CreateFactory(FactoryType.SingleThreaded, DebugLevel.None);
				this._renderTarget = this._factory.CreateWindowRenderTarget(this);
				OnCreateDeviceIndependentResources(this._factory);
				this._deviceIndependedResourcesCreated = true;
			} catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
				throw;
			}
		}

		private void CleanUpDeviceIndependentResources()
		{
			if(this._deviceIndependedResourcesCreated)
				OnCleanUpDeviceIndependentResources();
			if (this._imagingFactory != null)
			{
				this._imagingFactory.Dispose();
				this._imagingFactory = null;
			}
			if (this._directWriteFactory != null)
			{
				this._directWriteFactory.Dispose();
				this._directWriteFactory = null;
			}
			if (this._renderTarget != null)
			{
				this._renderTarget.Dispose();
				this._renderTarget = null;
			}
			if (this._factory != null)
			{
				this._factory.Dispose();
				this._factory = null;
			}
		}

		protected virtual void OnCleanUpDeviceIndependentResources()
		{
		}

		private void CleanUpDeviceResources()
		{
			OnCleanUpDeviceResources();
		}

		protected virtual void OnCleanUpDeviceResources()
		{
		}

		private void CreateDeviceResources()
		{
			OnCreateDeviceResources(this._renderTarget);
		}

		protected virtual void OnCreateDeviceResources(WindowRenderTarget renderTarget)
		{
		}

		protected virtual void OnCreateDeviceIndependentResources(Direct2DFactory factory)
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			CreateDeviceResources();
			try
			{
				this._renderTarget.BeginDraw();
				try
				{
					this._renderTarget.Transform = Matrix3x2.Identity;
					this._renderTarget.Clear(Color.FromRGB(this.BackColor.R, this.BackColor.G, this.BackColor.B));
					OnRender(RenderTarget);
				}
				finally
				{
					this._renderTarget.EndDraw();
				}
			}
			finally
			{
				CleanUpDeviceResources();
			}
		}

		public void Render()
		{
			OnPaint(null);
		}

		protected virtual void OnRender(WindowRenderTarget renderTarget)
		{
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (this.RenderTarget != null)
			{
				this.RenderTarget.Resize(new SizeU { Width = (uint)ClientSize.Width, Height = (uint)ClientSize.Height });
				Invalidate();
			}
		}
	}
}
