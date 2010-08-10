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
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;

namespace WpfTextBenchmark
{
	public class TextLineMatrixView : MatrixView
	{
		TextFormatter formatter = TextFormatter.Create(TextFormattingMode.Display);
		Typeface cachedTypeface;
		
		public override string ToString()
		{
			return "WPF TextLine";
		}
		
		public override IText AddText(string text, double fontSize, SolidColorBrush brush)
		{
			if (cachedTypeface == null) {
				cachedTypeface = CreateTypeface();
			}
			GlobalTextRunProperties p = new GlobalTextRunProperties {
				typeface = cachedTypeface,
				fontRenderingEmSize = fontSize,
				foregroundBrush = brush,
				cultureInfo = CultureInfo.CurrentCulture
			};
			MyTextSource myTextSource = new MyTextSource { text = text, textRunProperties = p };
			TextLine line = formatter.FormatLine(myTextSource, 0, 500, new MyTextParagraphProperties {defaultTextRunProperties = p}, null);
			MyText myText = new MyText { line = line, parent = this };
			texts.Add(myText);
			return myText;
		}
		
		List<MyText> texts = new List<MyText>();
		
		protected override void OnRender(DrawingContext drawingContext)
		{
			foreach (MyText text in texts) {
				text.line.Draw(drawingContext, text.Position, InvertAxes.None);
			}
			Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate { OnFrameRendered(EventArgs.Empty); }));
		}
		
		class MyText : IText
		{
			public TextLineMatrixView parent;
			public TextLine line;
			public Point Position { get; set; }
			
			public void Remove()
			{
				line.Dispose();
				parent.texts.Remove(this);
			}
		}
		
		class MyTextSource : TextSource
		{
			public string text;
			public TextRunProperties textRunProperties;
			
			public override TextRun GetTextRun(int textSourceCharacterIndex)
			{
				if (textSourceCharacterIndex == 0)
					return new TextCharacters(text, 0, text.Length, textRunProperties);
				return new TextEndOfParagraph(1);
			}
			
			public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
			{
				throw new NotImplementedException();
			}
			
			public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
			{
				throw new NotImplementedException();
			}
		}
		
		sealed class GlobalTextRunProperties : TextRunProperties
		{
			internal Typeface typeface;
			internal double fontRenderingEmSize;
			internal Brush foregroundBrush;
			internal Brush backgroundBrush = null;
			internal System.Globalization.CultureInfo cultureInfo;
			
			public override Typeface Typeface { get { return typeface; } }
			public override double FontRenderingEmSize { get { return fontRenderingEmSize; } }
			public override double FontHintingEmSize { get { return fontRenderingEmSize; } }
			public override TextDecorationCollection TextDecorations { get { return null; } }
			public override Brush ForegroundBrush { get { return foregroundBrush; } }
			public override Brush BackgroundBrush { get { return backgroundBrush; } }
			public override System.Globalization.CultureInfo CultureInfo { get { return cultureInfo; } }
			public override TextEffectCollection TextEffects { get { return null; } }
		}
		
		class MyTextParagraphProperties : TextParagraphProperties
		{
			internal TextRunProperties defaultTextRunProperties;
			internal TextWrapping textWrapping = TextWrapping.Wrap;
			internal double tabSize = 40;
			
			public override double DefaultIncrementalTab {
				get { return tabSize; }
			}
			
			public override FlowDirection FlowDirection { get { return FlowDirection.LeftToRight; } }
			public override TextAlignment TextAlignment { get { return TextAlignment.Left; } }
			public override double LineHeight { get { return double.NaN; } }
			public override bool FirstLineInParagraph { get { return false; } }
			public override TextRunProperties DefaultTextRunProperties { get { return defaultTextRunProperties; } }
			public override TextWrapping TextWrapping { get { return textWrapping; } }
			public override TextMarkerProperties TextMarkerProperties { get { return null; } }
			public override double Indent { get { return 0; } }
		}
	}
}
