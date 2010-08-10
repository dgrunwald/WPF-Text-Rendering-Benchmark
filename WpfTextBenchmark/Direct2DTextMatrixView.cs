/* 
 * Copyright (C) 2010 Daniel Grunwald
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
using System.Windows.Controls;

using Managed.Graphics.Direct2D;
using Managed.Graphics.DirectWrite;

namespace WpfTextBenchmark
{
	public class Direct2DTextMatrixView : WinFormsMatrixView
	{
		D2DView d2d;
		
		public Direct2DTextMatrixView()
		{
			host.Child = d2d = new D2DView() { parent = this };
		}
		
		public override string ToString()
		{
			return "Direct2D";
		}
		
		System.Windows.Media.Typeface cachedTypeface;
		List<MyText> texts = new List<MyText>();
		
		public override IText AddText(string text, double fontSize, System.Windows.Media.SolidColorBrush brush)
		{
			if (cachedTypeface == null) {
				cachedTypeface = CreateTypeface();
			}
			var textFormat = d2d.DirectWriteFactory.CreateTextFormat(
				cachedTypeface.FontFamily.Source,
				FontWeight.Normal,
				FontStyle.Normal,
				FontStretch.Normal,
				(float)fontSize);
			textFormat.TextAlignment = TextAlignment.Leading;
			textFormat.ParagraphAlignment = ParagraphAlignment.Near;
			
			var textLayout = d2d.DirectWriteFactory.CreateTextLayout(
				text,
				textFormat,
				300, 100);
			
			MyText myText = new MyText {
				parent = this,
				textFormat = textFormat,
				textLayout = textLayout,
				color = Color.FromRGB(brush.Color.R / 255f, brush.Color.G / 255f, brush.Color.B / 255f)
			};
			texts.Add(myText);
			return myText;
		}
		
		class MyText : IText
		{
			public Direct2DTextMatrixView parent;
			public TextFormat textFormat;
			public TextLayout textLayout;
			public Color color;
			public Brush brush;
			public System.Windows.Point Position { get; set; }
			
			public void Remove()
			{
				textFormat.Dispose();
				textLayout.Dispose();
				parent.texts.Remove(this);
			}
		}
		
		sealed class D2DView : Direct2DControl
		{
			public Direct2DTextMatrixView parent;
			
			protected override void OnCreateDeviceIndependentResources(Direct2DFactory factory)
			{
				base.OnCreateDeviceIndependentResources(factory);
			}
			
			protected override void OnCleanUpDeviceIndependentResources()
			{
				base.OnCleanUpDeviceIndependentResources();
			}
			
			protected override void OnCreateDeviceResources(WindowRenderTarget renderTarget)
			{
				base.OnCreateDeviceResources(renderTarget);
				foreach (MyText text in parent.texts) {
					text.brush = renderTarget.CreateSolidColorBrush(text.color);
				}
			}
			
			protected override void OnCleanUpDeviceResources()
			{
				base.OnCleanUpDeviceResources();
				foreach (MyText text in parent.texts) {
					text.brush.Dispose();
					text.brush = null;
				}
				parent.OnFrameRendered(EventArgs.Empty);
			}
			
			protected override void OnRender(WindowRenderTarget renderTarget)
			{
				foreach (MyText text in parent.texts) {
					renderTarget.DrawTextLayout(new PointF((float)text.Position.X, (float)text.Position.Y),
					                            text.textLayout,
					                            text.brush,
					                            DrawTextOptions.None);
				}
			}
		}
	}
}
