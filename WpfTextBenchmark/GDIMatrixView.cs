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
using System.Drawing;
using System.Windows.Forms;

namespace WpfTextBenchmark
{
	public class GDIMatrixView : WinFormsMatrixView
	{
		GDITextView view;
		
		public GDIMatrixView(bool textRenderer)
		{
			host.Child = view = new GDITextView() { parent = this, textRenderer = textRenderer };
		}
		
		public override string ToString()
		{
			return view.textRenderer ? "GDI (TextRenderer)" : "GDI+ (Graphics.DrawString)";
		}
		
		System.Windows.Media.Typeface cachedTypeface;
		List<MyText> texts = new List<MyText>();
		
		public override IText AddText(string text, double fontSize, System.Windows.Media.SolidColorBrush brush)
		{
			if (cachedTypeface == null) {
				cachedTypeface = CreateTypeface();
			}
			
			MyText myText = new MyText {
				parent = this,
				text = text,
				color = Color.FromArgb(brush.Color.R, brush.Color.G, brush.Color.B),
				font = new Font(cachedTypeface.FontFamily.Source, (float)(fontSize / 96 * 72), FontStyle.Regular),
			};
			myText.brush = new SolidBrush(myText.color);
			texts.Add(myText);
			return myText;
		}
		
		class MyText : IText
		{
			public string text;
			public GDIMatrixView parent;
			public Font font;
			public Color color;
			public SolidBrush brush;
			public System.Windows.Point Position { get; set; }
			
			public void Remove()
			{
				font.Dispose();
				brush.Dispose();
				parent.texts.Remove(this);
			}
		}
		
		sealed class GDITextView : Control
		{
			public GDIMatrixView parent;
			public bool textRenderer;
			
			public GDITextView()
			{
				this.BackColor = Color.Black;
				SetStyle(
					ControlStyles.AllPaintingInWmPaint |
					ControlStyles.DoubleBuffer |
					ControlStyles.Opaque |
					ControlStyles.UserPaint, true);
			}
			
			protected override void OnPaint(PaintEventArgs e)
			{
				e.Graphics.Clear(Color.Black);
				if (textRenderer) {
					foreach (MyText text in parent.texts) {
						TextRenderer.DrawText(e.Graphics, text.text, text.font, new Point((int)text.Position.X, (int)text.Position.Y), text.color, TextFormatFlags.NoClipping | TextFormatFlags.NoPrefix);
					}
				} else {
					StringFormat f = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox);
					foreach (MyText text in parent.texts) {
						e.Graphics.DrawString(text.text, text.font, text.brush, (float)text.Position.X, (float)text.Position.Y, f);
					}
					f.Dispose();
				}
				parent.OnFrameRendered(EventArgs.Empty);
			}
		}
	}
}
