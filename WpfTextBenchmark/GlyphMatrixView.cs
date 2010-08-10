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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfTextBenchmark
{
	/// <summary>
	/// Description of GlyphMatrixView.
	/// </summary>
	public class GlyphMatrixView : MatrixView
	{
		GlyphTypeface cachedTypeface;
		
		public override string ToString()
		{
			return "WPF GlyphRun";
		}
		
		public override IText AddText(string text, double fontSize, SolidColorBrush brush)
		{
			if (cachedTypeface == null) {
				var t = CreateTypeface();
				if (!t.TryGetGlyphTypeface(out cachedTypeface))
					throw new NotSupportedException();
			}
			
			ushort[] glyphIndexes = new ushort[text.Length];
			double[] advanceWidths = new double[text.Length];
			
			double totalWidth = 0;
			for (int n = 0; n < text.Length; n++) {
				ushort glyphIndex;
				cachedTypeface.CharacterToGlyphMap.TryGetValue(text[n], out glyphIndex);
				glyphIndexes[n] = glyphIndex;
				double width = cachedTypeface.AdvanceWidths[glyphIndex] * fontSize;
				advanceWidths[n] = width;
				totalWidth += width;
			}
			
			GlyphRun run = new GlyphRun(cachedTypeface,
			                            bidiLevel: 0,
			                            isSideways: false,
			                            renderingEmSize: fontSize,
			                            glyphIndices: glyphIndexes,
			                            baselineOrigin: new Point(0, Math.Round(cachedTypeface.Baseline * fontSize)),
			                            advanceWidths: advanceWidths,
			                            glyphOffsets: null,
			                            characters: null,
			                            deviceFontName: null,
			                            clusterMap: null,
			                            caretStops: null,
			                            language: null);
			MyText myText = new MyText { run = run, parent = this, brush = brush };
			texts.Add(myText);
			return myText;
		}
		
		List<MyText> texts = new List<MyText>();
		
		protected override void OnRender(DrawingContext drawingContext)
		{
			foreach (MyText text in texts) {
				drawingContext.PushTransform(new TranslateTransform(text.Position.X, text.Position.Y));
				drawingContext.DrawGlyphRun(text.brush, text.run);
				drawingContext.Pop();
			}
			Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate { OnFrameRendered(EventArgs.Empty); }));
		}
		
		class MyText : IText
		{
			public GlyphMatrixView parent;
			public GlyphRun run;
			public Brush brush;
			public Point Position { get; set; }
			
			public void Remove()
			{
				parent.texts.Remove(this);
			}
		}
	}
}
